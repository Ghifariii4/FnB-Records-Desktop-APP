using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Npgsql;
using FnB_Records.Koneksi_DB; // Pastikan namespace ini sesuai dengan project Anda

namespace FnB_Records
{
    public partial class UC_TambahMenu : UserControl
    {
        // Helper Class untuk ComboBox (Menyimpan ID dan Nama)
        public class ComboBoxItem
        {
            public string Text { get; set; }
            public int Value { get; set; }
            public override string ToString() { return Text; }
        }

        private string _imagePath = null; // Menyimpan path gambar sementara
        private int _currentUserId => Login.GlobalSession.CurrentUserId; // Ambil ID User Login

        public UC_TambahMenu()
        {
            InitializeComponent();
        }

        // --- 1. EVENT LOAD (Saat Halaman Dibuka) ---
        private void UC_TambahMenu_Load(object sender, EventArgs e)
        {
            // Default kondisi awal
            cbPilihDariResep.Visible = false;
            chkPilihDariResep.Checked = false;

            // Set Default Kategori
            cmbKategori.SelectedIndex = 0;

            // Load data resep ke ComboBox untuk persiapan
            LoadDataResep();
        }

        // --- 2. LOAD DATA RESEP KE COMBOBOX ---
        private void LoadDataResep()
        {
            try
            {
                cbPilihDariResep.Items.Clear(); // Bersihkan item hardcode/lama

                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // Ambil ID dan Nama Resep
                    string sql = "SELECT id, name FROM recipes WHERE user_id = @uid ORDER BY name ASC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", _currentUserId);

                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cbPilihDariResep.Items.Add(new ComboBoxItem
                                {
                                    Value = Convert.ToInt32(reader["id"]),
                                    Text = reader["name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat daftar resep: " + ex.Message);
            }
        }

        // --- 3. EVENT CHECKBOX (Tampilkan/Sembunyikan Combo) ---
        // Hubungkan event 'CheckedChanged' pada chkPilihDariResep ke sini lewat Designer
        private void chkPilihDariResep_CheckedChanged(object sender, EventArgs e)
        {
            bool isChecked = chkPilihDariResep.Checked;
            cbPilihDariResep.Visible = isChecked;

            if (!isChecked)
            {
                // Jika di-uncheck, bersihkan form agar bisa input baru manual
                BersihkanForm();
            }
        }

        // --- 4. EVENT COMBOBOX DIGANTI (AUTO-FILL FORM) ---
        // Hubungkan event 'SelectedIndexChanged' pada cbPilihDariResep ke sini
        private void cbPilihDariResep_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbPilihDariResep.SelectedItem == null) return;

            // Ambil ID dari item yang dipilih
            ComboBoxItem selectedItem = (ComboBoxItem)cbPilihDariResep.SelectedItem;
            int recipeId = selectedItem.Value;

            IsiFormDariDatabase(recipeId);
        }

        private void IsiFormDariDatabase(int id)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql = "SELECT * FROM recipes WHERE id = @id";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (NpgsqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                // Isi Textbox dan Numeric Up Down
                                txtNamaMenu.Text = r["name"].ToString();
                                cmbKategori.SelectedItem = r["category"].ToString(); // Pastikan text sama persis dengan Items Collection
                                numHarga.Value = Convert.ToDecimal(r["suggested_price"]);
                                numStok.Value = Convert.ToDecimal(r["stock"]);
                                numHPP.Value = r["hpp"] != DBNull.Value ? Convert.ToDecimal(r["hpp"]) : 0;
                                numPorsi.Value = r["serving_size"] != DBNull.Value ? Convert.ToDecimal(r["serving_size"]) : 1;

                                // Deskripsi (jika ada kolom description di DB, sesuaikan)
                                // txtDeskripsi.Text = r["description"].ToString(); 

                                // Load Gambar jika ada (BLOB / Byte Array)
                                if (r["image_data"] != DBNull.Value)
                                {
                                    byte[] imgData = (byte[])r["image_data"];
                                    using (MemoryStream ms = new MemoryStream(imgData))
                                    {
                                        picGambar.Image = Image.FromStream(ms);
                                    }
                                }
                                else
                                {
                                    picGambar.Image = null; // Gambar kosong
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengambil detail resep: " + ex.Message);
            }
        }

