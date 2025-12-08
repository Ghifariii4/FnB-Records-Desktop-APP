using FnB_Records.Koneksi_DB; // Sesuaikan namespace
using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class ContainerItemPO : UserControl
    {
        // Event agar Form Induk tahu ada perubahan
        public event EventHandler OnDataChanged; // Saat QTY/Harga berubah
        public event EventHandler OnDeleteRequest; // Saat tombol X diklik

        public ContainerItemPO()
        {
            InitializeComponent();
        }

        // --- PUBLIC PROPERTIES (Agar Form Induk bisa ambil data) ---

        public object SelectedItemId
        {
            get { return cbItemPO.SelectedValue; } // Jauh lebih simpel
        }

        public decimal Quantity
        {
            get
            {
                decimal.TryParse(txtQTY.Text, out decimal val);
                return val;
            }
        }

        public decimal Price
        {
            get
            {
                decimal.TryParse(txtHarga.Text, out decimal val);
                return val;
            }
        }

        public decimal Subtotal
        {
            get { return Quantity * Price; }
        }

        // --- METHOD LOAD DATA ---
        // Dipanggil dari Form Induk saat UC ini dibuat
        public void LoadBahanBaku(int userId)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();
                    string query = "SELECT id, name FROM ingredients WHERE user_id = @uid ORDER BY name ASC";
                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            cbItemPO.DataSource = dt;
                            cbItemPO.DisplayMember = "name";
                            cbItemPO.ValueMember = "id";
                            cbItemPO.SelectedIndex = -1;
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // Dan update property SelectedItemId-nya:
        

        // --- EVENTS ---

        private void txtQTY_TextChanged(object sender, EventArgs e)
        {
            // Panggil event ke induk
            OnDataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void txtHarga_TextChanged(object sender, EventArgs e)
        {
            OnDataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            // Minta induk untuk menghapus saya
            OnDeleteRequest?.Invoke(this, EventArgs.Empty);
        }

        // Validasi Angka
        private void txtAngka_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        }
    }
}