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
        // ✅ TAMBAHKAN CLASS RECIPEITEM DI SINI
        private class RecipeItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Hpp { get; set; }
            public double SuggestedPrice { get; set; }
            public int ServingSize { get; set; }
            public string DisplayText { get; set; } // ✅ Field biasa, bukan computed property

            public override string ToString()
            {
                return DisplayText; // ✅ Override ToString untuk fallback
            }
        }

        private int selectedBatchId = 0;
        private bool isEditMode = false;

        public UC_ProduksiOlahan()
        {
            InitializeComponent();
            gbEditCabang.Visible = false;
        }

        private void UC_ProduksiOlahan_Load(object sender, EventArgs e)
        {
            LoadDashboardStats();
            LoadBatchData();
            LoadRecipeCombo();
        }

        // ============================================
        // LOAD DATA & STATISTICS
        // ============================================

        private void LoadDashboardStats()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    string sql = @"
                        SELECT 
                            COUNT(*) as total_batch,
                            COUNT(CASE WHEN status = 'selesai' THEN 1 END) as batch_selesai,
                            COUNT(CASE WHEN status = 'sedang_produksi' THEN 1 END) as sedang_produksi,
                            COALESCE(SUM(CASE WHEN status = 'selesai' THEN produced_qty ELSE 0 END), 0) as total_produksi
                        FROM production_batches
                        WHERE user_id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lbltotalbatch.Text = reader.GetInt64(0).ToString();
                                lblbatchselesai.Text = reader.GetInt64(1).ToString();
                                lblsedangproduksi.Text = reader.GetInt64(2).ToString();
                                lbltotalproduksi.Text = reader.GetDouble(3).ToString("N0") + " unit";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading stats: " + ex.Message);
            }
        }

        private void LoadBatchData()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    string sql = @"
                        SELECT 
                            pb.id,
                            pb.batch_number,
                            r.name as recipe_name,
                            pb.target_qty,
                            pb.start_date,
                            CASE 
                                WHEN pb.status = 'sedang_produksi' THEN 'Sedang Produksi'
                                WHEN pb.status = 'selesai' THEN 'Selesai'
                                ELSE 'Dibatalkan'
                            END as status_display
                        FROM production_batches pb
                        JOIN recipes r ON r.id = pb.recipe_id
                        WHERE pb.user_id = @uid
                        ORDER BY pb.created_at DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);

                            dgvBatch.DataSource = dt;

                            if (dgvBatch.Columns.Count > 0)
                            {
                                dgvBatch.Columns["id"].Visible = false;
                                dgvBatch.Columns["batch_number"].HeaderText = "No. Batch";
                                dgvBatch.Columns["recipe_name"].HeaderText = "Resep";
                                dgvBatch.Columns["target_qty"].HeaderText = "Target (porsi/unit)";
                                dgvBatch.Columns["start_date"].HeaderText = "Tanggal Mulai";
                                dgvBatch.Columns["status_display"].HeaderText = "Status";

                                if (!dgvBatch.Columns.Contains("btnDetail"))
                                {
                                    DataGridViewButtonColumn btnCol = new DataGridViewButtonColumn();
                                    btnCol.Name = "btnDetail";
                                    btnCol.HeaderText = "Aksi";
                                    btnCol.Text = "Detail";
                                    btnCol.UseColumnTextForButtonValue = true;
                                    dgvBatch.Columns.Add(btnCol);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading batch data: " + ex.Message);
            }
        }

        // ✅ PERBAIKAN UTAMA: LoadRecipeCombo dengan BindingList
        private void LoadRecipeCombo()
        {
            try
            {
                var recipeList = new List<RecipeItem>();

                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    string sql = @"
                        SELECT id, name, hpp, suggested_price, serving_size
                        FROM recipes
                        WHERE user_id = @uid
                        ORDER BY name";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string name = reader.GetString(1);
                                double hpp = reader.GetDouble(2);
                                double suggestedPrice = reader.GetDouble(3);
                                int servingSize = reader.GetInt32(4);

                                recipeList.Add(new RecipeItem
                                {
                                    Id = id,
                                    Name = name,
                                    Hpp = hpp,
                                    SuggestedPrice = suggestedPrice,
                                    ServingSize = servingSize,
                                    DisplayText = $"{name} ({servingSize} porsi, HPP: Rp {hpp:#,##0})" // ✅ Set DisplayText
                                });
                            }
                        }
                    }
                }

                // ✅ Gunakan BindingList
                cbresep.DataSource = new BindingList<RecipeItem>(recipeList);
                cbresep.DisplayMember = "DisplayText"; // ✅ Gunakan DisplayText
                cbresep.ValueMember = "Id";
                cbresep.SelectedIndex = -1;

                // ✅ Debug: Cek jumlah item
                Console.WriteLine($"Loaded {recipeList.Count} recipes");
                foreach (var item in recipeList)
                {
                    Console.WriteLine($"- {item.DisplayText}");
                }

                if (recipeList.Count == 0)
                {
                    MessageBox.Show(
                        "Belum ada resep tersedia!\n\n" +
                        "Silakan buat resep terlebih dahulu di menu 'Resep Menu'.",
                        "Info",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading recipes: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // BUTTON ACTIONS
        // ============================================

        private void btBuatPOBaru_Click(object sender, EventArgs e)
        {
            gbEditCabang.Visible = true;
            isEditMode = false;
            selectedBatchId = 0;

            ClearForm();
            dttanggalmulai.Value = DateTime.Now;
        }

        private void btnsimpan_Click(object sender, EventArgs e)
        {
            // Validasi input
            if (cbresep.SelectedValue == null || !(cbresep.SelectedValue is int))
            {
                MessageBox.Show("Pilih resep terlebih dahulu!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtjumlahproduksi.Text))
            {
                MessageBox.Show("Masukkan jumlah produksi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int targetQty;
            if (!int.TryParse(txtjumlahproduksi.Text, out targetQty) || targetQty <= 0)
            {
                MessageBox.Show("Jumlah produksi harus berupa angka positif!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    // ✅ Ambil recipe_id dengan aman
                    int recipeId = (int)cbresep.SelectedValue;

                    // ✅ Validasi recipe exists dan milik user yang benar
                    string checkRecipeSql = "SELECT COUNT(*) FROM recipes WHERE id = @recipe_id AND user_id = @uid";
                    using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkRecipeSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@recipe_id", recipeId);
                        checkCmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        long recipeCount = (long)checkCmd.ExecuteScalar();
                        if (recipeCount == 0)
                        {
                            MessageBox.Show("Resep tidak ditemukan atau bukan milik Anda!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            LoadRecipeCombo(); // Reload combo
                            return;
                        }
                    }

                    // ✅ Validasi resep punya bahan
                    string checkItemsSql = "SELECT COUNT(*) FROM recipe_items WHERE recipe_id = @recipe_id";
                    using (NpgsqlCommand checkCmd = new NpgsqlCommand(checkItemsSql, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@recipe_id", recipeId);

                        long itemsCount = (long)checkCmd.ExecuteScalar();
                        if (itemsCount == 0)
                        {
                            MessageBox.Show("Resep ini belum memiliki bahan baku!\n\nTambahkan bahan terlebih dahulu di menu Resep Menu.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }

                    // Generate batch number
                    string batchNumber = GenerateBatchNumber(conn);

                    // Cek ketersediaan stok bahan baku
                    if (!CheckMaterialAvailability(conn, recipeId, targetQty))
                    {
                        DialogResult result = MessageBox.Show(
                            "⚠️ Beberapa bahan baku tidak mencukupi!\n\n" +
                            "Tetap lanjutkan produksi?",
                            "Peringatan Stok",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Warning
                        );

                        if (result == DialogResult.No) return;
                    }

                    // Insert batch baru
                    string sql = @"
                        INSERT INTO production_batches 
                        (user_id, batch_number, recipe_id, target_qty, start_date, status, notes, created_at, updated_at)
                        VALUES 
                        (@uid, @batch_number, @recipe_id, @target_qty, @start_date, 'sedang_produksi', @notes, NOW(), NOW())";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                        cmd.Parameters.AddWithValue("@batch_number", batchNumber);
                        cmd.Parameters.AddWithValue("@recipe_id", recipeId);
                        cmd.Parameters.AddWithValue("@target_qty", targetQty);
                        cmd.Parameters.AddWithValue("@start_date", dttanggalmulai.Value.Date);

                        if (string.IsNullOrWhiteSpace(txtInputcatatan.Text))
                            cmd.Parameters.AddWithValue("@notes", DBNull.Value);
                        else
                            cmd.Parameters.AddWithValue("@notes", txtInputcatatan.Text);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show($"✅ Batch produksi berhasil dibuat!\n\nNo. Batch: {batchNumber}",
                                "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadDashboardStats();
                            LoadBatchData();
                            gbEditCabang.Visible = false;
                            ClearForm();
                        }
                        else
                        {
                            MessageBox.Show("Gagal menyimpan batch!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (NpgsqlException ex)
            {
                string errorMsg = "Error menyimpan batch:\n\n";

                if (ex.SqlState == "23503")
                {
                    errorMsg += "❌ Data tidak valid:\n";
                    errorMsg += "- Resep mungkin sudah dihapus\n";
                    errorMsg += "- User ID tidak valid\n\n";
                    errorMsg += "Silakan pilih resep lain atau refresh halaman.";
                    LoadRecipeCombo(); // Reload
                }
                else if (ex.SqlState == "23505")
                {
                    errorMsg += "❌ Nomor batch sudah ada!\n";
                    errorMsg += "Coba lagi dalam beberapa detik.";
                }
                else
                {
                    errorMsg += ex.Message;
                }

                MessageBox.Show(errorMsg, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBatalEdit_Click(object sender, EventArgs e)
        {
            gbEditCabang.Visible = false;
            ClearForm();
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            btnBatalEdit_Click(sender, e);
        }

        // ============================================
        // DATAGRIDVIEW EVENTS
        // ============================================

        private void dgvBatch_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvBatch.Columns[e.ColumnIndex].Name == "btnDetail")
            {
                int batchId = Convert.ToInt32(dgvBatch.Rows[e.RowIndex].Cells["id"].Value);
                ShowBatchDetail(batchId);
            }
        }

        private void ShowBatchDetail(int batchId)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    string sql = @"
                        SELECT 
                            pb.batch_number,
                            r.name as recipe_name,
                            pb.target_qty,
                            pb.produced_qty,
                            pb.start_date,
                            pb.completed_date,
                            pb.status,
                            pb.notes,
                            (pb.target_qty * r.hpp) as estimated_cost
                        FROM production_batches pb
                        JOIN recipes r ON r.id = pb.recipe_id
                        WHERE pb.id = @batch_id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@batch_id", batchId);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string batchNumber = reader.GetString(0);
                                string recipeName = reader.GetString(1);
                                int targetQty = reader.GetInt32(2);
                                int producedQty = reader.GetInt32(3);
                                DateTime startDate = reader.GetDateTime(4);
                                DateTime? completedDate = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
                                string status = reader.GetString(6);
                                string notes = reader.IsDBNull(7) ? "" : reader.GetString(7);
                                double estimatedCost = reader.GetDouble(8);

                                reader.Close();
                                string materials = GetBatchMaterials(conn, batchId);

                                string detail = $"DETAIL BATCH PRODUKSI\n" +
                                              $"═══════════════════════════════\n\n" +
                                              $"No. Batch: {batchNumber}\n" +
                                              $"Resep: {recipeName}\n" +
                                              $"Target: {targetQty} porsi/unit\n" +
                                              $"Terproduksi: {producedQty} porsi/unit\n" +
                                              $"Tanggal Mulai: {startDate:dd MMM yyyy}\n" +
                                              $"Tanggal Selesai: {(completedDate.HasValue ? completedDate.Value.ToString("dd MMM yyyy") : "-")}\n" +
                                              $"Status: {GetStatusDisplay(status)}\n" +
                                              $"Estimasi HPP: Rp {estimatedCost:N0}\n\n" +
                                              $"Catatan: {notes}\n\n" +
                                              $"KEBUTUHAN BAHAN:\n" +
                                              $"═══════════════════════════════\n" +
                                              $"{materials}";

                                if (status == "sedang_produksi")
                                {
                                    DialogResult result = MessageBox.Show(
                                        detail + "\n\nSelesaikan batch ini?",
                                        "Detail Batch",
                                        MessageBoxButtons.YesNoCancel,
                                        MessageBoxIcon.Information
                                    );

                                    if (result == DialogResult.Yes)
                                    {
                                        CompleteBatch(batchId, targetQty);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show(detail, "Detail Batch", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error showing detail: " + ex.Message);
            }
        }

        private string GetBatchMaterials(NpgsqlConnection conn, int batchId)
        {
            StringBuilder sb = new StringBuilder();

            string sql = @"
                SELECT 
                    ingredient_name,
                    unit,
                    total_qty_required,
                    current_stock,
                    stock_status
                FROM batch_material_requirements
                WHERE batch_id = @batch_id
                ORDER BY ingredient_name";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@batch_id", batchId);

                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(0);
                        string unit = reader.GetString(1);
                        double required = reader.GetDouble(2);
                        double stock = reader.GetDouble(3);
                        string status = reader.GetString(4);

                        string statusIcon = status == "Cukup" ? "✅" : "⚠️";
                        sb.AppendLine($"{statusIcon} {name}: {required:N2} {unit} (Stok: {stock:N2})");
                    }
                }
            }

            return sb.ToString();
        }

        private void CompleteBatch(int batchId, int producedQty)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    string sql = "SELECT complete_production_batch(@batch_id, @produced_qty)";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@batch_id", batchId);
                        cmd.Parameters.AddWithValue("@produced_qty", producedQty);

                        cmd.ExecuteScalar();
                    }

                    MessageBox.Show("✅ Batch produksi berhasil diselesaikan!\n\nStok bahan baku telah dikurangi.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadDashboardStats();
                    LoadBatchData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error completing batch: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ============================================
        // HELPER FUNCTIONS
        // ============================================

        private string GenerateBatchNumber(NpgsqlConnection conn)
        {
            string sql = "SELECT generate_batch_number(@uid)";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                return cmd.ExecuteScalar().ToString();
            }
        }

        private bool CheckMaterialAvailability(NpgsqlConnection conn, int recipeId, int targetQty)
        {
            string sql = @"
                SELECT COUNT(*)
                FROM recipe_items ri
                JOIN ingredients i ON i.id = ri.ingredient_id
                WHERE ri.recipe_id = @recipe_id
                AND i.stock < (ri.qty * @target_qty)";

            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@recipe_id", recipeId);
                cmd.Parameters.AddWithValue("@target_qty", targetQty);

                long count = (long)cmd.ExecuteScalar();
                return count == 0;
            }
        }

        private string GetStatusDisplay(string status)
        {
            switch (status)
            {
                case "sedang_produksi": return "🔄 Sedang Produksi";
                case "selesai": return "✅ Selesai";
                case "dibatalkan": return "❌ Dibatalkan";
                default: return status;
            }
        }

        private void ClearForm()
        {
            cbresep.SelectedIndex = -1;
            txtjumlahproduksi.Clear();
            txtInputcatatan.Clear();
            dttanggalmulai.Value = DateTime.Now;
        }

        // ============================================
        // EVENT HANDLERS
        // ============================================

        private void cbresep_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void txtjumlahproduksi_TextChanged(object sender, EventArgs e) { }
        private void dttanggalmulai_ValueChanged(object sender, EventArgs e) { }
        private void txtInputcatatan_TextChanged(object sender, EventArgs e) { }
        private void btnEditCabang_Click(object sender, EventArgs e) { }
        private void lbltotalbatch_Click(object sender, EventArgs e) { }
        private void lblbatchselesai_Click(object sender, EventArgs e) { }
        private void lblsedangproduksi_Click(object sender, EventArgs e) { }
        private void lbltotalproduksi_Click(object sender, EventArgs e) { }
        private void cbresep_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (cbresep.SelectedValue != null && cbresep.SelectedValue is int)
            {
                try
                {
                    int recipeId = (int)cbresep.SelectedValue;

                    Koneksi db = new Koneksi();
                    using (NpgsqlConnection conn = db.GetKoneksi())
                    {
                        string sql = @"
                            SELECT 
                                r.name,
                                r.hpp,
                                r.suggested_price,
                                r.serving_size,
                                COUNT(ri.id) as ingredient_count
                            FROM recipes r
                            LEFT JOIN recipe_items ri ON ri.recipe_id = r.id
                            WHERE r.id = @recipe_id
                            GROUP BY r.id, r.name, r.hpp, r.suggested_price, r.serving_size";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@recipe_id", recipeId);

                            using (NpgsqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    string recipeName = reader.GetString(0);
                                    int ingredientCount = reader.GetInt32(4);

                                    if (ingredientCount == 0)
                                    {
                                        MessageBox.Show(
                                            $"⚠️ Resep '{recipeName}' belum memiliki bahan baku!\n\n" +
                                            "Silakan tambahkan bahan di menu 'Resep Menu' terlebih dahulu.",
                                            "Peringatan",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning
                                        );

                                        cbresep.SelectedIndex = -1;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }
        private void txtjumlahproduksi_TextChanged_1(object sender, EventArgs e) { }
        private void dttanggalmulai_ValueChanged_1(object sender, EventArgs e) { }
        private void gbEditCabang_Click(object sender, EventArgs e) { }
        private void guna2GroupBox6_Click(object sender, EventArgs e) { }
        private void guna2GroupBox3_Click(object sender, EventArgs e) { }
    }
}