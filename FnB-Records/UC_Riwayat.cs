using FnB_Records.Koneksi_DB;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class UC_Riwayat : UserControl
    {
        public UC_Riwayat()
        {
            InitializeComponent();
        }

        // --- SAAT USER CONTROL DIBUKA ---
        private void UC_Riwayat_Load(object sender, EventArgs e)
        {
            // Default: Set tanggal ke hari ini
            dtpTanggal.Value = DateTime.Now;

            // Tampilkan data hari ini
            LoadDataRiwayat(filterDate: true);
        }

        // --- FUNGSI LOAD DATA (INTI LOGIKA) ---
        public void LoadDataRiwayat(bool filterDate = false)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Query Dasar
                    string query = @"
                        SELECT 
                            s.id AS ""ID Transaksi"",
                            to_char(s.created_at, 'HH24:MI') AS ""Jam"", 
                            u.business_name AS ""Kasir"",
                            r.name AS ""Menu"",
                            s.qty AS ""Qty"",
                            s.total_price AS ""Total"",
                            s.profit AS ""Profit""
                        FROM sales s
                        JOIN users u ON s.user_id = u.id
                        JOIN recipes r ON s.recipe_id = r.id
                        WHERE 1=1 "; // Trik agar mudah menambah 'AND ...'

                    // --- LOGIKA FILTER TANGGAL (Dari DateTimePicker) ---
                    if (filterDate)
                    {
                        query += " AND DATE(s.created_at) = @tanggal";
                    }

                    // --- LOGIKA PENCARIAN TEKS (Dari TextBox) ---
                    // Jika user mengetik sesuatu di TextBox Pencarian
                    if (!string.IsNullOrEmpty(txtCariRiwayatTransaksi.Text))
                    {
                        // Cari berdasarkan ID Transaksi ATAU Nama Menu
                        query += " AND (CAST(s.id AS TEXT) LIKE @cari OR r.name ILIKE @cari)";
                    }

                    query += " ORDER BY s.created_at DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        // Parameter Tanggal
                        if (filterDate)
                        {
                            cmd.Parameters.AddWithValue("@tanggal", dtpTanggal.Value.Date);
                        }

                        // Parameter Pencarian
                        if (!string.IsNullOrEmpty(txtCariRiwayatTransaksi.Text))
                        {
                            // Tambahkan % di kiri kanan untuk pencarian parsial (SQL LIKE)
                            cmd.Parameters.AddWithValue("@cari", "%" + txtCariRiwayatTransaksi.Text + "%");
                        }

                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dgvRiwayat.DataSource = dt;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat riwayat: " + ex.Message);
            }
        }

        // --- FORMATTING TABEL (Agar Rupiah Rapi) ---
        private void dgvRiwayat_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Cek kolom "Total" dan "Profit" (index 5 dan 6)
            if (e.ColumnIndex == 5 || e.ColumnIndex == 6)
            {
                if (e.Value != null && double.TryParse(e.Value.ToString(), out double val))
                {
                    e.Value = "Rp " + val.ToString("N0");
                    e.FormattingApplied = true;
                }
            }
        }

        // --- EVENT HANDLER TOMBOL CARI (Kaca Pembesar) ---
        // Hubungkan event Click tombol guna2Button1 ke sini lewat Designer
        private void btRiwayat_Click(object sender, EventArgs e)
        {
            // Saat tombol cari ditekan, kita filter berdasarkan Tanggal DAN Teks pencarian
            LoadDataRiwayat(filterDate: true);
        }

        // --- EVENT JIKA TANGGAL DIGANTI ---
        // Hubungkan event ValueChanged dtpTanggal ke sini (Opsional, agar otomatis refresh)
        private void dtpTanggal_ValueChanged(object sender, EventArgs e)
        {
            LoadDataRiwayat(filterDate: true);
        }

        // --- EVENT JIKA TEKAN ENTER DI TEXTBOX ---
        // Hubungkan event KeyDown txtCariRiwayatTransaksi ke sini
        private void txtCariRiwayatTransaksi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                LoadDataRiwayat(filterDate: true);
                e.SuppressKeyPress = true; // Hilangkan bunyi 'ding'
            }
        }
    }
}