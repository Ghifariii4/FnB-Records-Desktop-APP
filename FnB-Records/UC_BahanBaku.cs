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
    public partial class UC_BahanBaku : UserControl
    {
        public UC_BahanBaku()
        {
            InitializeComponent();
        }

        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {

        }

        private void btnSimpanPopUp_Click(object sender, EventArgs e)
        {

        }

        private void btTambahBahanBaku_Click(object sender, EventArgs e)
        {
            if (gbBahanBakuPopUp.Visible == false)
            {
                gbBahanBakuPopUp.Visible = true;
                guna2Transition1.ShowSync(gbBahanBakuPopUp);
            }

        }

        private void btnClosePopUpBahanBaku_Click(object sender, EventArgs e)
        {
            if (gbBahanBakuPopUp.Visible == true)
            {
                guna2Transition1.HideSync(gbBahanBakuPopUp);
                gbBahanBakuPopUp.Visible = false;
            }
        }
    }
}
