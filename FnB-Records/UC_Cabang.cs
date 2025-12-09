using FnB_Records.Koneksi_DB;
using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class UC_Cabang : UserControl
    {
        // 1. AMBIL ID DARI SESI LOGIN (PENTING AGAR DATA TIDAK NYASAR)
        private int currentUserId => Login.GlobalSession.CurrentUserId;

        private int idCabang = 0;

        public UC_Cabang()
        {
            InitializeComponent();
        }

        private void UC_Cabang_Load(object sender, EventArgs e)
        {
            // Cek keamanan sesi
            if (currentUserId <= 0)
            {
                // Jika sesi hilang, set default atau minta login ulang (opsional)
                // Di sini kita biarkan saja agar query tidak error tapi hasil kosong
            }

            LoadDataCabang("");

            if (dgvDataCabang != null)
            {
                dgvDataCabang.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
            }
        }

        // ==========================================
        // 1. LOAD DATA (HANYA MILIK USER LOGIN)
        // ==========================================
        private void LoadDataCabang(string keyword)
        {
            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Query ini sudah AMAN.
                    // WHERE user_id = @uid  <-- Ini yang memfilter berdasarkan Akun/Email Login
                    string query = @"
                        SELECT id, name, address, phone
                        FROM branches  
                        WHERE user_id = @uid 
                        AND (name ILIKE @search OR address ILIKE @search OR phone ILIKE @search)
                        ORDER BY created_at DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        // Masukkan ID user yang sedang login
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@search", "%" + keyword + "%");

                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            dgvDataCabang.DataSource = dt;

                            // --- FORMAT TAMPILAN ---
                            if (dgvDataCabang.Columns.Contains("id")) dgvDataCabang.Columns["id"].Visible = false;

                            if (dgvDataCabang.Columns.Contains("name"))
                            {
                                dgvDataCabang.Columns["name"].HeaderText = "Nama Cabang";
                                dgvDataCabang.Columns["name"].DisplayIndex = 0;
                            }

                            if (dgvDataCabang.Columns.Contains("phone"))
                            {
                                dgvDataCabang.Columns["phone"].HeaderText = "Telepon";
                                dgvDataCabang.Columns["phone"].DisplayIndex = 1;
                            }

                            if (dgvDataCabang.Columns.Contains("address"))
                            {
                                dgvDataCabang.Columns["address"].HeaderText = "Alamat";
                                dgvDataCabang.Columns["address"].DisplayIndex = 2;
                            }

                            // Atur Tombol Aksi (Edit/Hapus)
                            if (dgvDataCabang.Columns.Contains("Edit"))
                            {
                                dgvDataCabang.Columns["Edit"].Width = 60;
                                dgvDataCabang.Columns["Edit"].DisplayIndex = 3;
                            }
                            if (dgvDataCabang.Columns.Contains("Hapus"))
                            {
                                dgvDataCabang.Columns["Hapus"].Width = 60;
                                dgvDataCabang.Columns["Hapus"].DisplayIndex = 4;
                            }

                            // Styling
                            dgvDataCabang.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            dgvDataCabang.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
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

        // ==========================================
        // 2. TAMBAH DATA (OTOMATIS SESUAI AKUN)
        // ==========================================
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
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Tidak perlu input email, karena otomatis ngikut user_id
                    string query = @"INSERT INTO branches (user_id, name, address, phone, created_at) 
                                     VALUES (@uid, @name, @addr, @phone, @created_at)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId); // <-- Ini kunci pengamannya
                        cmd.Parameters.AddWithValue("@name", txtInputNamaCabang.Text.Trim());
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(txtInputAlamat.Text) ? (object)DBNull.Value : txtInputAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(txtInputKontak.Text) ? (object)DBNull.Value : txtInputKontak.Text.Trim());
                        cmd.Parameters.AddWithValue("@created_at", DateTime.Now);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Cabang berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDataCabang("");
                        BersihkanInput();
                        if (gbCabangPopUp != null) gbCabangPopUp.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // 3. EDIT DATA
        // ==========================================
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
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Pastikan WHERE user_id = @uid ada supaya tidak sengaja edit punya orang lain (jika ID bocor)
                    string query = @"
                        UPDATE branches 
                        SET name = @name, 
                            address = @addr, 
                            phone = @phone
                        WHERE id = @id AND user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", idCabang);
                        cmd.Parameters.AddWithValue("@uid", currentUserId); // <-- Pengaman Ganda
                        cmd.Parameters.AddWithValue("@name", txtEditNama.Text.Trim());
                        cmd.Parameters.AddWithValue("@addr", string.IsNullOrEmpty(txtEditAlamat.Text) ? (object)DBNull.Value : txtEditAlamat.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", string.IsNullOrEmpty(txtEditTelepon.Text) ? (object)DBNull.Value : txtEditTelepon.Text.Trim());

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Data cabang berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            if (gbEditCabang != null) gbEditCabang.Visible = false;
                            idCabang = 0;
                            LoadDataCabang(txtCariCabang.Text);
                        }
                        else
                        {
                            MessageBox.Show("Gagal update. Data tidak ditemukan atau Anda tidak punya akses.", "Error");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memperbarui data: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================
        // 4. HAPUS DATA
        // ==========================================
        private void HapusCabang(int id)
        {
            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Pastikan WHERE user_id = @uid ada
                    string query = "DELETE FROM branches WHERE id = @id AND user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@uid", currentUserId); // <-- Pengaman Ganda
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            MessageBox.Show("Data cabang berhasil dihapus.", "Sukses");
                            LoadDataCabang(txtCariCabang.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal hapus: {ex.Message}");
            }
        }

        // ==========================================
        // EVENT HANDLERS & HELPERS
        // ==========================================
        private void BersihkanInput()
        {
            if (txtInputNamaCabang != null) txtInputNamaCabang.Clear();
            if (txtInputKontak != null) txtInputKontak.Clear();
            if (txtInputAlamat != null) txtInputAlamat.Clear();
        }

        private void dgvDataCabang_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dgvDataCabang.Columns[e.ColumnIndex].Name;

            if (dgvDataCabang.Rows[e.RowIndex].Cells["id"].Value != null &&
                int.TryParse(dgvDataCabang.Rows[e.RowIndex].Cells["id"].Value.ToString(), out int idDipilih))
            {
                if (colName == "Hapus")
                {
                    string namaCabang = dgvDataCabang.Rows[e.RowIndex].Cells["name"].Value.ToString();
                    DialogResult dialog = MessageBox.Show(
                        $"Hapus cabang '{namaCabang}'?",
                        "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                    if (dialog == DialogResult.Yes) HapusCabang(idDipilih);
                }
                else if (colName == "Edit")
                {
                    idCabang = idDipilih;
                    if (gbEditCabang != null)
                    {
                        txtEditNama.Text = dgvDataCabang.Rows[e.RowIndex].Cells["name"].Value.ToString();
                        txtEditAlamat.Text = dgvDataCabang.Rows[e.RowIndex].Cells["address"].Value.ToString();
                        txtEditTelepon.Text = dgvDataCabang.Rows[e.RowIndex].Cells["phone"].Value.ToString();

                        gbEditCabang.Visible = true;
                        gbEditCabang.BringToFront();
                    }
                }
            }
        }

        private void dgvDataCabang_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string col = dgvDataCabang.Columns[e.ColumnIndex].Name;
                if (col == "Edit" || col == "Hapus") dgvDataCabang.Cursor = Cursors.Hand;
            }
        }

        private void dgvDataCabang_CellMouseLeave(object sender, DataGridViewCellEventArgs e) => dgvDataCabang.Cursor = Cursors.Default;
        private void txtCariCabang_Click(object sender, EventArgs e) => LoadDataCabang(txtCariCabang.Text);
        private void txtCariCabang_TextChanged(object sender, EventArgs e) => LoadDataCabang(txtCariCabang.Text);
        private void btnTambahVendor_Click(object sender, EventArgs e) { if (gbCabangPopUp != null) gbCabangPopUp.Visible = true; }
        private void btClosePopUpCabang_Click(object sender, EventArgs e) { if (gbCabangPopUp != null) gbCabangPopUp.Visible = false; }
        private void btnBatalPopUp_Click(object sender, EventArgs e) { BersihkanInput(); if (gbCabangPopUp != null) gbCabangPopUp.Visible = false; }
        private void btnBatalEdit_Click(object sender, EventArgs e) { idCabang = 0; if (gbEditCabang != null) gbEditCabang.Visible = false; }
    }
}