using FnB_Records.Koneksi_DB;
using Guna.UI2.WinForms;
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
using static FnB_Records.Form_Struk;

namespace FnB_Records
{
    public partial class Mode_POS : Form
    {
        // [PENTING 1] Deklarasikan variabel ini di luar method agar bisa dikenal oleh SEMUA tombol
        private UC_DaftarMenu _ucDaftarMenu;
        private List<UC_CartItem> _cartItems = new List<UC_CartItem>();
        // Variabel untuk simpan ID

        public Mode_POS()
        {
            InitializeComponent();
        }



        // Method bantu untuk menampilkan User Control ke Panel
        private void navigationControl(UserControl uc)
        {
            uc.Dock = DockStyle.Fill;
            pnlModePOS.Controls.Clear(); // Bersihkan panel dari tampilan lama
            pnlModePOS.Controls.Add(uc); // Masukkan tampilan baru
            uc.BringToFront();
        }

        private void Mode_POS_Load(object sender, EventArgs e)
        {
            // 1. Inisialisasi User Control
            _ucDaftarMenu = new UC_DaftarMenu();

            CartHub.OnItemAdded += TerimaPesananBaru;
            // 2. Tampilkan User Control ke Panel
            navigationControl(_ucDaftarMenu);

            // 3. Set tombol "Daftar Menu" jadi aktif secara visual
            btDaftarMenu.Checked = true;

            // --- [TAMBAHAN BARU] ---
            // 4. Perintahkan UC untuk memuat "Semua" kategori secara default
            _ucDaftarMenu.TampilkanHalamanMenu("Semua");
        }

