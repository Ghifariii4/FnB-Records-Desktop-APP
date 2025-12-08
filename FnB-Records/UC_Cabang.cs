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
    public partial class UC_Cabang : UserControl
    {
        private int currentUserId = 1;
        private int idCabang = 0;


        public UC_Cabang()
        {
            InitializeComponent();
        }

       
        private void dgvDataVendor_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            // Pastikan yang di-hover bukan header (RowIndex >= 0)
            if (e.RowIndex >= 0)
            {
                string colName = dgvDataCabang.Columns[e.ColumnIndex].Name;

                // Cek apakah kolom tersebut adalah Edit atau Hapus
                if (colName == "Edit" || colName == "Hapus")
                {
                    dgvDataCabang.Cursor = Cursors.Hand;
                }
            }
        }

        // 2. Saat Mouse Keluar dari Sel
        private void dgvDataVendor_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            // Kembalikan kursor ke bentuk panah biasa (Default)
            dgvDataCabang.Cursor = Cursors.Default;
        }


        private void LoadDataCabang(string keyword)
        {
            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    // Query SQL Dinamis: Filter berdasarkan user_id DAN pencarian nama/kontak/alamat
                    // Menggunakan ILIKE agar pencarian tidak sensitif huruf besar/kecil (Case-Insensitive)
                    string query = @"
                        SELECT id, name, address, phone
                        FROM branches  
                        WHERE user_id = @uid 
                        AND (name ILIKE @search OR address ILIKE @search OR phone ILIKE @search)
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

                            dgvDataCabang.DataSource = dt;

                            // --- 1. FORMATTING HEADER & VISIBILITY ---
                            if (dgvDataCabang.Columns.Contains("id")) dgvDataCabang.Columns["id"].Visible = false;

                            if (dgvDataCabang.Columns.Contains("name"))
                                dgvDataCabang.Columns["name"].HeaderText = "Nama Cabang";

                            if (dgvDataCabang.Columns.Contains("address"))
                                dgvDataCabang.Columns["address"].HeaderText = "Alamat";

                            if (dgvDataCabang.Columns.Contains("phone"))
                                dgvDataCabang.Columns["phone"].HeaderText = "Telepon";




                            if (dgvDataCabang.Columns.Contains("Edit"))
                            {
                                dgvDataCabang.Columns["Edit"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                                dgvDataCabang.Columns["Edit"].Width = 60;
                            }

                            if (dgvDataCabang.Columns.Contains("Hapus"))
                            {
                                dgvDataCabang.Columns["Hapus"].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                                dgvDataCabang.Columns["Hapus"].Width = 60;
                            }


                            // --- 3. MEMBUAT TEKS RATA TENGAH (CENTER ALIGNMENT) ---

                            // A. Rata Tengah untuk Header (Judul Kolom)
                            dgvDataCabang.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                            // B. Rata Tengah untuk Isi Data (Semua Kolom)
                            dgvDataCabang.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                            // Jika ingin kolom tertentu (misal Alamat) tetap rata kiri agar mudah dibaca:
                            if (dgvDataCabang.Columns.Contains("address"))
                            {
                                dgvDataCabang.Columns["address"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            }


                            // --- 4. MENGATUR POSISI KOLOM (Display Index) ---
                            // Kolom Data di Kiri (0,1,2), Tombol Aksi di Kanan (3,4)

                            if (dgvDataCabang.Columns.Contains("name")) dgvDataCabang.Columns["name"].DisplayIndex = 0;
                            if (dgvDataCabang.Columns.Contains("address")) dgvDataCabang.Columns["address"].DisplayIndex = 1;
                            if (dgvDataCabang.Columns.Contains("phone")) dgvDataCabang.Columns["phone"].DisplayIndex = 2;
                            if (dgvDataCabang.Columns.Contains("Edit")) dgvDataCabang.Columns["Edit"].DisplayIndex = 3;
                            if (dgvDataCabang.Columns.Contains("Hapus")) dgvDataCabang.Columns["Hapus"].DisplayIndex = 4;

                            // Hilangkan seleksi baris pertama
                            dgvDataCabang.ClearSelection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BersihkanInput()
        {
            txtInputNamaCabang.Clear();
            txtInputKontak.Clear();
            txtInputAlamat.Clear();
        }

        private void HapusCabang(int id)
        {
            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Query Hapus
                    string query = "DELETE FROM branches WHERE id = @id AND user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        // Pastikan hanya menghapus milik user yang sedang login (Keamanan)
                        cmd.Parameters.AddWithValue("@uid", currentUserId);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Data cabang berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Refresh tabel agar data hilang dari tampilan
                            LoadDataCabang(txtCariCabang.Text);
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
                    MessageBox.Show("Gagal menghapus! Cabang ini sedang digunakan dalam data Bahan Baku atau Pembelian. Hapus data terkait terlebih dahulu.",
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

        private void btnTambahVendor_Click(object sender, EventArgs e)
        {
            gbCabangPopUp.Visible = true;
        }

        private void btClosePopUpCabang_Click(object sender, EventArgs e)
        {
            gbCabangPopUp.Visible = false;
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void btnSimpanPopUp_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputNamaCabang.Text))
            {
                MessageBox.Show("Nama Cabang wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    string query = "INSERT INTO branches (user_id, name, address, phone, created_at) VALUES (@uid, @name, @addr, @phone, @created_at)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@name", txtInputNamaCabang.Text.Trim());
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(txtInputAlamat.Text) ? (object)DBNull.Value : txtInputAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(txtInputKontak.Text) ? (object)DBNull.Value : txtInputKontak.Text.Trim());
                        cmd.Parameters.AddWithValue("@created_at", DateTime.Now);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Cabang berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);    

                        LoadDataCabang("");
                        BersihkanInput();
                        gbCabangPopUp.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void dgvDataVendor_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {


        }

        private void btnBatalPopUp_Click(object sender, EventArgs e)
        {
            BersihkanInput();
        }

        private void UC_Cabang_Load(object sender, EventArgs e)
        {
            LoadDataCabang("");
        }

        private void dgvDataCabang_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            // Ambil nama kolom yang diklik
            string colName = dgvDataCabang.Columns[e.ColumnIndex].Name;

            // Ambil ID dari kolom tersembunyi "id" (Pastikan kolom 'id' ada di query SQL Anda)
            // Gunakan TryParse untuk keamanan jika nilai null/error
            if (dgvDataCabang.Rows[e.RowIndex].Cells["id"].Value != null &&
                int.TryParse(dgvDataCabang.Rows[e.RowIndex].Cells["id"].Value.ToString(), out int idDipilih))
            {
                // --- LOGIKA HAPUS ---
                if (colName == "Hapus")
                {
                    string namaCabang = dgvDataCabang.Rows[e.RowIndex].Cells["name"].Value.ToString();

                    // Tampilkan Konfirmasi
                    DialogResult dialog = MessageBox.Show(
                        $"Apakah Anda yakin ingin menghapus cabang '{namaCabang}'?\nData yang dihapus tidak dapat dikembalikan.",
                        "Konfirmasi Hapus",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (dialog == DialogResult.Yes)
                    {
                        HapusCabang(idDipilih);
                    }
                }
                // --- LOGIKA EDIT (Tetap seperti kode Anda) ---
                else if (colName == "Edit")
                {
                    idCabang = idDipilih;
                    txtEditNama.Text = dgvDataCabang.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    txtEditAlamat.Text = dgvDataCabang.Rows[e.RowIndex].Cells["address"].Value.ToString();
                    txtEditTelepon.Text = dgvDataCabang.Rows[e.RowIndex].Cells["phone"].Value.ToString();

                    gbEditCabang.Visible = true;
                    gbEditCabang.BringToFront();
                }
            }
        }

        private void txtCariCabang_Click(object sender, EventArgs e)
        {
            LoadDataCabang(txtCariCabang.Text);
        }

        private void btnEditCabang_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtEditNama.Text))
            {
                MessageBox.Show("Nama cabang tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                UPDATE branches 
                SET name = @name, 
                    address = @addr, 
                    phone = @phone 
                WHERE id = @id AND user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idCabang);
                        cmd.Parameters.AddWithValue("@uid", currentUserId); // Security check
                        cmd.Parameters.AddWithValue("@name", txtEditNama.Text.Trim());

                        // Handle null/empty inputs
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(txtEditAlamat.Text) ? (object)DBNull.Value : txtEditAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(txtEditTelepon.Text) ? (object)DBNull.Value : txtEditTelepon.Text.Trim());

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Data cabang berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Sembunyikan Panel Edit
                        gbEditCabang.Visible = false;

                        // Reset Variabel ID
                        idCabang = 0;

                        // Refresh DataGrid agar perubahan terlihat
                        LoadDataCabang(txtCariVendor.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memperbarui data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBatalEdit_Click(object sender, EventArgs e)
        {
            idCabang = 0;
            gbEditCabang.Visible = false;
        }
    }
}

