using Guna.UI2.WinForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Npgsql;
using FnB_Records.Koneksi_DB;
using System.Data;

namespace FnB_Records
{
    public partial class UCDashboard : UserControl
    {
        // Variabel untuk Chatbot
        private DateTime lastRequestTime = DateTime.MinValue;
        private int requestCount = 0;
        private DateTime requestCountResetTime = DateTime.Now;
        private bool isTyping = false;

        public UCDashboard()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // Setup Panel Chat dengan AutoScroll
            if (pnlChatContainer != null)
            {
                pnlChatContainer.BackColor = Color.FromArgb(45, 45, 48);
                pnlChatContainer.AutoScroll = true; // Enable scroll
                pnlChatContainer.Padding = new Padding(0, 0, 5, 0); // Space untuk scrollbar

                typeof(Panel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, pnlChatContainer, new object[] { true });
            }

            // Setup placeholder text
            txtpromp.Text = "Silahkan tanya Bibot...";
            txtpromp.ForeColor = Color.Gray;

            txtpromp.Enter += (s, e) =>
            {
                if (txtpromp.Text == "Silahkan tanya Bibot...")
                {
                    txtpromp.Text = "";
                    txtpromp.ForeColor = Color.White;
                }
            };

            txtpromp.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtpromp.Text))
                {
                    txtpromp.Text = "Silahkan tanya Bibot...";
                    txtpromp.ForeColor = Color.Gray;
                }
            };

            txtpromp.KeyDown += async (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await HandleSendAction();
                }
            };
        }


        private async void UCDashboard_Load(object sender, EventArgs e)
        {
            LoadHeaderInfo();
            RefreshDashboardData();
            await AddChatBubbleTypingEffect($"Halo {Login.GlobalSession.BusinessName}! 👋 Saya Bibot.\nLaporan hari ini sudah siap. Ada yang bisa saya bantu?", false);
        }

        private void btRefresh_Click(object sender, EventArgs e)
        {
            RefreshDashboardData();
        }

        private void RefreshDashboardData()
        {
            try
            {
                LoadSummaryCards();
                LoadKPICards();
                LoadLineChart();
                LoadPieChart();
                LoadBestSellerMenu();
                LoadLowStockAlert();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data dashboard: " + ex.Message);
            }
        }

        private void LoadHeaderInfo()
        {
            lblNamaBisnis.Text = Login.GlobalSession.BusinessName;
            lblEmail.Text = Login.GlobalSession.CurrentUserEmail;
            lblStatusRole.Text = Login.GlobalSession.CurrentUserRole;
        }

        // --- PERBAIKAN DI SINI: MENGHAPUS conn.Open() ---

        private void LoadSummaryCards()
        {
            Koneksi db = new Koneksi();
            using (NpgsqlConnection conn = db.GetKoneksi()) // GetKoneksi sudah Open otomatis
            {
                // conn.Open(); <--- HAPUS INI

                string sql = @"
                    SELECT 
                        COALESCE(COUNT(id), 0) as total_transaksi,
                        COALESCE(SUM(revenue), 0) as total_pendapatan,
                        COALESCE(SUM(profit), 0) as total_laba
                    FROM sales 
                    WHERE user_id = @uid AND sale_date = CURRENT_DATE";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            long count = reader.GetInt64(0);
                            double revenue = reader.GetDouble(1);
                            double profit = reader.GetDouble(2);
                            double margin = (revenue > 0) ? (profit / revenue) * 100 : 0;

                            lblPenjualan.Text = count.ToString();
                            lblPendapatan.Text = "Rp " + revenue.ToString("N0");
                            lblLaba.Text = "Rp " + profit.ToString("N0");
                            lblMargin.Text = margin.ToString("0.0") + "%";
                        }
                    }
                }
            }
        }

        private void LoadKPICards()
        {
            Koneksi db = new Koneksi();
            using (NpgsqlConnection conn = db.GetKoneksi())
            {
                // conn.Open(); <--- HAPUS INI

                string sql = @"
                    SELECT 
                        COALESCE(AVG(revenue), 0) as avg_order_value,
                        COALESCE(COUNT(id), 0) as total_transaksi_all,
                        COALESCE(SUM(revenue), 0) as total_revenue_all,
                        COALESCE(SUM(profit), 0) as total_profit_all
                    FROM sales 
                    WHERE user_id = @uid AND sale_date >= DATE_TRUNC('year', CURRENT_DATE)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            double aov = reader.GetDouble(0);
                            long totalTx = reader.GetInt64(1);
                            double totRev = reader.GetDouble(2);
                            double totProf = reader.GetDouble(3);

                            int currentMonth = DateTime.Now.Month;
                            double avgMonthly = (currentMonth > 0) ? totRev / currentMonth : 0;
                            double marginYTD = (totRev > 0) ? (totProf / totRev) * 100 : 0;

                            lblAvgOrderValue.Text = "Rp " + aov.ToString("N0");
                            lblTotalTransaksi.Text = totalTx.ToString() + " Transaksi";
                            lblProfitMargin.Text = marginYTD.ToString("0.0") + " %";
                            lblAvgRevenueBulanan.Text = "Rp " + avgMonthly.ToString("N0");
                        }
                    }
                }
            }
        }

        private void LoadLineChart()
        {
            chartGrafikPenjualanBulanan.Series.Clear();
            chartGrafikPenjualanBulanan.ChartAreas.Clear();
            chartGrafikPenjualanBulanan.Legends.Clear();
            chartGrafikPenjualanBulanan.BackColor = Color.Transparent;

            ChartArea area = new ChartArea("AreaPenjualan");
            area.BackColor = Color.Transparent;
            area.AxisX.LabelStyle.ForeColor = Color.LightGray;
            area.AxisX.LineColor = Color.Gray;
            area.AxisX.MajorGrid.LineColor = Color.FromArgb(60, 60, 60);
            area.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            area.AxisY.LabelStyle.ForeColor = Color.LightGray;
            area.AxisY.MajorGrid.LineColor = Color.FromArgb(70, 70, 70);
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;
            area.AxisY.LabelStyle.Format = "{0:N0}";
            chartGrafikPenjualanBulanan.ChartAreas.Add(area);

            Series sRev = new Series("Pendapatan") { ChartType = SeriesChartType.Spline, BorderWidth = 3, Color = Color.FromArgb(100, 149, 237) };
            Series sProf = new Series("Laba") { ChartType = SeriesChartType.Spline, BorderWidth = 3, Color = Color.FromArgb(50, 205, 50) };

            Koneksi db = new Koneksi();
            using (NpgsqlConnection conn = db.GetKoneksi())
            {
                // conn.Open(); <--- HAPUS INI

                string sql = @"
                    SELECT 
                        TO_CHAR(sale_date, 'Mon') as bulan,
                        SUM(revenue) as revenue,
                        SUM(profit) as profit
                    FROM sales
                    WHERE user_id = @uid AND sale_date >= DATE_TRUNC('year', CURRENT_DATE)
                    GROUP BY TO_CHAR(sale_date, 'Mon'), EXTRACT(MONTH FROM sale_date)
                    ORDER BY EXTRACT(MONTH FROM sale_date)";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string bulan = reader.GetString(0);
                            double rev = reader.GetDouble(1);
                            double prof = reader.GetDouble(2);

                            sRev.Points.AddXY(bulan, rev);
                            sProf.Points.AddXY(bulan, prof);
                        }
                    }
                }
            }

            chartGrafikPenjualanBulanan.Series.Add(sProf);
            chartGrafikPenjualanBulanan.Series.Add(sRev);

            Legend l = new Legend("Legend1") { BackColor = Color.Transparent, ForeColor = Color.LightGray, Docking = Docking.Top };
            chartGrafikPenjualanBulanan.Legends.Add(l);
        }

        private void LoadPieChart()
        {
            chartDashboard.Series.Clear();
            chartDashboard.ChartAreas.Clear();
            chartDashboard.Legends.Clear();
            chartDashboard.BackColor = Color.Transparent;

            ChartArea area = new ChartArea("Area1") { BackColor = Color.Transparent };
            area.Position = new ElementPosition(0, 0, 100, 85);
            chartDashboard.ChartAreas.Add(area);

            Series series = new Series("Kategori") { ChartType = SeriesChartType.Pie, ChartArea = "Area1" };
            series["PieLabelStyle"] = "Outside";
            series.Label = "#VALX: #PERCENT";
            series.LabelForeColor = Color.White;

            Koneksi db = new Koneksi();
            using (NpgsqlConnection conn = db.GetKoneksi())
            {
                // conn.Open(); <--- HAPUS INI

                // Pastikan query ini sesuai dengan struktur tabel Anda
                string sql = @"
                    SELECT r.name, SUM(s.qty) as total_qty
                    FROM sales s
                    JOIN recipes r ON s.recipe_id = r.id
                    WHERE s.user_id = @uid
                    GROUP BY r.name
                    ORDER BY total_qty DESC
                    LIMIT 5";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            series.Points.AddXY(reader.GetString(0), reader.GetInt32(1));
                        }
                    }
                }
            }

            chartDashboard.Series.Add(series);
            Legend l = new Legend("Legend1") { BackColor = Color.Transparent, ForeColor = Color.LightGray, Docking = Docking.Bottom };
            chartDashboard.Legends.Add(l);
        }

        private void LoadBestSellerMenu()
        {
            pnlMenuTerlaris.Controls.Clear();
            FlowLayoutPanel flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent
            };
            pnlMenuTerlaris.Controls.Add(flow);

            Koneksi db = new Koneksi();
            using (NpgsqlConnection conn = db.GetKoneksi())
            {
                // conn.Open(); <--- HAPUS INI

                string sql = @"
                    SELECT r.name, SUM(s.qty) as terjual, SUM(s.revenue) as omset
                    FROM sales s
                    JOIN recipes r ON s.recipe_id = r.id
                    WHERE s.user_id = @uid
                    GROUP BY r.name
                    ORDER BY terjual DESC
                    LIMIT 5";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        int rank = 1;
                        while (reader.Read())
                        {
                            string menuName = reader.GetString(0);
                            long sold = reader.GetInt64(1);

                            Label lblItem = new Label();
                            lblItem.Text = $"{rank}. {menuName} ({sold} terjual)";
                            lblItem.ForeColor = Color.Black;
                            lblItem.Font = new Font("Inter; 14,25pt", 13, FontStyle.Bold);
                            lblItem.AutoSize = true;
                            lblItem.Margin = new Padding(10, 5, 0, 5);

                            flow.Controls.Add(lblItem);
                            rank++;
                        }
                    }
                }
            }
        }

        private void LoadLowStockAlert()
        {
            flowLayoutPanel1.Controls.Clear();

            Koneksi db = new Koneksi();
            using (NpgsqlConnection conn = db.GetKoneksi())
            {
                // conn.Open(); <--- HAPUS INI

                string sql = "SELECT name, stock, unit FROM ingredients WHERE user_id = @uid AND stock <= min_stock";

                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            Label lblAman = new Label { Text = "Stok Aman 👍", ForeColor = Color.LightGreen, AutoSize = true, Font = new Font("Segoe UI", 14) };
                            flowLayoutPanel1.Controls.Add(lblAman);
                            return;
                        }

                        while (reader.Read())
                        {
                            string name = reader.GetString(0);
                            double stock = reader.GetDouble(1);
                            string unit = reader.GetString(2);

                            Label lblAlert = new Label();
                            lblAlert.Text = $"⚠️ {name}: Sisa {stock} {unit}";
                            lblAlert.ForeColor = Color.Salmon;
                            lblAlert.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                            lblAlert.AutoSize = true;
                            lblAlert.Margin = new Padding(3, 3, 3, 5);

                            flowLayoutPanel1.Controls.Add(lblAlert);
                        }
                    }
                }
            }
        }

        // --- BAGIAN CHATBOT TIDAK PERLU DIUBAH ---
        private async void btnsend_Click_1(object sender, EventArgs e)
        {
            await HandleSendAction();
        }

        private async Task HandleSendAction()
        {
            string prompt = txtpromp.Text.Trim();
            if (string.IsNullOrEmpty(prompt) || prompt == "Silahkan tanya Bibot...") return;
            if (isTyping) return;

            AddChatBubble(prompt, true);
            txtpromp.Clear();
            txtpromp.Focus();

            if (!CheckRateLimit()) return;

            isTyping = true;
            ChatBubble loadingBubble = AddChatBubble("Sedang menganalisa data bisnis...", false);

            try
            {
                string response = await AskGemini(prompt);
                pnlChatContainer.Controls.Remove(loadingBubble);
                loadingBubble.Dispose();
                await AddChatBubbleTypingEffect(response, false);
            }
            catch (Exception ex)
            {
                pnlChatContainer.Controls.Remove(loadingBubble);
                AddChatBubble("Error: " + ex.Message, false);
            }
            finally { isTyping = false; }
        }

        private ChatBubble AddChatBubble(string text, bool isUser)
        {
            var bubble = new ChatBubble(text, isUser, pnlChatContainer.Width - 50);
            pnlChatContainer.Controls.Add(bubble);
            pnlChatContainer.ScrollControlIntoView(bubble);
            return bubble;
        }

        private async Task AddChatBubbleTypingEffect(string fullText, bool isUser)
        {
            var bubble = new ChatBubble("", isUser, pnlChatContainer.Width - 50);
            pnlChatContainer.Controls.Add(bubble);
            StringBuilder sb = new StringBuilder();
            foreach (char c in fullText)
            {
                sb.Append(c);
                bubble.UpdateText(sb.ToString());
                pnlChatContainer.ScrollControlIntoView(bubble);
                int delay = (c == ' ' || c == '.') ? 50 : 20;
                await Task.Delay(delay);
            }
        }

        private bool CheckRateLimit()
        {
            if ((DateTime.Now - requestCountResetTime).TotalMinutes >= 1)
            {
                requestCount = 0;
                requestCountResetTime = DateTime.Now;
            }
            if (requestCount >= 10)
            {
                MessageBox.Show("Tunggu sebentar, limit request tercapai.", "Info");
                return false;
            }
            requestCount++;
            return true;
        }

        private async Task<string> AskGemini(string prompt)
        {
            string apiKey = "AIzaSyCRCiVBrZeWLGBOFAF_fwmbFhY1lv37gwk";
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            string context = $"Nama Bisnis: {Login.GlobalSession.BusinessName}. Saya adalah pemilik bisnis F&B. Jawablah terkait manajemen stok, penjualan, atau strategi marketing.";
            string fullPrompt = $"{context}\nUser bertanya: {prompt}\nJawab:";

            var payload = new { contents = new[] { new { parts = new[] { new { text = fullPrompt } } } } };
            string jsonPayload = JsonSerializer.Serialize(payload);

            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return "Maaf, koneksi ke server AI terputus.";

                using (JsonDocument doc = JsonDocument.Parse(result))
                {
                    if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidates) && candidates.GetArrayLength() > 0)
                    {
                        return candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    }
                    return "Maaf, saya tidak mengerti.";
                }
            }
        }

        private void chartDashboard_Click(object sender, EventArgs e) { LoadPieChart(); }
        private void richTextBoxAi_TextChanged(object sender, EventArgs e) { }
        private void txtpromp_TextChanged(object sender, EventArgs e) { }
        private void guna2GroupBox4_Click(object sender, EventArgs e) { }
        private void guna2GroupBox8_Click(object sender, EventArgs e) { }
        private void guna2GroupBox12_Click(object sender, EventArgs e) { }
        private void txtpromp_TextChanged_1(object sender, EventArgs e) { }
        private void txtpromp_TextChanged_2(object sender, EventArgs e) { }

        private void pnlChatContainer_Paint(object sender, PaintEventArgs e)
        {

        }
    }

    public class ChatBubble : Panel
    {
        private Label lblText;
        private bool _isUser;
        private Color _bgColor;
        private int _maxWidth;

        public ChatBubble(string message, bool isUser, int containerWidth)
        {
            _isUser = isUser;
            _maxWidth = containerWidth;
            DoubleBuffered = true;
            Padding = new Padding(10);
            Margin = new Padding(10, 5, 10, 5);
            BackColor = Color.Transparent;
            _bgColor = isUser ? Color.FromArgb(0, 122, 204) : Color.FromArgb(60, 60, 60);

            lblText = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                MaximumSize = new Size(_maxWidth * 2 / 3, 0)
            };
            Controls.Add(lblText);
            ResizeBubble();
        }

        public void UpdateText(string newText)
        {
            lblText.Text = newText;
            ResizeBubble();
            Invalidate();
        }

        private void ResizeBubble()
        {
            Size textSize = TextRenderer.MeasureText(lblText.Text, lblText.Font, lblText.MaximumSize, TextFormatFlags.WordBreak);
            Size = new Size(_maxWidth, textSize.Height + 40);
            if (_isUser) lblText.Location = new Point(Width - textSize.Width - 30, 15);
            else lblText.Location = new Point(15, 15);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Size textSize = TextRenderer.MeasureText(lblText.Text, lblText.Font, lblText.MaximumSize, TextFormatFlags.WordBreak);
            Rectangle bubbleRect = _isUser ?
                new Rectangle(Width - textSize.Width - 40, 5, textSize.Width + 30, textSize.Height + 20) :
                new Rectangle(5, 5, textSize.Width + 30, textSize.Height + 20);

            using (GraphicsPath path = GetRoundedPath(bubbleRect, 15))
            using (SolidBrush brush = new SolidBrush(_bgColor))
            {
                e.Graphics.FillPath(brush, path);
            }
        }


        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.X + rect.Width - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.X + rect.Width - d, rect.Y + rect.Height - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}