        // Update parameter: Tambahkan 'int stockTersedia'
        private void TerimaPesananBaru(int id, string nama, double harga, int stockTersedia, Image gambar)
        {
            // 1. Cari apakah item sudah ada di keranjang
            UC_CartItem itemDitemukan = null;
            foreach (var item in _cartItems)
            {
                if (item.RecipeId == id)
                {
                    itemDitemukan = item;
                    break;
                }
            }

            if (itemDitemukan != null)
            {
                // --- LOGIKA VALIDASI STOK (Jika item sudah ada) ---
                // Cek: Apakah (Qty sekarang + 1) melebihi Stok Database?
                if (itemDitemukan.Quantity >= stockTersedia)
                {
                    MessageBox.Show($"Stok tidak mencukupi! Hanya tersedia {stockTersedia} porsi.",
                                    "Stok Habis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return; // Batalkan penambahan
                }

                // Jika aman, baru tambah qty
                itemDitemukan.TambahQty();
            }
            else
            {
                // --- LOGIKA VALIDASI STOK (Jika item baru) ---
                // Harusnya jarang terjadi karena tombol dimatikan jika stok 0, tapi buat jaga-jaga
                if (stockTersedia <= 0)
                {
                    MessageBox.Show("Stok Habis!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                UC_CartItem itemBaru = new UC_CartItem();
                itemBaru.NamaMenu = nama;

                // Panggil SetData (Anda tidak perlu ubah SetData di UC_CartItem, biarkan saja)
                itemBaru.SetData(id, nama, harga, gambar);

                itemBaru.OnDeleteRequest += HapusItemKeranjang;

                _cartItems.Add(itemBaru);
                flpKeranjang.Controls.Add(itemBaru);
            }

            HitungTotalBelanja();
        }

        private void HapusItemKeranjang(object sender, EventArgs e)
        {
            // 1. Ambil objek item yang diklik
            UC_CartItem itemYangDihapus = (UC_CartItem)sender;

            // 2. Hapus dari LIST DATA (Penting agar total harga berkurang)
            _cartItems.Remove(itemYangDihapus);

            // 3. Hapus dari TAMPILAN (UI)
            flpKeranjang.Controls.Remove(itemYangDihapus);

            // 4. Bersihkan memori
            itemYangDihapus.Dispose();

            // 5. [WAJIB] Hitung Ulang Total
            HitungTotalBelanja();
        }

        private void HitungTotalBelanja()
        {
            double subtotal = 0;

            // Hitung ulang berdasarkan isi List _cartItems yang tersisa
            foreach (var item in _cartItems)
            {
                subtotal += (item.HargaSatuan * item.Quantity);
            }

            // Jika list kosong, subtotal otomatis 0 karena loop tidak jalan
            double pajak = subtotal * 0.10;
            double grandTotal = subtotal + pajak;

            // Update Label UI
            lblSubtotal.Text = $"Rp {subtotal:N0}";
            lblPajak.Text = $"Rp {pajak:N0}";
            lblTotalAkhir.Text = $"Rp {grandTotal:N0}";
        }


        private void btDaftarMenu_Click(object sender, EventArgs e)
        {
            // 1. Pastikan UC Daftar Menu tampil di panel
            navigationControl(_ucDaftarMenu);

            // 3. Panggil fungsi "Semua" yang baru kita buat
            _ucDaftarMenu.TampilkanHalamanMenu("Semua");
            gbKeranjang.Visible = true;
            btDaftarMenu.Checked = false;

        }

        // --- Logika Tombol Kategori ---

        private void btMakananUtama_Click(object sender, EventArgs e)
        {
            // 1. Pastikan UC Daftar Menu sedang aktif di layar
            // Kita cek pakai method navigationControl biar rapi
            if (!pnlModePOS.Controls.Contains(_ucDaftarMenu))
            {
                navigationControl(_ucDaftarMenu);
            }

            // Pastikan tombol Menu induknya terlihat aktif (Visual saja)
            btDaftarMenu.Checked = false;

            // 2. [PENTING 3] Panggil fungsi di dalam UC untuk ubah isinya
            // Pastikan di UC_DaftarMenu.cs sudah ada method 'TampilkanHalamanMenu' yang public
            _ucDaftarMenu.TampilkanHalamanMenu("Makanan Utama");
            gbKeranjang.Visible = true;
        }

        // (Contoh untuk tombol lain, tinggal copy paste logika di atas)
        private void btMinumanDingin_Click(object sender, EventArgs e)
        {
            if (!pnlModePOS.Controls.Contains(_ucDaftarMenu))
            {
                navigationControl(_ucDaftarMenu);
            }
            btDaftarMenu.Checked = false;
            _ucDaftarMenu.TampilkanHalamanMenu("Minuman Dingin");
            gbKeranjang.Visible = true;
        }

        // --- Di dalam Mode_POS.cs ---

        // 1. Tombol Makanan Ringan
        private void btMakananRingan_Click(object sender, EventArgs e)
        {
            if (!pnlModePOS.Controls.Contains(_ucDaftarMenu))
            {
                navigationControl(_ucDaftarMenu);
            }
            btDaftarMenu.Checked = false;

            // Ganti teks sesuai Case di UC_Kategori
            _ucDaftarMenu.TampilkanHalamanMenu("Makanan Ringan");
            gbKeranjang.Visible = true;
        }
        // 3. Tombol Minuman Panas
        private void btMinumanPanas_Click(object sender, EventArgs e)
        {
            if (!pnlModePOS.Controls.Contains(_ucDaftarMenu))
            {
                navigationControl(_ucDaftarMenu);
            }
            btDaftarMenu.Checked = false;

            _ucDaftarMenu.TampilkanHalamanMenu("Minuman Panas");
            gbKeranjang.Visible = true;
        }

        // 4. Tombol Makanan Penutup
        private void btMakananPenutup_Click(object sender, EventArgs e)
        {
            if (!pnlModePOS.Controls.Contains(_ucDaftarMenu))
            {
                navigationControl(_ucDaftarMenu);
            }
            btDaftarMenu.Checked = false; 

            _ucDaftarMenu.TampilkanHalamanMenu("Makanan Penutup");
            gbKeranjang.Visible = true;
        }

        // --- LOGIKA CEK PASSWORD ---
        private bool CekPasswordValid(string inputPassword)
        {
            bool isValid = false;
            try
            {
                // Ambil ID User yang sedang Login dari GlobalSession
                // (Pastikan class GlobalSession Anda bisa diakses, jika di dalam class Login, pakai Login.GlobalSession)
                int currentId = Login.GlobalSession.CurrentUserId;

                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != System.Data.ConnectionState.Open) conn.Open();

                    // Ambil password asli dari database berdasarkan ID yang sedang login
                    string query = "SELECT password FROM users WHERE id = @uid";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentId);

                        object result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            string dbPassword = result.ToString();
                            // Bandingkan password inputan dengan password di database
                            if (inputPassword == dbPassword)
                            {
                                isValid = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal verifikasi password: " + ex.Message);
            }
            return isValid;
        }

        // --- EVENT TOMBOL UBAH MODE ---
        private void btUbahMode_Click(object sender, EventArgs e)
        {
            // 1. Munculkan Form Pop-up Verifikasi
            using (Form_Verifikasi formVerif = new Form_Verifikasi())
            {
                // Tampilkan sebagai Dialog (user tidak bisa klik form belakangnya)
                formVerif.ShowDialog();

                // 2. Jika User klik "Konfirmasi" di pop-up
                if (formVerif.IsConfirmed)
                {
                    string passwordInput = formVerif.PasswordInput;

                    // 3. Cek ke Database apakah password benar
                    if (CekPasswordValid(passwordInput))
                    {
                        // --- SUKSES: Password Benar ---

                        // Buka Main Form (Panel Admin)
                        Main_Form adminPanel = new Main_Form();
                        adminPanel.Show();

                        // Tutup Mode POS
                        this.Close();
                    }
                    else
                    {
                        // --- GAGAL: Password Salah ---
                        MessageBox.Show("Password Salah! Gagal berpindah mode.", "Akses Ditolak", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btKeluar_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("Apakah Anda yakin ingin keluar dari aplikasi?",
                                                  "Konfirmasi Logout",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Question);

            if (dialog == DialogResult.Yes)
            {
                // Hapus Session
                Login.GlobalSession.ClearSession();

                // Hapus Data "Ingat Saya"
                Properties.Settings.Default.StatusIngat = false;
                Properties.Settings.Default.DisimpanEmail = "";
                Properties.Settings.Default.DisimpanPassword = "";
                Properties.Settings.Default.Save();

                // Kembali ke Login
                Login formLogin = new Login();
                formLogin.Show();
                this.Hide();
            }
        }

        private void btnBayar_Click(object sender, EventArgs e)
        {
            // 1. Validasi Keranjang Kosong
            if (_cartItems.Count == 0)
            {
                MessageBox.Show("Keranjang masih kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Ambil Total Tagihan & Metode Pembayaran
            double totalBelanja = ParseCurrency(lblTotalAkhir.Text);

            // Pastikan cmbPembayaran tidak kosong
            string metodeDipilih = cmbPembayaran.Text;
            if (string.IsNullOrEmpty(metodeDipilih)) metodeDipilih = "Cash"; // Default

            // 3. --- BUKA FORM BAYAR (Pop-up) ---
            // Pastikan Anda sudah membuat Form_Bayar.cs sesuai tutorial sebelumnya
            using (Form_Bayar formBayar = new Form_Bayar(totalBelanja, metodeDipilih))
            {
                formBayar.StartPosition = FormStartPosition.CenterParent;

                // Tampilkan Form Bayar dan tunggu hasilnya
                var result = formBayar.ShowDialog();

                // 4. JIKA PEMBAYARAN SUKSES (User klik Konfirmasi di Form Bayar)
                if (result == DialogResult.OK && formBayar.IsPaid)
                {
                    // Ambil info pembayaran dari Form Bayar
                    double uangDiterima = formBayar.CashGiven;
                    double kembalian = formBayar.ChangeDue;

                    // --- 5. LAKUKAN TRANSAKSI KE DATABASE ---
                    SimpanTransaksiKeDatabase(uangDiterima, kembalian);
                }
            }
        }

        // Method Terpisah untuk Database agar rapi
        private void SimpanTransaksiKeDatabase(double uangDiterima, double kembalian)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Generate No Batch Otomatis
                            string batchDate = DateTime.Now.ToString("yyyyMMdd");
                            string autoBatchNo = $"POS-{batchDate}-{DateTime.Now:HHmmss}";

                            foreach (var item in _cartItems)
                            {
                                // [A] AMBIL HPP
                                double hppSatuan = 0;
                                string queryGetHpp = "SELECT hpp FROM recipes WHERE id = @rid";
                                using (NpgsqlCommand cmdHpp = new NpgsqlCommand(queryGetHpp, conn))
                                {
                                    cmdHpp.Transaction = transaction;
                                    cmdHpp.Parameters.AddWithValue("@rid", item.RecipeId);
                                    object resultHpp = cmdHpp.ExecuteScalar();
                                    if (resultHpp != null && resultHpp != DBNull.Value)
                                        hppSatuan = Convert.ToDouble(resultHpp);
                                }

                                // [B] HITUNG KEUANGAN
                                double revenue = item.HargaSatuan * item.Quantity;
                                double totalHpp = hppSatuan * item.Quantity;
                                double profit = revenue - totalHpp;

                                // [C] INSERT SALES (Data Penjualan)
                                string querySales = @"INSERT INTO sales 
                                            (user_id, recipe_id, qty, selling_price, total_price, revenue, total_hpp, profit, discount, other_costs, sale_date, created_at) 
                                            VALUES 
                                            (@uid, @rid, @qty, @price, @total, @revenue, @thpp, @profit, 0, 0, @date, @created)";

                                using (NpgsqlCommand cmd = new NpgsqlCommand(querySales, conn))
                                {
                                    cmd.Transaction = transaction;
                                    cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                                    cmd.Parameters.AddWithValue("@rid", item.RecipeId);
                                    cmd.Parameters.AddWithValue("@qty", item.Quantity);
                                    cmd.Parameters.AddWithValue("@price", item.HargaSatuan);
                                    cmd.Parameters.AddWithValue("@total", revenue);
                                    cmd.Parameters.AddWithValue("@revenue", revenue);
                                    cmd.Parameters.AddWithValue("@thpp", totalHpp);
                                    cmd.Parameters.AddWithValue("@profit", profit);
                                    cmd.Parameters.AddWithValue("@date", DateTime.Now);
                                    cmd.Parameters.AddWithValue("@created", DateTime.Now);
                                    cmd.ExecuteNonQuery();
                                }

                                // [D] UPDATE STOK MENU (Barang Jadi Berkurang karena Terjual)
                                string queryStock = "UPDATE recipes SET stock = stock - @qty WHERE id = @rid";
                                using (NpgsqlCommand cmdStock = new NpgsqlCommand(queryStock, conn))
                                {
                                    cmdStock.Transaction = transaction;
                                    cmdStock.Parameters.AddWithValue("@qty", item.Quantity);
                                    cmdStock.Parameters.AddWithValue("@rid", item.RecipeId);
                                    cmdStock.ExecuteNonQuery();
                                }

                                // ---------------------------------------------------------
                                // [E] INSERT KE PRODUKSI OLAHAN (AUTO GENERATE)
                                // ---------------------------------------------------------
                                // Status: 'sedang_produksi'
                                // Produced Qty: 0 (karena belum selesai)
                                // Ingredients: TIDAK dipotong disini (nanti dipotong di modul Produksi saat status diubah jadi Selesai)

                                string queryBatch = @"INSERT INTO production_batches 
                                            (user_id, batch_number, recipe_id, target_qty, produced_qty, start_date, status, notes, created_at)
                                            VALUES 
                                            (@uid, @batch, @rid, @qty, 0, @date, 'sedang_produksi', 'Order dari POS', NOW())";

                                using (NpgsqlCommand cmdBatch = new NpgsqlCommand(queryBatch, conn))
                                {
                                    cmdBatch.Transaction = transaction;
                                    cmdBatch.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                                    // Suffix ID Menu agar unik jika beli 2 menu beda sekaligus
                                    cmdBatch.Parameters.AddWithValue("@batch", $"{autoBatchNo}-{item.RecipeId}");
                                    cmdBatch.Parameters.AddWithValue("@rid", item.RecipeId);
                                    cmdBatch.Parameters.AddWithValue("@qty", item.Quantity);
                                    cmdBatch.Parameters.AddWithValue("@date", DateTime.Now);
                                    cmdBatch.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();

                            // --- [SISA KODE SAMA: STRUK & RESET UI] ---

                            Form_Struk.CurrentReceipt.Clear();
                            Form_Struk.CurrentReceipt.NoFaktur = "TRJ-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
                            Form_Struk.CurrentReceipt.KasirName = Login.GlobalSession.CurrentUserEmail;
                            Form_Struk.CurrentReceipt.UserId = "USR-" + Login.GlobalSession.CurrentUserId;
                            Form_Struk.CurrentReceipt.WaktuTransaksi = DateTime.Now;

                            foreach (var item in _cartItems)
                            {
                                Form_Struk.CurrentReceipt.Items.Add(new Form_Struk.ReceiptItem
                                {
                                    Name = item.NamaMenu,
                                    Qty = item.Quantity,
                                    Price = item.HargaSatuan
                                });
                            }

                            double subtotalVal = ParseCurrency(lblSubtotal.Text.Replace("Subtotal", ""));
                            string pajakClean = lblPajak.Text.Replace("Tax", "").Replace("(", "").Replace(")", "").Replace("%", "").Replace(":", "").Trim();
                            double taxVal = ParseCurrency(pajakClean);
                            double totalVal = ParseCurrency(lblTotalAkhir.Text);

                            Form_Struk.CurrentReceipt.Subtotal = subtotalVal;
                            Form_Struk.CurrentReceipt.Tax = taxVal;
                            Form_Struk.CurrentReceipt.GrandTotal = totalVal;
                            Form_Struk.CurrentReceipt.CashPaid = uangDiterima;
                            Form_Struk.CurrentReceipt.ChangeDue = kembalian;

                            Form_Struk struk = new Form_Struk();
                            struk.StartPosition = FormStartPosition.CenterScreen;
                            struk.ShowDialog();

                            flpKeranjang.Controls.Clear();
                            _cartItems.Clear();
                            HitungTotalBelanja();
                            _ucDaftarMenu.TampilkanHalamanMenu("Semua");
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal Transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- FUNGSI HELPER BARU (Tambahkan di bawah method btnBayar_Click) ---
        // Fungsi ini berguna membersihkan teks "Rp 25.000" menjadi angka 25000 agar tidak error saat perhitungan
        private double ParseCurrency(string text)
        {
            try
            {
                // Hapus 'Rp', spasi, dan titik ribuan
                string clean = text.Replace("Rp", "").Replace(".", "").Replace(" ", "").Trim();
                if (double.TryParse(clean, out double result))
                {
                    return result;
                }
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private void btRiwayatTransaksi_Click(object sender, EventArgs e)
        {
            // 1. Buat instance UC_Riwayat
            UC_Riwayat ucRiwayat = new UC_Riwayat();

            // 2. Tampilkan menggunakan fungsi navigasi yang sudah Anda punya
            navigationControl(ucRiwayat);

            // 3. Matikan tombol lain (Visual saja)
            btDaftarMenu.Checked = false;
            btRiwayatTransaksi.Checked = true;
            gbKeranjang.Visible = false;
        }
    }
}