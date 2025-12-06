using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Text.Json;
using System.IO;
using Npgsql; // WAJIB ADA: Untuk PostgreSQL
using FnB_Records.Koneksi_DB; // WAJIB ADA: Sesuaikan dengan namespace folder koneksi Anda

namespace FnB_Records
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
            // Event agar kolom konfirmasi password aktif saat password diisi
            txtInputKataSandi.TextChanged += TxtInputKataSandi_TextChanged;
        }

        // Logika UI: Mengaktifkan kolom konfirmasi hanya jika password diisi
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

        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {
            // Tidak digunakan
        }

        private void lblMasukSekarang_Click(object sender, EventArgs e)
        {
            Form loginForm = new Login();
            loginForm.Show();
            this.Hide();
        }

        // --- TOMBOL DAFTAR DIKLIK ---
        private async void btnMasuk_Click(object sender, EventArgs e)
        {
            // 1. Ambil Data Input
            string namaBisnis = txtNamaBisnis.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtInputKataSandi.Text.Trim();
            string confirmPass = txtKonfirmasiKataSandi.Text.Trim();
            string kodeAktivasi = txtKodeAktivasi.Text.Trim();

            // 2. Validasi Input Kosong
            if (string.IsNullOrEmpty(namaBisnis) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(kodeAktivasi))
            {
                MessageBox.Show("Semua kolom wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Validasi Password Cocok
            if (password != confirmPass)
            {
                MessageBox.Show("Konfirmasi kata sandi tidak cocok!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Matikan tombol saat loading
            btDaftarAkun.Enabled = false;
            btDaftarAkun.Text = "Memproses...";

            try
            {
                // 4. Cek Aktivasi ke Server API
                bool isValid = await CekAktivasi(kodeAktivasi);

                if (isValid)
                {
                    // 5. JIKA KODE VALID -> SIMPAN KE DATABASE LOKAL
                    if (SimpanUserKeDatabase(namaBisnis, email, password))
                    {
                        MessageBox.Show("Aktivasi Berhasil! Akun Premium telah dibuat.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Pindah ke Login
                        Login formLogin = new Login();
                        formLogin.Show();
                        this.Hide();
                    }
                }
                // Jika tidak valid, pesan error sudah muncul di fungsi CekAktivasi
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
            }
            finally
            {
                btDaftarAkun.Enabled = true;
                btDaftarAkun.Text = "Daftar Akun";
            }
        }

        // --- FUNGSI SIMPAN KE POSTGRESQL ---
        private bool SimpanUserKeDatabase(string bisnis, string email, string password)
        {
            try
            {
                Koneksi db = new Koneksi();

                // PENTING: Gunakan GetKoneksi() (Bukan GetConnection)
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    // Pastikan koneksi terbuka
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                    // Cek Email Ganda
                    string checkSql = "SELECT COUNT(*) FROM users WHERE email = @email";
                    using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@email", email);
                        long count = (long)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Email sudah terdaftar! Silakan gunakan email lain.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return false;
                        }
                    }

                    // Insert Data User Baru
                    string sql = @"INSERT INTO users (business_name, email, password, role, created_at) 
                                   VALUES (@bisnis, @email, @pass, 'premium', CURRENT_TIMESTAMP)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@bisnis", bisnis);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@pass", password);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan ke database lokal: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // --- FUNGSI CEK API (Sudah Benar) ---
        public async Task<bool> CekAktivasi(string kode)
        {
            try
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

                    // FIX: Case insensitive JSON parsing
                    var response = JsonSerializer.Deserialize<AktivasiResponse>(
                        result,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }
                    );

                    if (response != null && response.status == "valid")
                    {
                        SimpanLisensi(response.premium_until);
                        return true;
                    }
                    else
                    {
                        MessageBox.Show(
                            $"Aktivasi gagal:\nStatus: {response?.status}\nPesan: {response?.message}",
                            "Info Server",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error koneksi: " + ex.Message);
                return false;
            }
        }


        public void SimpanLisensi(string premiumUntil)
        {
            File.WriteAllText("license.key", $"premium=true\nexpires={premiumUntil}");
        }

        public bool CekLisensi()
        {
            return File.Exists("license.key");
        }

        public class AktivasiResponse
        {
            public string status { get; set; }
            public string premium_until { get; set; }
            public string message { get; set; }
        }
    }
}