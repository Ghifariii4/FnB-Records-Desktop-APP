using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing; // Wajib ada untuk Image
using System.IO; // Wajib ada untuk MemoryStream
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql; // Wajib ada untuk koneksi PostgreSQL
using FnB_Records.Koneksi_DB; // Pastikan namespace ini sesuai dengan project Anda

namespace FnB_Records
{
    public partial class UC_Kategori : UserControl
    {
        public UC_Kategori()
        {
            InitializeComponent();
        }


        // Fungsi untuk mengubah Judul Kategori (misal label di atas FLP)
        public void SetJudulKategori(string judul)
        {
            lblKategori.Text = judul; // Asumsi kamu punya label judul
        }

        private Image ByteArrayToImage(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                return Image.FromStream(ms);
            }
        }

        // Fungsi Inti: Mengisi flowlayoutpanel di dalam UC ini dengan kartu-kartu
        // --- Di dalam UC_Kategori.cs ---

        public void LoadItems(string kategori)
        {
            // 1. Bersihkan item lama
            flpCardMenu.Controls.Clear();

            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    // 2. Siapkan Query
                    // Kita ambil nama, harga, dan data gambar berdasarkan kategori
                    string query = "SELECT id, name, suggested_price, stock, image_data FROM recipes WHERE category = @cat";
                    // Khusus tombol "Semua" diabaikan disini karena logika "Semua" 
                    // sudah ditangani oleh looping di UC_DaftarMenu. 
                    // Jadi method ini hanya fokus load 1 jenis kategori spesifik.

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@cat", kategori);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            // 3. Looping membaca data baris per baris dari Database
                            while (reader.Read())
                            {

                                int idMenu = Convert.ToInt32(reader["id"]);
                                string namaMenu = reader["name"].ToString();

                                // 2. Ambil Stok dari Database
                                int stokMenu = Convert.ToInt32(reader["stock"]);

                                double hargaAngka = Convert.ToDouble(reader["suggested_price"]);
                                string hargaText = "Rp " + hargaAngka.ToString("N0");

                                Image gambarMenu = null;
                                if (reader["image_data"] != DBNull.Value)
                                {
                                    byte[] imgBytes = (byte[])reader["image_data"];
                                    gambarMenu = ByteArrayToImage(imgBytes);
                                }

                                UC_CardMenu item = new UC_CardMenu();

                                // 3. Masukkan stok ke parameter fungsi yang baru
                                item.SetProduk(idMenu, namaMenu, hargaText, stokMenu, gambarMenu);

                                item.Margin = new Padding(10);
                                flpCardMenu.Controls.Add(item);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat menu: " + ex.Message);
            }
        }


    }
}
