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
        private int? editingSalesId = null; // Penanda Edit

        public UC_Penjualan()
        {
            InitializeComponent();
            gbInputPenjualanPopUp.BackColor = Color.White;
        }

        // --- 1. SETUP & EVENTS ---
        private void UC_Penjualan_Load(object sender, EventArgs e)
        {
            if (currentUserId == 0) Login.GlobalSession.CurrentUserId = 1;

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

            // Event Klik Grid (Edit/Hapus)
            dgvDataBahanBaku.CellContentClick += dgvDataBahanBaku_CellContentClick;
        }

        // --- 2. LOAD DATA ---
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
                            s.total_price AS total_harga, 
                            s.profit AS profit, 
                            s.total_hpp AS hpp, 
                            s.tax AS ppn
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

        // --- 3. SIMPAN PENJUALAN (INSERT / UPDATE) ---
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

                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            // A. Jika EDIT: Kembalikan Stok Lama Dulu
                            if (editingSalesId != null)
                            {
                                RestoreStock(editingSalesId.Value, conn, trans);
                                // Hapus data lama agar diganti baru (simpel update)
                                new NpgsqlCommand($"DELETE FROM sales WHERE id={editingSalesId}", conn, trans).ExecuteNonQuery();
                            }

                            // B. Simpan Data Baru
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

                            // C. Kurangi Stok Baru
                            ReduceStock(recipeId, qty, conn, trans);

                            trans.Commit();
                            MessageBox.Show(editingSalesId == null ? "Penjualan disimpan!" : "Penjualan diperbarui!", "Sukses");

                            gbInputPenjualanPopUp.Visible = false;
                            BersihkanInput();
                            LoadDataPenjualan();
                            HitungTotalSemuaTransaksi();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal menyimpan: " + ex.Message); }
        }

        // --- 4. KLIK GRID (EDIT & HAPUS) ---
        // --- PERBAIKAN: KLIK GRID (AMBIL ID DARI SUMBER DATA LANGSUNG) ---
        private void dgvDataBahanBaku_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // 1. Cek validasi baris (bukan header)
            if (e.RowIndex < 0) return;

            // 2. Ambil baris data ASLI di balik layar (Bukan Visual Cell)
            // DataBoundItem adalah representasi baris dari DataTable
            if (dgvDataBahanBaku.Rows[e.RowIndex].DataBoundItem is DataRowView row)
            {
                // 3. Ambil ID langsung dari data mentah (Aman meskipun kolom id di-hide)
                int salesId = Convert.ToInt32(row["id"]);

                // Ambil nama kolom tombol yang diklik
                string colName = dgvDataBahanBaku.Columns[e.ColumnIndex].Name;

                // --- LOGIKA HAPUS ---
                if (colName == "Hapus")
                {
                    if (MessageBox.Show("Hapus transaksi ini? Stok akan dikembalikan.", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                    {
                        HapusTransaksi(salesId);
                    }
                }
                // --- LOGIKA EDIT ---
                else if (colName == "Edit")
                {
                    LoadDataForEdit(salesId);
                }
            }
        }

        private void HapusTransaksi(int id)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            RestoreStock(id, conn, trans); // Kembalikan stok
                            new NpgsqlCommand($"DELETE FROM sales WHERE id={id}", conn, trans).ExecuteNonQuery();

                            trans.Commit();
                            MessageBox.Show("Transaksi dihapus.");
                            LoadDataPenjualan();
                            HitungTotalSemuaTransaksi();
                        }
                        catch { trans.Rollback(); throw; }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal hapus: " + ex.Message); }
        }

        private void LoadDataForEdit(int id)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT * FROM sales WHERE id = @id";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                editingSalesId = id;
                                cbMenu.SelectedValue = r["recipe_id"];
                                inputJumlahTerjual.Text = r["qty"].ToString();
                                inputHargaJual.Text = Convert.ToDouble(r["selling_price"]).ToString("N0");
                                inputDiskon.Text = Convert.ToDouble(r["discount"]).ToString("N0");
                                inputBiayaLain.Text = Convert.ToDouble(r["other_fees"]).ToString("N0");
                                dtpTanggalPenjualan.Value = Convert.ToDateTime(r["sale_date"]);

                                label16.Text = "Edit Penjualan"; // Ubah Judul Popup
                                btnSimpanPopUp.Text = "Update";
                                gbInputPenjualanPopUp.Visible = true;
                                gbInputPenjualanPopUp.BringToFront();
                                HitungEstimasiLive();
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load edit: " + ex.Message); }
        }

        // --- 5. LOGIKA STOK (KURANGI & KEMBALIKAN) ---

        // Fungsi mengurangi stok saat jual
        private void ReduceStock(int recipeId, int qty, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            string sql = "SELECT ingredient_id, amount FROM recipe_ingredients WHERE recipe_id=@rid";
            using (var cmd = new NpgsqlCommand(sql, conn, trans))
            {
                cmd.Parameters.AddWithValue("@rid", recipeId);
                using (var r = cmd.ExecuteReader())
                {
                    var list = new System.Collections.Generic.List<(int, double)>();
                    while (r.Read()) list.Add((r.GetInt32(0), r.GetDouble(1)));
                    r.Close();

                    foreach (var item in list)
                    {
                        double used = item.Item2 * qty;
                        new NpgsqlCommand($"UPDATE ingredients SET stock = stock - {used} WHERE id={item.Item1}", conn, trans).ExecuteNonQuery();
                    }
                }
            }
        }

        // Fungsi mengembalikan stok saat hapus/edit
        private void RestoreStock(int salesId, NpgsqlConnection conn, NpgsqlTransaction trans)
        {
            // Ambil data resep & qty dari penjualan lama
            int rid = 0, qty = 0;
            using (var cmd = new NpgsqlCommand($"SELECT recipe_id, qty FROM sales WHERE id={salesId}", conn, trans))
            using (var r = cmd.ExecuteReader())
            {
                if (r.Read()) { rid = r.GetInt32(0); qty = r.GetInt32(1); }
            }

            if (rid > 0 && qty > 0)
            {
                // Ambil bahan dari resep tersebut
                string sql = "SELECT ingredient_id, amount FROM recipe_ingredients WHERE recipe_id=@rid";
                using (var cmd = new NpgsqlCommand(sql, conn, trans))
                {
                    cmd.Parameters.AddWithValue("@rid", rid);
                    using (var r = cmd.ExecuteReader())
                    {
                        var list = new System.Collections.Generic.List<(int, double)>();
                        while (r.Read()) list.Add((r.GetInt32(0), r.GetDouble(1)));
                        r.Close();

                        // Kembalikan stok (stock + used)
                        foreach (var item in list)
                        {
                            double used = item.Item2 * qty;
                            new NpgsqlCommand($"UPDATE ingredients SET stock = stock + {used} WHERE id={item.Item1}", conn, trans).ExecuteNonQuery();
                        }
                    }
                }
            }
        }

        // --- UTILS ---
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

        private void HitungTotalSemuaTransaksi()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT COUNT(*) FROM sales WHERE user_id = @uid", conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        long jumlah = Convert.ToInt64(cmd.ExecuteScalar());
                        if (lblTotalTransaksi != null) lblTotalTransaksi.Text = jumlah.ToString() + " Transaksi";
                    }
                }
            }
            catch { }
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

        private double ParseCurrency(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return double.TryParse(text.Replace(".", "").Replace(",", ""), out double result) ? result : 0;
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
            editingSalesId = null;
            label16.Text = "Input Penjualan Baru";
            btnSimpanPopUp.Text = "Simpan";

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