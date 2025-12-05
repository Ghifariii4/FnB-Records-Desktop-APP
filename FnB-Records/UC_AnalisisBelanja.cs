using System;
using System.Drawing; // Wajib untuk pengaturan Warna
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting; // Wajib untuk Chart

namespace FnB_Records
{
    public partial class UC_AnalisisBelanja : UserControl
    {
        public UC_AnalisisBelanja()
        {
            InitializeComponent();
            // Panggil setup saat UserControl dimuat
            this.Load += UC_AnalisisBelanja_Load;
        }

        private void UC_AnalisisBelanja_Load(object sender, EventArgs e)
        {
            BuatChartSederhana();
        }

        private void BuatChartSederhana()
        {
            // 1. Reset Chart
            chart1.Series.Clear();
            chart1.ChartAreas.Clear();
            chart1.Legends.Clear();

            // --- PENGATURAN FONT CUSTOM ---
            // Pastikan nama font sama persis dengan yang diinstall di Windows
            // Jika error/tidak berubah, ganti stringnya kembali ke "Segoe UI"
            Font fontData;
            Font fontLegend;

            try
            {
                // Inter untuk data (Angka & Label Sumbu)
                fontData = new Font("Inter", 8, FontStyle.Regular);
                // Manrope untuk Legend (Keterangan)
                fontLegend = new Font("Manrope", 9, FontStyle.Bold);
            }
            catch
            {
                // Fallback jika font tidak ditemukan di komputer
                fontData = new Font("Segoe UI", 8, FontStyle.Regular);
                fontLegend = new Font("Segoe UI", 9, FontStyle.Bold);
            }

            // --- WARNA TEMA DARK MODE ---
            Color textLight = Color.WhiteSmoke;
            Color gridColor = Color.FromArgb(80, 80, 80);
            Color barColor1 = Color.FromArgb(72, 201, 176); // Hijau Tosca
            Color barColor2 = Color.FromArgb(22, 160, 133); // Hijau Gelap

            // 2. Setup Background Transparan
            chart1.BackColor = Color.Transparent;

            // 3. Setup Area
            ChartArea area = new ChartArea("AreaBelanja");
            area.BackColor = Color.Transparent;

            // --- SETUP SUMBU Y (Harga) ---
            area.AxisY.LabelStyle.Font = fontData; // Pakai Inter
            area.AxisY.LabelStyle.ForeColor = textLight;
            area.AxisY.LabelStyle.Format = "Rp #,0";
            area.AxisY.LineColor = Color.Transparent;

            // Grid Halus
            area.AxisY.MajorGrid.LineColor = gridColor;
            area.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dash;

            // --- SETUP SUMBU X (Nama Barang) ---
            area.AxisX.LabelStyle.Font = fontData; // Pakai Inter
            area.AxisX.LabelStyle.ForeColor = textLight;
            area.AxisX.LineColor = gridColor;
            area.AxisX.MajorGrid.Enabled = false;
            area.AxisX.Interval = 1;

            chart1.ChartAreas.Add(area);

            // 4. Setup Legend
            Legend legend = new Legend("LegendUtama");
            legend.BackColor = Color.Transparent;
            legend.ForeColor = textLight;
            legend.Font = fontLegend; // Pakai Manrope
            legend.Docking = Docking.Top;
            legend.Alignment = StringAlignment.Center;
            chart1.Legends.Add(legend);

            // 5. Setup Data
            Series series = new Series("Total Pengeluaran");
            series.ChartType = SeriesChartType.Column;
            series.Color = barColor1;
            series.BackSecondaryColor = barColor2;
            series.BackGradientStyle = GradientStyle.TopBottom;
            series.BorderColor = barColor2;
            series.BorderWidth = 1;

            // Agar batang tidak terlalu gemuk
            series["PixelPointWidth"] = "50";
            series["DrawingStyle"] = "Cylinder";

            // Data Dummy
            series.Points.AddXY("Tepung Terigu", 750000);
            series.Points.AddXY("Gula Pasir", 300000);
            series.Points.AddXY("Telur", 500000);

            chart1.Series.Add(series);
        }

        private void guna2PictureBox1_Click(object sender, EventArgs e)
        {
            // Kode event click gambar (jika ada)
        }
    }
}