using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Npgsql;
using System.IO; // Wajib untuk mengelola Gambar (MemoryStream)
using FnB_Records.Koneksi_DB; // Memanggil class Koneksi Anda

namespace FnB_Records
{
    public partial class UC_TambahMenu : UserControl
    {
        public UC_TambahMenu()
        {
            InitializeComponent();
        }

        private byte[] ImageToByteArray(Image imageIn)
        {
            using (var ms = new MemoryStream())
            {
                // Simpan sebagai PNG agar kualitas terjaga
                imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return ms.ToArray();
            }
        }

        // --- 2. TOMBOL BROWSE GAMBAR ---
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "Pilih Gambar(*.jpg;*.png;*.jpeg)|*.jpg;*.png;*.jpeg";

            if (opf.ShowDialog() == DialogResult.OK)
            {
                // Tampilkan di PictureBox
                picGambar.Image = Image.FromFile(opf.FileName);
                // Beri tanda (Tag) bahwa ada gambar yang dipilih
                picGambar.Tag = "HasImage";
            }
        }

        // --- 3. TOMBOL SIMPAN ---
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            // A. Validasi Input Dasar
            if (string.IsNullOrEmpty(txtNamaMenu.Text) || cmbKategori.SelectedItem == null)
            {
                MessageBox.Show("Nama Menu dan Kategori wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // B. Panggil Class Koneksi Anda
                Koneksi db = new Koneksi();

                // Gunakan 'using' agar koneksi otomatis ditutup setelah selesai
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    // Query Insert sesuai ERD (Tabel recipes)
                    // Catatan: Saya set 'hpp' default 0 karena di ERD wajib isi, tapi belum ada inputnya
                    string query = @"INSERT INTO recipes 
                (user_id, name, description, category, suggested_price, stock, image_data, hpp, serving_size, created_at, updated_at) 
                VALUES 
                (@uid, @name, @desc, @cat, @price, @stock, @img, @hpp, @serving, @created, @updated)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        // -- MAPPING PARAMETER --

                        // 1. User ID (PENTING: Harus ada ID 1 di tabel users, atau ganti dengan ID yang login)
                        cmd.Parameters.AddWithValue("@uid", 1);

                        // 2. Data Text & Angka
                        cmd.Parameters.AddWithValue("@name", txtNamaMenu.Text);
                        cmd.Parameters.AddWithValue("@desc", txtDeskripsi.Text);
                        cmd.Parameters.AddWithValue("@cat", cmbKategori.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@price", (double)numHarga.Value); // Asumsi pakai NumericUpDown
                        cmd.Parameters.AddWithValue("@stock", (int)numStok.Value);     // Asumsi pakai NumericUpDown
                        cmd.Parameters.AddWithValue("@serving", (int)numPorsi.Value);
                        cmd.Parameters.AddWithValue("@hpp", (double)numHPP.Value);

                        // 3. Logika Gambar
                        if (picGambar.Image != null && picGambar.Tag?.ToString() == "HasImage")
                        {
                            cmd.Parameters.AddWithValue("@img", NpgsqlTypes.NpgsqlDbType.Bytea, ImageToByteArray(picGambar.Image));
                        }
                        else
                        {
                            // Jika user tidak upload gambar, masukkan NULL
                            cmd.Parameters.AddWithValue("@img", DBNull.Value);
                        }

                        // 4. Timestamp
                        cmd.Parameters.AddWithValue("@created", DateTime.Now);
                        cmd.Parameters.AddWithValue("@updated", DateTime.Now);

                        // Eksekusi Query
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Menu berhasil disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Reset Form
                        BersihkanForm();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BersihkanForm()
        {
            txtNamaMenu.Clear();
            txtDeskripsi.Clear();
            cmbKategori.SelectedIndex = -1;
            numHarga.Value = 0;
            numStok.Value = 0;
            picGambar.Image = null;
            picGambar.Tag = null;
        }

    }
}
