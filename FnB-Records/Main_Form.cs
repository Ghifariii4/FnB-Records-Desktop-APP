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

        private void btnExitApp_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void gbStatusRole_Click(object sender, EventArgs e)
        {

        }

        private void guna2Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void navigationControl(UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            paneluc.Controls.Clear();
            paneluc.Controls.Add(uc);
            uc.BringToFront();

        }

        private void guna2Button22_Click(object sender, EventArgs e)
        {
            UCDashboard dashboard = new UCDashboard();
            navigationControl(dashboard);
        }

        private void Main_Form_Load(object sender, EventArgs e)
        {
            UCDashboard uc = new UCDashboard();
            navigationControl(uc);
            btdashboard.Checked = true;
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
    }
}
