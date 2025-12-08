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
    public partial class UC_ManajemenInventori : UserControl
    {
        private DataTable dtOriginal = new DataTable();
        private int idBahanTerpilih = 0;
        private string satuanTerpilih = "";
        public UC_ManajemenInventori()
        {
            InitializeComponent();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvDataInventori.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada data untuk diekspor.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV File (*.csv)|*.csv";
            sfd.FileName = "Laporan_Inventori_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder csvContent = new StringBuilder();

                    // 1. Header Laporan (Agar terlihat profesional)
                    csvContent.AppendLine("LAPORAN INVENTORI BAHAN BAKU");
                    csvContent.AppendLine("Tanggal Ekspor:," + DateTime.Now.ToString("dd MMMM yyyy HH:mm"));
                    csvContent.AppendLine("Total Nilai Aset:," + lblNilaiTotal.Text.Replace("Rp ", "").Trim());
                    csvContent.AppendLine(); // Baris kosong

                    // 2. Header Tabel
                    for (int i = 0; i < dgvDataInventori.Columns.Count; i++)
                    {
                        if (dgvDataInventori.Columns[i].Visible)
                        {
                            csvContent.Append(dgvDataInventori.Columns[i].HeaderText + ",");
                        }
                    }
                    csvContent.AppendLine();

                    // 3. Isi Data
                    foreach (DataGridViewRow row in dgvDataInventori.Rows)
                    {
                        for (int i = 0; i < dgvDataInventori.Columns.Count; i++)
                        {
                            if (dgvDataInventori.Columns[i].Visible)
                            {
                                string cellValue = row.Cells[i].Value?.ToString().Replace(",", "."); // Ganti koma dengan titik agar CSV aman
                                csvContent.Append(cellValue + ",");
                            }
                        }
                        csvContent.AppendLine();
                    }

                    File.WriteAllText(sfd.FileName, csvContent.ToString());
                    MessageBox.Show("Data berhasil diekspor ke CSV!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal mengekspor data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void txtCari_TextChanged(object sender, EventArgs e)
        {
            TerapkanFilter();
        }

        private void cbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            TerapkanFilter();
        }

        private void dgvDataInventori_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dgvDataInventori.Columns[e.ColumnIndex].Name;

            // Ambil Data Row dari baris yang diklik
            if (dgvDataInventori.Rows[e.RowIndex].DataBoundItem is DataRowView row)
            {
                // Simpan data ke variabel global
                idBahanTerpilih = Convert.ToInt32(row["id"]);
                satuanTerpilih = row["unit"].ToString();
                string namaBahan = row["name"].ToString();
                string stokSaatIni = row["stock"].ToString();

                // --- LOGIKA TOMBOL TAMBAH (+) ---
                if (colName == "colTambah")
                {
                    // 1. Reset Form Input (Kosongkan Teks)
                    txttbhJumlahMasuk.Text = "";
                    txttbhCatatan.Text = "";

                    // 2. Set Placeholder Guna UI (Native)
                    // Ini akan membuat teks samar "Jumlah dalam Kg" otomatis muncul
                    txttbhJumlahMasuk.PlaceholderText = $"Jumlah dalam {satuanTerpilih}";

                    // 3. Isi Label (HANYA NAMA & ANGKA)
                    // Teks statis "Tambah Stok :" dihapus dari sini karena sudah ada di Desain Label
                    lbltbhItem.Text = namaBahan;
                    lbltbhStok.Text = $"{stokSaatIni} {satuanTerpilih}";

                    // 4. Atur Visibilitas Panel
                    gbTambahStok.Visible = true;
                    gbKurangStok.Visible = false;
                    gbTambahStok.BringToFront();
                }
                // --- LOGIKA TOMBOL KURANG (-) ---
                else if (colName == "colKurang")
                {
                    // 1. Reset Form Input (Kosongkan Teks)
                    txtkrgJumlahKeluar.Text = "";
                    txtkrgCatatan.Text = "";

                    // 2. Set Placeholder Guna UI (Native)
                    txtkrgJumlahKeluar.PlaceholderText = $"Jumlah dalam {satuanTerpilih}";

                    // 3. Isi Label (HANYA NAMA & ANGKA)
                    // Teks statis "Kurangi Stok :" dihapus agar tidak double
                    lblkrgItem.Text = namaBahan;
                    lblkrgStok.Text = $"{stokSaatIni} {satuanTerpilih}";

                    // 4. Atur Visibilitas Panel
                    gbKurangStok.Visible = true;
                    gbTambahStok.Visible = false;
                    gbKurangStok.BringToFront();
                }
            }
        }

        private void UC_ManajemenInventori_Load(object sender, EventArgs e)
        {
            if (Login.GlobalSession.CurrentUserId == 0) Login.GlobalSession.CurrentUserId = 1;

            LoadDataInventori();
            IsiComboBoxStatus();
        }

        private void LoadDataInventori()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Ambil semua data bahan baku
                    string query = @"
                        SELECT 
                            id, 
                            name, 
                            stock, 
                            min_stock, 
                            unit, 
                            price,
                            (stock * price) as total_value
                        FROM ingredients 
                        WHERE user_id = @uid 
                        ORDER BY name ASC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            dtOriginal = new DataTable();
                            da.Fill(dtOriginal);

                            // Tambahkan kolom 'Status' manual ke DataTable untuk memudahkan filter
                            dtOriginal.Columns.Add("status_text", typeof(string));

                            foreach (DataRow row in dtOriginal.Rows)
                            {
                                decimal stock = Convert.ToDecimal(row["stock"]);
                                decimal minStock = Convert.ToDecimal(row["min_stock"]);

                                if (stock <= 0) row["status_text"] = "Habis";
                                else if (stock <= minStock) row["status_text"] = "Stok Rendah";
                                else row["status_text"] = "Aman";
                            }

                            // Tampilkan ke Grid & Hitung Dashboard
                            TerapkanFilter();
                            HitungDashboardMetrics();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data inventori: " + ex.Message);
            }
        }

        private void HitungDashboardMetrics()
        {
            // Menghitung statistik dari DataTable (dtOriginal) agar tidak perlu query DB lagi
            int totalItem = dtOriginal.Rows.Count;
            int stokKritis = 0; // Stok Habis
            int stokRendah = 0; // Di bawah minimum tapi ada isinya
            decimal nilaiTotal = 0;

            foreach (DataRow row in dtOriginal.Rows)
            {
                decimal stock = Convert.ToDecimal(row["stock"]);
                decimal minStock = Convert.ToDecimal(row["min_stock"]);
                decimal totalVal = row["total_value"] != DBNull.Value ? Convert.ToDecimal(row["total_value"]) : 0;

                if (stock <= 0) stokKritis++;
                else if (stock <= minStock) stokRendah++;

                nilaiTotal += totalVal;
            }

            // Update Labels
            lblTotalItem.Text = totalItem.ToString();
            lblStokKritis.Text = stokKritis.ToString();
            lblStokRendah.Text = stokRendah.ToString();
            lblNilaiTotal.Text = "Rp " + nilaiTotal.ToString("N0");
        }

        // --- 2. FILTERING (CARI & COMBOBOX) ---
        private void IsiComboBoxStatus()
        {
            cbStatus.Items.Clear();
            cbStatus.Items.Add("Semua Status");
            cbStatus.Items.Add("Aman");
            cbStatus.Items.Add("Stok Rendah");
            cbStatus.Items.Add("Habis");
            cbStatus.SelectedIndex = 0;
        }

        private void TerapkanFilter()
        {
            string keyword = txtCari.Text.Trim();
            string statusFilter = cbStatus.SelectedItem?.ToString();

            // Gunakan DataView untuk filter tanpa query ulang
            DataView dv = dtOriginal.DefaultView;
            string filterQuery = "";

            // 1. Filter Pencarian Nama
            if (!string.IsNullOrEmpty(keyword))
            {
                filterQuery += $"name LIKE '%{keyword}%'";
            }

            // 2. Filter Status Dropdown
            if (statusFilter != "Semua Status" && !string.IsNullOrEmpty(statusFilter))
            {
                if (filterQuery.Length > 0) filterQuery += " AND ";
                filterQuery += $"status_text = '{statusFilter}'";
            }

            dv.RowFilter = filterQuery;
            dgvDataInventori.DataSource = dv;

            FormatTabel();
        }

        private void FormatTabel()
        {
            // 1. Sembunyikan ID dan UserID
            if (dgvDataInventori.Columns.Contains("id")) dgvDataInventori.Columns["id"].Visible = false;
            if (dgvDataInventori.Columns.Contains("user_id")) dgvDataInventori.Columns["user_id"].Visible = false;
            if (dgvDataInventori.Columns.Contains("total_value")) dgvDataInventori.Columns["total_value"].Visible = false;

            // 2. Rename Header
            SetHeader("name", "Nama Bahan");
            SetHeader("stock", "Stok Saat Ini");
            SetHeader("min_stock", "Stok Min");
            SetHeader("unit", "Satuan");
            SetHeader("price", "Harga/Unit");
            SetHeader("status_text", "Status");

            // 3. Format Angka & Alignment
            if (dgvDataInventori.Columns.Contains("price"))
            {
                dgvDataInventori.Columns["price"].DefaultCellStyle.Format = "C0";
                dgvDataInventori.Columns["price"].DefaultCellStyle.FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("id-ID");
            }

            dgvDataInventori.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDataInventori.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            if (dgvDataInventori.Columns.Contains("name"))
                dgvDataInventori.Columns["name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            // 4. --- TAMBAHAN TOMBOL AKSI (+ dan -) ---

            // Hapus kolom tombol jika sudah ada (agar tidak duplikat saat reload)
            if (dgvDataInventori.Columns.Contains("colTambah")) dgvDataInventori.Columns.Remove("colTambah");
            if (dgvDataInventori.Columns.Contains("colKurang")) dgvDataInventori.Columns.Remove("colKurang");

            // Tombol Tambah (+)
            DataGridViewButtonColumn btnTambah = new DataGridViewButtonColumn();
            btnTambah.Name = "colTambah";
            btnTambah.HeaderText = "Aksi";
            btnTambah.Text = "➕";
            btnTambah.UseColumnTextForButtonValue = true;
            btnTambah.Width = 20;

            // --- TAMBAHAN PENTING ---
            btnTambah.FlatStyle = FlatStyle.Flat; // Agar warna terlihat
            btnTambah.DefaultCellStyle.ForeColor = Color.SeaGreen; // Warna Hijau
            btnTambah.DefaultCellStyle.SelectionForeColor = Color.SeaGreen; // Tetap hijau saat diklik
                                                                            // ------------------------
            dgvDataInventori.Columns.Add(btnTambah);

            // Tombol Kurang (-)
            DataGridViewButtonColumn btnKurang = new DataGridViewButtonColumn();
            btnKurang.Name = "colKurang";
            btnKurang.HeaderText = "";
            btnKurang.Text = "➖";
            btnKurang.UseColumnTextForButtonValue = true;
            btnKurang.Width = 20;

            // --- TAMBAHAN PENTING ---
            btnKurang.FlatStyle = FlatStyle.Flat; // Agar warna terlihat
            btnKurang.DefaultCellStyle.ForeColor = Color.Crimson; // Warna Merah
            btnKurang.DefaultCellStyle.SelectionForeColor = Color.Crimson; // Tetap merah saat diklik
                                                                           // ------------------------
            dgvDataInventori.Columns.Add(btnKurang);

            // Pindahkan ke Paling Kanan
            dgvDataInventori.Columns["colTambah"].DisplayIndex = dgvDataInventori.Columns.Count - 2;
            dgvDataInventori.Columns["colKurang"].DisplayIndex = dgvDataInventori.Columns.Count - 1;

            // 5. Warnai Baris (Status)
            foreach (DataGridViewRow row in dgvDataInventori.Rows)
            {
                string status = row.Cells["status_text"].Value?.ToString();
                if (status == "Habis")
                {
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
                else if (status == "Stok Rendah")
                {
                    row.DefaultCellStyle.BackColor = Color.LightYellow;
                    row.DefaultCellStyle.ForeColor = Color.DarkGoldenrod;
                }
            }

            dgvDataInventori.ClearSelection();
        }

        private void SetHeader(string colName, string text)
        {
            if (dgvDataInventori.Columns.Contains(colName)) dgvDataInventori.Columns[colName].HeaderText = text;
        }

        // --- LOGIKA PLACEHOLDER DINAMIS ---

        private void AturPlaceholder(Guna.UI2.WinForms.Guna2TextBox txt, bool isInit = false)
        {
            string placeholderText = $"Jumlah dalam {satuanTerpilih}";

            if (isInit) // Saat form dibuka
            {
                txt.Text = placeholderText;
                txt.ForeColor = Color.Gray;
            }
            else if (txt.Text == placeholderText) // Saat user klik (Enter)
            {
                txt.Text = "";
                txt.ForeColor = Color.Black;
            }
            else if (string.IsNullOrWhiteSpace(txt.Text)) // Saat user pergi (Leave)
            {
                txt.Text = placeholderText;
                txt.ForeColor = Color.Gray;
            }
        }

        // Event Enter (Saat textbox diklik)
        private void txttbhJumlahMasuk_Enter(object sender, EventArgs e) => AturPlaceholder(txttbhJumlahMasuk);
        private void txtkrgJumlahKeluar_Enter(object sender, EventArgs e) => AturPlaceholder(txtkrgJumlahKeluar);

        // Event Leave (Saat kursor keluar textbox)
        private void txttbhJumlahMasuk_Leave(object sender, EventArgs e) => AturPlaceholder(txttbhJumlahMasuk, false);
        private void txtkrgJumlahKeluar_Leave(object sender, EventArgs e) => AturPlaceholder(txtkrgJumlahKeluar, false);

        private void btntbhSimpan_Click(object sender, EventArgs e)
        {
            UpdateStokDatabase(true);
        }

        private void btnkrgSimpan_Click(object sender, EventArgs e)
        {
            UpdateStokDatabase(false);
        }

        private void UpdateStokDatabase(bool isTambah)
        {
            Guna.UI2.WinForms.Guna2TextBox txtJumlah = isTambah ? txttbhJumlahMasuk : txtkrgJumlahKeluar;
            Guna.UI2.WinForms.Guna2TextBox txtCatatan = isTambah ? txttbhCatatan : txtkrgCatatan;
            Guna.UI2.WinForms.Guna2GroupBox gbTerkait = isTambah ? gbTambahStok : gbKurangStok;

            string inputBersih = txtJumlah.Text.Trim();

            if (!decimal.TryParse(inputBersih, out decimal jumlah) || jumlah <= 0)
            {
                MessageBox.Show("Masukkan jumlah yang valid!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Query Update
                    // Jika Tambah: stock = stock + jumlah
                    // Jika Kurang: stock = stock - jumlah
                    string operatorSql = isTambah ? "+" : "-";

                    string query = $@"
                UPDATE ingredients 
                SET stock = stock {operatorSql} @qty, 
                    updated_at = CURRENT_TIMESTAMP 
                WHERE id = @id AND user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@qty", jumlah);
                        cmd.Parameters.AddWithValue("@id", idBahanTerpilih);
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Stok berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            gbTerkait.Visible = false; // Tutup popup
                            LoadDataInventori(); // Refresh Grid
                        }
                        else
                        {
                            MessageBox.Show("Gagal mengupdate stok. Data mungkin sudah dihapus.", "Gagal");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error database: " + ex.Message);
            }
        }

        // Tombol Batal
        private void btntbhBatal_Click(object sender, EventArgs e) => gbTambahStok.Visible = false;
        private void btnkrgBatal_Click(object sender, EventArgs e) => gbKurangStok.Visible = false;

        private void guna2GroupBox3_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            gbKurangStok.Visible = false;
        }

        private void btnClosePopUpBahanBaku_Click(object sender, EventArgs e)
        {
            gbTambahStok.Visible = false;
        }
    }
}
