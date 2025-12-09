using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
using FnB_Records.Koneksi_DB;

namespace FnB_Records
{
    public partial class UCProduksiOlahan : UserControl
    {
        // --- 1. MODEL DATA ---
        private class RecipeItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Hpp { get; set; }
            public int ServingSize { get; set; }
            public string DisplayText { get; set; }
            public override string ToString() => DisplayText;
        }

        // --- 2. VARIABEL GLOBAL ---
        private int currentUserId => Login.GlobalSession.CurrentUserId;
        private int selectedBatchId = 0;

        public UCProduksiOlahan()
        {
            InitializeComponent();

            // Sembunyikan panel pop-up saat awal
            if (gbTambahBatch != null) gbTambahBatch.Visible = false;
            if (gbDetailBatch != null) gbDetailBatch.Visible = false;

            // Hubungkan Event Handler Secara Manual
            this.Load += UCProduksiOlahan_Load;

            // Tombol Dashboard
            if (btBuatPOBaru != null) btBuatPOBaru.Click += (s, e) => ShowCreatePanel();

            // Tombol di Panel Tambah
            //if (btnSimpan != null) btnSimpan.Click += btnSimpan_Click;
            //if (btnBatal != null) btnBatal.Click += btnBatal_Click;
            //if (btnCloseBuat != null) btnCloseBuat.Click += btnBatal_Click;

            // Tombol di Panel Detail
            if (btnCloseDetail != null) btnCloseDetail.Click += btnCloseDetail_Click;

            // Grid
            if (dgvBatch != null) dgvBatch.CellContentClick += dgvBatch_CellContentClick;
        }

        private void UCProduksiOlahan_Load(object sender, EventArgs e)
        {
            if (currentUserId == 0) Login.GlobalSession.CurrentUserId = 1;

            if (dgvBatch != null)
            {
                dgvBatch.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dgvBatch.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvBatch.AllowUserToAddRows = false;
                dgvBatch.RowTemplate.Height = 50;
                SetupGridColumns();
            }

            LoadDashboardStats();
            LoadBatchData();
            LoadRecipeCombo();
        }

        // ==========================================
        // 1. DASHBOARD STATS (Disesuaikan Labelnya)
        // ==========================================

