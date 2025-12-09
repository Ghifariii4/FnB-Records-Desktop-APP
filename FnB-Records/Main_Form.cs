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
    public partial class Main_Form : Form
    {
        public Main_Form()
        {
            InitializeComponent();
        }

        private void Main_Form_Load(object sender, EventArgs e)
        {
            // Panggil fungsi refresh saat form pertama kali dimuat
            RefreshUserAccess();

            // Load Dashboard Awal (hanya jika email verified)
            if (Login.GlobalSession.IsEmailVerified)
            {
                UCDashboard uc = new UCDashboard();
                navigationControl(uc);
                btDashboard.Checked = true;
            }
            else
            {
                TampilkanPesanVerifikasi();
            }
        }

        // ==========================================
        // TAMBAHKAN METHOD PUBLIC INI
        // Agar bisa dipanggil dari UC_Pengaturan
        // ==========================================
        public void RefreshUserAccess()
        {
            // 1. Ambil data TERBARU dari GlobalSession
            string namaBisnis = Login.GlobalSession.BusinessName ?? "Nama Bisnis";
            string emailUser = Login.GlobalSession.CurrentUserEmail ?? "email@example.com";
            string roleUser = Login.GlobalSession.CurrentUserRole ?? "free";
            bool emailVerified = Login.GlobalSession.IsEmailVerified; // Ini harus sudah true

            // 2. Tampilkan ke Label
            lblNamaBisnis.Text = namaBisnis;
            lblEmail.Text = emailUser;
            lblStatusRole.Text = roleUser == "premium" ? "Premium ✨" : "Free";

            // 3. Atur Warna Status Role
            if (roleUser == "premium")
            {
                gbStatusRole.FillColor = Color.Gold;
                lblStatusRole.ForeColor = Color.White;
            }
            else
            {
                gbStatusRole.FillColor = Color.FromArgb(34, 139, 34);
                lblStatusRole.ForeColor = Color.White;
            }

            // 4. Buka/Kunci Menu berdasarkan status terbaru
            AturAksesMenu(emailVerified, roleUser);
        }

        // --- FUNGSI UNTUK MENGATUR AKSES MENU (SAMA SEPERTI SEBELUMNYA) ---
        private void AturAksesMenu(bool emailVerified, string role)
        {
            if (!emailVerified)
            {
                KunciSemuaMenu();
                btPengaturan.Enabled = true; // Tetap buka pengaturan
                return;
            }

            // Jika sudah terverifikasi, buka menu
            if (role == "premium")
            {
                BukaSemuaMenu();
            }
            else
            {
                BukaSemuaMenu(); // Buka dasar

                // Kunci menu premium
                btCabang.Enabled = false;
                btAnalisisBelanja.Enabled = false;
                btSimulasiKebutuhan.Enabled = false;
                btManajemenInventori.Enabled = false;
                btProduksiOlahan.Enabled = false;

                btCabang.Text = "🔒 Cabang (Premium)";
                btAnalisisBelanja.Text = "🔒 Analisis Belanja (Premium)";
                btSimulasiKebutuhan.Text = "🔒 Simulasi Kebutuhan (Premium)";
                btManajemenInventori.Text = "🔒 Manajemen Inventori (Premium)";
                btProduksiOlahan.Text = "🔒 Produksi Olahan (Premium)";
            }
        }
        // --- FUNGSI KUNCI SEMUA MENU ---
        private void KunciSemuaMenu()
        {
            btDashboard.Enabled = false;
            btVendor.Enabled = false;
            btBahanBaku.Enabled = false;
            btResepMenu.Enabled = false;
            btPO.Enabled = false;
            btPenjualan.Enabled = false;
            btCabang.Enabled = false;
            btAnalisisBelanja.Enabled = false;
            btSimulasiKebutuhan.Enabled = false;
            btManajemenInventori.Enabled = false;
            btProduksiOlahan.Enabled = false;
            btPengaturan.Enabled = false; // Akan di-enable khusus untuk verifikasi
        }

        // --- FUNGSI BUKA SEMUA MENU ---
        private void BukaSemuaMenu()
        {
            btDashboard.Enabled = true;
            btVendor.Enabled = true;
            btBahanBaku.Enabled = true;
            btResepMenu.Enabled = true;
            btPO.Enabled = true;
            btPenjualan.Enabled = true;
            btCabang.Enabled = true;
            btAnalisisBelanja.Enabled = true;
            btSimulasiKebutuhan.Enabled = true;
            btManajemenInventori.Enabled = true;
            btProduksiOlahan.Enabled = true;
            btPengaturan.Enabled = true;

            // Reset text (hilangkan gembok)
            btCabang.Text = "Cabang";
            btAnalisisBelanja.Text = "Analisis Belanja";
            btSimulasiKebutuhan.Text = "Simulasi Kebutuhan";
            btManajemenInventori.Text = "Manajemen Inventori";
            btProduksiOlahan.Text = "Produksi Olahan";
        }

        // --- TAMPILKAN PESAN VERIFIKASI ---
        private void TampilkanPesanVerifikasi()
        {
            Label lblPesan = new Label();
            lblPesan.Text = "⚠️ EMAIL BELUM TERVERIFIKASI\n\n" +
                           "Silakan buka menu PENGATURAN\n" +
                           "untuk verifikasi email Anda.";
            lblPesan.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblPesan.ForeColor = Color.Orange;
            lblPesan.TextAlign = ContentAlignment.MiddleCenter;
            lblPesan.Dock = DockStyle.Fill;

            paneluc.Controls.Clear();
            paneluc.Controls.Add(lblPesan);
        }

        private void btnExitApp_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void navigationControl(UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            paneluc.Controls.Clear();
            paneluc.Controls.Add(uc);
            uc.BringToFront();
        }

        // --- VALIDASI SEBELUM BUKA MENU ---
        private bool ValidasiAksesMenu(string namaMenu)
        {
            // Cek verifikasi email
            if (!Login.GlobalSession.IsEmailVerified)
            {
                MessageBox.Show(
                    "⚠️ Email belum terverifikasi!\n\n" +
                    "Silakan verifikasi email di menu PENGATURAN terlebih dahulu.",
                    "Akses Ditolak",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }

            // Cek role untuk menu premium
            string[] menuPremium = { "Cabang", "Analisis Belanja", "Simulasi Kebutuhan",
                                    "Manajemen Inventori", "Produksi Olahan" };

            if (menuPremium.Contains(namaMenu) && Login.GlobalSession.CurrentUserRole != "premium")
            {
                MessageBox.Show(
                    $"🔒 Fitur '{namaMenu}' hanya tersedia untuk akun PREMIUM!\n\n" +
                    "Upgrade ke Premium untuk akses:\n" +
                    "✨ Unlimited Products\n" +
                    "✨ Advanced Analytics\n" +
                    "✨ Multi-Branch Management\n" +
                    "✨ Priority Support\n\n" +
                    "Hubungi admin untuk upgrade!",
                    "Fitur Premium",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return false;
            }

            return true;
        }

        // --- TOMBOL MENU ---
        private void btDashboard_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Dashboard")) return;
            UCDashboard dashboard = new UCDashboard();
            navigationControl(dashboard);
        }

        private void btVendor_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Vendor")) return;
            UC_Vendor vendor = new UC_Vendor();
            navigationControl(vendor);
        }

        private void btBahanBaku_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Bahan Baku")) return;
            UC_BahanBaku bahanBaku = new UC_BahanBaku();
            navigationControl(bahanBaku);
        }

        private void btResepMenu_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Resep Menu")) return;
            UC_Resep resep = new UC_Resep();
            navigationControl(resep);
        }

        private void btPO_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Purchase Order")) return;
            UC_PurchaseOrder PO = new UC_PurchaseOrder();
            navigationControl(PO);
        }

        private void btPenjualan_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Penjualan")) return;
            UC_Penjualan penjualan = new UC_Penjualan();
            navigationControl(penjualan);
        }

        private void btCabang_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Cabang")) return;
            UC_Cabang cabang = new UC_Cabang();
            navigationControl(cabang);
        }

        private void btAnalisisBelanja_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Analisis Belanja")) return;
            UC_AnalisisBelanja analisisBelanja = new UC_AnalisisBelanja();
            navigationControl(analisisBelanja);
        }

        private void btSimulasiKebutuhan_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Simulasi Kebutuhan")) return;
            UC_SimulasiKebutuhan simulasiKebutuhan = new UC_SimulasiKebutuhan();
            navigationControl(simulasiKebutuhan);
        }

        private void btManajemenInventori_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Manajemen Inventori")) return;
            UC_ManajemenInventori manajemenInventori = new UC_ManajemenInventori();
            navigationControl(manajemenInventori);
        }

        private void btProduksiOlahan_Click(object sender, EventArgs e)
        {
            if (!ValidasiAksesMenu("Produksi Olahan")) return;
            var produksiOlahan = new UCProduksiOlahan();
            navigationControl(produksiOlahan);
        }

        private void btPengaturan_Click(object sender, EventArgs e)
        {
            // Pengaturan selalu bisa diakses (untuk verifikasi email)
            UC_Pengaturan pengaturan = new UC_Pengaturan();
            navigationControl(pengaturan);
        }

        // --- TOMBOL LOGOUT ---
        private void btKeluar_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Apakah Anda yakin ingin keluar dari aplikasi?",
                                                  "Konfirmasi Logout",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                // Hapus Session
                Login.GlobalSession.ClearSession();

                // Hapus Data "Ingat Saya"
                Properties.Settings.Default.StatusIngat = false;
                Properties.Settings.Default.DisimpanEmail = "";
                Properties.Settings.Default.DisimpanPassword = "";
                Properties.Settings.Default.Save();

                // Kembali ke Login
                Login formLogin = new Login();
                formLogin.Show();
                this.Hide();
            }
        }

        // --- EVENT KLIK STATUS ROLE ---
        private void gbStatusRole_Click(object sender, EventArgs e)
        {
            TampilkanInfoRole();
        }

        private void lblStatusRole_Click(object sender, EventArgs e)
        {
            TampilkanInfoRole();
        }

        private void TampilkanInfoRole()
        {
            string role = Login.GlobalSession.CurrentUserRole ?? "free";
            bool verified = Login.GlobalSession.IsEmailVerified;

            string message = verified ? "" : "⚠️ EMAIL BELUM TERVERIFIKASI!\n" +
                                            "Verifikasi email untuk akses penuh.\n\n";

            if (role == "premium")
            {
                message += "✨ Status: PREMIUM\n\n" +
                          "Fitur Aktif:\n" +
                          "✅ Unlimited Products\n" +
                          "✅ Multi-Branch Management\n" +
                          "✅ Advanced Analytics\n" +
                          "✅ Inventory Management\n" +
                          "✅ Production Management\n" +
                          "✅ Priority Support\n\n" +
                          "Terima kasih telah menjadi member Premium!";
            }
            else
            {
                message += "📦 Status: FREE\n\n" +
                          "Fitur Tersedia:\n" +
                          "✅ Basic Product Management\n" +
                          "✅ Vendor Management\n" +
                          "✅ Recipe Management\n" +
                          "✅ Purchase Orders\n" +
                          "✅ Sales Tracking\n\n" +
                          "🔒 Fitur Terkunci (Premium):\n" +
                          "• Multi-Branch Management\n" +
                          "• Advanced Analytics\n" +
                          "• Inventory Management\n" +
                          "• Production Management\n\n" +
                          "💡 Upgrade ke Premium untuk akses penuh!\n" +
                          "Hubungi admin untuk informasi lebih lanjut.";
            }

            MessageBox.Show(message, "Status Akun", MessageBoxButtons.OK,
                          role == "premium" ? MessageBoxIcon.Information : MessageBoxIcon.Question);
        }

        private void guna2PictureBox2_Click(object sender, EventArgs e)
        {
            // Gambar crown - tampilkan info premium
            if (Login.GlobalSession.CurrentUserRole == "free")
            {
                MessageBox.Show(
                    "👑 UPGRADE KE PREMIUM!\n\n" +
                    "Dapatkan akses ke semua fitur advanced:\n" +
                    "✨ Multi-Branch Management\n" +
                    "✨ Advanced Analytics\n" +
                    "✨ Inventory Management\n" +
                    "✨ Production Management\n" +
                    "✨ Priority Support\n\n" +
                    "Hubungi admin untuk upgrade!",
                    "Premium Features",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        // Event lainnya
        private void guna2Panel2_Paint(object sender, PaintEventArgs e) { }
        private void paneluc_Paint(object sender, PaintEventArgs e) { }
    }
}