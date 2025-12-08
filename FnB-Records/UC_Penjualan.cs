using FnB_Records.Koneksi_DB;
using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class UC_Penjualan : UserControl
    {
        private int currentUserId => Login.GlobalSession.CurrentUserId;

        public UC_Penjualan()
        {
            InitializeComponent();
            gbInputPenjualanPopUp.BackColor = Color.White;
        }

        // --- 1. SETUP EVENT & LOAD ---
        private void UC_Penjualan_Load(object sender, EventArgs e)
        {
            if (currentUserId == 0) Login.GlobalSession.CurrentUserId = 1;

            // Setting Default Tanggal
            dtpFilterTanggal.Value = DateTime.Now;
            dtpTanggalPenjualan.Value = DateTime.Now;
            dtpTanggalPenjualan.MinDate = DateTime.Today;

            SetupTableStyle();
            LoadComboMenu();
            AttachInputEvents();
            LoadDataPenjualan();
            HitungTotalSemuaTransaksi();
        }

        private void AttachInputEvents()
        {
            inputHargaJual.TextChanged += (s, ev) => HitungEstimasiLive();
            inputJumlahTerjual.TextChanged += (s, ev) => HitungEstimasiLive();
            inputDiskon.TextChanged += (s, ev) => HitungEstimasiLive();
            inputBiayaLain.TextChanged += (s, ev) => HitungEstimasiLive();
            cbMenu.SelectedIndexChanged += cbMenu_SelectedIndexChanged;

            txtCariRiwayatPenjualan.TextChanged += (s, ev) => LoadDataPenjualan();
            dtpFilterTanggal.ValueChanged += (s, ev) => LoadDataPenjualan();

            btinputPenjualanPopUp.Click += btinputPenjualanPopUp_Click;
            btnClosePopUpBahanBaku.Click += btnClosePopUpBahanBaku_Click;
            btnBatalPopUp.Click += btnBatalPopUp_Click;
            btnSimpanPopUp.Click += btnSimpanPopUp_Click;
        }

        // --- 2. LOAD DATA (FIX SUBTOTAL) ---
        private void LoadDataPenjualan()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT 
                            s.id, 
                            r.name AS nama_menu, 
                            s.qty AS jumlah_qty, 
                            s.selling_price AS harga_jual, 
                            (s.qty * s.selling_price) AS subtotal, -- Pastikan ini sesuai
                            s.tax AS ppn,
                            s.total_price AS total_harga,
                            s.profit,
                            s.total_hpp AS hpp
                        FROM sales s
                        JOIN recipes r ON s.recipe_id = r.id
                        WHERE s.user_id = @uid 
                        AND DATE(s.sale_date) = DATE(@date)
                        AND r.name ILIKE @search
                        ORDER BY s.created_at DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@date", dtpFilterTanggal.Value);
                        cmd.Parameters.AddWithValue("@search", "%" + txtCariRiwayatPenjualan.Text.Trim() + "%");

                        DataTable dt = new DataTable();
                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd)) da.Fill(dt);

                        dgvDataBahanBaku.AutoGenerateColumns = false;
                        dgvDataBahanBaku.DataSource = dt;
                        FormatKolom();
                        HitungRingkasan(dt);
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal Load Data: " + ex.Message); }
        }

        // --- 3. LOGIKA SIMPAN & KURANGI STOK ---
        private void btnSimpanPopUp_Click(object sender, EventArgs e)
        {
            if (cbMenu.SelectedValue == null || string.IsNullOrWhiteSpace(inputJumlahTerjual.Text))
            {
                MessageBox.Show("Lengkapi data!", "Peringatan"); return;
            }

            try
            {
                double harga = ParseCurrency(inputHargaJual.Text);
                int qty = (int)ParseCurrency(inputJumlahTerjual.Text);
                double diskon = ParseCurrency(inputDiskon.Text);
                double biayaLain = ParseCurrency(inputBiayaLain.Text);
                int recipeId = Convert.ToInt32(cbMenu.SelectedValue);

                double subtotal = harga * qty;
                double ppn = subtotal * 0.10;
                double total = (subtotal + ppn + biayaLain) - diskon;

                double hppSatuan = HitungHPP(recipeId);
                double totalHPP = hppSatuan * qty;
                double profit = (subtotal - totalHPP) - diskon;

                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // GUNAKAN TRANSAKSI: Agar Simpan Penjualan & Kurangi Stok atomic (sukses semua atau gagal semua)
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // A. Simpan Data Penjualan
                            string sqlSales = @"INSERT INTO sales 
                                (user_id, recipe_id, qty, selling_price, discount, other_fees, tax, revenue, profit, total_price, total_hpp, sale_date, created_at)
                                VALUES 
                                (@uid, @rid, @qty, @price, @disc, @fees, @tax, @rev, @prof, @total, @thpp, @date, CURRENT_TIMESTAMP)";

                            using (var cmd = new NpgsqlCommand(sqlSales, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@uid", currentUserId);
                                cmd.Parameters.AddWithValue("@rid", recipeId);
                                cmd.Parameters.AddWithValue("@qty", qty);
                                cmd.Parameters.AddWithValue("@price", harga);
                                cmd.Parameters.AddWithValue("@disc", diskon);
                                cmd.Parameters.AddWithValue("@fees", biayaLain);
                                cmd.Parameters.AddWithValue("@tax", ppn);
                                cmd.Parameters.AddWithValue("@rev", subtotal);
                                cmd.Parameters.AddWithValue("@prof", profit);
                                cmd.Parameters.AddWithValue("@total", total);
                                cmd.Parameters.AddWithValue("@thpp", totalHPP);
                                cmd.Parameters.AddWithValue("@date", dtpTanggalPenjualan.Value);
                                cmd.ExecuteNonQuery();
                            }

                            // B. Kurangi Stok Bahan Baku (LOGIKA BARU)
                            // Ambil bahan-bahan dari resep ini
                            string sqlBahan = "SELECT ingredient_id, amount FROM recipe_ingredients WHERE recipe_id = @rid";
                            using (var cmdGetBahan = new NpgsqlCommand(sqlBahan, conn, trans))
                            {
                                cmdGetBahan.Parameters.AddWithValue("@rid", recipeId);
                                using (var reader = cmdGetBahan.ExecuteReader())
                                {
                                    // Tampung dulu data bahan agar reader bisa ditutup sebelum update
                                    var bahanList = new System.Collections.Generic.List<(int id, double amount)>();
                                    while (reader.Read())
                                    {
                                        bahanList.Add((reader.GetInt32(0), reader.GetDouble(1)));
                                    }
                                    reader.Close(); // Tutup reader

                                    // Lakukan Update Stok
                                    foreach (var bahan in bahanList)
                                    {
                                        double jumlahTerpakai = bahan.amount * qty; // Takaran x Jumlah Porsi Terjual

                                        string sqlUpdateStok = "UPDATE ingredients SET stock = stock - @jml WHERE id = @iid";
                                        using (var cmdUpdate = new NpgsqlCommand(sqlUpdateStok, conn, trans))
                                        {
                                            cmdUpdate.Parameters.AddWithValue("@jml", jumlahTerpakai);
                                            cmdUpdate.Parameters.AddWithValue("@iid", bahan.id);
                                            cmdUpdate.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            trans.Commit(); // Simpan Permanen
                            MessageBox.Show("Penjualan disimpan & Stok berkurang!", "Sukses");
                            gbInputPenjualanPopUp.Visible = false;
                            BersihkanInput();
                            LoadDataPenjualan();
                            HitungTotalSemuaTransaksi();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback(); // Batalkan semua jika error
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal menyimpan: " + ex.Message, "Error"); }
        }

        // --- 4. HITUNG RINGKASAN ---
        private void HitungRingkasan(DataTable dt)
        {
            double totalPendapatan = 0, totalHPP = 0, totalLaba = 0;
            foreach (DataRow row in dt.Rows)
            {
                totalPendapatan += row["total_harga"] != DBNull.Value ? Convert.ToDouble(row["total_harga"]) : 0;
                totalHPP += row["hpp"] != DBNull.Value ? Convert.ToDouble(row["hpp"]) : 0;
                totalLaba += row["profit"] != DBNull.Value ? Convert.ToDouble(row["profit"]) : 0;
            }
            double margin = (totalPendapatan > 0) ? (totalLaba / totalPendapatan) * 100 : 0;

            System.Globalization.CultureInfo id = System.Globalization.CultureInfo.GetCultureInfo("id-ID");
            if (lblPendapatan != null) lblPendapatan.Text = totalPendapatan.ToString("C0", id);
            if (lblTotal_Hpp != null) lblTotal_Hpp.Text = totalHPP.ToString("C0", id);
            if (lblLaba != null) lblLaba.Text = totalLaba.ToString("C0", id);
            if (lbl_Margin != null) lbl_Margin.Text = margin.ToString("0.0") + "%";
        }

        private void HitungEstimasiLive()
        {
            try
            {
                double harga = ParseCurrency(inputHargaJual.Text);
                double qty = ParseCurrency(inputJumlahTerjual.Text);
                double diskon = ParseCurrency(inputDiskon.Text);
                double biayaLain = ParseCurrency(inputBiayaLain.Text);

                double subtotal = harga * qty;
                double ppn = subtotal * 0.10;
                double totalAkhir = (subtotal + ppn + biayaLain) - diskon;

                double hppSatuan = 0;
                if (cbMenu.SelectedValue != null && int.TryParse(cbMenu.SelectedValue.ToString(), out int rid))
                {
                    hppSatuan = HitungHPP(rid);
                }
                double totalHPP = hppSatuan * qty;
                double profitBersih = (subtotal - totalHPP) - diskon;
                double margin = (subtotal > 0) ? (profitBersih / subtotal) * 100 : 0;

                System.Globalization.CultureInfo id = System.Globalization.CultureInfo.GetCultureInfo("id-ID");
                lblTotalHpp.Text = totalHPP.ToString("C0", id);
                lblSubtotal.Text = subtotal.ToString("C0", id);
                lblPPN.Text = ppn.ToString("C0", id);
                lblTotalPendapatan.Text = totalAkhir.ToString("C0", id);
                lblLabaBersih.Text = profitBersih.ToString("C0", id);
                lblMargin.Text = margin.ToString("0.0") + "%";
                guna2GroupBox9.Visible = true;
            }
            catch { }
        }

        // --- HELPER LAINNYA ---
        private double ParseCurrency(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return double.TryParse(text.Replace(".", "").Replace(",", ""), out double result) ? result : 0;
        }

        private double HitungHPP(int recipeId)
        {
            double hpp = 0;
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT COALESCE(hpp, 0) FROM recipes WHERE id = @rid";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@rid", recipeId);
                        object result = cmd.ExecuteScalar();
                        if (result != null) hpp = Convert.ToDouble(result);
                    }
                }
            }
            catch { hpp = 0; }
            return hpp;
        }

        private void LoadComboMenu()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT id, name FROM recipes WHERE user_id = @uid ORDER BY name ASC";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        DataTable dt = new DataTable();
                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd)) da.Fill(dt);
                        cbMenu.DataSource = dt; cbMenu.DisplayMember = "name"; cbMenu.ValueMember = "id"; cbMenu.SelectedIndex = -1;
                    }
                }
            }
            catch { }
        }

        private void cbMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMenu.SelectedValue == null) return;
            try
            {
                if (int.TryParse(cbMenu.SelectedValue.ToString(), out int rid))
                {
                    Koneksi db = new Koneksi();
                    using (NpgsqlConnection conn = db.GetKoneksi())
                    {
                        if (conn.State != ConnectionState.Open) conn.Open();
                        string sql = "SELECT suggested_price FROM recipes WHERE id = @id";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", rid);
                            object res = cmd.ExecuteScalar();
                            if (res != null)
                            {
                                inputHargaJual.Text = Convert.ToDouble(res).ToString("N0");
                                HitungEstimasiLive();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void FormatKolom()
        {
            System.Globalization.CultureInfo id = System.Globalization.CultureInfo.GetCultureInfo("id-ID");
            string[] moneyCols = { "col_HargaJual", "col_subtotal", "col_ppn", "col_total", "col_hpp", "col_profit" };
            foreach (string colName in moneyCols)
            {
                if (dgvDataBahanBaku.Columns.Contains(colName))
                {
                    dgvDataBahanBaku.Columns[colName].DefaultCellStyle.Format = "C0";
                    dgvDataBahanBaku.Columns[colName].DefaultCellStyle.FormatProvider = id;
                }
            }
        }

        // --- FUNGSI KHUSUS: HITUNG TOTAL SEMUA TRANSAKSI (DARI DATABASE) ---
        private void HitungTotalSemuaTransaksi()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Query menghitung jumlah SEMUA data penjualan user ini (Tanpa filter tanggal)
                    string sql = "SELECT COUNT(*) FROM sales WHERE user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);

                        // ExecuteScalar: Mengambil satu nilai angka hasil COUNT
                        long jumlah = Convert.ToInt64(cmd.ExecuteScalar());

                        // Tampilkan ke Label
                        if (lblTotalTransaksi != null)
                        {
                            lblTotalTransaksi.Text = jumlah.ToString() + " Transaksi";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal hitung total transaksi: " + ex.Message);
            }
        }

        private void SetupTableStyle()
        {
            dgvDataBahanBaku.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Light;
            dgvDataBahanBaku.BackgroundColor = Color.White;
            dgvDataBahanBaku.BorderStyle = BorderStyle.None;
            dgvDataBahanBaku.ColumnHeadersHeight = 50;
            dgvDataBahanBaku.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvDataBahanBaku.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvDataBahanBaku.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvDataBahanBaku.RowTemplate.Height = 45;
        }

        private void BersihkanInput()
        {
            cbMenu.SelectedIndex = -1;
            inputHargaJual.Clear(); inputJumlahTerjual.Clear(); inputDiskon.Clear(); inputBiayaLain.Clear();
            dtpTanggalPenjualan.Value = DateTime.Now;
            guna2GroupBox9.Visible = false;
        }

        private void btinputPenjualanPopUp_Click(object sender, EventArgs e) { BersihkanInput(); gbInputPenjualanPopUp.Visible = true; gbInputPenjualanPopUp.BringToFront(); }
        private void btnClosePopUpBahanBaku_Click(object sender, EventArgs e) => gbInputPenjualanPopUp.Visible = false;
        private void btnBatalPopUp_Click(object sender, EventArgs e) => gbInputPenjualanPopUp.Visible = false;
        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e) { }
        private void label43_Click(object sender, EventArgs e) { }
    }
}