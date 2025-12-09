using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;
using FnB_Records.Koneksi_DB; // Sesuaikan namespace

namespace FnB_Records
{
    public partial class UC_Menu : UserControl
    {
        // Ambil ID User yang sedang login
        private int _currentUserId => Login.GlobalSession.CurrentUserId;

        public UC_Menu()
        {
            InitializeComponent();
        }

        // --- 1. SAAT LOAD ---
        private void UC_Menu_Load(object sender, EventArgs e)
        {
            if (_currentUserId == 0) Login.GlobalSession.CurrentUserId = 1; // Fallback jika null

            SetupGridView(); // Rapikan tampilan tabel
            LoadDataMenu();  // Ambil data dari DB
            dgvMenu.AlternatingRowsDefaultCellStyle.BackColor = Color.White;
        }

        // --- 2. CONFIG TABLE ---
        private void SetupGridView()
        {
            dgvMenu.AutoGenerateColumns = false;
            dgvMenu.Columns.Clear();

            // Style untuk rata tengah
            DataGridViewCellStyle centerStyle = new DataGridViewCellStyle();
            centerStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 1. Kolom ID (Hidden)
            dgvMenu.Columns.Add("id", "ID");
            dgvMenu.Columns["id"].Visible = false;
            dgvMenu.Columns["id"].DataPropertyName = "id";

            // 2. Kolom Nama Menu
            dgvMenu.Columns.Add("name", "Nama Menu");
            dgvMenu.Columns["name"].DataPropertyName = "name";
            dgvMenu.Columns["name"].DefaultCellStyle = centerStyle;

            // 3. Kolom Kategori
            dgvMenu.Columns.Add("category", "Kategori");
            dgvMenu.Columns["category"].DataPropertyName = "category";
            dgvMenu.Columns["category"].DefaultCellStyle = centerStyle;

            // 4. Kolom Harga Jual
            dgvMenu.Columns.Add("price", "Harga Jual");
            dgvMenu.Columns["price"].DataPropertyName = "suggested_price";
            dgvMenu.Columns["price"].DefaultCellStyle.Format = "N0";
            dgvMenu.Columns["price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 5. Kolom HPP
            dgvMenu.Columns.Add("hpp", "HPP");
            dgvMenu.Columns["hpp"].DataPropertyName = "hpp";
            dgvMenu.Columns["hpp"].DefaultCellStyle.Format = "N0";
            dgvMenu.Columns["hpp"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 6. Kolom Stok
            dgvMenu.Columns.Add("stock", "Stok");
            dgvMenu.Columns["stock"].DataPropertyName = "stock";
            dgvMenu.Columns["stock"].DefaultCellStyle = centerStyle;

            // --- 7. KOLOM HAPUS (EMOJI MERAH) ---
            DataGridViewButtonColumn btnDel = new DataGridViewButtonColumn();
            btnDel.Name = "btnHapus";
            btnDel.HeaderText = "Aksi";
            btnDel.Text = "🗑️"; // Pakai Emoji Sampah
            btnDel.UseColumnTextForButtonValue = true;
            btnDel.FlatStyle = FlatStyle.Flat;

            // --- Styling Emoji Merah ---
            // 1. Warna Font (Emoji) jadi Merah
            btnDel.DefaultCellStyle.ForeColor = Color.Red;

            // 2. Background Putih (biar kontras dengan merah)
            btnDel.DefaultCellStyle.BackColor = Color.White;

            // 3. Saat diklik/dipilih, tetap merah (jangan jadi biru default windows)
            btnDel.DefaultCellStyle.SelectionForeColor = Color.Red;
            btnDel.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 240, 240); // Merah muda sangat pudar

            // 4. Perbesar Font agar Emoji terlihat jelas
            // Kita gunakan font yang support emoji standar Windows
            btnDel.DefaultCellStyle.Font = new Font("Segoe UI Emoji", 12F, FontStyle.Regular);

            btnDel.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            dgvMenu.Columns.Add(btnDel);
        }

        // --- 3. LOAD DATA (SELECT) ---
        private void LoadDataMenu()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string query = @"SELECT id, name, category, suggested_price, hpp, stock 
                                     FROM recipes 
                                     WHERE user_id = @uid 
                                     AND name ILIKE @cari 
                                     ORDER BY name ASC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", _currentUserId);
                        // Filter pencarian
                        cmd.Parameters.AddWithValue("@cari", "%" + txtCariMenu.Text + "%");

                        DataTable dt = new DataTable();
                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }

                        dgvMenu.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message);
            }
        }

        // --- 4. EVENT PENCARIAN ---
        // Hubungkan event TextChanged pada txtCariMenu ke sini
        private void txtCariMenu_TextChanged(object sender, EventArgs e)
        {
            LoadDataMenu();
        }

        // --- 5. LOGIKA HAPUS (KLIK GRID) ---
        // Hubungkan event CellContentClick pada dgvMenu ke sini
        private void dgvMenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Pastikan yang diklik adalah tombol dan bukan header (-1)
            if (e.RowIndex >= 0 && dgvMenu.Columns[e.ColumnIndex].Name == "btnHapus")
            {
                // Ambil Nama dan ID dari baris yang diklik
                string namaMenu = dgvMenu.Rows[e.RowIndex].Cells["name"].Value.ToString();
                int idMenu = Convert.ToInt32(dgvMenu.Rows[e.RowIndex].Cells["id"].Value);

                // Konfirmasi Hapus
                DialogResult dialog = MessageBox.Show(
                    $"Apakah Anda yakin ingin menghapus menu '{namaMenu}'?\n\nPERINGATAN: Data tidak dapat dikembalikan.",
                    "Konfirmasi Hapus",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (dialog == DialogResult.Yes)
                {
                    HapusMenuDariDB(idMenu);
                }
            }
        }

        private void HapusMenuDariDB(int id)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Query Delete
                    string query = "DELETE FROM recipes WHERE id = @id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Menu berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh Data agar yang dihapus hilang dari layar
                LoadDataMenu();
            }
            catch (PostgresException pgEx)
            {
                // Error 23503 adalah Foreign Key Violation (Data dipakai di tabel lain)
                if (pgEx.SqlState == "23503")
                {
                    MessageBox.Show("Gagal menghapus! Menu ini sudah memiliki riwayat transaksi atau stok produksi.\n\nData yang sudah dipakai tidak boleh dihapus demi integritas laporan.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Database Error: " + pgEx.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
            }
        }
    }
}