using FnB_Records.Koneksi_DB;
using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class UC_BahanBaku : UserControl
    {
        // Sesuaikan dengan session login Anda
        private int currentUserId = Login.GlobalSession.CurrentUserId;
        private int idBahanTerpilih = 0;

        public UC_BahanBaku()
        {
            InitializeComponent();
            gbBahanBakuPopUp.BackColor = Color.White;
        }

        // --- 1. EVENT LOAD ---
        private void UC_BahanBaku_Load(object sender, EventArgs e)
        {
            if (Login.GlobalSession.CurrentUserId == 0)
            {
                Login.GlobalSession.CurrentUserId = 1; // ID User Anda di Database
            }

            dgvDataBahanBaku.AlternatingRowsDefaultCellStyle.BackColor = Color.White;

            LoadComboVendor(); // Load Vendor DULUAN
            LoadDataBahan(""); // Baru Load Data Tab
        }

        // --- 2. LOAD VENDOR (Untuk ComboBox) ---
        // --- PERBAIKAN FUNGSI LOAD VENDOR ---
        // --- PERBAIKAN: LOAD COMBOBOX VENDOR ---
        // --- 2. LOAD VENDOR (FIX) ---
        private void LoadComboVendor()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Ambil ID dan Name
                    string sql = "SELECT id, name FROM vendors WHERE user_id = @uid ORDER BY name ASC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        // Menggunakan ID yang sudah dipastikan tidak 0
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        DataTable dt = new DataTable();
                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        // Debugging: Cek jika masih kosong
                        if (dt.Rows.Count == 0)
                        {
                            // Jika muncul pesan ini, berarti di database tabel vendors benar-benar kosong untuk user_id=1
                            // MessageBox.Show("Tidak ada data vendor di database untuk User ID " + Login.GlobalSession.CurrentUserId);
                        }

                        cbVendor.DataSource = dt;
                        cbVendor.DisplayMember = "name";
                        cbVendor.ValueMember = "id";
                        cbVendor.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat vendor: " + ex.Message);
            }
        }


        // --- 3. LOAD DATA & FORMATTING (DISATUKAN SEPERTI UC_VENDOR) ---
        private void LoadDataBahan(string keyword)
        {
            try
            {
                Koneksi koneksiDB = new Koneksi();
                using (NpgsqlConnection conn = koneksiDB.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string query = @"
                SELECT 
                    i.id,
                    i.vendor_id, 
                    i.name AS nama_bahan, 
                    v.name AS nama_vendor, 
                    i.price AS harga, 
                    i.stock AS stok, 
                    i.unit AS satuan,
                    i.min_stock
                FROM ingredients i
                LEFT JOIN vendors v ON i.vendor_id = v.id
                WHERE i.user_id = @uid 
                AND (i.name ILIKE @search OR v.name ILIKE @search)
                ORDER BY i.created_at DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@search", "%" + keyword + "%");

                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            // --- KOLOM CUSTOM (Status, Harga Display, dll) ---
                            // Nama kolom ini HARUS SAMA dengan DataPropertyName di Designer
                            dt.Columns.Add("status", typeof(string));
                            dt.Columns.Add("harga_display", typeof(string));
                            dt.Columns.Add("stok_display", typeof(string));

                            foreach (DataRow row in dt.Rows)
                            {
                                double stok = row["stok"] != DBNull.Value ? Convert.ToDouble(row["stok"]) : 0;
                                double minStok = row["min_stock"] != DBNull.Value ? Convert.ToDouble(row["min_stock"]) : 0;

                                // Isi kolom custom
                                row["status"] = (stok <= minStok) ? "⚠️ Menipis" : "✅ Aman";

                                double harga = row["harga"] != DBNull.Value ? Convert.ToDouble(row["harga"]) : 0;
                                string unit = row["satuan"].ToString();

                                row["harga_display"] = "Rp " + harga.ToString("N0") + " / " + unit;
                                row["stok_display"] = stok.ToString() + " " + unit;
                            }

                            // --- BAGIAN INI SANGAT PENTING ---
                            // Matikan generate otomatis agar settingan manual Designer tidak tertimpa
                            dgvDataBahanBaku.AutoGenerateColumns = false;

                            // Masukkan data
                            dgvDataBahanBaku.DataSource = dt;

                            // Tidak perlu lagi memanggil FormatTabel()!
                            // Tidak perlu hide kolom 'id', karena di Designer kita tidak membuat kolom 'id'.
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat data: {ex.Message}");
            }
        }

        // --- 4. TOMBOL SIMPAN (LOGIKA INSERT) ---
        private void btnSimpanPopUp_Click(object sender, EventArgs e)
        {
            // 1. Validasi Input Kosong
            if (string.IsNullOrWhiteSpace(txtInputNamaBahan.Text) ||
                string.IsNullOrWhiteSpace(txtInputHargaPerSatuan.Text) ||
                string.IsNullOrWhiteSpace(txtInputStokSaatIni.Text) ||
                string.IsNullOrWhiteSpace(txtInputStokMinimum.Text) ||
                cbSatuan.SelectedIndex == -1 ||
                cbVendor.SelectedValue == null)
            {
                MessageBox.Show("Mohon lengkapi semua data (Nama, Harga, Stok, Vendor, Satuan).", "Data Tidak Lengkap", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 2. Parsing Angka (Support Format Titik/Koma)
                // Kita hapus titik pemisah ribuan agar "10.000" terbaca sebagai 10000
                string hargaClean = txtInputHargaPerSatuan.Text.Replace(".", "").Replace(",", "");
                string stokClean = txtInputStokSaatIni.Text.Replace(".", "").Replace(",", "."); // Stok mungkin desimal (pake titik)
                string minStokClean = txtInputStokMinimum.Text.Replace(".", "").Replace(",", ".");

                if (!double.TryParse(hargaClean, out double harga) ||
                    !double.TryParse(stokClean, out double stok) ||
                    !double.TryParse(minStokClean, out double minStok))
                {
                    MessageBox.Show("Format Harga atau Stok salah! Masukkan angka yang valid.", "Error Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 3. Simpan ke Database
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string query = "";

                    // --- LOGIKA BARU: CEK APAKAH EDIT ATAU BARU ---
                    if (idBahanTerpilih == 0)
                    {
                        // INSERT (Tambah Baru)
                        query = @"INSERT INTO ingredients (user_id, vendor_id, name, unit, price, stock, min_stock, created_at, updated_at) 
                  VALUES (@uid, @vid, @name, @unit, @price, @stock, @min, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";
                    }
                    else
                    {
                        // UPDATE (Edit Data Lama)
                        query = @"UPDATE ingredients 
                  SET vendor_id=@vid, name=@name, unit=@unit, price=@price, stock=@stock, min_stock=@min, updated_at=CURRENT_TIMESTAMP
                  WHERE id=@id AND user_id=@uid";
                    }
                    // ---------------------------------------------

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        // Parameter Umum
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                        cmd.Parameters.AddWithValue("@vid", Convert.ToInt32(cbVendor.SelectedValue));
                        cmd.Parameters.AddWithValue("@name", txtInputNamaBahan.Text.Trim());
                        cmd.Parameters.AddWithValue("@unit", cbSatuan.SelectedItem.ToString());
                        cmd.Parameters.AddWithValue("@price", harga);
                        cmd.Parameters.AddWithValue("@stock", stok);
                        cmd.Parameters.AddWithValue("@min", minStok);

                        // --- TAMBAHAN KHUSUS EDIT ---
                        if (idBahanTerpilih > 0)
                        {
                            cmd.Parameters.AddWithValue("@id", idBahanTerpilih);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                // 4. Feedback & Reset
                MessageBox.Show(idBahanTerpilih == 0 ? "Berhasil Ditambahkan!" : "Berhasil Diperbarui!", "Sukses");
                gbBahanBakuPopUp.Visible = false;
                BersihkanInput();
                LoadDataBahan("");

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan data: {ex.Message}", "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 5. EVENT KLIK GRID (EDIT & HAPUS) ---
        private void dgvDataBahanBaku_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1. Validasi Klik (Hindari Header)
            if (e.RowIndex < 0) return;

            // 2. Ambil Nama Kolom Tombol
            string colName = dgvDataBahanBaku.Columns[e.ColumnIndex].Name;

            // 3. AMBIL DATA DARI DATA SOURCE (BUKAN DARI CELL)
            // Kita ambil objek baris aslinya (DataRowView) di balik baris grid tersebut
            if (dgvDataBahanBaku.Rows[e.RowIndex].DataBoundItem is DataRowView row)
            {
                // Ambil ID langsung dari data aslinya
                int idDipilih = Convert.ToInt32(row["id"]);
                string namaBahan = row["nama_bahan"].ToString(); // Ambil nama juga dari sini lebih aman

                // --- LOGIKA HAPUS ---
                if (colName == "Hapus")
                {
                    if (MessageBox.Show($"Hapus '{namaBahan}'?", "Konfirmasi Hapus",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        HapusBahan(idDipilih);
                    }
                }
                // --- LOGIKA EDIT ---
                else if (colName == "Edit")
                {
                    // --- TAMBAHKAN LOGIKA INI ---

                    // 1. Set ID agar tombol Simpan tahu ini adalah Edit
                    idBahanTerpilih = Convert.ToInt32(row["id"]);

                    // 2. Isi Textbox dengan data lama
                    txtInputNamaBahan.Text = row["nama_bahan"].ToString();

                    // Format angka: Hilangkan "Rp" dan titik ribuan agar jadi angka murni (contoh: 10000)
                    txtInputHargaPerSatuan.Text = Convert.ToDouble(row["harga"]).ToString("N0").Replace(".", "");

                    txtInputStokSaatIni.Text = row["stok"].ToString();
                    txtInputStokMinimum.Text = row["min_stock"].ToString();

                    // 3. Set Dropdown
                    cbSatuan.SelectedItem = row["satuan"].ToString();
                    if (row["vendor_id"] != DBNull.Value)
                    {
                        cbVendor.SelectedValue = Convert.ToInt32(row["vendor_id"]);
                    }

                    // 4. Ubah Judul Tombol & Tampilkan Popup
                    btnSimpanPopUp.Text = "Update"; // Ubah teks tombol biar user sadar

                    gbBahanBakuPopUp.Visible = true;
                    gbBahanBakuPopUp.BringToFront();
                }
            }
        }

        // --- 6. HAPUS DATA ---
        private void HapusBahan(int id)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "DELETE FROM ingredients WHERE id = @id AND user_id = @uid";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Data dihapus.", "Sukses");
                LoadDataBahan(txtCariBahanBaku.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menghapus: " + ex.Message);
            }
        }

        // --- 7. UTILITIES & EVENTS LAIN ---
        private void dgvDataBahanBaku_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string colName = dgvDataBahanBaku.Columns[e.ColumnIndex].Name;
                if (colName == "Edit" || colName == "Hapus") dgvDataBahanBaku.Cursor = Cursors.Hand;
            }
        }
        private void dgvDataBahanBaku_CellMouseLeave(object sender, DataGridViewCellEventArgs e) => dgvDataBahanBaku.Cursor = Cursors.Default;
        private void txtCariBahanBaku_TextChanged(object sender, EventArgs e) => LoadDataBahan(txtCariBahanBaku.Text.Trim());
        private void btTambahBahanBaku_Click(object sender, EventArgs e) { BersihkanInput(); gbBahanBakuPopUp.Visible = true; }
        private void btnClosePopUpBahanBaku_Click(object sender, EventArgs e) => gbBahanBakuPopUp.Visible = false;
        private void btnBatalPopUp_Click(object sender, EventArgs e) => gbBahanBakuPopUp.Visible = false;
        private void BersihkanInput()
        {

            idBahanTerpilih = 0;             
            btnSimpanPopUp.Text = "Simpan";
            txtInputNamaBahan.Clear();
            txtInputHargaPerSatuan.Clear();
            txtInputStokSaatIni.Clear();
            txtInputStokMinimum.Clear();
            cbSatuan.SelectedIndex = -1;
            cbVendor.SelectedIndex = -1;
        }
    }
}