        private void LoadDashboardStats()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = @"
                        SELECT 
                            COUNT(*) as total, 
                            COUNT(CASE WHEN status='selesai' THEN 1 END) as selesai, 
                            COUNT(CASE WHEN status='sedang_produksi' THEN 1 END) as proses, 
                            COALESCE(SUM(CASE WHEN status='selesai' THEN produced_qty ELSE 0 END), 0) as qty 
                        FROM production_batches 
                        WHERE user_id=@uid";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                // ✅ Update Nama Label Sesuai Request
                                if (lblTotalBatch != null) lblTotalBatch.Text = r["total"].ToString();
                                if (lblBatchSelesai != null) lblBatchSelesai.Text = r["selesai"].ToString();
                                if (lblSedangProduksi != null) lblSedangProduksi.Text = r["proses"].ToString();
                                if (lblTotalProduksi != null) lblTotalProduksi.Text = r["qty"].ToString();
                            }
                        }
                    }
                }
            }
            catch { }
        }

        // ==========================================
        // 2. SETUP GRID & LOAD DATA TABEL
        // ==========================================

        private void SetupGridColumns()
        {
            dgvBatch.Columns.Clear();

            // Kolom ID (Hidden)
            dgvBatch.Columns.Add("id", "ID");
            dgvBatch.Columns["id"].Visible = false;

            dgvBatch.Columns.Add("batch_number", "No. Batch");
            dgvBatch.Columns.Add("recipe_name", "Resep");
            dgvBatch.Columns.Add("target_qty", "Target");
            dgvBatch.Columns.Add("start_date", "Tgl Mulai");
            dgvBatch.Columns.Add("status_text", "Status");

            // Kolom Action 1: Proses
            DataGridViewImageColumn colProses = new DataGridViewImageColumn();
            colProses.Name = "btnActionMain";
            colProses.HeaderText = "Proses";
            colProses.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colProses.Width = 50;
            dgvBatch.Columns.Add(colProses);

            // Kolom Action 2: Detail
            DataGridViewImageColumn colDetail = new DataGridViewImageColumn();
            colDetail.Name = "btnActionView";
            colDetail.HeaderText = "Detail";
            colDetail.ImageLayout = DataGridViewImageCellLayout.Zoom;
            colDetail.Width = 50;
            dgvBatch.Columns.Add(colDetail);
        }

        private void LoadBatchData()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = @"
                        SELECT pb.id, pb.batch_number, r.name as recipe_name, 
                               pb.target_qty, pb.start_date, pb.status 
                        FROM production_batches pb
                        JOIN recipes r ON r.id = pb.recipe_id
                        WHERE pb.user_id = @uid ORDER BY pb.created_at DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        dgvBatch.Rows.Clear();

                        using (NpgsqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                int idx = dgvBatch.Rows.Add();
                                var row = dgvBatch.Rows[idx];

                                row.Cells["id"].Value = r["id"];
                                row.Cells["batch_number"].Value = r["batch_number"];
                                row.Cells["recipe_name"].Value = r["recipe_name"];
                                row.Cells["target_qty"].Value = r["target_qty"] + " porsi";

                                var dateObj = r["start_date"];
                                row.Cells["start_date"].Value = (dateObj is DateOnly d ? d.ToDateTime(TimeOnly.MinValue) : Convert.ToDateTime(dateObj)).ToString("dd MMM yyyy");

                                string status = r["status"].ToString();

                                if (status == "perencanaan")
                                {
                                    row.Cells["status_text"].Value = "🕒 Perencanaan";
                                    row.Cells["btnActionMain"].Value = DrawIcon("play", Color.Green);
                                    row.DefaultCellStyle.BackColor = Color.White;
                                }
                                else if (status == "sedang_produksi")
                                {
                                    row.Cells["status_text"].Value = "⏳ Produksi";
                                    row.Cells["btnActionMain"].Value = DrawIcon("check", Color.Orange);
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 225);
                                }
                                else
                                {
                                    row.Cells["status_text"].Value = "✅ Selesai";
                                    row.Cells["btnActionMain"].Value = new Bitmap(1, 1);
                                    row.Cells["status_text"].Style.ForeColor = Color.Green;
                                }
                                row.Cells["btnActionView"].Value = DrawIcon("eye", Color.Gray);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal load data: " + ex.Message); }
        }

        private void LoadRecipeCombo()
        {
            try
            {
                var list = new List<RecipeItem>();
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT id, name, hpp, serving_size FROM recipes WHERE user_id=@uid ORDER BY name";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read()) list.Add(new RecipeItem { Id = r.GetInt32(0), Name = r.GetString(1), Hpp = r.GetDouble(2), ServingSize = r.GetInt32(3), DisplayText = r.GetString(1) });
                        }
                    }
                }
                if (cbResep != null)
                {
                    cbResep.DataSource = new BindingList<RecipeItem>(list);
                    cbResep.DisplayMember = "DisplayText";
                    cbResep.ValueMember = "Id";
                    cbResep.SelectedIndex = -1;
                }
            }
            catch { }
        }

        // ==========================================
        // 3. LOGIKA TAMBAH BATCH
        // ==========================================

        private void ShowCreatePanel()
        {
            ClearForm();
            if (gbTambahBatch != null) { gbTambahBatch.Visible = true; gbTambahBatch.BringToFront(); }
            if (gbDetailBatch != null) gbDetailBatch.Visible = false;
        }

        private void ClearForm()
        {
            if (cbResep != null) cbResep.SelectedIndex = -1;
            if (txtTargetProduksi != null) txtTargetProduksi.Clear();
            if (txtInputCatatan != null) txtInputCatatan.Clear();
            if (dtpTanggalMulai != null) dtpTanggalMulai.Value = DateTime.Now;
        }

        private void btnBatal_Click(object sender, EventArgs e)
        {
            if (gbTambahBatch != null) gbTambahBatch.Visible = false;
            ClearForm();
        }

        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (cbResep.SelectedValue == null || string.IsNullOrWhiteSpace(txtTargetProduksi.Text))
            {
                MessageBox.Show("Data tidak lengkap!", "Warning"); return;
            }

            int recipeId = (int)cbResep.SelectedValue;
            if (!int.TryParse(txtTargetProduksi.Text, out int qty) || qty <= 0) return;

            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Cek Bahan
                    string check = "SELECT COUNT(*) FROM recipe_ingredients WHERE recipe_id=@rid";
                    using (var cmd = new NpgsqlCommand(check, conn))
                    {
                        cmd.Parameters.AddWithValue("@rid", recipeId);
                        if ((long)cmd.ExecuteScalar() == 0) { MessageBox.Show("Resep ini tidak punya bahan!", "Error"); return; }
                    }

                    // Generate Batch No
                    string dateCode = DateTime.Now.ToString("yyyyMMdd");
                    string batchNo = $"PROD-{dateCode}-001";
                    using (var cmd = new NpgsqlCommand("SELECT batch_number FROM production_batches WHERE batch_number LIKE @pat ORDER BY id DESC LIMIT 1", conn))
                    {
                        cmd.Parameters.AddWithValue("@pat", $"PROD-{dateCode}-%");
                        var res = cmd.ExecuteScalar();
                        if (res != null)
                        {
                            string last = res.ToString();
                            if (int.TryParse(last.Substring(last.Length - 3), out int seq))
                                batchNo = $"PROD-{dateCode}-{(seq + 1):D3}";
                        }
                    }

                    // Simpan
                    // Perhatikan bagian 'sedang_produksi'
                    string sql = @"INSERT INTO production_batches (user_id, batch_number, recipe_id, target_qty, start_date, status, notes, created_at)
               VALUES (@uid, @batch, @rid, @qty, @date, 'sedang_produksi', @note, NOW())";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@batch", batchNo);
                        cmd.Parameters.AddWithValue("@rid", recipeId);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@date", dtpTanggalMulai.Value.Date);
                        cmd.Parameters.AddWithValue("@note", txtInputCatatan.Text ?? (object)DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                MessageBox.Show("Batch Dibuat!", "Sukses");
                if (gbTambahBatch != null) gbTambahBatch.Visible = false;
                LoadBatchData();
                LoadDashboardStats();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        // ==========================================
        // 4. LOGIKA GRID KLIK & DETAIL
        // ==========================================

        private void dgvBatch_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            string colName = dgvBatch.Columns[e.ColumnIndex].Name;
            int batchId = Convert.ToInt32(dgvBatch.Rows[e.RowIndex].Cells["id"].Value);
            string statusText = dgvBatch.Rows[e.RowIndex].Cells["status_text"].Value.ToString();

            if (colName == "btnActionMain")
            {
                if (statusText.Contains("Perencanaan"))
                {
                    UbahStatus(batchId, "sedang_produksi");
                }
                else if (statusText.Contains("Produksi"))
                {
                    if (MessageBox.Show("Selesaikan? Stok akan berkurang.", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        FinishBatch(batchId);
                }
            }
            else if (colName == "btnActionView")
            {
                ShowDetail(batchId);
            }
        }

        private void UbahStatus(int id, string status)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var cmd = new NpgsqlCommand("UPDATE production_batches SET status=@s WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@s", status);
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadBatchData();
                LoadDashboardStats();
            }
            catch { }
        }

        private void FinishBatch(int batchId)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            int rid = 0, qty = 0;
                            using (var cmd = new NpgsqlCommand("SELECT recipe_id, target_qty FROM production_batches WHERE id=@id", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@id", batchId);
                                using (var r = cmd.ExecuteReader()) { if (r.Read()) { rid = r.GetInt32(0); qty = r.GetInt32(1); } }
                            }

                            var listBahan = new List<(int id, double used)>();
                            string sqlBahan = "SELECT ingredient_id, amount FROM recipe_ingredients WHERE recipe_id=@rid";
                            using (var cmd = new NpgsqlCommand(sqlBahan, conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@rid", rid);
                                using (var r = cmd.ExecuteReader()) { while (r.Read()) listBahan.Add((r.GetInt32(0), r.GetDouble(1) * qty)); }
                            }

                            foreach (var item in listBahan)
                            {
                                using (var cmd = new NpgsqlCommand("UPDATE ingredients SET stock = stock - @u WHERE id=@id", conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@u", item.used);
                                    cmd.Parameters.AddWithValue("@id", item.id);
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            // Ganti 'completed' menjadi 'selesai'
                            using (var cmd = new NpgsqlCommand("UPDATE production_batches SET status='selesai', produced_qty=@qty, completed_date=NOW() WHERE id=@id", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@qty", qty);
                                cmd.Parameters.AddWithValue("@id", batchId);
                                cmd.ExecuteNonQuery();
                            }

                            trans.Commit();
                            MessageBox.Show("Batch Selesai!", "Sukses");
                            LoadBatchData();
                            LoadDashboardStats();
                        }
                        catch { trans.Rollback(); throw; }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error finish: " + ex.Message); }
        }

        private void ShowDetail(int batchId)
        {
            selectedBatchId = batchId;
            try
            {
                Koneksi db = new Koneksi();
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = @"SELECT pb.batch_number, r.name, pb.target_qty, pb.produced_qty, 
                                          pb.start_date, pb.completed_date, pb.status, pb.notes
                                   FROM production_batches pb 
                                   JOIN recipes r ON r.id = pb.recipe_id 
                                   WHERE pb.id = @id";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", batchId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                if (lblNoBatch != null) lblNoBatch.Text = r["batch_number"].ToString();
                                if (lblResep != null) lblResep.Text = r["name"].ToString();
                                if (lblTargetProduksi != null) lblTargetProduksi.Text = r["target_qty"].ToString() + " porsi";

                                if (lblSudahDiproduksi != null)
                                    lblSudahDiproduksi.Text = (r["produced_qty"] != DBNull.Value ? r["produced_qty"].ToString() : "0") + " porsi";

                                if (lblCatatan != null) lblCatatan.Text = r["notes"].ToString();

                                var start = r["start_date"];
                                if (lblTanggalMulai != null)
                                    lblTanggalMulai.Text = (start is DateOnly d ? d.ToDateTime(TimeOnly.MinValue) : Convert.ToDateTime(start)).ToString("dd MMM yyyy");

                                var end = r["completed_date"];
                                if (lblTanggalSelesai != null)
                                {
                                    if (end != DBNull.Value)
                                        lblTanggalSelesai.Text = (end is DateOnly d2 ? d2.ToDateTime(TimeOnly.MinValue) : Convert.ToDateTime(end)).ToString("dd MMM yyyy");
                                    else
                                        lblTanggalSelesai.Text = "-";
                                }

                                string status = r["status"].ToString();
                                if (lblStatus != null)
                                {
                                    if (status == "sedang_produksi") { lblStatus.Text = "⏳ Produksi"; lblStatus.ForeColor = Color.Orange; }
                                    else if (status == "selesai") { lblStatus.Text = "✅ Selesai"; lblStatus.ForeColor = Color.Green; }
                                    else { lblStatus.Text = "🕒 Perencanaan"; lblStatus.ForeColor = Color.Gray; }
                                }
                            }
                        }
                    }
                    LoadIngredientsToCard(conn, batchId);
                }

                if (gbDetailBatch != null)
                {
                    gbDetailBatch.Visible = true;
                    gbDetailBatch.BringToFront();
                }
            }
            catch (Exception ex) { MessageBox.Show("Detail Error: " + ex.Message); }
        }

        private void LoadIngredientsToCard(NpgsqlConnection conn, int batchId)
        {
            if (flpBahan == null) return;
            flpBahan.Controls.Clear();

            string sql = @"SELECT i.name, i.unit, (ri.amount * pb.target_qty) as total 
                           FROM production_batches pb 
                           JOIN recipe_ingredients ri ON ri.recipe_id = pb.recipe_id 
                           JOIN ingredients i ON i.id = ri.ingredient_id 
                           WHERE pb.id = @id";

            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", batchId);
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        Panel p = new Panel { Width = flpBahan.Width - 25, Height = 40, BackColor = Color.FromArgb(250, 240, 230), Margin = new Padding(0, 0, 0, 5) };
                        Label lName = new Label { Text = r.GetString(0), AutoSize = false, Width = 200, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Left, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                        Label lQty = new Label { Text = $"{Convert.ToDouble(r["total"]):N0} {r.GetString(1)}", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Right };

                        p.Controls.Add(lQty);
                        p.Controls.Add(lName);
                        flpBahan.Controls.Add(p);
                    }
                }
            }
        }

        private void btnCloseDetail_Click(object sender, EventArgs e)
        {
            if (gbDetailBatch != null) gbDetailBatch.Visible = false;
        }

        // Helper Icon
        private Bitmap DrawIcon(string type, Color color)
        {
            Bitmap bmp = new Bitmap(24, 24);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                Pen p = new Pen(color, 2);
                if (type == "play") g.FillPolygon(new SolidBrush(color), new Point[] { new Point(8, 5), new Point(18, 12), new Point(8, 19) });
                else if (type == "check") g.DrawLines(p, new Point[] { new Point(5, 12), new Point(10, 18), new Point(20, 6) });
                else if (type == "eye") { g.DrawEllipse(p, 4, 8, 16, 8); g.FillEllipse(new SolidBrush(color), 10, 10, 4, 4); }
            }
            return bmp;
        }

        // --- Event Kosong untuk Menghindari Error Designer ---
        private void cbResep_SelectedIndexChanged(object sender, EventArgs e) { }
        private void txtTargetProduksi_TextChanged(object sender, EventArgs e) { }
        private void dtpTanggalMulai_ValueChanged(object sender, EventArgs e) { }
        private void txtInputCatatan_TextChanged(object sender, EventArgs e) { }
        private void lblTotalBatch_Click(object sender, EventArgs e) { }
        private void lblBatchSelesai_Click(object sender, EventArgs e) { }
        private void lblSedangProduksi_Click(object sender, EventArgs e) { }
        private void lblTotalProduksi_Click(object sender, EventArgs e) { }
    }
}