using Npgsql;
using System;
using System.Windows.Forms;
using FnB_Records.Koneksi_DB;

namespace FnB_Records
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        // 1. SAAT APLIKASI DIBUKA (Cek apakah "Ingat Saya" aktif)
        private void Login_Load(object sender, EventArgs e)
        {
            try
            {
                // Cek apakah user sebelumnya mencentang "Ingat Saya"
                if (Properties.Settings.Default.StatusIngat)
                {
                    // Isi Textbox dari data yang tersimpan
                    txtEmail.Text = Properties.Settings.Default.DisimpanEmail;
                    txtPassword.Text = Properties.Settings.Default.DisimpanPassword;
                    chkIngatSaya.Checked = true;

                    // --- TAMBAHAN PENTING DI SINI ---
                    // Langsung panggil fungsi login agar otomatis masuk ke Main_Form
                    LakukanLogin();
                }
            }
            catch (Exception ex)
            {
                // Abaikan error setting jika baru pertama kali run
                Console.WriteLine("Info Setting: " + ex.Message);
            }
        }

        // 2. TOMBOL MASUK DITEKAN
        private void btnMasuk_Click(object sender, EventArgs e)
        {
            LakukanLogin();
        }

        // 3. FUNGSI UTAMA LOGIN
        private void LakukanLogin()
        {
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text.Trim();

            // Validasi Input
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Email dan Password harus diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // --- BAGIAN INI YANG MEMPERBAIKI ERROR SEBELUMNYA ---

                // 1. Buat Instance class Koneksi (karena Constructor-nya tidak static)
                Koneksi db = new Koneksi();

                // 2. Panggil fungsi 'GetKoneksi' (Pakai 'K') bukan 'GetConnection'
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    // Pastikan koneksi terbuka (berjaga-jaga)
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                    // Query: Ambil ID, Nama Bisnis, Password, dan Role
                    string query = "SELECT id, business_name, password, role FROM users WHERE email = @email LIMIT 1";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbPassword = reader.GetString(2); // Ambil password dari DB (index 2)

                                // Cek Password (Plain Text)
                                if (password == dbPassword)
                                {
                                    // --- LOGIN SUKSES ---

                                    // A. Simpan Data User ke GlobalSession (Memory Sementara)
                                    GlobalSession.CurrentUserId = reader.GetInt32(0);
                                    GlobalSession.BusinessName = reader.GetString(1);
                                    GlobalSession.CurrentUserRole = reader.GetString(3);
                                    GlobalSession.CurrentUserEmail = email;

                                    // B. Simpan ke Properties Settings (Ingat Saya)
                                    AturIngatSaya(email, password);

                                    // C. Pindah ke Menu Utama (Ganti 'Main_Form' dengan nama form menu Anda)
                                    Main_Form menu = new Main_Form();
                                    menu.Show();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Password salah!", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Email tidak terdaftar.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal terhubung ke database:\n" + ex.Message, "Error Koneksi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 4. LOGIKA SIMPAN SETTING (Ingat Saya)
        private void AturIngatSaya(string email, string password)
        {
            if (chkIngatSaya.Checked)
            {
                // Jika dicentang, simpan email & password
                Properties.Settings.Default.DisimpanEmail = email;
                Properties.Settings.Default.DisimpanPassword = password;
                Properties.Settings.Default.StatusIngat = true;
            }
            else
            {
                // Jika tidak dicentang, hapus data tersimpan
                Properties.Settings.Default.DisimpanEmail = "";
                Properties.Settings.Default.DisimpanPassword = "";
                Properties.Settings.Default.StatusIngat = false;
            }
            // Jangan lupa Save!
            Properties.Settings.Default.Save();
        }

        // Event Klik Daftar (Belum ada form register, jadi pesan saja dulu)
        private void lblToDaftar_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Fitur pendaftaran belum dibuat.");
             Register reg = new Register();
             reg.Show();
             this.Hide();
        }

        // 5. CLASS SESSION (Menyimpan data user yang sedang aktif)
        public static class GlobalSession
        {
            public static int CurrentUserId { get; set; }
            public static string BusinessName { get; set; }
            public static string CurrentUserRole { get; set; }
            public static string CurrentUserEmail { get; set; }

            public static void ClearSession()
            {
                CurrentUserId = 0;
                BusinessName = null;
                CurrentUserRole = null;
                CurrentUserEmail = null;

                Properties.Settings.Default.StatusIngat = false;
                Properties.Settings.Default.DisimpanEmail = "";
                Properties.Settings.Default.DisimpanPassword = "";
                Properties.Settings.Default.Save();
            }
        }
    }
}