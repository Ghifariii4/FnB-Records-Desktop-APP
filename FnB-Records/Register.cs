using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using Npgsql;
using FnB_Records.Koneksi_DB;

namespace FnB_Records
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
            txtInputKataSandi.TextChanged += TxtInputKataSandi_TextChanged;
        }

        private void Register_Load(object sender, EventArgs e)
        {
            // Sembunyikan Panel S&K saat awal
            if (gbsnk != null) gbsnk.Visible = false;
        }

        // --- VALIDASI PASSWORD ---
        private void TxtInputKataSandi_TextChanged(object sender, EventArgs e)
        {
            if (txtInputKataSandi.Text.Length > 0)
            {
                txtKonfirmasiKataSandi.Enabled = true;
            }
            else
            {
                txtKonfirmasiKataSandi.Enabled = false;
                txtKonfirmasiKataSandi.Clear();
            }
        }

        // --- 1. TAHAP PERTAMA: TOMBOL MASUK/DAFTAR DIKLIK ---
        private void btnMasuk_Click(object sender, EventArgs e)
        {
            // Validasi Input
            if (string.IsNullOrEmpty(txtNamaBisnis.Text.Trim()) ||
                string.IsNullOrEmpty(txtEmail.Text.Trim()) ||
                string.IsNullOrEmpty(txtInputKataSandi.Text.Trim()))
            {
                MessageBox.Show("Semua kolom (kecuali kode aktivasi) wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (txtInputKataSandi.Text.Trim() != txtKonfirmasiKataSandi.Text.Trim())
            {
                MessageBox.Show("Konfirmasi kata sandi tidak cocok!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // TAMPILKAN PANEL S&K
            if (gbsnk != null)
            {
                // Reset checkbox agar user harus mencentang ulang setiap kali buka panel
                if (chksnk != null) chksnk.Checked = false;

                gbsnk.Visible = true;
                gbsnk.BringToFront();
            }
        }

        // --- 2. LOGIKA CHECKBOX (PEMICU UTAMA) ---
        // Saat dicentang -> Hide Panel -> Proses Daftar
        private async void chksnk_Click(object sender, EventArgs e)
        {
            // Pastikan kondisi checked true (menghindari trigger saat uncheck)
            if (chksnk.Checked)
            {
                // A. Sembunyikan Panel S&K
                if (gbsnk != null) gbsnk.Visible = false;

                // B. Langsung Jalankan Proses Registrasi
                // Tambahkan sedikit delay agar UI terasa natural (opsional)
                await Task.Delay(200);

                await ProsesRegistrasiUtama();
            }
        }

        // --- 3. PROSES UTAMA REGISTRASI ---
        private async Task ProsesRegistrasiUtama()
        {
            // Matikan tombol utama agar tidak diklik berulang
            btDaftarAkun.Enabled = false;
            btDaftarAkun.Text = "Memproses...";

            string namaBisnis = txtNamaBisnis.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtInputKataSandi.Text.Trim();
            string kodeAktivasi = txtKodeAktivasi.Text.Trim();
            string userRole = "free";

            try
            {
                // Cek Kode Aktivasi API
                if (!string.IsNullOrEmpty(kodeAktivasi))
                {
                    bool isValid = await CekAktivasi(kodeAktivasi);
                    if (isValid) userRole = "premium";
                    else return; // Stop jika kode salah
                }

                // Simpan ke Database
                if (SimpanUserKeDatabase(namaBisnis, email, password, userRole))
                {
                    string pesan = (userRole == "premium") ?
                        "Aktivasi Berhasil! Akun Premium telah dibuat." :
                        "Pendaftaran Berhasil! Akun Free telah dibuat.";

                    MessageBox.Show(pesan, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Pindah ke Login
                    Form loginForm = new Login();
                    loginForm.Show();
                    this.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
                // Jika gagal, munculkan tombol daftar lagi (opsional: atau munculkan S&K lagi)
                if (gbsnk != null) gbsnk.Visible = false;
            }
            finally
            {
                btDaftarAkun.Enabled = true;
                btDaftarAkun.Text = "Daftar Akun";
            }
        }

        // --- TOMBOL BATAL DI S&K ---
        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (gbsnk != null) gbsnk.Visible = false;
            if (chksnk != null) chksnk.Checked = false;
        }

        // --- FUNGSI DATABASE (TETAP SAMA) ---
        private bool SimpanUserKeDatabase(string bisnis, string email, string password, string role)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                    string checkSql = "SELECT COUNT(*) FROM users WHERE email = @email";
                    using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@email", email);
                        if ((long)checkCmd.ExecuteScalar() > 0)
                        {
                            MessageBox.Show("Email sudah terdaftar!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    string sql = @"INSERT INTO users (business_name, email, password, role, created_at) 
                                   VALUES (@bisnis, @email, @pass, @role, CURRENT_TIMESTAMP)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@bisnis", bisnis);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@pass", password);
                        cmd.Parameters.AddWithValue("@role", role);
                        return cmd.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database Error: " + ex.Message);
                return false;
            }
        }

        // --- FUNGSI API (TETAP SAMA) ---
        public async Task<bool> CekAktivasi(string kode)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var data = new { code = kode, device_id = Environment.MachineName };
                    string json = JsonSerializer.Serialize(data);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage res = await client.PostAsync(
                        "https://kode-aktivasi-dashboard-manager.vercel.app/api/activate", content
                    );

                    string result = await res.Content.ReadAsStringAsync();
                    var response = JsonSerializer.Deserialize<AktivasiResponse>(result,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (response != null && response.status == "valid")
                    {
                        SimpanLisensi(response.premium_until);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show($"Aktivasi gagal: {response?.message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error API: " + ex.Message);
                return false;
            }
        }

        public void SimpanLisensi(string premiumUntil) => File.WriteAllText("license.key", $"premium=true\nexpires={premiumUntil}");

        public class AktivasiResponse
        {
            public string status { get; set; }
            public string premium_until { get; set; }
            public string message { get; set; }
        }

        // Event handler kosong (biarkan saja atau hapus)
        private void guna2GroupBox1_Click(object sender, EventArgs e) { }
        private void gbsnk_Click(object sender, EventArgs e) { }
        private void lblMasukSekarang_Click(object sender, EventArgs e)
        {
            Form login = new Login(); login.Show(); this.Hide();
        }
    }
}