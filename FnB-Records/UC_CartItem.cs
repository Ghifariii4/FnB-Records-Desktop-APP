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
    public partial class UC_CartItem : UserControl
    {

        public event EventHandler OnDeleteRequest;

        public int RecipeId { get; set; }

        public double HargaSatuan { get; set; }
        public int Quantity { get; set; } = 1;
        public UC_CartItem()
        {
            InitializeComponent();
        }

        public void SetData(int id, string nama, double harga, Image img)
        {
            RecipeId = id; // Simpan ID
            lblCartNama.Text = nama;
            HargaSatuan = harga;
            picThumb.Image = img;
            UpdateTampilan();
        }

        public void TambahQty()
        {
            Quantity++;
            UpdateTampilan();
        }

        public string NamaMenu
        {
            get { return lblCartNama.Text; }
            set { lblCartNama.Text = value; }
        }

        private void UpdateTampilan()
        {
            lblQty.Text = $"{Quantity}x";
            // Total per item (Harga x Qty)
            double total = HargaSatuan * Quantity;
            lblCartHarga.Text = "Rp " + total.ToString("N0");
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            // Panggil event hapus agar Parent yang membuang kontrol ini
            OnDeleteRequest?.Invoke(this, EventArgs.Empty);
        }
    }
}
