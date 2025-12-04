using Npgsql;
using System;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class Login : Form
    {
        // Connection string WAJIB tanpa karakter aneh (tanda '-' di database harus dihilangkan)


        public Login()
        {
            InitializeComponent();
        }

        private void btnMasuk_Click(object sender, EventArgs e)
        {

        }

        public static class GlobalSession
        {

        }

    }
}
