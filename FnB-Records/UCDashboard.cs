using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
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
        // Tracking untuk rate limiting
        private DateTime lastRequestTime = DateTime.MinValue;
        private int requestCount = 0;
        private DateTime requestCountResetTime = DateTime.Now;
        private bool isTyping = false; // Flag untuk cek apakah sedang typing

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadPieChart();
           // LoadGrafikPenjualanBulanan();
        }


        public UCDashboard()
        {
            InitializeComponent();

            // Set placeholder text untuk txtprompt
            txtprompt.Text = "Silahkan tanya Bibot...";
            txtprompt.ForeColor = Color.Gray;

            // Event untuk menghilangkan placeholder saat fokus
            txtprompt.Enter += (s, e) =>
            {
                if (txtprompt.Text == "Silahkan tanya Bibot...")
                {
                    txtprompt.Text = "";
                    txtprompt.ForeColor = Color.Black;
                }
            };

            // Event untuk menampilkan kembali placeholder saat kehilangan fokus
            txtprompt.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtprompt.Text))
                {
                    txtprompt.Text = "Silahkan tanya Bibot...";
                    txtprompt.ForeColor = Color.Gray;
                }
            };
        }

        private void LoadPieChart()
        {
            chartDashboard.Series.Clear();
            chartDashboard.Legends.Clear();
            chartDashboard.ChartAreas.Clear();

            // === ChartArea ===
            var area = new ChartArea("ChartArea1");
            area.BackColor = Color.FromArgb(45, 45, 45);
            area.BackSecondaryColor = Color.FromArgb(45, 45, 45);
            area.BackGradientStyle = GradientStyle.None;
            chartDashboard.ChartAreas.Add(area);

            // === Legend ===
            var legend = new Legend("Legend1");
            legend.BackColor = Color.FromArgb(45, 45, 45);
            legend.ForeColor = Color.White;
            chartDashboard.Legends.Add(legend);

            // === Series ===
            var series = new Series("Kategori");
            series.ChartType = SeriesChartType.Pie;

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

            series["PieLabelStyle"] = "Disabled";

            chartDashboard.Series.Add(series);

        }


        // Fungsi untuk typing animation seperti ChatGPT
        private async Task TypeText(string text, int delayPerChar = 15)
        {
            isTyping = true;

            foreach (char c in text)
            {
                richTextBoxAi.AppendText(c.ToString());
                richTextBoxAi.SelectionStart = richTextBoxAi.TextLength;
                richTextBoxAi.ScrollToCaret();

                // Delay lebih pendek untuk spasi dan newline agar lebih natural
                if (c == ' ')
                    await Task.Delay(5);
                else if (c == '\n')
                    await Task.Delay(10);
                else
                    await Task.Delay(delayPerChar);
            }

            isTyping = false;
        }

        private void LoadGrafikPenjualanBulanan()
        {
            chartpenjualan.Series.Clear();
            chartpenjualan.ChartAreas.Clear();

            // Chart Area
            var area = new ChartArea("ChartArea1");
            area.BackColor = Color.FromArgb(45, 45, 45);
            area.AxisX.LineColor = Color.White;
            area.AxisY.LineColor = Color.White;

            area.AxisX.LabelStyle.ForeColor = Color.White;
            area.AxisY.LabelStyle.ForeColor = Color.White;

            area.AxisX.MajorGrid.LineColor = Color.Gray;
            area.AxisY.MajorGrid.LineColor = Color.Gray;

            chartpenjualan.ChartAreas.Add(area);

            // Series
            var series = new Series("Penjualan Bulanan");
            series.ChartType = SeriesChartType.Spline;   // grafik smooth/investasi
            series.Color = Color.DeepSkyBlue;
            series.BorderWidth = 3;
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerSize = 7;
            series.MarkerColor = Color.White;

            // Data Penjualan per Bulan
            Dictionary<string, int> dataBulanan = new Dictionary<string, int>()
    {
        { "Jan", 120 },
        { "Feb", 150 },
        { "Mar", 180 },
        { "Apr", 200 },
        { "Mei", 240 },
        { "Jun", 300 },
        { "Jul", 320 },
        { "Agu", 310 },
        { "Sep", 350 },
        { "Okt", 400 },
        { "Nov", 420 },
        { "Des", 500 }
    };

            foreach (var item in dataBulanan)
            {
                series.Points.AddXY(item.Key, item.Value);
            }

            chartpenjualan.Series.Add(series);
        }



        // Fungsi untuk smooth scroll
        private async Task SmoothScrollToBottom()
        {
            int targetPosition = richTextBoxAi.Text.Length;
            int currentPosition = richTextBoxAi.SelectionStart;
            int steps = 20;
            int delay = 10;

            for (int i = 0; i < steps; i++)
            {
                int newPosition = currentPosition + ((targetPosition - currentPosition) * (i + 1) / steps);
                richTextBoxAi.SelectionStart = newPosition;
                richTextBoxAi.ScrollToCaret();
                await Task.Delay(delay);
            }

            richTextBoxAi.SelectionStart = targetPosition;
            richTextBoxAi.ScrollToCaret();
        }

        private async Task<string> AskGemini(string prompt)
        {
            // GANTI API KEY ANDA DI SINI - Generate baru di: https://aistudio.google.com/apikey
            string apiKey = "AIzaSyDdALPaNH5coce61pKqkV7VmmOEuo0K8mo";
            string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

            string systemPrompt = @"Kamu adalah Bibot, asisten AI yang ahli dalam bisnis Food & Beverage (F&B).
Kamu HANYA menjawab pertanyaan yang berkaitan dengan:
- Manajemen restoran, cafe, atau bisnis F&B
- Menu makanan dan minuman
- Resep masakan
- Strategi penjualan F&B
- Pengelolaan bahan baku dan stok
- Pricing dan HPP (Harga Pokok Penjualan)
- Tips dan trik operasional bisnis F&B

Jika user bertanya di luar topik F&B (misalnya tentang politik, teknologi umum, olahraga, dll), 
kamu dengan sopan menolak dan mengarahkan kembali ke topik F&B dengan berkata:
'Maaf, Bibot hanya bisa membantu masalah bisnis F&B ya! Ada yang ingin kamu tanyakan tentang menu, resep, atau strategi bisnis kuliner? 😊'

Gaya bicara kamu ramah, profesional, dan helpful.

PENTING: Saat merekomendasikan menu, SELALU gunakan format berikut:

[Nama Menu]
Menu #[nomor urut]
[Deskripsi singkat menu]

Bahan:
[List bahan dengan bullet point atau dash]

Estimasi:
HPP: Rp [harga]
Harga Jual: Rp [harga]

Kenapa menu ini?
[Penjelasan alasan memilih menu ini]

---

[Ulangi format di atas untuk menu berikutnya]";

            // Gabungkan system prompt dengan user prompt
            string fullPrompt = systemPrompt + "\n\nUser: " + prompt + "\n\nBibot:";

            // Buat JSON manual
            string jsonData = $@"{{
                ""contents"": [{{
                    ""parts"": [{{
                        ""text"": ""{fullPrompt.Replace("\"", "\\\"").Replace("\n", "\\n")}""
                    }}]
                }}]
            }}";

            using (var client = new HttpClient())
            {
                try
                {
                    var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(url, content);
                    string result = await response.Content.ReadAsStringAsync();

                    // Cek jika response tidak OK (error)
                    if (!response.IsSuccessStatusCode)
                    {
                        try
                        {
                            using (JsonDocument errorDoc = JsonDocument.Parse(result))
                            {
                                var errorMsg = errorDoc.RootElement
                                    .GetProperty("error")
                                    .GetProperty("message")
                                    .GetString();

                                if (result.Contains("quota") || result.Contains("RESOURCE_EXHAUSTED"))
                                {
                                    return "⚠️ Quota API Gemini habis. Silakan:\n" +
                                           "1. Tunggu 24 jam untuk reset otomatis\n" +
                                           "2. Atau generate API key baru di: https://makersuite.google.com/app/apikey\n" +
                                           "3. Ganti API key di kode aplikasi";
                                }

                                return "Error API: " + errorMsg;
                            }
                        }
                        catch
                        {
                            return "Error: " + result;
                        }
                    }

                    // Parse response menggunakan System.Text.Json
                    using (JsonDocument doc = JsonDocument.Parse(result))
                    {
                        var root = doc.RootElement;

                        if (root.TryGetProperty("error", out JsonElement error))
                        {
                            return "Error: " + error.GetProperty("message").GetString();
                        }

                        var text = root
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text")
                            .GetString();

                        return text;
                    }
                }
                catch (Exception ex)
                {
                    return "Error: " + ex.Message;
                }
            }
        }

        private void guna2GroupBox4_Click(object sender, EventArgs e)
        {
        }

        private async void btnsend_Click_1(object sender, EventArgs e)
        {
            string prompt = txtprompt.Text.Trim();

            // Cek apakah masih placeholder atau kosong
            if (string.IsNullOrEmpty(prompt) || prompt == "Silahkan tanya Bibot...")
            {
                MessageBox.Show("Isi prompt dulu!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Cek apakah sedang typing
            if (isTyping)
            {
                MessageBox.Show("Tunggu Bibot selesai mengetik ya! 😊", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // RATE LIMITING - Free tier protection
            if ((DateTime.Now - requestCountResetTime).TotalMinutes >= 1)
            {
                requestCount = 0;
                requestCountResetTime = DateTime.Now;
            }

            if (requestCount >= 10)
            {
                MessageBox.Show(
                    "Terlalu banyak request! Tunggu 1 menit ya 😊\n" +
                    "Free tier Gemini ada limit 15 request/menit.",
                    "Rate Limit",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            var timeSinceLastRequest = (DateTime.Now - lastRequestTime).TotalSeconds;
            if (timeSinceLastRequest < 4)
            {
                int waitTime = (int)(4 - timeSinceLastRequest);
                MessageBox.Show(
                    $"Tunggu {waitTime} detik lagi ya! 😊\n" +
                    "Ini untuk menghemat quota free tier.",
                    "Rate Limit",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            lastRequestTime = DateTime.Now;
            requestCount++;

            // Tampilkan prompt user dengan typing effect
            await TypeText("Anda: " + prompt + "\n\n", 10);

            // Clear textbox prompt dan kembalikan placeholder
            txtprompt.Clear();
            txtprompt.Text = "Silahkan tanya Bibot...";
            txtprompt.ForeColor = Color.Gray;
            txtprompt.Focus();

            // Tampilkan label Bibot
            richTextBoxAi.AppendText("Bibot: ");

            // Tampilkan animasi loading dengan dots
            for (int i = 0; i < 3; i++)
            {
                richTextBoxAi.AppendText(".");
                await Task.Delay(300);
            }
            richTextBoxAi.AppendText(" ");

            // Panggil AI
            string response = await AskGemini(prompt);

            // Hapus dots loading (hapus "Bibot: ... ")
            int removeLength = "Bibot: ... ".Length;
            if (richTextBoxAi.TextLength >= removeLength)
            {
                richTextBoxAi.Select(richTextBoxAi.TextLength - removeLength, removeLength);
                richTextBoxAi.SelectedText = "";
            }

            // Tampilkan label Bibot lagi
            richTextBoxAi.AppendText("Bibot: ");

            // Tampilkan jawaban AI dengan typing effect (lebih cepat = 15ms per char)
            await TypeText(response, 15);

            // Tambah newline
            richTextBoxAi.AppendText("\n\n");

            // Smooth scroll ke bawah
            await SmoothScrollToBottom();
        }

        private async void UCDashboard_Load(object sender, EventArgs e)
        {
            // Set properti RichTextBox
            richTextBoxAi.ScrollBars = RichTextBoxScrollBars.Vertical;
            richTextBoxAi.WordWrap = true;

            // Bibot menyapa saat pertama kali load dengan typing effect
            richTextBoxAi.Clear();
            await TypeText("Bibot: Halo! 👋 Saya Bibot, asisten AI untuk bisnis F&B Anda. ", 20);
            await TypeText("Ada yang bisa saya bantu hari ini? Silahkan bertanya! 😊\n\n", 20);
        }

        private void richTextBoxAi_TextChanged(object sender, EventArgs e)
        {
        }

        private void txtprompt_TextChanged(object sender, EventArgs e)
        {
        }

        private void guna2GroupBox8_Click(object sender, EventArgs e)
        {

        }

        private void txtprompt_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void chartDashboard_Click(object sender, EventArgs e)
        {

            LoadPieChart();
        }

        private void guna2GroupBox12_Click(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {
            LoadGrafikPenjualanBulanan();
        }
    }
}