using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.IO;


namespace FnB_Records
{
    public partial class Register : Form
    {

        public Register()
        {
            InitializeComponent();
        }

        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {

        }

        private void lblMasukSekarang_Click(object sender, EventArgs e)
        {
            Form loginForm = new Login();
            loginForm.ShowDialog();
            this.Hide();
        }

        private async void btnMasuk_Click(object sender, EventArgs e)
        {
            string kode = txtkode.Text.Trim();


            bool valid = await CekAktivasi(kode);

            if (valid)
            {
                MessageBox.Show("Aktivasi Berhasil! Premium aktif.");
            }
            else
            {
                MessageBox.Show("Kode salah / tidak valid!");
            }
        }

        public async Task<bool> CekAktivasi(string kode)
        {
            using (HttpClient client = new HttpClient())
            {
                var data = new
                {
                    code = kode,
                    device_id = Environment.MachineName
                };

                string json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage res = await client.PostAsync(
                    "https://kode-aktivasi-dashboard-manager.vercel.app/api/activate",
                    content
                );

                string result = await res.Content.ReadAsStringAsync();

                var response = JsonSerializer.Deserialize<AktivasiResponse>(result);

                if (response != null && response.status == "valid")
                {
                    SimpanLisensi(response.premium_until);
                    return true;
                }

                return false;
            }
        }

        public void SimpanLisensi(string premiumUntil)
        {
            File.WriteAllText("license.key",
                $"premium=true\nexpires={premiumUntil}");
        }

        public bool CekLisensi()
        {
            return File.Exists("license.key");
        }

        public class AktivasiResponse
        {
            public string status { get; set; }
            public string premium_until { get; set; }
        }
    }
}
