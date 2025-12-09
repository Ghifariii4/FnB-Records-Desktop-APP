using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class Form_Verifikasi : Form
    {
        // Variabel untuk menyimpan hasil inputan agar bisa dibaca Form lain
        public string PasswordInput { get; private set; }
        public bool IsConfirmed { get; private set; } = false;

        public Form_Verifikasi()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.None; // Tampilan modern tanpa border window

            // Setting TextBox Password agar muncul bintang/titik
            txtPassword.PasswordChar = '●';
        }

        private void btnKonfirmasi_Click(object sender, EventArgs e)
        {
            PasswordInput = txtPassword.Text; // Ambil text
            IsConfirmed = true; // Tandai user menekan konfirmasi
            this.Close(); // Tutup pop-up
        }

        private void btnBatal_Click(object sender, EventArgs e)
        {
            IsConfirmed = false;
            this.Close();
        }
    }
}
