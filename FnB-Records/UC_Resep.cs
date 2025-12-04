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
    public partial class UC_Resep : UserControl
    {
        public UC_Resep()
        {
            InitializeComponent();
        }

        private void btTambahResep_Click(object sender, EventArgs e)
        {
            gbResepPopUp.Visible = true;
        }

        private void btnClosePopUpBahanBaku_Click(object sender, EventArgs e)
        {
            gbResepPopUp.Visible = false;
        }

        private void UC_Resep_Load(object sender, EventArgs e)
        {

        }
    }
}
