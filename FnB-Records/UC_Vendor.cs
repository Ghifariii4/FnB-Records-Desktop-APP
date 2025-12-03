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
    public partial class UC_Vendor : UserControl
    {
        public UC_Vendor()
        {
            InitializeComponent();
        }



        private void guna2GroupBox2_Click(object sender, EventArgs e)
        {

        }

        private void guna2DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnSimpanPopUp_Click(object sender, EventArgs e)
        {

        }

        private void btnTambahVendor_Click(object sender, EventArgs e)
        {
            gbVendorPopUp.Visible = true;
        }

        private void btnClosePopUpVendor_Click(object sender, EventArgs e)
        {
            gbVendorPopUp.Visible = false;
        }
    }
}
