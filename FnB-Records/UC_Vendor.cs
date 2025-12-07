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
        private int currentUserId => Login.GlobalSession.CurrentUserId;

        private int idVendorTerpilih = 0;

        public UC_Vendor()
        {
            InitializeComponent();
            gbEditVendor.BackColor = Color.White;
            gbVendorPopUp.BackColor = Color.White;
        }

        private void UC_Vendor_Load(object sender, EventArgs e)
        {
            // SAFETY CHECK: (Sama seperti UC_BahanBaku)
            // Jika aplikasi dijalankan langsung tanpa Login (User ID 0), paksa jadi 1 untuk testing.
            if (Login.GlobalSession.CurrentUserId == 0)
            {
                Login.GlobalSession.CurrentUserId = 1;
            }

            LoadDataVendor("");
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
                        gbVendorPopUp.Visible = false;
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


        private void dgvDataVendor_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Cek agar tidak error jika klik header (RowIndex < 0)
            if (e.RowIndex < 0) return;

            // Ambil nama kolom yang diklik
            string colName = dgvDataVendor.Columns[e.ColumnIndex].Name;

            // Ambil ID dari kolom tersembunyi "id" (Pastikan kolom 'id' ada di query SQL Anda)
            // Gunakan TryParse untuk keamanan jika nilai null/error
            if (dgvDataVendor.Rows[e.RowIndex].Cells["id"].Value != null &&
                int.TryParse(dgvDataVendor.Rows[e.RowIndex].Cells["id"].Value.ToString(), out int idDipilih))
            {
                // --- LOGIKA HAPUS ---
                if (colName == "Hapus")
                {
                    string namaVendor = dgvDataVendor.Rows[e.RowIndex].Cells["name"].Value.ToString();

                    // Tampilkan Konfirmasi
                    DialogResult dialog = MessageBox.Show(
                        $"Apakah Anda yakin ingin menghapus vendor '{namaVendor}'?\nData yang dihapus tidak dapat dikembalikan.",
                        "Konfirmasi Hapus",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (dialog == DialogResult.Yes)
                    {
                        HapusVendor(idDipilih);
                    }
                }
                // --- LOGIKA EDIT (Tetap seperti kode Anda) ---
                else if (colName == "Edit")
                {
                    idVendorTerpilih = idDipilih;
                    txtEditNamaVendor.Text = dgvDataVendor.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    txtEditKontak.Text = dgvDataVendor.Rows[e.RowIndex].Cells["contact"].Value.ToString();
                    txtEditAlamat.Text = dgvDataVendor.Rows[e.RowIndex].Cells["address"].Value.ToString();

                    gbEditVendor.Visible = true;
                    gbEditVendor.BringToFront();
                }
            }
        }

        // 1. Saat Mouse Masuk ke Sel (Hover)
        private void dgvDataVendor_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            // Pastikan yang di-hover bukan header (RowIndex >= 0)
            if (e.RowIndex >= 0)
            {
                string colName = dgvDataVendor.Columns[e.ColumnIndex].Name;

                // Cek apakah kolom tersebut adalah Edit atau Hapus
                if (colName == "Edit" || colName == "Hapus")
                {
                    dgvDataVendor.Cursor = Cursors.Hand;
                }
            }
        }

        // 2. Saat Mouse Keluar dari Sel
        private void dgvDataVendor_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            // Kembalikan kursor ke bentuk panah biasa (Default)
            dgvDataVendor.Cursor = Cursors.Default;
        }

        // 2. FUNGSI EKSEKUSI HAPUS KE DATABASE
        private void HapusVendor(int id)
        {
            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Query Hapus
                    string query = "DELETE FROM vendors WHERE id = @id AND user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        // Pastikan hanya menghapus milik user yang sedang login (Keamanan)
                        cmd.Parameters.AddWithValue("@uid", currentUserId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data vendor berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Refresh tabel agar data hilang dari tampilan
                            LoadDataVendor(txtCariVendor.Text);
                        }
                        else
                        {
                            MessageBox.Show("Data tidak ditemukan atau sudah terhapus.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (PostgresException ex)
            {
                // Menangani Error Foreign Key (Jika vendor dipakai di tabel bahan baku/pembelian)
                if (ex.SqlState == "23503")
                {
                    MessageBox.Show("Gagal menghapus! Vendor ini sedang digunakan dalam data Bahan Baku atau Pembelian. Hapus data terkait terlebih dahulu.",
                        "Gagal Hapus", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void guna2GroupBox1_Click_1(object sender, EventArgs e)
        {

        }
    }
}
