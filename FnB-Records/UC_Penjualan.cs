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

        // --- 1. EVENT LOAD ---
        private void UC_Penjualan_Load(object sender, EventArgs e)
        {
            if (currentUserId == 0) Login.GlobalSession.CurrentUserId = 1; // Safety check

            SetupTableStyle(); // Styling tabel agar putih bersih
            LoadComboMenu();   // Isi dropdown menu
            LoadDataPenjualan(); // Isi tabel penjualan
        }

        // --- 2. SETUP TAMPILAN TABEL (PUTIH & BERSIH) ---
        private void SetupTableStyle()
        {
            dgvDataBahanBaku.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Light;
            dgvDataBahanBaku.BackgroundColor = Color.White;
            dgvDataBahanBaku.GridColor = Color.FromArgb(231, 229, 255);
            dgvDataBahanBaku.BorderStyle = BorderStyle.None;
            dgvDataBahanBaku.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

            // Header
            dgvDataBahanBaku.ColumnHeadersHeight = 50;
            dgvDataBahanBaku.ColumnHeadersDefaultCellStyle.BackColor = Color.White;
            dgvDataBahanBaku.ColumnHeadersDefaultCellStyle.ForeColor = Color.Black;
            dgvDataBahanBaku.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvDataBahanBaku.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.White;

            // Baris
            dgvDataBahanBaku.RowTemplate.Height = 45;
            dgvDataBahanBaku.DefaultCellStyle.BackColor = Color.White;
            dgvDataBahanBaku.DefaultCellStyle.ForeColor = Color.FromArgb(70, 70, 70);
            dgvDataBahanBaku.DefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            dgvDataBahanBaku.DefaultCellStyle.SelectionBackColor = Color.FromArgb(231, 229, 255);
            dgvDataBahanBaku.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgvDataBahanBaku.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
        }

        // --- 3. ISI COMBOBOX MENU ---
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

                        cbMenu.DataSource = dt;
                        cbMenu.DisplayMember = "name";
                        cbMenu.ValueMember = "id";
                        cbMenu.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal memuat menu: " + ex.Message); }
        }

        // --- 4. LOAD DATA PENJUALAN ---
        private void LoadDataPenjualan()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Query Ambil Data Penjualan + Nama Menu
                    string sql = @"
                        SELECT s.id, r.name AS nama_menu, s.qty AS jumlah_qty, s.price AS harga_jual,
                               (s.qty * s.price) AS subtotal,
                               s.tax AS ppn,
                               s.total_price AS total_harga,
                               s.profit,
                               (s.total_price - s.profit) AS hpp
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

                        HitungRingkasan(dt); // Update Label Total di bawah
                        FormatKolom();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal memuat data penjualan: " + ex.Message); }
        }

        private void FormatKolom()
        {
            // Format Rupiah untuk kolom angka
            System.Globalization.CultureInfo id = System.Globalization.CultureInfo.GetCultureInfo("id-ID");

            string[] moneyCols = { "col_HargaJual", "col_subtotal", "col_ppn", "col_total", "col_hpp", "col_profit" };
            foreach (string col in moneyCols)
            {
                if (dgvDataBahanBaku.Columns.Contains(col))
                {
                    dgvDataBahanBaku.Columns[col].DefaultCellStyle.Format = "C0";
                    dgvDataBahanBaku.Columns[col].DefaultCellStyle.FormatProvider = id;
                }
            }
        }

        // --- 5. HITUNG RINGKASAN (TOTAL BAWAH) ---
        private void HitungRingkasan(DataTable dt)
        {
            double totalHPP = 0, subtotal = 0, totalPPN = 0, totalPendapatan = 0, labaBersih = 0;

            foreach (DataRow row in dt.Rows)
            {
                totalHPP += Convert.ToDouble(row["hpp"]);
                subtotal += Convert.ToDouble(row["subtotal"]);
                totalPPN += Convert.ToDouble(row["ppn"]);
                totalPendapatan += Convert.ToDouble(row["total_harga"]);
                labaBersih += Convert.ToDouble(row["profit"]);
            }

            double margin = (totalPendapatan > 0) ? (labaBersih / totalPendapatan) * 100 : 0;

            // Update Label
            System.Globalization.CultureInfo id = System.Globalization.CultureInfo.GetCultureInfo("id-ID");
            lblTotalHpp.Text = totalHPP.ToString("C0", id);
            lblSubtotal.Text = subtotal.ToString("C0", id);
            lblPPN.Text = totalPPN.ToString("C0", id);
            lblTotalPendapatan.Text = totalPendapatan.ToString("C0", id);
            lblLabaBersih.Text = labaBersih.ToString("C0", id);
            lblMargin.Text = margin.ToString("0.0") + "%";

            // Tampilkan panel ringkasan
            guna2GroupBox9.Visible = true;
        }

        // --- 6. SAAT MEMILIH MENU DI POPUP (AUTO ISI HARGA) ---
        private void cbMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbMenu.SelectedIndex == -1 || cbMenu.SelectedValue == null) return;

            try
            {
                // Ambil harga jual menu dari database
                if (int.TryParse(cbMenu.SelectedValue.ToString(), out int recipeId))
                {
                    Koneksi db = new Koneksi();
                    using (NpgsqlConnection conn = db.GetKoneksi())
                    {
                        conn.Open();
                        string sql = "SELECT price FROM recipes WHERE id = @id";
                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", recipeId);
                            object result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                inputHargaJual.Text = Convert.ToDouble(result).ToString("N0");
                            }
                        }
                    }
                }
            }
            catch { }
        }

        // --- 7. SIMPAN PENJUALAN ---
        private void btnSimpanPopUp_Click(object sender, EventArgs e)
        {
            if (cbMenu.SelectedValue == null || string.IsNullOrWhiteSpace(inputJumlahTerjual.Text))
            {
                MessageBox.Show("Pilih menu dan masukkan jumlah terjual.", "Warning");
                return;
            }

            try
            {
                // Bersihkan input angka
                double hargaJual = double.Parse(inputHargaJual.Text.Replace(".", "").Replace(",", ""));
                int qty = int.Parse(inputJumlahTerjual.Text.Replace(".", ""));
                double diskon = string.IsNullOrEmpty(inputDiskon.Text) ? 0 : double.Parse(inputDiskon.Text.Replace(".", ""));
                double biayaLain = string.IsNullOrEmpty(inputBiayaLain.Text) ? 0 : double.Parse(inputBiayaLain.Text.Replace(".", ""));
                int recipeId = Convert.ToInt32(cbMenu.SelectedValue);

                // Hitung Keuangan
                double subtotal = hargaJual * qty;
                double ppn = subtotal * 0.10; // PPN 10%
                double total = subtotal + ppn + biayaLain - diskon;

                // Hitung HPP (Modal) untuk menghitung Profit
                double hppSatuan = HitungHPP(recipeId);
                double totalHPP = hppSatuan * qty;
                double profit = (subtotal - totalHPP) - diskon; // Profit bersih (exclude tax & other fees usually)

                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    conn.Open();
                    string sql = @"INSERT INTO sales 
                                (user_id, recipe_id, qty, price, discount, other_fees, tax, revenue, profit, total_price, sale_date, created_at)
                                VALUES 
                                (@uid, @rid, @qty, @price, @disc, @fees, @tax, @rev, @prof, @total, @date, CURRENT_TIMESTAMP)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@rid", recipeId);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@price", hargaJual);
                        cmd.Parameters.AddWithValue("@disc", diskon);
                        cmd.Parameters.AddWithValue("@fees", biayaLain);
                        cmd.Parameters.AddWithValue("@tax", ppn);
                        cmd.Parameters.AddWithValue("@rev", subtotal); // Revenue biasanya adalah Subtotal
                        cmd.Parameters.AddWithValue("@prof", profit);
                        cmd.Parameters.AddWithValue("@total", total);
                        cmd.Parameters.AddWithValue("@date", dtpTanggalPenjualan.Value);

                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Penjualan berhasil disimpan!", "Sukses");
                gbInputPenjualanPopUp.Visible = false;
                BersihkanInput();
                LoadDataPenjualan();
            }
            catch (Exception ex) { MessageBox.Show("Gagal menyimpan: " + ex.Message); }
        }

        // Fungsi Helper Hitung HPP dari Resep
        private double HitungHPP(int recipeId)
        {
            double hpp = 0;
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    conn.Open();
                    // Hitung total harga bahan baku berdasarkan komposisi resep
                    // Asumsi: tabel ingredients punya kolom 'price' per satuan
                    string sql = @"
                        SELECT COALESCE(SUM(ri.amount * (i.price / 1)), 0) 
                        FROM recipe_ingredients ri
                        JOIN ingredients i ON ri.ingredient_id = i.id
                        WHERE ri.recipe_id = @rid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@rid", recipeId);
                        hpp = Convert.ToDouble(cmd.ExecuteScalar());
                    }
                }
            }
            catch { hpp = 0; } // Jika gagal, anggap HPP 0
            return hpp;
        }

        // --- UTILITIES ---
        private void BersihkanInput()
        {
            cbMenu.SelectedIndex = -1;
            inputHargaJual.Clear();
            inputJumlahTerjual.Clear();
            inputDiskon.Clear();
            inputBiayaLain.Clear();
        }

        // Event UI
        private void btinputPenjualanPopUp_Click(object sender, EventArgs e)
        {
            BersihkanInput();
            gbInputPenjualanPopUp.Visible = true;
            gbInputPenjualanPopUp.BringToFront();
        }

        private void btnClosePopUpBahanBaku_Click(object sender, EventArgs e) => gbInputPenjualanPopUp.Visible = false;
        private void btnBatalPopUp_Click(object sender, EventArgs e) => gbInputPenjualanPopUp.Visible = false;
        private void guna2DateTimePicker1_ValueChanged(object sender, EventArgs e) => LoadDataPenjualan(); // Filter tanggal
        private void txtCariRiwayatPenjualan_TextChanged(object sender, EventArgs e) => LoadDataPenjualan(); // Filter pencarian

        // Event kosong dari designer
        private void label43_Click(object sender, EventArgs e) { }
    }
}