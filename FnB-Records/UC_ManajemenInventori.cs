using FnB_Records.Koneksi_DB;
using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace FnB_Records
{
    public partial class UC_ManajemenInventori : UserControl
    {
        private int currentUserId = Login.GlobalSession.CurrentUserId;
        private Koneksi db = new Koneksi();

        // Untuk popup
        private int selectedIngredientId = 0;
        private string popupMode = ""; // "tambah" atau "kurang"

        public UC_ManajemenInventori()
        {
            InitializeComponent();
            AttachEvents();
        }

        private void AttachEvents()
        {
            // Events
            txtCariInventori.TextChanged += TxtCariInventori_TextChanged;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            btnExportLaporan.Click += BtnExportLaporan_Click;
            dgvManajemenInventori.CellContentClick += DgvManajemenInventori_CellContentClick;
            btnSimpanStok.Click += BtnSimpanStok_Click;
            btnBatalStok.Click += BtnBatalStok_Click;
        }

        private void UC_ManajemenInventori_Load(object sender, EventArgs e)
        {
            // Pastikan user sudah login
            if (Login.GlobalSession.CurrentUserId == 0)
            {
                Login.GlobalSession.CurrentUserId = 1; // Default untuk testing
            }
            currentUserId = Login.GlobalSession.CurrentUserId;

            LoadComboStatus();
            EnsureGridColumns();
            LoadInventoryData();
            LoadSummaryLabels();

            // Hide popup saat load
            gbPopupStok.Visible = false;
        }

        #region Load Combo Status
        private void LoadComboStatus()
        {
            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("Semua Status");
            cmbStatus.Items.Add("Kritis");
            cmbStatus.Items.Add("Stok Rendah");
            cmbStatus.Items.Add("Normal");
            cmbStatus.Items.Add("Aman");
            cmbStatus.SelectedIndex = 0;
        }
        #endregion

        #region Setup DataGridView Columns
        private void EnsureGridColumns()
        {
            dgvManajemenInventori.Columns.Clear();
            dgvManajemenInventori.Rows.Clear();

            dgvManajemenInventori.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_id",
                HeaderText = "ID",
                Visible = false
            });

            dgvManajemenInventori.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_nama",
                HeaderText = "Nama Bahan",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvManajemenInventori.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_stok",
                HeaderText = "Stok Saat Ini",
                Width = 120
            });

            dgvManajemenInventori.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_min_stock",
                HeaderText = "Stok Min",
                Width = 100
            });

            dgvManajemenInventori.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_harga",
                HeaderText = "Harga/Unit",
                Width = 120
            });

            dgvManajemenInventori.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_total",
                HeaderText = "Nilai Total",
                Width = 130
            });

            dgvManajemenInventori.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_status",
                HeaderText = "Status",
                Width = 120
            });

            // Column Aksi Tambah (+)
            var colTambah = new DataGridViewButtonColumn
            {
                Name = "col_tambah",
                HeaderText = "+",
                Text = "+",
                UseColumnTextForButtonValue = true,
                Width = 50
            };
            dgvManajemenInventori.Columns.Add(colTambah);

            // Column Aksi Kurang (-)
            var colKurang = new DataGridViewButtonColumn
            {
                Name = "col_kurang",
                HeaderText = "-",
                Text = "-",
                UseColumnTextForButtonValue = true,
                Width = 50
            };
            dgvManajemenInventori.Columns.Add(colKurang);

            // Styling
            dgvManajemenInventori.RowHeadersVisible = false;
            dgvManajemenInventori.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvManajemenInventori.AllowUserToAddRows = false;
            dgvManajemenInventori.ReadOnly = false;
            dgvManajemenInventori.Columns["col_tambah"].ReadOnly = false;
            dgvManajemenInventori.Columns["col_kurang"].ReadOnly = false;
        }
        #endregion

        #region Load Inventory Data
        private void LoadInventoryData(string keyword = "", string statusFilter = "Semua Status")
        {
            dgvManajemenInventori.Rows.Clear();

            try
            {
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string query = @"
                        SELECT 
                            i.id,
                            i.name,
                            i.stock,
                            i.min_stock,
                            i.price,
                            i.unit,
                            (i.stock * i.price) as total_value
                        FROM ingredients i
                        WHERE i.user_id = @uid 
                        AND (i.name ILIKE @search)
                        ORDER BY i.name ASC";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@search", "%" + keyword + "%");

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string name = reader.GetString(1);
                                double stock = reader.IsDBNull(2) ? 0 : Convert.ToDouble(reader.GetValue(2));
                                double minStock = reader.IsDBNull(3) ? 0 : Convert.ToDouble(reader.GetValue(3));
                                double price = reader.IsDBNull(4) ? 0 : Convert.ToDouble(reader.GetValue(4));
                                string unit = reader.IsDBNull(5) ? "" : reader.GetString(5);
                                double totalValue = reader.IsDBNull(6) ? 0 : Convert.ToDouble(reader.GetValue(6));

                                // Hitung status
                                string status = GetStockStatus(stock, minStock);

                                // Filter berdasarkan status
                                if (statusFilter != "Semua Status" && status != statusFilter)
                                    continue;

                                // Tambah row
                                int idx = dgvManajemenInventori.Rows.Add();
                                dgvManajemenInventori.Rows[idx].Cells["col_id"].Value = id;
                                dgvManajemenInventori.Rows[idx].Cells["col_nama"].Value = name;
                                dgvManajemenInventori.Rows[idx].Cells["col_stok"].Value = $"{stock:0.##} {unit}";
                                dgvManajemenInventori.Rows[idx].Cells["col_min_stock"].Value = $"{minStock:0.##} {unit}";
                                dgvManajemenInventori.Rows[idx].Cells["col_harga"].Value = $"Rp {price:#,##0}";
                                dgvManajemenInventori.Rows[idx].Cells["col_total"].Value = $"Rp {totalValue:#,##0}";
                                dgvManajemenInventori.Rows[idx].Cells["col_status"].Value = status;

                                // Warna status
                                var statusCell = dgvManajemenInventori.Rows[idx].Cells["col_status"];
                                if (status == "Kritis")
                                {
                                    statusCell.Style.BackColor = Color.FromArgb(255, 205, 210);
                                    statusCell.Style.ForeColor = Color.FromArgb(198, 40, 40);
                                }
                                else if (status == "Stok Rendah")
                                {
                                    statusCell.Style.BackColor = Color.FromArgb(255, 243, 224);
                                    statusCell.Style.ForeColor = Color.FromArgb(230, 81, 0);
                                }
                                else if (status == "Normal")
                                {
                                    statusCell.Style.BackColor = Color.FromArgb(232, 245, 233);
                                    statusCell.Style.ForeColor = Color.FromArgb(46, 125, 50);
                                }
                                else // Aman
                                {
                                    statusCell.Style.BackColor = Color.FromArgb(200, 230, 201);
                                    statusCell.Style.ForeColor = Color.FromArgb(27, 94, 32);
                                }
                            }
                        }
                    }
                }

                dgvManajemenInventori.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading inventory: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetStockStatus(double stock, double minStock)
        {
            if (stock == 0)
                return "Kritis";
            else if (stock <= minStock)
                return "Stok Rendah";
            else if (stock <= minStock * 1.5)
                return "Normal";
            else
                return "Aman";
        }
        #endregion

        #region Load Summary Labels
        private void LoadSummaryLabels()
        {
            try
            {
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string query = @"
                        SELECT 
                            COUNT(*) as total_items,
                            COUNT(CASE WHEN stock = 0 THEN 1 END) as kritis,
                            COUNT(CASE WHEN stock > 0 AND stock <= min_stock THEN 1 END) as rendah,
                            COALESCE(SUM(stock * price), 0) as total_value
                        FROM ingredients
                        WHERE user_id = @uid";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblTotalBahanBaku.Text = reader.GetInt64(0).ToString();
                                lblStokKritis.Text = reader.GetInt64(1).ToString();
                                lblStokRendah.Text = reader.GetInt64(2).ToString();

                                double totalValue = Convert.ToDouble(reader.GetValue(3));
                                lblNilaiInventori.Text = $"Rp {totalValue:#,##0}";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading summary: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Search & Filter
        private void TxtCariInventori_TextChanged(object sender, EventArgs e)
        {
            LoadInventoryData(txtCariInventori.Text, cmbStatus.SelectedItem?.ToString() ?? "Semua Status");
            LoadSummaryLabels();
        }

        private void CmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadInventoryData(txtCariInventori.Text, cmbStatus.SelectedItem?.ToString() ?? "Semua Status");
        }
        #endregion

        #region DataGridView Click Events
        private void DgvManajemenInventori_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var colName = dgvManajemenInventori.Columns[e.ColumnIndex].Name;
            var row = dgvManajemenInventori.Rows[e.RowIndex];

            if (row.Cells["col_id"].Value == null) return;

            int ingredientId = Convert.ToInt32(row.Cells["col_id"].Value);
            string ingredientName = row.Cells["col_nama"].Value.ToString();
            string stockText = row.Cells["col_stok"].Value.ToString();

            // Parse stock (format: "25 kg" -> 25)
            double currentStock = 0;
            var stockParts = stockText.Split(' ');
            if (stockParts.Length > 0)
            {
                double.TryParse(stockParts[0].Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out currentStock);
            }

            if (colName == "col_tambah")
            {
                ShowPopupStok(ingredientId, ingredientName, currentStock, "tambah");
            }
            else if (colName == "col_kurang")
            {
                ShowPopupStok(ingredientId, ingredientName, currentStock, "kurang");
            }
        }
        #endregion

        #region Popup Stok
        private void ShowPopupStok(int id, string name, double currentStock, string mode)
        {
            selectedIngredientId = id;
            popupMode = mode;

            if (mode == "tambah")
            {
                lblPopupTitle.Text = "Tambah Stok";
                lblPopupTitle.ForeColor = Color.FromArgb(46, 125, 50);
            }
            else
            {
                lblPopupTitle.Text = "Kurangi Stok";
                lblPopupTitle.ForeColor = Color.FromArgb(198, 40, 40);
            }

            lblNamaBahanPopup.Text = $"Bahan: {name}";
            lblStokSaatIniPopup.Text = $"Stok Saat Ini: {currentStock:0.##}";

            nudJumlahStok.Value = 0;
            txtCatatanStok.Text = "";

            gbPopupStok.Visible = true;
            gbPopupStok.BringToFront();
        }

        private void BtnSimpanStok_Click(object sender, EventArgs e)
        {
            if (nudJumlahStok.Value == 0)
            {
                MessageBox.Show("Jumlah tidak boleh 0!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Get current stock
                    double currentStock = 0;
                    string query = "SELECT stock FROM ingredients WHERE id = @id AND user_id = @uid";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", selectedIngredientId);
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            currentStock = Convert.ToDouble(result);
                        }
                    }

                    // Calculate new stock
                    double jumlah = Convert.ToDouble(nudJumlahStok.Value);
                    double newStock = popupMode == "tambah" ? currentStock + jumlah : currentStock - jumlah;

                    if (newStock < 0)
                    {
                        MessageBox.Show("Stok tidak boleh negatif!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Update stock
                    query = "UPDATE ingredients SET stock = @stock, updated_at = CURRENT_TIMESTAMP WHERE id = @id AND user_id = @uid";
                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@stock", newStock);
                        cmd.Parameters.AddWithValue("@id", selectedIngredientId);
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.ExecuteNonQuery();
                    }

                    // TODO: Log history jika ada tabel inventory_logs
                    // LogStockHistory(selectedIngredientId, popupMode, jumlah, txtCatatanStok.Text);
                }

                MessageBox.Show($"Stok berhasil {(popupMode == "tambah" ? "ditambah" : "dikurangi")}!",
                    "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                gbPopupStok.Visible = false;
                LoadInventoryData(txtCariInventori.Text, cmbStatus.SelectedItem?.ToString() ?? "Semua Status");
                LoadSummaryLabels();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating stock: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnBatalStok_Click(object sender, EventArgs e)
        {
            gbPopupStok.Visible = false;
        }
        #endregion

        #region Export PDF
        private void BtnExportLaporan_Click(object sender, EventArgs e)
        {
            try
            {
                // Create PDF document
                PdfDocument document = new PdfDocument();
                document.Info.Title = "Laporan Inventori";

                PdfPage page = document.AddPage();
                page.Size = PdfSharp.PageSize.A4;
                XGraphics gfx = XGraphics.FromPdfPage(page);

                // Fonts
                XFont titleFont = new XFont("Arial", 20);
                XFont headerFont = new XFont("Arial", 12);
                XFont normalFont = new XFont("Arial", 10);
                XFont smallFont = new XFont("Arial", 8);

                double yPos = 40;

                // Title
                gfx.DrawString("LAPORAN INVENTORI BAHAN BAKU", titleFont, XBrushes.Black,
                    new XRect(0, yPos, page.Width, 30), XStringFormats.TopCenter);
                yPos += 40;

                // Date
                gfx.DrawString($"Tanggal: {DateTime.Now:dd MMMM yyyy}", normalFont, XBrushes.Black,
                    new XRect(40, yPos, page.Width - 80, 20), XStringFormats.TopLeft);
                yPos += 30;

                // Summary Box
                gfx.DrawRectangle(XPens.Black, XBrushes.LightGray, 40, yPos, page.Width - 80, 60);
                gfx.DrawString($"Total Bahan: {lblTotalBahanBaku.Text}", headerFont, XBrushes.Black, 50, yPos + 15);
                gfx.DrawString($"Stok Kritis: {lblStokKritis.Text}", normalFont, XBrushes.Red, 50, yPos + 35);
                gfx.DrawString($"Stok Rendah: {lblStokRendah.Text}", normalFont, XBrushes.Orange, 200, yPos + 35);
                gfx.DrawString($"Nilai Total: {lblNilaiInventori.Text}", headerFont, XBrushes.Green, 350, yPos + 35);
                yPos += 80;

                // Table Header
                gfx.DrawRectangle(XPens.Black, XBrushes.LightBlue, 40, yPos, page.Width - 80, 25);
                gfx.DrawString("Nama Bahan", headerFont, XBrushes.Black, 45, yPos + 8);
                gfx.DrawString("Stok", headerFont, XBrushes.Black, 250, yPos + 8);
                gfx.DrawString("Harga", headerFont, XBrushes.Black, 330, yPos + 8);
                gfx.DrawString("Total", headerFont, XBrushes.Black, 420, yPos + 8);
                gfx.DrawString("Status", headerFont, XBrushes.Black, 500, yPos + 8);
                yPos += 25;

                // Table Data
                foreach (DataGridViewRow row in dgvManajemenInventori.Rows)
                {
                    if (yPos > page.Height - 100) // New page if needed
                    {
                        page = document.AddPage();
                        gfx = XGraphics.FromPdfPage(page);
                        yPos = 40;
                    }

                    string nama = row.Cells["col_nama"].Value?.ToString() ?? "";
                    string stok = row.Cells["col_stok"].Value?.ToString() ?? "";
                    string harga = row.Cells["col_harga"].Value?.ToString() ?? "";
                    string total = row.Cells["col_total"].Value?.ToString() ?? "";
                    string status = row.Cells["col_status"].Value?.ToString() ?? "";

                    gfx.DrawString(nama, smallFont, XBrushes.Black, 45, yPos + 5);
                    gfx.DrawString(stok, smallFont, XBrushes.Black, 250, yPos + 5);
                    gfx.DrawString(harga, smallFont, XBrushes.Black, 330, yPos + 5);
                    gfx.DrawString(total, smallFont, XBrushes.Black, 420, yPos + 5);
                    gfx.DrawString(status, smallFont, XBrushes.Black, 500, yPos + 5);

                    gfx.DrawLine(XPens.LightGray, 40, yPos + 20, page.Width - 40, yPos + 20);
                    yPos += 20;
                }

                // Save dialog
                SaveFileDialog saveDialog = new SaveFileDialog
                {
                    Filter = "PDF Files|*.pdf",
                    Title = "Save Laporan Inventori",
                    FileName = $"Laporan_Inventori_{DateTime.Now:yyyyMMdd}.pdf"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    document.Save(saveDialog.FileName);
                    MessageBox.Show("Laporan berhasil diekspor!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Open PDF
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion
    }
}