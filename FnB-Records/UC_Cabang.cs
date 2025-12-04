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
    public partial class UC_Cabang : UserControl
    {
        public UC_Cabang()
        {
            InitializeComponent();
        }

        private void btnTambahVendor_Click(object sender, EventArgs e)
        {
            gbCabangPopUp.Visible = true;
        }

        private void btClosePopUpCabang_Click(object sender, EventArgs e)
        {
            gbCabangPopUp.Visible = false;
        }
    }
}
