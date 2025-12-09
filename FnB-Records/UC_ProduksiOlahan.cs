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
    public partial class UC_ProduksiOlahan : UserControl
    {
        // --- CLASS MODEL (JANGAN DIUBAH) ---
        private class RecipeItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Hpp { get; set; }
            public double SuggestedPrice { get; set; }
            public int ServingSize { get; set; }
            public string DisplayText { get; set; }

            public override string ToString() => DisplayText;
        }

        private int selectedBatchId = 0;
        private int currentUserId => Login.GlobalSession.CurrentUserId;

        public UC_ProduksiOlahan()
        {
            InitializeComponent();

            // Inisialisasi awal UI
            if (gbTambahBatch != null) gbTambahBatch.Visible = false;
            if (gbDetailBatch != null) gbDetailBatch.Visible = false;
        }

        // =======================================================================
        // BAGIAN 1: EVENT HANDLER ASLI (JANGAN DIHAPUS/DIUBAH NAMANYA)
        // =======================================================================

        // Ini adalah Event Load yang terhubung di Designer Anda
        private void UC_ProduksiOlahan_Load_1(object sender, EventArgs e)
        {
            if (currentUserId == 0) Login.GlobalSession.CurrentUserId = 1;

            if (dgvBatch != null)
            {
                dgvBatch.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
                dgvBatch.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvBatch.AllowUserToAddRows = false;
                dgvBatch.RowTemplate.Height = 45;
            }

            LoadDashboardStats();
            LoadBatchData();
            LoadRecipeCombo();
        }

        // Ini tombol "Buat Batch Baru"
        private void btBuatPOBaru_Click(object sender, EventArgs e)
        {
            gbTambahBatch.Visible = true;
            gbTambahBatch.BringToFront();
            gbDetailBatch.Visible = false;
            ClearForm();
        }

        // Ini tombol Simpan di dalam groupbox tambah
        private void btnsimpan_Click(object sender, EventArgs e)
        {
            SimpanBatchBaru();
        }

        // Ini tombol Batal (User bilang ini btnbataledit)
        private void btnBatalEdit_Click(object sender, EventArgs e)
        {
            gbTambahBatch.Visible = false;
            ClearForm();
        }

        // Ini tombol Close di Detail (User bilang gunabutton2)
        private void guna2Button2_Click(object sender, EventArgs e)
        {
            gbDetailBatch.Visible = false;
        }

        // Ini event grid ketika diklik (untuk tombol Lihat Detail)
        private void dgvBatch_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && (dgvBatch.Columns[e.ColumnIndex].Name == "btnDetail" || dgvBatch.Columns[e.ColumnIndex].Name == "Aksi"))
            {
                if (dgvBatch.Rows[e.RowIndex].Cells["id"].Value != DBNull.Value)
                {
                    int id = Convert.ToInt32(dgvBatch.Rows[e.RowIndex].Cells["id"].Value);
                    ShowBatchDetail(id);
                }
            }
        }

        // Ini event ketika Combobox Resep dipilih (User bilang cbresep_SelectedIndexChanged_1)
        private void cbresep_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // Logika validasi resep saat dipilih
            if (cbresep.SelectedValue != null && cbresep.SelectedValue is int)
            {
                int rid = (int)cbresep.SelectedValue;
                // Bisa ditambahkan logika untuk menampilkan estimasi bahan jika perlu
            }
        }

        // --- FUNGSI KOSONG (JANGAN DIHAPUS) ---
        // Fungsi-fungsi ini ada di Designer Anda. Jika dihapus, error merah muncul.
        private void txtjumlahproduksi_TextChanged(object sender, EventArgs e) { }
        private void dttanggalmulai_ValueChanged(object sender, EventArgs e) { }
        private void txtInputcatatan_TextChanged(object sender, EventArgs e) { }
        private void btnEditCabang_Click(object sender, EventArgs e) { } // Mungkin sisa tombol lama
        private void lbltotalbatch_Click(object sender, EventArgs e) { }
        private void lblbatchselesai_Click(object sender, EventArgs e) { }
        private void lblsedangproduksi_Click(object sender, EventArgs e) { }
        private void lbltotalproduksi_Click(object sender, EventArgs e) { }
        private void cbresep_SelectedIndexChanged(object sender, EventArgs e) { }
        private void UC_ProduksiOlahan_Load(object sender, EventArgs e) { } // Load versi lama
        private void txtjumlahproduksi_TextChanged_1(object sender, EventArgs e) { }
        private void dttanggalmulai_ValueChanged_1(object sender, EventArgs e) { }
        private void gbEditCabang_Click(object sender, EventArgs e) { } // Sisa groupbox lama
        private void guna2GroupBox6_Click(object sender, EventArgs e) { }
        private void guna2GroupBox3_Click(object sender, EventArgs e) { }
        private void gbDetailBatch_Click(object sender, EventArgs e) { }
        private void guna2GroupBox1_Click(object sender, EventArgs e) { }

        // Tombol Close tambahan (guna2button4) yang Anda sebutkan
        private void guna2Button4_Click(object sender, EventArgs e)
        {
            gbTambahBatch.Visible = false;
            ClearForm();
        }

        // =======================================================================
        // BAGIAN 2: LOGIKA UTAMA (LOAD, SAVE, DETAIL)
        // =======================================================================

        private void LoadDashboardStats()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = @"
                        SELECT 
                            COUNT(*) as total,
                            COUNT(CASE WHEN status = 'selesai' THEN 1 END) as selesai,
                            COUNT(CASE WHEN status = 'sedang_produksi' THEN 1 END) as proses,
                            COALESCE(SUM(CASE WHEN status = 'selesai' THEN produced_qty ELSE 0 END), 0) as total_qty
                        FROM production_batches
                        WHERE user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        using (NpgsqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                lbltotalbatch.Text = r["total"].ToString();
                                lblbatchselesai.Text = r["selesai"].ToString();
                                lblsedangproduksi.Text = r["proses"].ToString();
                                lbltotalproduksi.Text = r["total_qty"].ToString() + " Unit";
                            }
                        }
                    }
                }
            }
            catch { }
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
                        SELECT pb.id, pb.batch_number, r.name as recipe_name, pb.target_qty, pb.start_date, pb.status
                        FROM production_batches pb
                        JOIN recipes r ON r.id = pb.recipe_id
                        WHERE pb.user_id = @uid ORDER BY pb.created_at DESC";

                    using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(sql, conn))
                    {
                        da.SelectCommand.Parameters.AddWithValue("@uid", currentUserId);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        dgvBatch.DataSource = dt;
                        FormatGrid();
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error load data: " + ex.Message); }
        }

        private void FormatGrid()
        {
            if (dgvBatch.Columns.Contains("id")) dgvBatch.Columns["id"].Visible = false;

            if (dgvBatch.Columns.Contains("batch_number")) dgvBatch.Columns["batch_number"].HeaderText = "No. Batch";
            if (dgvBatch.Columns.Contains("recipe_name")) dgvBatch.Columns["recipe_name"].HeaderText = "Resep";
            if (dgvBatch.Columns.Contains("target_qty")) dgvBatch.Columns["target_qty"].HeaderText = "Target";
            if (dgvBatch.Columns.Contains("start_date")) dgvBatch.Columns["start_date"].HeaderText = "Tgl Mulai";
            if (dgvBatch.Columns.Contains("status")) dgvBatch.Columns["status"].HeaderText = "Status";

            if (!dgvBatch.Columns.Contains("btnDetail"))
            {
                DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                btn.Name = "btnDetail";
                btn.HeaderText = "Aksi";
                btn.Text = "Lihat";
                btn.UseColumnTextForButtonValue = true;
                dgvBatch.Columns.Add(btn);
            }
        }

        private void LoadRecipeCombo()
        {
            try
            {
                var list = new List<RecipeItem>();
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string sql = "SELECT id, name, hpp, serving_size FROM recipes WHERE user_id = @uid ORDER BY name";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        using (NpgsqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                list.Add(new RecipeItem
                                {
                                    Id = r.GetInt32(0),
                                    Name = r.GetString(1),
                                    Hpp = r.GetDouble(2),
                                    ServingSize = r.GetInt32(3),
                                    DisplayText = r.GetString(1)
                                });
                            }
                        }
                    }
                }
                cbresep.DataSource = new BindingList<RecipeItem>(list);
                cbresep.DisplayMember = "DisplayText";
                cbresep.ValueMember = "Id";
                cbresep.SelectedIndex = -1;
            }
            catch { }
        }

        private void SimpanBatchBaru()
        {
            if (cbresep.SelectedValue == null || string.IsNullOrWhiteSpace(txtTargetProduksi.Text))
            {
                MessageBox.Show("Lengkapi data Resep dan Target Produksi!", "Peringatan");
                return;
            }

            int recipeId = (int)cbresep.SelectedValue;
            if (!int.TryParse(txtTargetProduksi.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("Target produksi harus angka valid!", "Error");
                return;
            }

            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // 1. Cek Resep Punya Bahan
                    string checkSql = "SELECT COUNT(*) FROM recipe_ingredients WHERE recipe_id=@rid";
                    using (var cmd = new NpgsqlCommand(checkSql, conn))
                    {
                        cmd.Parameters.AddWithValue("@rid", recipeId);
                        if ((long)cmd.ExecuteScalar() == 0)
                        {
                            MessageBox.Show("Resep ini belum ada bahannya! Tambahkan di menu Resep.", "Error");
                            return;
                        }
                    }

                    // 2. Generate No Batch
                    string dateCode = DateTime.Now.ToString("yyyyMMdd");
                    string batchNo = $"PROD-{dateCode}-001";
                    string sqlCheck = "SELECT batch_number FROM production_batches WHERE batch_number LIKE @pat ORDER BY id DESC LIMIT 1";
                    using (var cmdChk = new NpgsqlCommand(sqlCheck, conn))
                    {
                        cmdChk.Parameters.AddWithValue("@pat", $"PROD-{dateCode}-%");
                        var result = cmdChk.ExecuteScalar();
                        if (result != null)
                        {
                            string last = result.ToString();
                            string seqStr = last.Substring(last.Length - 3);
                            if (int.TryParse(seqStr, out int seq))
                                batchNo = $"PROD-{dateCode}-{(seq + 1):D3}";
                        }
                    }

                    // 3. Insert
                    string sqlInsert = @"
                        INSERT INTO production_batches 
                        (user_id, batch_number, recipe_id, target_qty, start_date, status, notes, created_at)
                        VALUES (@uid, @batch, @rid, @qty, @date, 'sedang_produksi', @note, NOW())";

                    using (var cmd = new NpgsqlCommand(sqlInsert, conn))
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

                MessageBox.Show("Batch Berhasil Dibuat!", "Sukses");
                gbTambahBatch.Visible = false;
                ClearForm();
                LoadBatchData();
                LoadDashboardStats();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal simpan: " + ex.Message);
            }
        }

        private void ShowBatchDetail(int batchId)
        {
            selectedBatchId = batchId;
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = @"
                        SELECT pb.batch_number, r.name, pb.target_qty, pb.produced_qty, 
                               pb.start_date, pb.completed_date, pb.status, pb.notes
                        FROM production_batches pb
                        JOIN recipes r ON r.id = pb.recipe_id
                        WHERE pb.id = @id";

                    string status = "";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", batchId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                lblNoBatch.Text = r["batch_number"].ToString();
                                lblTargetProduksi.Text = r["target_qty"].ToString() + " porsi";
                                lblSudahDiproduksi.Text = (r["produced_qty"] != DBNull.Value ? r["produced_qty"].ToString() : "0") + " porsi";

                                // FIX: DateOnly
                                var startObj = r["start_date"];
                                lblTanggalMulai.Text = (startObj is DateOnly d ? d.ToDateTime(TimeOnly.MinValue) : Convert.ToDateTime(startObj)).ToString("dd MMM yyyy");

                                var endObj = r["completed_date"];
                                if (endObj != DBNull.Value)
                                    lblTanggalSelesai.Text = (endObj is DateOnly d2 ? d2.ToDateTime(TimeOnly.MinValue) : Convert.ToDateTime(endObj)).ToString("dd MMM yyyy");
                                else
                                    lblTanggalSelesai.Text = "-";

                                status = r["status"].ToString();
                                lblStatus.Text = status == "selesai" ? "✅ Selesai" : "⏳ Sedang Produksi";
                                lblCatatan.Text = r["notes"].ToString();
                            }
                        }
                    }

                    LoadIngredientsToCard(conn, batchId);
                }

                gbDetailBatch.Visible = true;
                gbDetailBatch.BringToFront();
            }
            catch (Exception ex) { MessageBox.Show("Gagal load detail: " + ex.Message); }
        }

        private void LoadIngredientsToCard(NpgsqlConnection conn, int batchId)
        {
            flpBahan.Controls.Clear();

            string sql = @"
                SELECT i.name, i.unit, (ri.amount * pb.target_qty) as total_butuh
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
                        string nama = r["name"].ToString();
                        string unit = r["unit"].ToString();
                        double butuh = Convert.ToDouble(r["total_butuh"]);

                        // Buat Card Bahan (Orange Muda)
                        Panel pnl = new Panel();
                        pnl.Width = flpBahan.Width - 25;
                        pnl.Height = 40;
                        pnl.BackColor = Color.FromArgb(250, 240, 230); // Linen
                        pnl.Margin = new Padding(0, 0, 0, 5);

                        Label lblName = new Label { Text = nama, AutoSize = false, Width = 200, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Left, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
                        Label lblQty = new Label { Text = $"{butuh:N0} {unit}", TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Right, Font = new Font("Segoe UI", 9) };

                        pnl.Controls.Add(lblQty);
                        pnl.Controls.Add(lblName);
                        flpBahan.Controls.Add(pnl);
                    }
                }
            }
        }

        private void ClearForm()
        {
            cbresep.SelectedIndex = -1;
            txtTargetProduksi.Clear();
            txtInputCatatan.Clear();
            dtpTanggalMulai.Value = DateTime.Now;
        }
    }
}