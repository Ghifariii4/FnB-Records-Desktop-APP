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

namespace FnB_Records
{
    public partial class UCDashboard : UserControl
    {
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
            if (pnlChatContainer != null)
            {
                pnlChatContainer.BackColor = Color.FromArgb(45, 45, 48);

                typeof(Panel).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic,
                    null, pnlChatContainer, new object[] { true });
            }

            txtpromp.Text = "Silahkan tanya Bibot...";
            txtpromp.ForeColor = Color.Gray;

            txtpromp.Enter += (s, e) => {
                if (txtpromp.Text == "Silahkan tanya Bibot...")
                {
                    txtpromp.Text = "";
                    txtpromp.ForeColor = Color.White;
                }
            };

            txtpromp.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(txtpromp.Text))
                {
                    txtpromp.Text = "Silahkan tanya Bibot...";
                    txtpromp.ForeColor = Color.Gray;
                }
            };

            txtpromp.KeyDown += async (s, e) => {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    await HandleSendAction();
                }
            };
        }

        private async void UCDashboard_Load(object sender, EventArgs e)
        {
            LoadPieChart();
            LoadGrafikPenjualanBulanan();

            await AddChatBubbleTypingEffect("Halo! 👋 Saya Bibot, asisten AI FnB Records.\nAda yang bisa saya bantu hari ini?", false);
        }

        private void LoadPieChart()
        {
            if (chartDashboard == null) return;

            chartDashboard.SuspendLayout();
            try
            {
                chartDashboard.Series.Clear();
                chartDashboard.Legends.Clear();
                chartDashboard.ChartAreas.Clear();

                chartDashboard.AntiAliasing = AntiAliasingStyles.All;
                chartDashboard.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

                var area = new ChartArea("ChartArea1");
                area.BackColor = Color.FromArgb(45, 45, 45);
                area.BackSecondaryColor = Color.FromArgb(45, 45, 45);
                area.BackGradientStyle = GradientStyle.None;
                area.Position = new ElementPosition(0, 0, 100, 85);
                chartDashboard.ChartAreas.Add(area);

                var legend = new Legend("Legend1");
                legend.BackColor = Color.FromArgb(45, 45, 45);
                legend.ForeColor = Color.White;
                legend.Docking = Docking.Bottom;
                legend.IsTextAutoFit = false;
                chartDashboard.Legends.Add(legend);

                var series = new Series("Kategori");
                series.ChartType = SeriesChartType.Pie;
                series["PieLabelStyle"] = "Disabled";
                series["PieDrawingStyle"] = "SoftEdge";

                int makanan = 150;
                int minuman = 200;
                int dessert = 80;
                int snack = 120;
                int dll = 60;

                series.Points.AddXY("Makanan", makanan);
                series.Points.AddXY("Minuman", minuman);
                series.Points.AddXY("Dessert", dessert);
                series.Points.AddXY("Snack", snack);
                series.Points.AddXY("Dll", dll);

                series.Points[0].Color = Color.RoyalBlue;
                series.Points[1].Color = Color.LightGreen;
                series.Points[2].Color = Color.Yellow;
                series.Points[3].Color = Color.Orange;
                series.Points[4].Color = Color.MediumPurple;

                chartDashboard.Series.Add(series);
            }
            finally
            {
                chartDashboard.ResumeLayout();
                chartDashboard.Invalidate();
            }
        }

        private void LoadGrafikPenjualanBulanan()
        {
            if (chartpenjualan == null) return;

            chartpenjualan.SuspendLayout();
            try
            {
                chartpenjualan.Series.Clear();
                chartpenjualan.ChartAreas.Clear();
                chartpenjualan.Legends.Clear();

                chartpenjualan.AntiAliasing = AntiAliasingStyles.All;
                chartpenjualan.TextAntiAliasingQuality = TextAntiAliasingQuality.High;

                var area = new ChartArea("ChartArea1");
                area.BackColor = Color.FromArgb(45, 45, 45);
                area.AxisX.LineColor = Color.White;
                area.AxisY.LineColor = Color.White;

                area.AxisX.LabelStyle.ForeColor = Color.White;
                area.AxisY.LabelStyle.ForeColor = Color.White;

                area.AxisX.MajorGrid.LineColor = Color.FromArgb(80, Color.Gray);
                area.AxisY.MajorGrid.LineColor = Color.FromArgb(80, Color.Gray);
                chartpenjualan.ChartAreas.Add(area);

                var series = new Series("Penjualan Bulanan");
                series.ChartType = SeriesChartType.Spline;
                series.Color = Color.DeepSkyBlue;
                series.BorderWidth = 3;
                series.MarkerStyle = MarkerStyle.Circle;
                series.MarkerSize = 7;
                series.MarkerColor = Color.White;

                Dictionary<string, int> dataBulanan = new Dictionary<string, int>()
                {
                    { "Jan", 120 }, { "Feb", 150 }, { "Mar", 180 },
                    { "Apr", 200 }, { "Mei", 240 }, { "Jun", 300 },
                    { "Jul", 320 }, { "Agu", 310 }, { "Sep", 350 },
                    { "Okt", 400 }, { "Nov", 420 }, { "Des", 500 }
                };

                foreach (var item in dataBulanan)
                {
                    series.Points.AddXY(item.Key, item.Value);
                }

                chartpenjualan.Series.Add(series);
            }
            finally
            {
                chartpenjualan.ResumeLayout();
                chartpenjualan.Invalidate();
            }
        }

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
            ChatBubble loadingBubble = AddChatBubble("Sedang berpikir...", false);

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
            finally
            {
                isTyping = false;
            }
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

                int delay = (c == ' ' || c == '.') ? 100 : 80;
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
            string apiKey = "MASUKKAN_API_KEY_KAMU_DISINI";
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={apiKey}";

            string systemPrompt = "Kamu adalah Bibot, asisten bisnis F&B. Jawab dengan sopan, format rapi, gunakan emoji.";
            string fullPrompt = systemPrompt + "\nUser: " + prompt + "\nBibot:";

            var payload = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = fullPrompt } } }
                }
            };

            string jsonPayload = JsonSerializer.Serialize(payload);

            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, content);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode) return "Maaf, ada gangguan koneksi ke server AI.";

                using (JsonDocument doc = JsonDocument.Parse(result))
                {
                    if (doc.RootElement.TryGetProperty("candidates", out JsonElement candidates) && candidates.GetArrayLength() > 0)
                    {
                        return candidates[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString();
                    }
                    return "Maaf, saya tidak mengerti pertanyaan itu.";
                }
            }
        }

        private void chartDashboard_Click(object sender, EventArgs e)
        {
            LoadPieChart();
        }

        private void chart1_Click(object sender, EventArgs e)
        {
            LoadGrafikPenjualanBulanan();
        }

        private void richTextBoxAi_TextChanged(object sender, EventArgs e) { }
        private void txtpromp_TextChanged(object sender, EventArgs e) { }
        private void guna2GroupBox4_Click(object sender, EventArgs e) { }
        private void guna2GroupBox8_Click(object sender, EventArgs e) { }
        private void guna2GroupBox12_Click(object sender, EventArgs e) { }
        private void txtpromp_TextChanged_1(object sender, EventArgs e) { }
        private void txtpromp_TextChanged_2(object sender, EventArgs e) { }
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

            this.DoubleBuffered = true;
            this.Padding = new Padding(10);
            this.Margin = new Padding(10, 5, 10, 5);
            this.BackColor = Color.Transparent;

            _bgColor = isUser ? Color.FromArgb(0, 122, 204) : Color.FromArgb(60, 60, 60);

            lblText = new Label();
            lblText.Text = message;
            lblText.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            lblText.ForeColor = Color.White;
            lblText.AutoSize = true;
            lblText.MaximumSize = new Size(_maxWidth * 2 / 3, 0);

            lblText.Click += (s, e) => this.OnClick(e);

            this.Controls.Add(lblText);

            ResizeBubble();
        }

        public void UpdateText(string newText)
        {
            lblText.Text = newText;
            ResizeBubble();
            this.Invalidate();
        }

        private void ResizeBubble()
        {
            Size textSize = TextRenderer.MeasureText(lblText.Text, lblText.Font, lblText.MaximumSize, TextFormatFlags.WordBreak);

            int bubbleWidth = textSize.Width + 20;
            int bubbleHeight = textSize.Height + 20;

            this.Size = new Size(_maxWidth, bubbleHeight + 10);

            if (_isUser)
            {
                lblText.Location = new Point(this.Width - bubbleWidth - 5 + 10, 5 + 10);
            }
            else
            {
                lblText.Location = new Point(5 + 10, 5 + 10);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Size textSize = TextRenderer.MeasureText(lblText.Text, lblText.Font, lblText.MaximumSize, TextFormatFlags.WordBreak);
            int w = textSize.Width + 25;
            int h = textSize.Height + 25;

            Rectangle bubbleRect;

            if (_isUser)
            {
                bubbleRect = new Rectangle(this.Width - w - 5, 5, w, h);
            }
            else
            {
                bubbleRect = new Rectangle(5, 5, w, h);
            }

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