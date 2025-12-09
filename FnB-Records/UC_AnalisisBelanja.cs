using FnB_Records.Koneksi_DB;
using Npgsql;
using System;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FnB_Records
{
    public partial class UC_AnalisisBelanja : UserControl
    {
        private Koneksi koneksi = new Koneksi();

        // Email user yang sedang login
        private string userEmail;
        private int userId;
        private string businessName;
        private bool emailVerified;

        public UC_AnalisisBelanja()
        {
            InitializeComponent();
            this.Load += UC_AnalisisBelanja_Load;
        }

        // Ambil profil user lengkap berdasarkan email menggunakan VIEW user_profiles
        private void GetUserProfileByEmail()
        {
            try
            {
                using (var conn = koneksi.GetKoneksi())
                {
                    string query = @"
                        SELECT 
                            id, 
                            business_name, 
                            email_verified
                        FROM user_profiles 
                        WHERE email = @email";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", userEmail);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                userId = reader.GetInt32(0);
                                businessName = reader.GetString(1);
                                emailVerified = reader.GetBoolean(2);

                                Console.WriteLine($"✅ User berhasil dimuat: {businessName} (ID: {userId})");
                            }
                            else
                            {
                                Console.WriteLine($"❌ User dengan email '{userEmail}' tidak ditemukan di database.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error getting user profile: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UC_AnalisisBelanja_Load(object sender, EventArgs e)
        {
            // Ambil email dari GlobalSession Login
            userEmail = Login.GlobalSession.CurrentUserEmail;

            // Debug: Cek apakah email ada
            if (string.IsNullOrEmpty(userEmail))
            {
                MessageBox.Show("Session tidak ditemukan. Email user kosong!\nSilakan login ulang.",
                    "Error Session", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Console.WriteLine($"📧 Email dari GlobalSession: {userEmail}");

            // Load user profile berdasarkan email
            GetUserProfileByEmail();

            if (userId > 0)
            {
                if (!emailVerified)
                {
                    MessageBox.Show("Email Anda belum diverifikasi. Beberapa fitur mungkin terbatas.",
                        "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                LoadDashboardData();
                BuatChartTrendBelanja();
                LoadTopVendors();
            }
            else
            {
                MessageBox.Show($"User dengan email '{userEmail}' tidak ditemukan di database.\n\nPastikan:\n1. Email sudah terdaftar\n2. Database sudah dijalankan\n3. Tabel 'user_profiles' VIEW sudah dibuat",
                    "User Tidak Ditemukan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Load data dashboard (Total PO, Rata-rata, Total Pengeluaran)
        private void LoadDashboardData()
        {
            try
            {
                using (var conn = koneksi.GetKoneksi())
                {
                    // Query Total PO (semua status kecuali draft/cancelled)
                    string queryTotalPO = @"
                        SELECT COUNT(*) 
                        FROM purchases 
                        WHERE user_id = @userId 
                        AND status NOT IN ('draft', 'cancelled')";

                    using (var cmd = new NpgsqlCommand(queryTotalPO, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        int totalPO = Convert.ToInt32(cmd.ExecuteScalar());
                        lbltotalpo.Text = totalPO.ToString();
                    }

                    // Query Rata-rata PO
                    string queryAvgPO = @"
                        SELECT COALESCE(AVG(total_amount), 0) 
                        FROM purchases 
                        WHERE user_id = @userId 
                        AND status NOT IN ('draft', 'cancelled')";

                    using (var cmd = new NpgsqlCommand(queryAvgPO, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        decimal avgPO = Convert.ToDecimal(cmd.ExecuteScalar());
                        lblrata2po.Text = $"Rp {avgPO:N0}";
                    }

                    // Query Total Pengeluaran
                    string queryTotalPengeluaran = @"
                        SELECT COALESCE(SUM(total_amount), 0) 
                        FROM purchases 
                        WHERE user_id = @userId 
                        AND status NOT IN ('draft', 'cancelled')";

                    using (var cmd = new NpgsqlCommand(queryTotalPengeluaran, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        decimal totalPengeluaran = Convert.ToDecimal(cmd.ExecuteScalar());
                        lbltotalpengeluaran.Text = $"Rp {totalPengeluaran:N0}";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading dashboard data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Chart Trend Belanja 6 Bulan Terakhir
        private void BuatChartTrendBelanja()
        {
            try
            {
                // 1. Reset Chart
                charttrendbelanja6bulanterakhir.Series.Clear();
                charttrendbelanja6bulanterakhir.ChartAreas.Clear();
                charttrendbelanja6bulanterakhir.Legends.Clear();

                // --- PENGATURAN FONT CUSTOM ---
                Font fontData;
                Font fontLegend;

                try
                {
                    fontData = new Font("Inter", 8, FontStyle.Regular);
                    fontLegend = new Font("Manrope", 9, FontStyle.Bold);
                }
                catch
                {
                    fontData = new Font("Segoe UI", 8, FontStyle.Regular);
                    fontLegend = new Font("Segoe UI", 9, FontStyle.Bold);
                }

                // --- WARNA TEMA DARK MODE ---
                Color textLight = Color.WhiteSmoke;
                Color gridColor = Color.FromArgb(80, 80, 80);
                Color barColor1 = Color.FromArgb(72, 201, 176); // Hijau Tosca
                Color barColor2 = Color.FromArgb(22, 160, 133); // Hijau Gelap

                // 2. Setup Background Transparan
                charttrendbelanja6bulanterakhir.BackColor = Color.Transparent;

                // 3. Setup Area
                ChartArea area = new ChartArea("AreaBelanja");
                area.BackColor = Color.Transparent;

                // --- SETUP SUMBU Y (Harga) ---
                area.AxisY.LabelStyle.Font = fontData;
                area.AxisY.LabelStyle.ForeColor = textLight;
                area.AxisY.LabelStyle.Format = "Rp #,0";
                area.AxisY.LineColor = Color.Transparent;
                area.AxisY.MajorGrid.LineColor = gridColor;
                area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

                // --- SETUP SUMBU X (Bulan) ---
                area.AxisX.LabelStyle.Font = fontData;
                area.AxisX.LabelStyle.ForeColor = textLight;
                area.AxisX.LineColor = gridColor;
                area.AxisX.MajorGrid.Enabled = false;
                area.AxisX.Interval = 1;

                charttrendbelanja6bulanterakhir.ChartAreas.Add(area);

                // 4. Setup Legend
                Legend legend = new Legend("LegendUtama");
                legend.BackColor = Color.Transparent;
                legend.ForeColor = textLight;
                legend.Font = fontLegend;
                legend.Docking = Docking.Top;
                legend.Alignment = StringAlignment.Center;
                charttrendbelanja6bulanterakhir.Legends.Add(legend);

                // 5. Ambil Data dari Database
                Series series = new Series("Total Pengeluaran");
                series.ChartType = SeriesChartType.Column;
                series.Color = barColor1;
                series.BackSecondaryColor = barColor2;
                series.BackGradientStyle = GradientStyle.TopBottom;
                series.BorderColor = barColor2;
                series.BorderWidth = 1;
                series["PixelPointWidth"] = "50";
                series["DrawingStyle"] = "Cylinder";

                using (var conn = koneksi.GetKoneksi())
                {
                    // Query untuk 6 bulan terakhir (semua status kecuali draft/cancelled)
                    string query = @"
                        SELECT 
                            TO_CHAR(created_at, 'Mon YYYY') as bulan,
                            SUM(total_amount) as total
                        FROM purchases
                        WHERE user_id = @userId
                        AND status NOT IN ('draft', 'cancelled')
                        AND created_at >= NOW() - INTERVAL '6 months'
                        GROUP BY TO_CHAR(created_at, 'Mon YYYY'), DATE_TRUNC('month', created_at)
                        ORDER BY DATE_TRUNC('month', created_at)";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            bool hasData = false;

                            while (reader.Read())
                            {
                                hasData = true;
                                string bulan = reader.GetString(0);
                                decimal total = reader.GetDecimal(1);
                                series.Points.AddXY(bulan, (double)total);
                            }

                            // Jika tidak ada data, tampilkan pesan
                            if (!hasData)
                            {
                                series.Points.AddXY("Belum Ada Data", 0);
                                Console.WriteLine("⚠️ Tidak ada data pembelian dalam 6 bulan terakhir.");
                            }
                        }
                    }
                }

                charttrendbelanja6bulanterakhir.Series.Add(series);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chart data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Load Top Vendors ke DataGridView (sortir berdasarkan email vendor)
        private void LoadTopVendors()
        {
            try
            {
                using (var conn = koneksi.GetKoneksi())
                {
                    // Top 5 Vendors berdasarkan total amount
                    string query = @"
                        SELECT 
                            v.name as vendor_name,
                            v.contact as email,
                            COUNT(p.id) as total_orders,
                            COALESCE(SUM(p.total_amount), 0) as total_amount
                        FROM vendors v
                        LEFT JOIN purchases p ON p.vendor_id = v.id 
                            AND p.user_id = @userId 
                            AND p.status NOT IN ('draft', 'cancelled')
                        WHERE v.user_id = @userId
                        GROUP BY v.id, v.name, v.contact
                        ORDER BY total_amount DESC
                        LIMIT 5";

                    using (var adapter = new NpgsqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@userId", userId);

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvtopVendor.DataSource = dt;

                        // Formatting kolom
                        if (dgvtopVendor.Columns.Count > 0)
                        {
                            dgvtopVendor.Columns["vendor_name"].HeaderText = "Vendor";
                            dgvtopVendor.Columns["email"].HeaderText = "Email";
                            dgvtopVendor.Columns["total_orders"].HeaderText = "Total Orders";
                            dgvtopVendor.Columns["total_amount"].HeaderText = "Total Amount";
                            dgvtopVendor.Columns["total_amount"].DefaultCellStyle.Format = "Rp #,0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading vendor data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEksporLaporancsv_Click(object sender, EventArgs e)
        {
            if (userId <= 0)
            {
                MessageBox.Show("User tidak teridentifikasi. Silakan login ulang.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Dialog untuk simpan file
                SaveFileDialog saveDialog = new SaveFileDialog();
                saveDialog.Filter = "CSV File (*.csv)|*.csv";
                saveDialog.Title = "Ekspor Laporan Analisis Belanja";
                saveDialog.FileName = $"Laporan_Belanja_{DateTime.Now:yyyyMMdd_HHmmss}.csv";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, Encoding.UTF8))
                    {
                        // Header CSV
                        writer.WriteLine("LAPORAN ANALISIS BELANJA");
                        writer.WriteLine($"Business: {businessName}");
                        writer.WriteLine($"Email: {userEmail}");
                        writer.WriteLine($"Tanggal Ekspor: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                        writer.WriteLine();

                        // === RINGKASAN DASHBOARD ===
                        writer.WriteLine("=== RINGKASAN DASHBOARD ===");
                        writer.WriteLine("Metrik,Nilai");
                        writer.WriteLine($"Total Purchase Orders,{lbltotalpo.Text}");
                        writer.WriteLine($"Rata-rata PO,{lblrata2po.Text}");
                        writer.WriteLine($"Total Pengeluaran,{lbltotalpengeluaran.Text}");
                        writer.WriteLine();

                        // === TREND BELANJA 6 BULAN TERAKHIR ===
                        writer.WriteLine("=== TREND BELANJA 6 BULAN TERAKHIR ===");
                        writer.WriteLine("Bulan,Total Pengeluaran (Rp)");

                        using (var conn = koneksi.GetKoneksi())
                        {
                            string queryTrend = @"
                                SELECT 
                                    TO_CHAR(created_at, 'Mon YYYY') as bulan,
                                    SUM(total_amount) as total
                                FROM purchases
                                WHERE user_id = @userId
                                AND status NOT IN ('draft', 'cancelled')
                                AND created_at >= NOW() - INTERVAL '6 months'
                                GROUP BY TO_CHAR(created_at, 'Mon YYYY'), DATE_TRUNC('month', created_at)
                                ORDER BY DATE_TRUNC('month', created_at)";

                            using (var cmd = new NpgsqlCommand(queryTrend, conn))
                            {
                                cmd.Parameters.AddWithValue("@userId", userId);

                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string bulan = reader.GetString(0);
                                        decimal total = reader.GetDecimal(1);
                                        writer.WriteLine($"{bulan},{total:N0}");
                                    }
                                }
                            }
                        }
                        writer.WriteLine();

                        // === TOP 5 VENDORS ===
                        writer.WriteLine("=== TOP 5 VENDORS ===");
                        writer.WriteLine("Nama Vendor,Email,Total Orders,Total Amount (Rp)");

                        using (var conn = koneksi.GetKoneksi())
                        {
                            string queryVendors = @"
                                SELECT 
                                    v.name as vendor_name,
                                    v.contact as email,
                                    COUNT(p.id) as total_orders,
                                    COALESCE(SUM(p.total_amount), 0) as total_amount
                                FROM vendors v
                                LEFT JOIN purchases p ON p.vendor_id = v.id 
                                    AND p.user_id = @userId 
                                    AND p.status NOT IN ('draft', 'cancelled')
                                WHERE v.user_id = @userId
                                GROUP BY v.id, v.name, v.contact
                                ORDER BY total_amount DESC
                                LIMIT 5";

                            using (var cmd = new NpgsqlCommand(queryVendors, conn))
                            {
                                cmd.Parameters.AddWithValue("@userId", userId);

                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        string vendorName = reader.GetString(0);
                                        string email = reader.IsDBNull(1) ? "-" : reader.GetString(1);
                                        int totalOrders = reader.GetInt32(2);
                                        decimal totalAmount = reader.GetDecimal(3);

                                        // Escape koma dalam nama vendor
                                        vendorName = vendorName.Replace(",", ";");
                                        email = email.Replace(",", ";");

                                        writer.WriteLine($"{vendorName},{email},{totalOrders},{totalAmount:N0}");
                                    }
                                }
                            }
                        }
                        writer.WriteLine();

                        // === DETAIL PEMBELIAN TERBARU (30 hari terakhir) ===
                        writer.WriteLine("=== DETAIL PEMBELIAN 30 HARI TERAKHIR ===");
                        writer.WriteLine("Tanggal,PO Number,Vendor,Status,Total Amount (Rp)");

                        using (var conn = koneksi.GetKoneksi())
                        {
                            string queryDetail = @"
                                SELECT 
                                    p.created_at,
                                    p.po_number,
                                    v.name as vendor_name,
                                    p.status,
                                    p.total_amount
                                FROM purchases p
                                LEFT JOIN vendors v ON v.id = p.vendor_id
                                WHERE p.user_id = @userId
                                AND p.created_at >= NOW() - INTERVAL '30 days'
                                ORDER BY p.created_at DESC
                                LIMIT 100";

                            using (var cmd = new NpgsqlCommand(queryDetail, conn))
                            {
                                cmd.Parameters.AddWithValue("@userId", userId);

                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        DateTime tanggal = reader.GetDateTime(0);
                                        string poNumber = reader.GetString(1);
                                        string vendorName = reader.IsDBNull(2) ? "-" : reader.GetString(2);
                                        string status = reader.GetString(3);
                                        decimal totalAmount = reader.GetDecimal(4);

                                        vendorName = vendorName.Replace(",", ";");

                                        writer.WriteLine($"{tanggal:dd/MM/yyyy},{poNumber},{vendorName},{status},{totalAmount:N0}");
                                    }
                                }
                            }
                        }
                    }

                    MessageBox.Show($"Laporan berhasil diekspor ke:\n{saveDialog.FileName}",
                        "Ekspor Berhasil", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Buka folder tempat file disimpan
                    System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{saveDialog.FileName}\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saat ekspor CSV: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cbsemuawaktu_SelectedIndexChanged(object sender, EventArgs e)
        {
            // TODO: Filter berdasarkan waktu yang dipilih
            // Reload chart dan data sesuai filter
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e) { }
        private void lbltotalpo_Click(object sender, EventArgs e) { }
        private void lblrata2po_Click(object sender, EventArgs e) { }
        private void lbltotalpengeluaran_Click(object sender, EventArgs e) { }
        private void charttrendbelanja6bulanterakhir_Click(object sender, EventArgs e) { }
        private void chartbreakdownpengeluaranperkategori_Click(object sender, EventArgs e) { }
        private void chart5bahanbakuberdasarpembelian_Click(object sender, EventArgs e) { }
        private void guna2GroupBox8_Click(object sender, EventArgs e) { }
        private void dgvtopVendor_CellContentClick(object sender, DataGridViewCellEventArgs e) { }

        private void UC_AnalisisBelanja_Load_1(object sender, EventArgs e)
        {

        }
    }
}