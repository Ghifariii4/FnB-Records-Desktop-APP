using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using FnB_Records.Koneksi_DB;
using static FnB_Records.Login;

namespace FnB_Records
{
    public partial class UC_Pengaturan : UserControl
    {
        // ============================================
        // KONFIGURASI EMAIL (untuk verifikasi)
        // ============================================
        private string smtpHost = "smtp.gmail.com";
        private int smtpPort = 587;
        private string smtpEmail = "fnbrecords72@gmail.com";
        private string smtpPassword = "scht sxgg giex fyzv";

        private int currentUserId;
        private string currentUserEmail;
        private string originalUserEmail;
        private string originalBusinessName;
        private string verificationCode;
        private bool isEmailVerified;

        public UC_Pengaturan()
        {
            InitializeComponent();
            LoadUserData();
        }

        // ============================================
        // LOAD DATA USER DARI DATABASE
        // ============================================
        private async void LoadUserData()
        {
            try
            {
                currentUserId = GlobalSession.CurrentUserId;

                if (currentUserId <= 0)
                {
                    MessageBox.Show("Session tidak valid. Silakan login kembali.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    string query = @"
                        SELECT business_name, email, phone, role, created_at 
                        FROM users 
                        WHERE id = @userId";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", currentUserId);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                originalBusinessName = reader["business_name"].ToString();
                                originalUserEmail = reader["email"].ToString();

                                txtnamabisnis.Text = originalBusinessName;
                                txtemail.Text = originalUserEmail;
                                txtnotelp.Text = reader["phone"] != DBNull.Value ? reader["phone"].ToString() : "";
                                currentUserEmail = originalUserEmail;

                                // Tampilkan email di label
                                lblemail.Text = currentUserEmail;

                                string role = reader["role"].ToString();
                                UpdateRoleUI(role);

                                DateTime createdAt = Convert.ToDateTime(reader["created_at"]);
                                lbltanggalbergabung.Text = $"Bergabung sejak {createdAt.ToString("dd MMMM yyyy")}";

                                CheckEmailVerificationStatus();
                            }
                            else
                            {
                                MessageBox.Show("Data user tidak ditemukan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // UPDATE UI ROLE (FREE/PREMIUM)
        // ============================================
        private void UpdateRoleUI(string role)
        {
            if (role == "premium")
            {
                // Status Premium
                lblrole.Text = "Premium ✨";
                lblrole.ForeColor = Color.White;

                // Guna2Panel/GroupBox FillColor kuning
                gbStatusRole.FillColor = Color.Gold;
                // Atau jika pakai warna custom
                // gbStatusRole.FillColor = Color.FromArgb(255, 215, 0); // Gold
            }
            else
            {
                // Status Free
                lblrole.Text = "Free";
                lblrole.ForeColor = Color.White;

                // Guna2Panel/GroupBox FillColor hijau
                gbStatusRole.FillColor = Color.LimeGreen;
                // Atau jika pakai warna custom
                // gbStatusRole.FillColor = Color.FromArgb(50, 205, 50); // LimeGreen
            }
        }
        private void UpdateStatusPicture(bool emailVerified)
        {
            try
            {
                if (emailVerified)
                {
                    // Coba load gambar dari Resources
                    try
                    {
                        // GANTI NAMA INI SESUAI NAMA RESOURCE ANDA
                        // Contoh: jika nama resource adalah "_29" atau "verified29"
                        guna2PictureBox1.Image = Properties.Resources._29;
                        guna2PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    catch
                    {
                        // Jika resource tidak ada, gunakan background color hijau
                        guna2PictureBox1.Image = null;
                        guna2PictureBox1.BackColor = Color.LimeGreen;
                    }
                }
                else
                {
                    // Email belum terverifikasi
                    try
                    {
                        // GANTI NAMA INI SESUAI NAMA RESOURCE ANDA
                        // Contoh: jika nama resource adalah "ErrorIconRed" tanpa spasi dan tanda -
                        guna2PictureBox1.Image = Properties.Resources.unferivied;
                        guna2PictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
                    }
                    catch
                    {
                        // Jika resource tidak ada, gunakan background color orange
                        guna2PictureBox1.Image = null;
                        guna2PictureBox1.BackColor = Color.Orange;
                    }
                }
            }
            catch (Exception ex)
            {
                // Fallback ke warna jika ada error
                guna2PictureBox1.Image = null;
                guna2PictureBox1.BackColor = emailVerified ? Color.LimeGreen : Color.Orange;
            }
        }

        // ============================================
        // CEK STATUS VERIFIKASI EMAIL
        // ============================================
        private async void CheckEmailVerificationStatus()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    string query = "SELECT email_verified FROM users WHERE id = @userId";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", currentUserId);

                        var result = await cmd.ExecuteScalarAsync();
                        isEmailVerified = result != null && Convert.ToBoolean(result);

                        UpdateVerificationUI();
                    }
                }
            }
            catch
            {
                isEmailVerified = false;
                UpdateVerificationUI();
            }
        }

        private void UpdateVerificationUI()
        {
            if (isEmailVerified)
            {
                lblstatusverifikasiemail.Text = "✅ Email Terverifikasi";
                lblstatusverifikasiemail.ForeColor = Color.Green;
                btnverifemail.Enabled = false;
                btnverifemail.Text = "Terverifikasi";
            }
            else
            {
                lblstatusverifikasiemail.Text = "⚠️ Email Belum Terverifikasi";
                lblstatusverifikasiemail.ForeColor = Color.Orange;
                btnverifemail.Enabled = true;
                btnverifemail.Text = "Verifikasi Email";
            }

            // Update gambar berdasarkan status verifikasi
            UpdateStatusPicture(isEmailVerified);
        }

        // ============================================
        // KIRIM KODE VERIFIKASI EMAIL
        // ============================================
        private async void btnverifemail_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(currentUserEmail))
            {
                MessageBox.Show("Email tidak valid.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Random random = new Random();
            verificationCode = random.Next(100000, 999999).ToString();

            btnverifemail.Enabled = false;
            btnverifemail.Text = "Mengirim...";

            try
            {
                await SendVerificationEmail(currentUserEmail, verificationCode);

                MessageBox.Show(
                    $"Kode verifikasi telah dikirim ke {currentUserEmail}\n\nSilakan cek email Anda dan masukkan kode 6 digit.",
                    "Email Terkirim",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                string inputCode = Microsoft.VisualBasic.Interaction.InputBox(
                    "Masukkan kode verifikasi 6 digit yang dikirim ke email Anda:",
                    "Verifikasi Email",
                    ""
                );

                if (inputCode == verificationCode)
                {
                    await MarkEmailAsVerified();
                    MessageBox.Show("Email berhasil diverifikasi! ✅", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    isEmailVerified = true;
                    UpdateVerificationUI();
                }
                else if (!string.IsNullOrWhiteSpace(inputCode))
                {
                    MessageBox.Show("Kode verifikasi salah!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal mengirim email: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnverifemail.Enabled = !isEmailVerified;
                btnverifemail.Text = isEmailVerified ? "Terverifikasi" : "Verifikasi Email";
            }
        }

        private async Task SendVerificationEmail(string toEmail, string code)
        {
            using (var mail = new MailMessage())
            {
                mail.From = new MailAddress(smtpEmail, "FnB Records");
                mail.To.Add(toEmail);
                mail.Subject = "Verifikasi Email - FnB Records";
                mail.Body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2>Verifikasi Email Anda</h2>
                        <p>Terima kasih telah menggunakan FnB Records!</p>
                        <p>Kode verifikasi Anda adalah:</p>
                        <h1 style='color: #007bff; letter-spacing: 5px;'>{code}</h1>
                        <p>Kode ini berlaku selama 10 menit.</p>
                        <br>
                        <p>Jika Anda tidak merasa melakukan permintaan ini, abaikan email ini.</p>
                        <p>Salam,<br>Tim FnB Records</p>
                    </body>
                    </html>
                ";
                mail.IsBodyHtml = true;

                using (var smtp = new SmtpClient(smtpHost, smtpPort))
                {
                    smtp.Credentials = new NetworkCredential(smtpEmail, smtpPassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mail);
                }
            }
        }

        private async Task MarkEmailAsVerified()
        {
            Koneksi db = new Koneksi();
            using (var conn = db.GetKoneksi())
            {
                if (conn.State != System.Data.ConnectionState.Open)
                    conn.Open();

                string query = "UPDATE users SET email_verified = true WHERE id = @userId";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("userId", currentUserId);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        // ============================================
        // UPDATE PROFIL - SIMPAN PERUBAHAN
        // ============================================
        private async void btnsimpanprofil_Click(object sender, EventArgs e)
        {
            string namaBisnis = txtnamabisnis.Text.Trim();
            string email = txtemail.Text.Trim();
            string noTelp = txtnotelp.Text.Trim();

            // Validasi input
            if (string.IsNullOrWhiteSpace(namaBisnis))
            {
                MessageBox.Show("Nama bisnis tidak boleh kosong!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtnamabisnis.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                MessageBox.Show("Email tidak valid!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtemail.Focus();
                return;
            }

            // Cek apakah ada perubahan
            bool emailBerubah = email != originalUserEmail;
            bool namaBerubah = namaBisnis != originalBusinessName;
            bool noTelpBerubah = txtnotelp.Text != (string.IsNullOrEmpty(txtnotelp.Text) ? "" : txtnotelp.Text);

            // Jika tidak ada perubahan sama sekali
            if (!emailBerubah && !namaBerubah && !noTelpBerubah)
            {
                MessageBox.Show("Tidak ada perubahan yang dilakukan.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // PENTING: Jika email berubah, beri peringatan
            if (emailBerubah)
            {
                var result = MessageBox.Show(
                    "⚠️ PERHATIAN!\n\n" +
                    "Mengubah email akan:\n" +
                    "• Membuat status verifikasi menjadi BELUM TERVERIFIKASI\n" +
                    "• Email LAMA tidak dapat digunakan untuk login lagi\n" +
                    "• Anda harus verifikasi email BARU setelah menyimpan\n\n" +
                    $"Email lama: {originalUserEmail}\n" +
                    $"Email baru: {email}\n\n" +
                    "Apakah Anda yakin ingin melanjutkan?",
                    "Konfirmasi Perubahan Email",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning
                );

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    // Cek apakah email baru sudah digunakan user lain
                    if (emailBerubah)
                    {
                        string checkQuery = "SELECT COUNT(*) FROM users WHERE email = @email AND id != @userId";
                        using (var checkCmd = new NpgsqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("email", email);
                            checkCmd.Parameters.AddWithValue("userId", currentUserId);

                            long count = (long)await checkCmd.ExecuteScalarAsync();
                            if (count > 0)
                            {
                                MessageBox.Show(
                                    "Email sudah digunakan oleh user lain!\n\nSilakan gunakan email yang berbeda.",
                                    "Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error
                                );
                                return;
                            }
                        }
                    }

                    // Update profil ke database
                    string updateQuery = @"
                        UPDATE users 
                        SET business_name = @businessName,
                            email = @email,
                            phone = @phone" +
                            (emailBerubah ? ", email_verified = false" : "") + @"
                        WHERE id = @userId";

                    using (var cmd = new NpgsqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("businessName", namaBisnis);
                        cmd.Parameters.AddWithValue("email", email);
                        cmd.Parameters.AddWithValue("phone", string.IsNullOrWhiteSpace(noTelp) ? (object)DBNull.Value : noTelp);
                        cmd.Parameters.AddWithValue("userId", currentUserId);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    // Update data lokal dan GlobalSession
                    if (emailBerubah)
                    {
                        // Update semua referensi email
                        originalUserEmail = email;
                        currentUserEmail = email;
                        GlobalSession.CurrentUserEmail = email;

                        // Update label email
                        lblemail.Text = email;

                        // Set status verifikasi menjadi false
                        isEmailVerified = false;
                        UpdateVerificationUI();
                    }

                    if (namaBerubah)
                    {
                        originalBusinessName = namaBisnis;
                        GlobalSession.BusinessName = namaBisnis;
                    }

                    // Tampilkan pesan sukses
                    string message = "✅ Profil berhasil diperbarui!";

                    if (emailBerubah)
                    {
                        message += "\n\n⚠️ PENTING:\n" +
                                  "• Email Anda telah diubah\n" +
                                  "• Status verifikasi: BELUM TERVERIFIKASI\n" +
                                  "• Gunakan email BARU untuk login berikutnya\n" +
                                  "• Segera klik tombol 'Verifikasi Email' untuk verifikasi!";
                    }

                    MessageBox.Show(message, "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Jika email berubah, highlight tombol verifikasi
                    if (emailBerubah)
                    {
                        btnverifemail.BackColor = Color.Orange;
                        System.Threading.Tasks.Task.Delay(500).ContinueWith(_ =>
                        {
                            if (btnverifemail.InvokeRequired)
                            {
                                btnverifemail.Invoke(new Action(() => {
                                    btnverifemail.BackColor = Color.FromArgb(94, 148, 255);
                                }));
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat menyimpan perubahan:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // UBAH PASSWORD
        // ============================================
        private async void btnubahpassword_Click(object sender, EventArgs e)
        {
            string currentPassword = txtpasssaatini.Text;
            string newPassword = txtpassbaru.Text;
            string confirmPassword = txtulangpwbaru.Text;

            if (string.IsNullOrWhiteSpace(currentPassword))
            {
                MessageBox.Show("Password saat ini harus diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtpasssaatini.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Password baru harus diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtpassbaru.Focus();
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("Password baru minimal 6 karakter!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtpassbaru.Focus();
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("Password baru tidak cocok dengan konfirmasi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtulangpwbaru.Focus();
                return;
            }

            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    string checkQuery = "SELECT password FROM users WHERE id = @userId";
                    string storedPassword;

                    using (var cmd = new NpgsqlCommand(checkQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", currentUserId);
                        storedPassword = (await cmd.ExecuteScalarAsync())?.ToString();
                    }

                    if (storedPassword != currentPassword)
                    {
                        MessageBox.Show("Password saat ini salah!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        txtpasssaatini.Focus();
                        return;
                    }

                    string updateQuery = "UPDATE users SET password = @newPassword WHERE id = @userId";

                    using (var cmd = new NpgsqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("newPassword", newPassword);
                        cmd.Parameters.AddWithValue("userId", currentUserId);

                        await cmd.ExecuteNonQueryAsync();
                    }

                    MessageBox.Show(
                        "✅ Password berhasil diubah!\n\n" +
                        "Password baru akan digunakan untuk login berikutnya.",
                        "Sukses",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    txtpasssaatini.Clear();
                    txtpassbaru.Clear();
                    txtulangpwbaru.Clear();

                    if (Properties.Settings.Default.StatusIngat)
                    {
                        Properties.Settings.Default.DisimpanPassword = newPassword;
                        Properties.Settings.Default.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error mengubah password:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // EVENT HANDLERS
        // ============================================
        private void lblstatusverifikasiemail_Click(object sender, EventArgs e)
        {
            if (!isEmailVerified)
            {
                MessageBox.Show(
                    "⚠️ Email Anda belum diverifikasi!\n\n" +
                    "Verifikasi email diperlukan untuk:\n" +
                    "• Keamanan akun Anda\n" +
                    "• Reset password\n" +
                    "• Notifikasi penting\n\n" +
                    "Klik tombol 'Verifikasi Email' untuk memverifikasi.",
                    "Status Verifikasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
            else
            {
                MessageBox.Show(
                    "✅ Email Anda sudah terverifikasi!\n\n" +
                    "Akun Anda aman dan dapat menerima notifikasi.",
                    "Status Verifikasi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        private void lblemail_Click(object sender, EventArgs e)
        {
            // Saat label email diklik, tampilkan info lengkap
            MessageBox.Show(
                $"📧 Email Login Saat Ini:\n{currentUserEmail}\n\n" +
                $"Status Verifikasi: {(isEmailVerified ? "✅ Terverifikasi" : "⚠️ Belum Terverifikasi")}\n\n" +
                "Email ini digunakan untuk login ke akun Anda.\n" +
                (isEmailVerified ? "Akun Anda aman dan terverifikasi." : "Segera verifikasi email Anda untuk keamanan akun!"),
                "Informasi Email",
                MessageBoxButtons.OK,
                isEmailVerified ? MessageBoxIcon.Information : MessageBoxIcon.Warning
            );
        }

        private void txtInputNamaBahan_TextChanged(object sender, EventArgs e)
        {
            // Tidak perlu implementasi khusus
            // Perubahan akan terdeteksi saat klik Simpan Perubahan
        }

        private void txtemail_TextChanged(object sender, EventArgs e)
        {
            string newEmail = txtemail.Text.Trim();

            if (!string.IsNullOrEmpty(newEmail) && newEmail != originalUserEmail)
            {
                lblstatusverifikasiemail.Text = "⚠️ Email berubah - Perlu verifikasi ulang setelah disimpan";
                lblstatusverifikasiemail.ForeColor = Color.Red;
            }
            else if (newEmail == originalUserEmail)
            {
                UpdateVerificationUI();
            }
        }

        private void txtnotelp_TextChanged(object sender, EventArgs e)
        {
            string phone = txtnotelp.Text.Trim();

            if (!string.IsNullOrEmpty(phone))
            {
                string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());

                if (phone.Length > 0 && digitsOnly.Length < 10)
                {
                    txtnotelp.ForeColor = Color.Red;
                }
                else
                {
                    txtnotelp.ForeColor = Color.Black;
                }
            }
        }

        private void txtpasssaatini_TextChanged(object sender, EventArgs e)
        {
            if (txtpasssaatini.Text.Length > 0)
            {
                txtpasssaatini.UseSystemPasswordChar = true;
            }
        }

        private void txtpassbaru_TextChanged(object sender, EventArgs e)
        {
            txtpassbaru.UseSystemPasswordChar = true;

            if (txtpassbaru.Text.Length > 0 && txtpassbaru.Text.Length < 6)
            {
                txtpassbaru.ForeColor = Color.Red;
            }
            else if (txtpassbaru.Text.Length >= 6)
            {
                txtpassbaru.ForeColor = Color.Green;
            }
            else
            {
                txtpassbaru.ForeColor = Color.Black;
            }
        }

        private void txtulangpwbaru_TextChanged(object sender, EventArgs e)
        {
            txtulangpwbaru.UseSystemPasswordChar = true;

            if (txtulangpwbaru.Text.Length > 0)
            {
                if (txtulangpwbaru.Text == txtpassbaru.Text)
                {
                    txtulangpwbaru.ForeColor = Color.Green;
                }
                else
                {
                    txtulangpwbaru.ForeColor = Color.Red;
                }
            }
            else
            {
                txtulangpwbaru.ForeColor = Color.Black;
            }
        }

        private void lbltanggalbergabung_Click(object sender, EventArgs e)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    string query = "SELECT created_at FROM users WHERE id = @userId";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", currentUserId);

                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            DateTime createdAt = Convert.ToDateTime(result);
                            TimeSpan duration = DateTime.Now - createdAt;

                            string message = $"Anda bergabung sejak:\n{createdAt.ToString("dd MMMM yyyy")}\n\n";
                            message += $"Sudah {duration.Days} hari bersama FnB Records! 🎉";

                            MessageBox.Show(message, "Informasi Akun", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            // Tampilkan informasi status verifikasi email
            string statusMessage = isEmailVerified
                ? "✅ Email Terverifikasi\n\n" +
                  $"Email: {currentUserEmail}\n\n" +
                  "Akun Anda sudah aman dan terverifikasi!"
                : "⚠️ Email Belum Terverifikasi\n\n" +
                  $"Email: {currentUserEmail}\n\n" +
                  "Segera verifikasi email Anda untuk keamanan akun!\n" +
                  "Klik tombol 'Verifikasi Email' untuk memulai.";

            MessageBox.Show(
                statusMessage,
                "Status Verifikasi Email",
                MessageBoxButtons.OK,
                isEmailVerified ? MessageBoxIcon.Information : MessageBoxIcon.Warning
            );
        }

        private void gbStatusRole_Click(object sender, EventArgs e)
        {
            try
            {
                // Ambil role dari database
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != System.Data.ConnectionState.Open)
                        conn.Open();

                    string query = "SELECT role FROM users WHERE id = @userId";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("userId", currentUserId);

                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            string role = result.ToString();

                            // Tampilkan info detail
                            string statusInfo = role == "premium"
                                ? "✨ Status Premium Aktif\n\n" +
                                  "Fitur yang Anda miliki:\n" +
                                  "✅ Unlimited Products\n" +
                                  "✅ Advanced Analytics\n" +
                                  "✅ Multi-User Access\n" +
                                  "✅ Priority Support\n" +
                                  "✅ Export Data\n\n" +
                                  "Terima kasih telah menjadi member Premium!"
                                : "📦 Status Free\n\n" +
                                  "Fitur yang Anda miliki:\n" +
                                  "✅ Basic Product Management\n" +
                                  "✅ Standard Reports\n" +
                                  "⚠️ Limited to 50 Products\n\n" +
                                  "Upgrade ke Premium untuk:\n" +
                                  "🌟 Unlimited Products\n" +
                                  "🌟 Advanced Features\n" +
                                  "🌟 Priority Support\n\n" +
                                  "Hubungi admin untuk upgrade!";

                            MessageBox.Show(
                                statusInfo,
                                role == "premium" ? "Status Premium" : "Status Free",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler tambahan jika ada di designer
        private void btnsimpanprofil_Click_1(object sender, EventArgs e)
        {
            // Redirect ke method utama
            btnsimpanprofil_Click(sender, e);
        }

        private void btnkirimkode_Click(object sender, EventArgs e)
        {
            // Method ini tidak digunakan karena tidak ada tombol kirim kode
            // Semua proses dilakukan via tombol Simpan Perubahan
        }
    }
}