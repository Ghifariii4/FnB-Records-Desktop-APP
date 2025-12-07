using FnB_Records.Koneksi_DB;
using Npgsql;
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
        private int currentUserId = 1;
        private int idVendorTerpilih = 0;
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
            if (string.IsNullOrWhiteSpace(txtInputNamaVendor.Text))
            {
                MessageBox.Show("Nama Vendor wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    string query = "INSERT INTO vendors (user_id, name, contact, address, created_at) VALUES (@uid, @name, @contact, @addr, @created_at)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@name", txtInputNamaVendor.Text.Trim());
                        cmd.Parameters.AddWithValue("@contact", string.IsNullOrEmpty(txtInputKontak.Text) ? (object)DBNull.Value : txtInputKontak.Text.Trim());
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(txtInputAlamat.Text) ? (object)DBNull.Value : txtInputAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@created_at", DateTime.Now);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Vendor berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadDataVendor("");
                        BersihkanInput();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTambahVendor_Click(object sender, EventArgs e)
        {
            gbVendorPopUp.Visible = true;
        }

        private void btnClosePopUpVendor_Click(object sender, EventArgs e)
        {
            gbVendorPopUp.Visible = false;
        }

        private void UC_Vendor_Load(object sender, EventArgs e)
        {
            LoadDataVendor("");
        }

        private void dgvDataVendor_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // --- LOGIKA TOMBOL HAPUS (Yang tadi) ---
            if (dgvDataVendor.Columns[e.ColumnIndex].Name == "Hapus")
            {
                // ... (Kode hapus yang tadi, biarkan saja) ...
            }

            // --- LOGIKA TOMBOL EDIT (BARU) ---
            else if (dgvDataVendor.Columns[e.ColumnIndex].Name == "Edit")
            {
                // 1. Ambil data dari baris yang diklik
                DataGridViewRow row = dgvDataVendor.Rows[e.RowIndex];

                // 2. Simpan ID ke variabel global
                idVendorTerpilih = Convert.ToInt32(row.Cells["id"].Value);

                // 3. Auto-Fill: Masukkan data grid ke Textbox Edit
                // Pastikan nama textbox di form Edit Anda sesuai kode ini
                txtEditNamaVendor.Text = row.Cells["name"].Value.ToString();
                txtEditKontak.Text = row.Cells["contact"].Value.ToString();
                txtEditAlamat.Text = row.Cells["address"].Value.ToString();

                // 4. Munculkan GroupBox Edit
                gbEditVendor.Visible = true;

                // Opsional: Bawa panel ke paling depan agar tidak tertutup
                gbEditVendor.BringToFront();
            }
        }

        private void HapusVendor(int id)
        {
            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    // Query Delete
                    string query = "DELETE FROM vendors WHERE id = @id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Data vendor berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh Grid (Gunakan text pencarian agar user tetap di konteks pencarian yang sama)
                LoadDataVendor(txtCariVendor.Text);
            }
            catch (PostgresException ex)
            {
                // Menangani Error Foreign Key (Relasi Database)
                // Kode '23503' adalah foreign_key_violation di PostgreSQL
                if (ex.SqlState == "23503")
                {
                    MessageBox.Show(
                        "Gagal menghapus! Vendor ini sedang digunakan di data lain (Bahan Baku atau Pembelian). Hapus data terkait terlebih dahulu.",
                        "Gagal Hapus",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show($"Error database: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Terjadi kesalahan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void guna2GroupBox1_Click(object sender, EventArgs e)
        {

        }

        private void LoadDataVendor(string keyword)
        {
            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    // Query SQL Dinamis: Filter berdasarkan user_id DAN pencarian nama/kontak/alamat
                    // Menggunakan ILIKE agar pencarian tidak sensitif huruf besar/kecil (Case-Insensitive)
                    string query = @"
                        SELECT id, name, contact, address 
                        FROM vendors 
                        WHERE user_id = @uid 
                        AND (name ILIKE @search OR contact ILIKE @search OR address ILIKE @search)
                        ORDER BY created_at DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        // Tambahkan wildcard '%' di awal dan akhir untuk pencarian parsial
                        cmd.Parameters.AddWithValue("@search", "%" + keyword + "%");

                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            dgvDataVendor.DataSource = dt;

                            // --- 1. FORMATTING HEADER & VISIBILITY ---
                            if (dgvDataVendor.Columns.Contains("id")) dgvDataVendor.Columns["id"].Visible = false;

                            if (dgvDataVendor.Columns.Contains("name"))
                                dgvDataVendor.Columns["name"].HeaderText = "Nama Vendor";

                            if (dgvDataVendor.Columns.Contains("contact"))
                                dgvDataVendor.Columns["contact"].HeaderText = "Kontak";

                            if (dgvDataVendor.Columns.Contains("address"))
                                dgvDataVendor.Columns["address"].HeaderText = "Alamat";


                            // --- 2. MENGECILKAN KOLOM EDIT & HAPUS ---
                            // Set AutoSizeMode ke None agar lebar bisa diatur manual
                            // Set Width ke angka kecil (misal 50 pixel)

                            if (dgvDataVendor.Columns.Contains("Edit"))
                            {
                                dgvDataVendor.Columns["Edit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                                dgvDataVendor.Columns["Edit"].Width = 60;
                            }

                            if (dgvDataVendor.Columns.Contains("Hapus"))
                            {
                                dgvDataVendor.Columns["Hapus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                                dgvDataVendor.Columns["Hapus"].Width = 60;
                            }


                            // --- 3. MEMBUAT TEKS RATA TENGAH (CENTER ALIGNMENT) ---

                            // A. Rata Tengah untuk Header (Judul Kolom)
                            dgvDataVendor.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                            // B. Rata Tengah untuk Isi Data (Semua Kolom)
                            dgvDataVendor.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                            // Jika ingin kolom tertentu (misal Alamat) tetap rata kiri agar mudah dibaca:
                            if (dgvDataVendor.Columns.Contains("address"))
                            {
                                dgvDataVendor.Columns["address"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            }


                            // --- 4. MENGATUR POSISI KOLOM (Display Index) ---
                            // Kolom Data di Kiri (0,1,2), Tombol Aksi di Kanan (3,4)

                            if (dgvDataVendor.Columns.Contains("name")) dgvDataVendor.Columns["name"].DisplayIndex = 0;
                            if (dgvDataVendor.Columns.Contains("contact")) dgvDataVendor.Columns["contact"].DisplayIndex = 1;
                            if (dgvDataVendor.Columns.Contains("address")) dgvDataVendor.Columns["address"].DisplayIndex = 2;
                            if (dgvDataVendor.Columns.Contains("Edit")) dgvDataVendor.Columns["Edit"].DisplayIndex = 3;
                            if (dgvDataVendor.Columns.Contains("Hapus")) dgvDataVendor.Columns["Hapus"].DisplayIndex = 4;

                            // Hilangkan seleksi baris pertama
                            dgvDataVendor.ClearSelection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBatalPopUp_Click(object sender, EventArgs e)
        {
            BersihkanInput();
        }

        private void BersihkanInput()
        {
            txtInputNamaVendor.Clear();
            txtInputKontak.Clear();
            txtInputAlamat.Clear();
        }

        private void txtCariVendor_TextChanged(object sender, EventArgs e)
        {
            LoadDataVendor(txtCariVendor.Text);
        }

        private void btnPerbarui_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEditNamaVendor.Text))
            {
                MessageBox.Show("Nama Vendor tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    // Query UPDATE
                    // Kita gunakan idVendorTerpilih yang sudah di-set saat klik ikon pensil
                    string query = @"
                UPDATE vendors 
                SET name = @name, 
                    contact = @contact, 
                    address = @address 
                WHERE id = @id AND user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idVendorTerpilih);
                        cmd.Parameters.AddWithValue("@uid", currentUserId); // Security check
                        cmd.Parameters.AddWithValue("@name", txtEditNamaVendor.Text.Trim());

                        // Handle null/empty inputs
                        cmd.Parameters.AddWithValue("@contact", string.IsNullOrEmpty(txtEditKontak.Text) ? (object)DBNull.Value : txtEditKontak.Text.Trim());
                        cmd.Parameters.AddWithValue("@address", string.IsNullOrEmpty(txtEditAlamat.Text) ? (object)DBNull.Value : txtEditAlamat.Text.Trim());

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Data vendor berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Sembunyikan Panel Edit
                        gbEditVendor.Visible = false;

                        // Reset Variabel ID
                        idVendorTerpilih = 0;

                        // Refresh DataGrid agar perubahan terlihat
                        LoadDataVendor(txtCariVendor.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memperbarui data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBatal_Click(object sender, EventArgs e)
        {
            idVendorTerpilih = 0;
            gbEditVendor.Visible = false;
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            gbEditVendor.Visible = false;
            idVendorTerpilih = 0;
        }
    }
}
