using FnB_Records.Koneksi_DB;
using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using PdfSharp.Fonts;

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

            // PENTING: Set FontResolver untuk PdfSharp
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new WindowsFontResolver();
            }

            this.Load += UC_ManajemenInventori_Load; // PENTING: Pasang event Load
        }

        private void UC_ManajemenInventori_Load(object sender, EventArgs e)
        {
            // Pastikan user sudah login
            if (Login.GlobalSession.CurrentUserId == 0)
            {
                Login.GlobalSession.CurrentUserId = 1; // Default untuk testing
            }
            currentUserId = Login.GlobalSession.CurrentUserId;

            // Setup dulu sebelum attach events
            LoadComboStatus();
            SetupDataGridViewColumns();

            // Attach events SETELAH setup
            txtCariInventori.TextChanged += TxtCariInventori_TextChanged;
            cmbStatus.SelectedIndexChanged += CmbStatus_SelectedIndexChanged;
            btnExportLaporan.Click += BtnExportLaporan_Click;
            dgvManajemenInventori.CellContentClick += DgvManajemenInventori_CellContentClick;
            btnSimpanStok.Click += BtnSimpanStok_Click;
            btnBatalStok.Click += BtnBatalStok_Click;

            // Load data terakhir
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
        private void SetupDataGridViewColumns()
        {
            // Clear existing columns
            dgvManajemenInventori.Columns.Clear();

            // PENTING: Set AutoGenerateColumns = false
            dgvManajemenInventori.AutoGenerateColumns = false;
            dgvManajemenInventori.RowHeadersVisible = false;
            dgvManajemenInventori.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvManajemenInventori.AllowUserToAddRows = false;
            dgvManajemenInventori.ReadOnly = true;
            dgvManajemenInventori.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Hidden ID Column (untuk referensi)
            var colId = new DataGridViewTextBoxColumn
            {
                Name = "colId",
                HeaderText = "ID",
                DataPropertyName = "id",
                Visible = false
            };
            dgvManajemenInventori.Columns.Add(colId);

            // 1. Nama Bahan
            var colNama = new DataGridViewTextBoxColumn
            {
                Name = "colNamaBahan",
                HeaderText = "Nama Bahan",
                DataPropertyName = "nama_bahan",
                FillWeight = 150
            };
            dgvManajemenInventori.Columns.Add(colNama);

            // 2. Stok Saat Ini
            var colStok = new DataGridViewTextBoxColumn
            {
                Name = "colStokSaatIni",
                HeaderText = "Stok Saat Ini",
                DataPropertyName = "stok_display",
                FillWeight = 100
            };
            dgvManajemenInventori.Columns.Add(colStok);

            // 3. Stok Minimum
            var colMin = new DataGridViewTextBoxColumn
            {
                Name = "colStokMin",
                HeaderText = "Stok Min",
                DataPropertyName = "min_stok_display",
                FillWeight = 100
            };
            dgvManajemenInventori.Columns.Add(colMin);

            // 4. Harga/Unit
            var colHarga = new DataGridViewTextBoxColumn
            {
                Name = "colHargaUnit",
                HeaderText = "Harga/Unit",
                DataPropertyName = "harga_display",
                FillWeight = 120
            };
            dgvManajemenInventori.Columns.Add(colHarga);

            // 5. Nilai Total
            var colNilai = new DataGridViewTextBoxColumn
            {
                Name = "colNilaiTotal",
                HeaderText = "Nilai Total",
                DataPropertyName = "nilai_total_display",
                FillWeight = 120
            };
            dgvManajemenInventori.Columns.Add(colNilai);

            // 6. Status
            var colStatus = new DataGridViewTextBoxColumn
            {
                Name = "colStatus",
                HeaderText = "Status",
                DataPropertyName = "status",
                FillWeight = 100
            };
            dgvManajemenInventori.Columns.Add(colStatus);

            // 7. Tambah (Image Column)
            var colTambah = new DataGridViewImageColumn
            {
                Name = "colTambah",
                HeaderText = "Tambah",
                FillWeight = 60,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            // Set image jika ada di resources
            try
            {
                colTambah.Image = Properties.Resources.add_button_icon_putih;
            }
            catch
            {
                // Jika resource tidak ada, buat placeholder
                var bmp = new Bitmap(30, 30);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Green);
                    g.DrawString("+", new Font("Arial", 16, FontStyle.Bold), Brushes.White, 5, 2);
                }
                colTambah.Image = bmp;
            }
            dgvManajemenInventori.Columns.Add(colTambah);

            // 8. Kurangi (Image Column)
            var colKurang = new DataGridViewImageColumn
            {
                Name = "colKurangi",
                HeaderText = "Kurangi",
                FillWeight = 60,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            // Set image jika ada di resources
            try
            {
                colKurang.Image = Properties.Resources.exit_icon_hitam;
            }
            catch
            {
                // Jika resource tidak ada, buat placeholder
                var bmp = new Bitmap(30, 30);
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Red);
                    g.DrawString("-", new Font("Arial", 16, FontStyle.Bold), Brushes.White, 8, 2);
                }
                colKurang.Image = bmp;
            }
            dgvManajemenInventori.Columns.Add(colKurang);

            // Set alignment untuk kolom numerik
            dgvManajemenInventori.Columns["colStokSaatIni"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvManajemenInventori.Columns["colStokMin"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvManajemenInventori.Columns["colHargaUnit"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvManajemenInventori.Columns["colNilaiTotal"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgvManajemenInventori.Columns["colStatus"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
        #endregion

        #region Load Inventory Data
        private void LoadInventoryData(string keyword = "", string statusFilter = "Semua Status")
        {
            try
            {
                using (var conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string query = @"
                        SELECT 
                            i.id,
                            i.name AS nama_bahan,
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

                        using (var da = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            // Tambah kolom display (seperti di UC_BahanBaku)
                            dt.Columns.Add("stok_display", typeof(string));
                            dt.Columns.Add("min_stok_display", typeof(string));
                            dt.Columns.Add("harga_display", typeof(string));
                            dt.Columns.Add("nilai_total_display", typeof(string));
                            dt.Columns.Add("status", typeof(string));

                            // Proses setiap baris
                            foreach (DataRow row in dt.Rows)
                            {
                                double stock = row["stock"] != DBNull.Value ? Convert.ToDouble(row["stock"]) : 0;
                                double minStock = row["min_stock"] != DBNull.Value ? Convert.ToDouble(row["min_stock"]) : 0;
                                double price = row["price"] != DBNull.Value ? Convert.ToDouble(row["price"]) : 0;
                                string unit = row["unit"]?.ToString() ?? "";
                                double totalValue = row["total_value"] != DBNull.Value ? Convert.ToDouble(row["total_value"]) : 0;

                                // Hitung status
                                string status = GetStockStatus(stock, minStock);
                                row["status"] = status;

                                // Format display
                                row["stok_display"] = $"{stock:0.##} {unit}";
                                row["min_stok_display"] = $"{minStock:0.##} {unit}";
                                row["harga_display"] = $"Rp {price:#,##0}";
                                row["nilai_total_display"] = $"Rp {totalValue:#,##0}";
                            }

                            // Filter berdasarkan status
                            if (statusFilter != "Semua Status")
                            {
                                DataView dv = dt.DefaultView;
                                dv.RowFilter = $"status = '{statusFilter}'";
                                dt = dv.ToTable();
                            }

                            // Bind ke DataGridView
                            dgvManajemenInventori.DataSource = dt;

                            // Terapkan warna status
                            ApplyStatusColors();
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

        private void ApplyStatusColors()
        {
            foreach (DataGridViewRow row in dgvManajemenInventori.Rows)
            {
                if (row.Cells["colStatus"].Value != null)
                {
                    string status = row.Cells["colStatus"].Value.ToString();
                    var statusCell = row.Cells["colStatus"];

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

            // Ambil data dari DataBoundItem (seperti di UC_BahanBaku)
            if (row.DataBoundItem is DataRowView dataRow)
            {
                int ingredientId = Convert.ToInt32(dataRow["id"]);
                string ingredientName = dataRow["nama_bahan"].ToString();
                double currentStock = dataRow["stock"] != DBNull.Value ? Convert.ToDouble(dataRow["stock"]) : 0;
                string unit = dataRow["unit"]?.ToString() ?? "";

                if (colName == "colTambah")
                {
                    ShowPopupStok(ingredientId, ingredientName, currentStock, unit, "tambah");
                }
                else if (colName == "colKurangi")
                {
                    ShowPopupStok(ingredientId, ingredientName, currentStock, unit, "kurang");
                }
            }
        }
        #endregion

        #region Popup Stok
        private void ShowPopupStok(int id, string name, double currentStock, string unit, string mode)
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
            lblStokSaatIniPopup.Text = $"Stok Saat Ini: {currentStock:0.##} {unit}";

            nudJumlahStok.Value = 0;
            nudJumlahStok.Minimum = 0;
            nudJumlahStok.Maximum = 999999;
            nudJumlahStok.DecimalPlaces = 2;

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

                // Fonts - Gunakan XFontStyleEx untuk versi PdfSharp Anda
                XFont titleFont = new XFont("Arial", 20, XFontStyleEx.Bold);
                XFont headerFont = new XFont("Arial", 12, XFontStyleEx.Bold);
                XFont normalFont = new XFont("Arial", 10, XFontStyleEx.Regular);
                XFont smallFont = new XFont("Arial", 8, XFontStyleEx.Regular);

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

                    string nama = row.Cells["colNamaBahan"].Value?.ToString() ?? "";
                    string stok = row.Cells["colStokSaatIni"].Value?.ToString() ?? "";
                    string harga = row.Cells["colHargaUnit"].Value?.ToString() ?? "";
                    string total = row.Cells["colNilaiTotal"].Value?.ToString() ?? "";
                    string status = row.Cells["colStatus"].Value?.ToString() ?? "";

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

        private void UC_ManajemenInventori_Load_1(object sender, EventArgs e)
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

    // WindowsFontResolver untuk PdfSharp
    public class WindowsFontResolver : IFontResolver
    {
        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            string style = isBold ? "bold" : "regular";
            string key = (familyName + "#" + style).ToLowerInvariant();

            switch (key)
            {
                case "arial#regular": return new FontResolverInfo("arial#regular");
                case "arial#bold": return new FontResolverInfo("arial#bold");
                case "verdana#regular": return new FontResolverInfo("verdana#regular");
                case "verdana#bold": return new FontResolverInfo("verdana#bold");
                case "helvetica#regular": return new FontResolverInfo("arial#regular");
                case "helvetica#bold": return new FontResolverInfo("arial#bold");
                default:
                    return isBold ? new FontResolverInfo("arial#bold") : new FontResolverInfo("arial#regular");
            }
        }

        public byte[] GetFont(string faceName)
        {
            string fileName = null;
            switch (faceName.ToLowerInvariant())
            {
                case "arial#regular": fileName = "arial.ttf"; break;
                case "arial#bold": fileName = "arialbd.ttf"; break;
                case "verdana#regular": fileName = "verdana.ttf"; break;
                case "verdana#bold": fileName = "verdanab.ttf"; break;
                default:
                    var parts = faceName.Split('#');
                    if (parts.Length > 0) fileName = parts[0] + ".ttf";
                    break;
            }

            if (string.IsNullOrEmpty(fileName))
                throw new FileNotFoundException("Font mapping not found for faceName: " + faceName);

            string fontsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Fonts");
            string fullPath = Path.Combine(fontsFolder, fileName);

            if (!File.Exists(fullPath))
            {
                string nameWithoutExt = Path.GetFileNameWithoutExtension(fileName).ToLowerInvariant();
                foreach (var f in Directory.GetFiles(fontsFolder))
                {
                    if (Path.GetFileName(f).ToLowerInvariant().StartsWith(nameWithoutExt))
                    {
                        fullPath = f;
                        break;
                    }
                }
            }

            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"Font file not found: {fileName}. Expected in {fontsFolder}");

            return File.ReadAllBytes(fullPath);
        }
    }
}