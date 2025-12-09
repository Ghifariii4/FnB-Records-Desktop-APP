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
    public partial class UC_CardMenu : UserControl
    {
        public UC_CardMenu()
        {
            InitializeComponent();
        }

        public int IdMenu { get; private set; }

        private int _stokSaatIni;

        public void SetProduk(int id, string nama, string harga, int stok, Image gambar)
        {
            IdMenu = id;
            lblNama.Text = nama;
            lblHarga.Text = harga;
            _stokSaatIni = stok; // Simpan ke variabel

            // Tampilkan Stok
            lblStok.Text = $"Stok: {stok}";

            // Atur Gambar
            if (gambar != null) picProduk.Image = gambar;
            else picProduk.Image = null; // atau gambar placeholder

            // --- LOGIKA STOK HABIS ---
            if (stok <= 0)
            {
                lblStok.Text = "HABIS";
                lblStok.ForeColor = Color.Red;
                // Matikan tombol
                btTambah.Enabled = false;
                btTambah.FillColor = Color.Gray; // Ubah warna jadi abu biar terlihat mati
            }
            else
            {
                // Reset jika stok ada (penting untuk reload)
                btTambah.Enabled = true;
                btTambah.FillColor = Color.FromArgb(212, 122, 71); // Warna asli oranye
                lblStok.ForeColor = Color.Gray;
            }
        }

        private void btTambah_Click(object sender, EventArgs e)
        {
            if (_stokSaatIni > 0)
            {
                string hargaBersih = lblHarga.Text.Replace("Rp", "").Replace(".", "").Replace(" ", "").Trim();

                if (double.TryParse(hargaBersih, out double hargaDouble))
                {
                    // PENTING: Sekarang kita kirim '_stokSaatIni' juga
                    // Format: (ID, Nama, Harga, STOK, Gambar)
                    CartHub.AddToCart(IdMenu, lblNama.Text, hargaDouble, _stokSaatIni, picProduk.Image);
                }
            }
        }
    }
}
