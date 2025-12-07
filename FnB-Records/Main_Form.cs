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

        // --- UPDATE 1: EVENT LOAD (Menampilkan Data User) ---
        private void Main_Form_Load(object sender, EventArgs e)
        {
            // 1. Ambil data dari GlobalSession (Login.cs)
            // Jika datanya null (misal testing tanpa login), kasih default text
            string namaBisnis = Login.GlobalSession.BusinessName ?? "Nama Bisnis";
            string emailUser = Login.GlobalSession.CurrentUserEmail ?? "email@example.com";
            string roleUser = Login.GlobalSession.CurrentUserRole ?? "Free";

            // 2. Tampilkan ke Label
            lblNamaBisnis.Text = namaBisnis;
            lblEmail.Text = emailUser;

            // Opsional: Jika Anda ingin menampilkan Role di suatu tempat (misal di label5 atau label4)
            lblStatusRole.Text = roleUser; // Label di bawah "Premium" (gbStatusRole)

            // 3. Load Dashboard Awal
            UCDashboard uc = new UCDashboard();
            navigationControl(uc);
            btDashboard.Checked = true;
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

        // --- TOMBOL MENU ---
        private void btDashboard_Click(object sender, EventArgs e)
        {
            UCDashboard dashboard = new UCDashboard();
            navigationControl(dashboard);
        }

        private void btVendor_Click(object sender, EventArgs e)
        {
            UC_Vendor vendor = new UC_Vendor();
            navigationControl(vendor);
        }

        private void btBahanBaku_Click(object sender, EventArgs e)
        {
            UC_BahanBaku bahanBaku = new UC_BahanBaku();
            navigationControl(bahanBaku);
        }

        private void btResepMenu_Click(object sender, EventArgs e)
        {
            UC_Resep resep = new UC_Resep();
            navigationControl(resep);
        }

        private void btPO_Click(object sender, EventArgs e)
        {
            UC_PurchaseOrder PO = new UC_PurchaseOrder();
            navigationControl(PO);
        }

        private void btPenjualan_Click(object sender, EventArgs e)
        {
            UC_Penjualan penjualan = new UC_Penjualan();
            navigationControl(penjualan);
        }

        private void btCabang_Click(object sender, EventArgs e)
        {
            UC_Cabang cabang = new UC_Cabang();
            navigationControl(cabang);
        }

        private void btAnalisisBelanja_Click(object sender, EventArgs e)
        {
            UC_AnalisisBelanja analisisBelanja = new UC_AnalisisBelanja();
            navigationControl(analisisBelanja);
        }

        private void btSimulasiKebutuhan_Click(object sender, EventArgs e)
        {
            UC_SimulasiKebutuhan simulasiKebutuhan = new UC_SimulasiKebutuhan();
            navigationControl(simulasiKebutuhan);
        }

        private void btManajemenInventori_Click(object sender, EventArgs e)
        {
            UC_ManajemenInventori manajemenInventori = new UC_ManajemenInventori();
            navigationControl(manajemenInventori);
        }

        private void btProduksiOlahan_Click(object sender, EventArgs e)
        {
            UC_ProduksiOlahan produksiOlahan = new UC_ProduksiOlahan();
            navigationControl(produksiOlahan);
        }

        private void btPengaturan_Click(object sender, EventArgs e)
        {
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

                // Hapus Data "Ingat Saya" (Opsional - Aktifkan jika ingin logout total)
                
                Properties.Settings.Default.StatusIngat = false;
                Properties.Settings.Default.DisimpanEmail = "";
                Properties.Settings.Default.DisimpanPassword = "";
                Properties.Settings.Default.Save();

                // Kembali ke Login
                Login formLogin = new Login();
                formLogin.Show();
                this.Hide(); // atau this.Close(); tapi hati-hati kalau Main_Form adalah form utama startup
            }
        }

        // Event-event kosong (biarkan saja atau hapus jika tidak perlu)
        private void gbStatusRole_Click(object sender, EventArgs e) { }
        private void guna2Panel2_Paint(object sender, PaintEventArgs e) { }
        private void paneluc_Paint(object sender, PaintEventArgs e) { }
    }
}