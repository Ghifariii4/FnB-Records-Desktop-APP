using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Net.WebRequestMethods;


namespace AplikasiCrud
{
    public partial class Form1 : Form
    {

        private string connectionString = "Host=localhost;Port=5432;Database=AplikasiCrudDB;Username=postgres;Password=Gforce271208*";
        private object conn;

        private void LoadData(string katakunci = "") 
        {

            DataTable dt = new DataTable();
            using (NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT id, nama_produk, harga FORM produk";


                    var parameters = new List<NpgsqlParameter>();

                    if (!string.IsNullOrWhiteSpace(katakunci))
                    {
                        sql += "WHERE nama_produk ILIKE @katakunci OR CAST(harga AS TEXT) ILIKE @katakunci";
                        parameters.Add(new NpgsqlParameter("katakunci", "%" + katakunci + "%"));
                    }
                    {
                        sql += "ORDER BY id";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                        {
                            foreach  (var p in parameters)
                            {
                                cmd.Parameters.Add(p);  
                            }

                            using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter(cmd))
                            {
                                adapter.Fill(dt);
                            }

                            dataGridView1.DataSource = dt;

                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal memuat data :" + ex.Message);
                }

            }
        }

        public void tampilkanGrafikharga()
        {
            chartProduk.Series.Clear();

            Series series = new Series("Harga Produk");
            series.ChartType = SeriesChartType.Bar;

            chartProduk.ChartAreas.Clear();
            ChartArea chartArea = new ChartArea();
            chartArea.AxisX.Interval = 1;
            chartArea.AxisY.Title = "Harga (IDR)";
            chartArea.AxisX.Title = "Nama Produk";
            chartProduk.ChartAreas.Add(chartArea);
        
            using(NpgsqlConnection conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT nama_produk FROM produk ORDER BY harga DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            string nama = reader.GetString(0);
                            decimal harga = reader.GetDecimal(1);

                            series.Points.AddXY(nama, harga);
                        }
                    }

                }

                catch
                {

                }
            }
        }



        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