        // --- 5. LOGIKA SIMPAN ---
        private void btnSimpan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNamaMenu.Text))
            {
                MessageBox.Show("Nama menu tidak boleh kosong!", "Peringatan");
                return;
            }

            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string sql;
                    NpgsqlCommand cmd = new NpgsqlCommand();
                    cmd.Connection = conn;

                    // --- PERSIAPAN GAMBAR (Convert Image ke Byte Array) ---
                    byte[] imgBytes = null;
                    if (picGambar.Image != null)
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            // Simpan gambar ke MemoryStream dengan format aslinya (atau PNG default)
                            picGambar.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                            imgBytes = ms.ToArray();
                        }
                    }

                    // --- TENTUKAN SQL (INSERT atau UPDATE) ---
                    if (chkPilihDariResep.Checked && cbPilihDariResep.SelectedItem != null)
                    {
                        // MODE UPDATE
                        ComboBoxItem selectedItem = (ComboBoxItem)cbPilihDariResep.SelectedItem;
                        int idToUpdate = selectedItem.Value;

                        sql = @"UPDATE recipes SET 
                        name=@name, category=@cat, suggested_price=@price, stock=@stock, 
                        hpp=@hpp, serving_size=@serv, updated_at=CURRENT_TIMESTAMP";

                        // Hanya update gambar jika ada gambar baru yang dipilih/di-load
                        if (imgBytes != null)
                        {
                            sql += ", image_data=@img";
                        }

                        sql += " WHERE id=@id";

                        cmd.Parameters.AddWithValue("@id", idToUpdate);
                    }
                    else
                    {
                        // MODE INSERT BARU
                        sql = @"INSERT INTO recipes 
                        (user_id, name, category, suggested_price, stock, hpp, serving_size, image_data, created_at) 
                        VALUES 
                        (@uid, @name, @cat, @price, @stock, @hpp, @serv, @img, CURRENT_TIMESTAMP)";

                        cmd.Parameters.AddWithValue("@uid", _currentUserId);
                    }

                    cmd.CommandText = sql;

                    // --- ISI PARAMETER UMUM ---
                    cmd.Parameters.AddWithValue("@name", txtNamaMenu.Text);
                    cmd.Parameters.AddWithValue("@cat", cmbKategori.Text);
                    cmd.Parameters.AddWithValue("@price", (double)numHarga.Value);
                    cmd.Parameters.AddWithValue("@stock", (int)numStok.Value);
                    cmd.Parameters.AddWithValue("@hpp", (double)numHPP.Value);
                    cmd.Parameters.AddWithValue("@serv", (int)numPorsi.Value);

                    // --- ISI PARAMETER GAMBAR ---
                    if (imgBytes != null)
                    {
                        cmd.Parameters.AddWithValue("@img", imgBytes);
                    }
                    else
                    {
                        // Jika Insert tapi tidak ada gambar, kirim NULL
                        if (!chkPilihDariResep.Checked)
                        {
                            cmd.Parameters.AddWithValue("@img", DBNull.Value);
                        }
                    }

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Data berhasil disimpan!", "Sukses");
                    BersihkanForm();
                    LoadDataResep();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan: " + ex.Message);
            }
        }

        // --- 6. FUNGSI BROWSE GAMBAR ---
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    _imagePath = ofd.FileName;
                    picGambar.Image = Image.FromFile(_imagePath);
                    picGambar.SizeMode = PictureBoxSizeMode.Zoom;
                }
            }
        }

        // --- Helper Bersihkan Form ---
        private void BersihkanForm()
        {
            txtNamaMenu.Clear();
            txtDeskripsi.Clear();
            numHarga.Value = 0;
            numStok.Value = 0;
            numHPP.Value = 0;
            numPorsi.Value = 1;
            picGambar.Image = null;
            _imagePath = null;
            cmbKategori.SelectedIndex = 0;

            // Reset pilihan combobox tapi jangan trigger event change
            cbPilihDariResep.SelectedIndexChanged -= cbPilihDariResep_SelectedIndexChanged;
            cbPilihDariResep.SelectedIndex = -1;
            cbPilihDariResep.SelectedIndexChanged += cbPilihDariResep_SelectedIndexChanged;
        }
    }
}