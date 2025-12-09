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
    public partial class UC_DaftarMenu : UserControl
    {
        public UC_DaftarMenu()
        {
            InitializeComponent();
        }


        // --- Buka file UC_DaftarMenu.cs ---

        public void TampilkanHalamanMenu(string kategoriDipilih)
        {
            // 1. Bersihkan layar terlebih dahulu
            flpDaftarMenu.Controls.Clear();

            // 2. Daftar Kategori yang tersedia di aplikasi Anda
            // Tips: Urutan di array ini menentukan urutan tampilan dari atas ke bawah
            string[] listKategori = {
        "Makanan Utama",
        "Makanan Ringan",
        "Minuman Dingin",
        "Minuman Panas",
        "Makanan Penutup"
    };

            // 3. Cek apakah user meminta "Semua" atau spesifik
            if (kategoriDipilih == "Semua")
            {
                // --- LOGIKA LOOPING SEMUA KATEGORI ---
                foreach (string kategori in listKategori)
                {
                    // Buat wadah kategori baru
                    UC_Kategori wadah = new UC_Kategori();

                    // Set judulnya (Misal: "Makanan Utama")
                    wadah.SetJudulKategori(kategori);

                    // Isi perut wadah tersebut dengan item-item yang sesuai
                    wadah.LoadItems(kategori);

                    // Masukkan ke halaman utama
                    flpDaftarMenu.Controls.Add(wadah);
                }
            }
            else
            {
                // --- LOGIKA SATU KATEGORI SAJA (Seperti sebelumnya) ---
                UC_Kategori wadah = new UC_Kategori();
                wadah.SetJudulKategori(kategoriDipilih);
                wadah.LoadItems(kategoriDipilih);
                flpDaftarMenu.Controls.Add(wadah);
            }
        }

        private void UC_DaftarMenu_Load(object sender, EventArgs e)
        {
        }

        private void guna2vScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {

        }
    }
}
