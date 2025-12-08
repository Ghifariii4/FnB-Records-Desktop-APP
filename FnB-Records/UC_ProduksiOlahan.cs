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
        private int selectedBatchId = 0;
        private bool isEditMode = false;

        public UC_ProduksiOlahan()
        {
            InitializeComponent();
            gbEditCabang.Visible = false; // Panel input hidden saat awal
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

                            // Setup kolom
                            if (dgvBatch.Columns.Count > 0)
                            {
                                dgvBatch.Columns["id"].Visible = false;
                                dgvBatch.Columns["batch_number"].HeaderText = "No. Batch";
                                dgvBatch.Columns["recipe_name"].HeaderText = "Resep";
                                dgvBatch.Columns["target_qty"].HeaderText = "Target (porsi/unit)";
                                dgvBatch.Columns["start_date"].HeaderText = "Tanggal Mulai";
                                dgvBatch.Columns["status_display"].HeaderText = "Status";

                                // Tambah kolom button Detail
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

        private void LoadRecipeCombo()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    string sql = @"
                        SELECT id, name, hpp, suggested_price
                        FROM recipes
                        WHERE user_id = @uid
                        ORDER BY name";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        DataTable dt = new DataTable();
                        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }

                        cbresep.DataSource = dt;
                        cbresep.DisplayMember = "name";
                        cbresep.ValueMember = "id";
                        cbresep.SelectedIndex = -1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading recipes: " + ex.Message);
            }
        }

        // ============================================
        // BUTTON ACTIONS
        // ============================================

        private void btBuatPOBaru_Click(object sender, EventArgs e)
        {
            // Tampilkan panel input untuk buat batch baru
            gbEditCabang.Visible = true;
            isEditMode = false;
            selectedBatchId = 0;

            ClearForm();
            dttanggalmulai.Value = DateTime.Now;
        }

        private void btnsimpan_Click(object sender, EventArgs e)
        {
            // Validasi input
            if (cbresep.SelectedIndex == -1)
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
                    // Generate batch number
                    string batchNumber = GenerateBatchNumber(conn);

                    int recipeId = Convert.ToInt32(cbresep.SelectedValue);

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
                        cmd.Parameters.AddWithValue("@notes", txtInputcatatan.Text ?? "");

                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show($"✅ Batch produksi berhasil dibuat!\n\nNo. Batch: {batchNumber}", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh data
                    LoadDashboardStats();
                    LoadBatchData();
                    gbEditCabang.Visible = false;
                    ClearForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving batch: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnBatalEdit_Click(object sender, EventArgs e)
        {
            gbEditCabang.Visible = false;
            ClearForm();
        }

        private void guna2Button4_Click(object sender, EventArgs e)
        {
            // Sama dengan batal
            btnBatalEdit_Click(sender, e);
        }

        // ============================================
        // DATAGRIDVIEW EVENTS
        // ============================================

        private void dgvBatch_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Cek apakah klik pada kolom button Detail
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
                    // Ambil detail batch
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

                                // Ambil material requirements
                                reader.Close();
                                string materials = GetBatchMaterials(conn, batchId);

                                // Tampilkan di MessageBox atau Form Detail
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

                                // Jika sedang produksi, tampilkan opsi untuk menyelesaikan
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
                    // Call stored procedure untuk complete batch
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
                return count == 0; // True jika semua bahan cukup
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
        // EVENT HANDLERS (Tidak digunakan tapi harus ada)
        // ============================================

        private void cbresep_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ketika resep dipilih, tampilkan info HPP (opsional)
            if (cbresep.SelectedValue != null && cbresep.SelectedValue is int)
            {
                // Bisa tampilkan estimasi HPP
            }
        }

        private void txtjumlahproduksi_TextChanged(object sender, EventArgs e)
        {
            // Auto calculate estimasi HPP (opsional)
        }

        private void dttanggalmulai_ValueChanged(object sender, EventArgs e) { }
        private void txtInputcatatan_TextChanged(object sender, EventArgs e) { }
        private void btnEditCabang_Click(object sender, EventArgs e) { }
        private void lbltotalbatch_Click(object sender, EventArgs e) { }
        private void lblbatchselesai_Click(object sender, EventArgs e) { }
        private void lblsedangproduksi_Click(object sender, EventArgs e) { }
        private void lbltotalproduksi_Click(object sender, EventArgs e) { }
        private void cbresep_SelectedIndexChanged_1(object sender, EventArgs e) { }
        private void txtjumlahproduksi_TextChanged_1(object sender, EventArgs e) { }
        private void dttanggalmulai_ValueChanged_1(object sender, EventArgs e) { }
        private void gbEditCabang_Click(object sender, EventArgs e) { }
        private void guna2GroupBox6_Click(object sender, EventArgs e) { }
    }
}