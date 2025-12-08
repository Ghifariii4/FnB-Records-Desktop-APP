using FnB_Records.Koneksi_DB;
using Guna.UI2.WinForms;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class UC_PurchaseOrder : UserControl
    {
        private readonly string SUPABASE_URL = "https://gvzcpqyxzormnzjixhap.supabase.co";
        private readonly string SUPABASE_KEY = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Imd2emNwcXl4em9ybW56aml4aGFwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjUwMTA3OTYsImV4cCI6MjA4MDU4Njc5Nn0.gU5F8HavI6A4KngY-vnNdfchW5poHJT_jDDeP6mNubc";

        private const int MIN_HEIGHT = 742;
        private const int MAX_HEIGHT = 899;

        // --- KONFIGURASI SIZE DETAIL PO (BARU) ---
        private const int MIN_DETAIL_HEIGHT = 622;

        private const int WIDTH_FORM = 540;
        private const int ITEM_HEIGHT = 85;
        private const int GAP = 10;

        public UC_PurchaseOrder()
        {
            InitializeComponent();
            this.Size = new Size(WIDTH_FORM, MIN_HEIGHT);

            flpItemPO.AutoScroll = false;
            flpItemPO.FlowDirection = FlowDirection.TopDown;
            flpItemPO.WrapContents = false;
        }

        private void UC_PurchaseOrder_Load(object sender, EventArgs e)
        {
            if (Login.GlobalSession.CurrentUserId == 0)
            {
                Login.GlobalSession.CurrentUserId = 1;
            }

            dgvDataPurchaseOrder.AlternatingRowsDefaultCellStyle.BackColor = Color.White;

            LoadVendor();
            TambahItemPO();
            LoadDataPO();
        }

        private void btBuatPOBaru_Click(object sender, EventArgs e)
        {
            gbPopUpPO.Visible = true;
            gbPopUpPO.BringToFront();
        }

        private void btnClosePopUpPO_Click(object sender, EventArgs e)
        {
            gbPopUpPO.Visible = false;
        }

        private void btnTambahItem_Click(object sender, EventArgs e)
        {
            TambahItemPO();
        }

        private void LoadVendor()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    string query = "SELECT id, name FROM vendors WHERE user_id = @uid ORDER BY name ASC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            cbVendor.DataSource = dt;
                            cbVendor.DisplayMember = "name";
                            cbVendor.ValueMember = "id";
                            cbVendor.SelectedIndex = -1;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat vendor: " + ex.Message);
            }
        }

        private void TambahItemPO()
        {
            ContainerItemPO itemBaru = new ContainerItemPO();

            itemBaru.LoadBahanBaku(Login.GlobalSession.CurrentUserId);

            itemBaru.OnDataChanged += (s, e) => HitungTotalKeseluruhan();
            itemBaru.OnDeleteRequest += (s, e) => HapusItemPO(itemBaru);

            flpItemPO.Controls.Add(itemBaru);

            UpdateLayoutPosisi();
        }

        private void HapusItemPO(ContainerItemPO item)
        {
            flpItemPO.Controls.Remove(item);
            item.Dispose();

            UpdateLayoutPosisi();
            HitungTotalKeseluruhan();
        }

        private void UpdateLayoutPosisi()
        {
            int totalTinggiItem = flpItemPO.Controls.Count * ITEM_HEIGHT;

            flpItemPO.Height = totalTinggiItem;

            if (pnlFooter != null)
            {
                pnlFooter.Location = new Point(pnlFooter.Location.X, flpItemPO.Location.Y + flpItemPO.Height + GAP);

                int newHeight = pnlFooter.Location.Y + pnlFooter.Height + 20;

                if (newHeight > MAX_HEIGHT)
                {
                    gbPopUpPO.Height = MAX_HEIGHT;

                    int sisaRuang = MAX_HEIGHT - pnlFooter.Height - flpItemPO.Location.Y - 30;
                    flpItemPO.Height = sisaRuang;
                    flpItemPO.AutoScroll = true;

                    pnlFooter.Location = new Point(pnlFooter.Location.X, flpItemPO.Location.Y + flpItemPO.Height + GAP);
                }
                else if (newHeight < MIN_HEIGHT)
                {
                    gbPopUpPO.Height = MIN_HEIGHT;
                    flpItemPO.AutoScroll = false;
                }
                else
                {
                    gbPopUpPO.Height = newHeight;
                    flpItemPO.AutoScroll = false;
                }
            }
        }

        private void HitungTotalKeseluruhan()
        {
            decimal subtotal = 0;

            foreach (Control ctrl in flpItemPO.Controls)
            {
                if (ctrl is ContainerItemPO item)
                {
                    subtotal += item.Subtotal;
                }
            }

            decimal ppn = subtotal * 0.1m;
            decimal total = subtotal + ppn;

            lblSubtotal.Text = $"Rp. {subtotal:N2}";
            lblPPn.Text = $"Rp. {ppn:N2}";
            lblTotal.Text = $"Rp. {total:N2}";
        }

        private async void guna2Button19_Click(object sender, EventArgs e)
        {
            if (cbVendor.SelectedItem == null)
            {
                MessageBox.Show("Pilih Vendor terlebih dahulu!");
                return;
            }

            bool adaItem = false;
            foreach (Control c in flpItemPO.Controls)
            {
                if (c is ContainerItemPO)
                {
                    adaItem = true;
                    break;
                }
            }

            if (!adaItem)
            {
                MessageBox.Show("Tambahkan minimal 1 item!");
                return;
            }

            string poNumber = "PO-" + DateTime.Now.ToString("yyyyMMdd-HHmmss");
            int vendorId = Convert.ToInt32(cbVendor.SelectedValue);
            string catatan = txtCatatan.Text;
            decimal totalAmount = decimal.Parse(lblTotal.Text.Replace("Rp. ", "").Replace(".", ""));

            try
            {
                btnSimpan.Enabled = false;

                int newPOId = SimpanKePostgres(poNumber, vendorId, totalAmount, catatan);

                if (newPOId > 0)
                {
                    bool supabaseSuccess = await SimpanKeSupabase(newPOId, poNumber, vendorId, totalAmount, catatan);

                    if (supabaseSuccess)
                    {
                        MessageBox.Show("PO Berhasil disimpan ke Local & Cloud!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Disimpan di Local, tapi gagal upload ke Cloud (Cek koneksi).", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    btnClosePopUpPO_Click(sender, e);
                    LoadDataPO();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat menyimpan: " + ex.Message);
            }
            finally
            {
                btnSimpan.Enabled = true;
            }
        }

        private int SimpanKePostgres(string poNo, int vendorId, decimal total, string note)
        {
            int poId = 0;
            Koneksi db = new Koneksi();

            using (NpgsqlConnection conn = db.GetKoneksi())
            {
                using (NpgsqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string queryHeader = @"
                            INSERT INTO purchases (user_id, vendor_id, po_number, status, total_amount, notes, created_at)
                            VALUES (@uid, @vid, @pono, 'draft', @total, @notes, @created)
                            RETURNING id";

                        using (NpgsqlCommand cmd = new NpgsqlCommand(queryHeader, conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);
                            cmd.Parameters.AddWithValue("@vid", vendorId);
                            cmd.Parameters.AddWithValue("@pono", poNo);
                            cmd.Parameters.AddWithValue("@total", total);
                            cmd.Parameters.AddWithValue("@notes", note ?? (object)DBNull.Value);
                            cmd.Parameters.AddWithValue("@created", DateTime.Now);

                            poId = (int)cmd.ExecuteScalar();
                        }

                        string queryItem = @"
                            INSERT INTO purchase_items (purchase_id, ingredient_id, qty, price, subtotal, created_at)
                            VALUES (@poid, @iid, @qty, @price, @sub, @created)";

                        foreach (Control ctrl in flpItemPO.Controls)
                        {
                            if (ctrl is ContainerItemPO item)
                            {
                                if (item.SelectedItemId != null)
                                {
                                    int itemId = (int)item.SelectedItemId;
                                    decimal qty = item.Quantity;
                                    decimal price = item.Price;
                                    decimal subtotal = item.Subtotal;

                                    using (NpgsqlCommand cmdItem = new NpgsqlCommand(queryItem, conn, trans))
                                    {
                                        cmdItem.Parameters.AddWithValue("@poid", poId);
                                        cmdItem.Parameters.AddWithValue("@iid", itemId);
                                        cmdItem.Parameters.AddWithValue("@qty", qty);
                                        cmdItem.Parameters.AddWithValue("@price", price);
                                        cmdItem.Parameters.AddWithValue("@sub", subtotal);
                                        cmdItem.Parameters.AddWithValue("@created", DateTime.Now);
                                        cmdItem.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        trans.Commit();
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
            return poId;
        }

        private async Task<bool> SimpanKeSupabase(int localId, string poNo, int vendorId, decimal total, string note)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("apikey", SUPABASE_KEY);
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + SUPABASE_KEY);

                    var payload = new
                    {
                        user_id = Login.GlobalSession.CurrentUserId,
                        vendor_id = vendorId,
                        po_number = poNo,
                        status = "draft",
                        total_amount = total,
                        notes = note,
                        created_at = DateTime.Now.ToString("o")
                    };

                    var jsonContent = new StringContent(
                        JsonSerializer.Serialize(payload),
                        Encoding.UTF8,
                        "application/json");

                    string url = $"{SUPABASE_URL}/rest/v1/purchase_orders";

                    var response = await client.PostAsync(url, jsonContent);

                    if (!response.IsSuccessStatusCode)
                    {
                        string errorDetail = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Gagal Upload ke Cloud!\nKode: {response.StatusCode}\nPesan: {errorDetail}",
                                        "Supabase Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Koneksi Supabase: " + ex.Message);
                return false;
            }
        }

        private void LoadDataPO()
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    string query = @"
                SELECT 
                    p.id,
                    p.po_number,
                    v.name AS vendor_name,
                    (p.total_amount / 1.1) AS subtotal, 
                    (p.total_amount - (p.total_amount / 1.1)) AS ppn,
                    p.total_amount,
                    p.status,
                    p.created_at
                FROM purchases p
                JOIN vendors v ON p.vendor_id = v.id
                WHERE p.user_id = @uid
                ORDER BY p.created_at DESC";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", Login.GlobalSession.CurrentUserId);

                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            dgvDataPurchaseOrder.DataSource = dt;

                            if (dgvDataPurchaseOrder.Columns.Contains("id"))
                                dgvDataPurchaseOrder.Columns["id"].Visible = false;

                            SetHeaderText("po_number", "No. PO");
                            SetHeaderText("vendor_name", "Vendor");
                            SetHeaderText("subtotal", "Subtotal");
                            SetHeaderText("ppn", "PPN 10%");
                            SetHeaderText("total_amount", "Total");
                            SetHeaderText("status", "Status");
                            SetHeaderText("created_at", "Tanggal");

                            FormatCurrencyColumn("subtotal");
                            FormatCurrencyColumn("ppn");
                            FormatCurrencyColumn("total_amount");

                            if (dgvDataPurchaseOrder.Columns.Contains("created_at"))
                                dgvDataPurchaseOrder.Columns["created_at"].DefaultCellStyle.Format = "dd/MM/yyyy";

                            if (!dgvDataPurchaseOrder.Columns.Contains("Aksi"))
                            {
                                DataGridViewImageColumn imgCol = new DataGridViewImageColumn();
                                imgCol.Name = "Aksi";
                                imgCol.HeaderText = "Aksi";
                                imgCol.ImageLayout = DataGridViewImageCellLayout.Zoom;
                                dgvDataPurchaseOrder.Columns.Add(imgCol);
                            }

                            if (dgvDataPurchaseOrder.Columns.Contains("Aksi"))
                            {
                                dgvDataPurchaseOrder.Columns["Aksi"].DisplayIndex = dgvDataPurchaseOrder.Columns.Count - 1;
                                dgvDataPurchaseOrder.Columns["Aksi"].Width = 50;
                            }

                            dgvDataPurchaseOrder.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                            dgvDataPurchaseOrder.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                            if (dgvDataPurchaseOrder.Columns.Contains("vendor_name"))
                                dgvDataPurchaseOrder.Columns["vendor_name"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                            dgvDataPurchaseOrder.ClearSelection();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data PO: " + ex.Message);
            }
        }

        private void SetHeaderText(string colName, string headerText)
        {
            if (dgvDataPurchaseOrder.Columns.Contains(colName))
                dgvDataPurchaseOrder.Columns[colName].HeaderText = headerText;
        }

        private void FormatCurrencyColumn(string colName)
        {
            if (dgvDataPurchaseOrder.Columns.Contains(colName))
            {
                dgvDataPurchaseOrder.Columns[colName].DefaultCellStyle.Format = "C0";
                dgvDataPurchaseOrder.Columns[colName].DefaultCellStyle.FormatProvider = System.Globalization.CultureInfo.GetCultureInfo("id-ID");
            }
        }

        private void dgvDataPurchaseOrder_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dgvDataPurchaseOrder.Columns[e.ColumnIndex].Name == "status" && e.Value != null)
            {
                string status = e.Value.ToString().ToLower();

                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.Font = new Font(dgvDataPurchaseOrder.Font, FontStyle.Bold);

                switch (status)
                {
                    case "draft":
                        e.CellStyle.BackColor = Color.Gray;
                        e.CellStyle.SelectionBackColor = Color.Gray;
                        break;
                    case "pending":
                        e.CellStyle.BackColor = Color.Orange;
                        e.CellStyle.SelectionBackColor = Color.Orange;
                        break;
                    case "diproses":
                        e.CellStyle.BackColor = Color.DodgerBlue;
                        e.CellStyle.SelectionBackColor = Color.DodgerBlue;
                        break;
                    case "selesai":
                        e.CellStyle.BackColor = Color.SeaGreen;
                        e.CellStyle.SelectionBackColor = Color.SeaGreen;
                        break;
                    default:
                        e.CellStyle.BackColor = Color.White;
                        e.CellStyle.ForeColor = Color.Black;
                        break;
                }
            }
        }

        private void dgvDataPurchaseOrder_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvDataPurchaseOrder.Columns[e.ColumnIndex].Name == "Aksi")
            {
                if (dgvDataPurchaseOrder.Rows[e.RowIndex].Cells["id"].Value != null)
                {
                    int idPO = Convert.ToInt32(dgvDataPurchaseOrder.Rows[e.RowIndex].Cells["id"].Value);
                    TampilkanDetailPO(idPO);
                }
            }
        }

        private void guna2Button21_Click(object sender, EventArgs e)
        {
            gbDetailItemPO.Visible = false;
        }

        // --- METHOD TAMBAHAN UNTUK DETAIL DINAMIS ---

        private void TampilkanDetailPO(int poId)
        {
            try
            {
                Koneksi db = new Koneksi();
                using (NpgsqlConnection conn = db.GetKoneksi())
                {
                    if (conn.State != ConnectionState.Open) conn.Open();

                    // --- QUERY 1: HEADER ---
                    string queryHeader = @"SELECT p.po_number, v.name AS vendor_name, p.created_at, p.status, p.total_amount, (p.total_amount / 1.1) AS subtotal, (p.total_amount - (p.total_amount / 1.1)) AS ppn FROM purchases p JOIN vendors v ON p.vendor_id = v.id WHERE p.id = @id";

                    using (NpgsqlCommand cmd = new NpgsqlCommand(queryHeader, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", poId);
                        using (NpgsqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblNoPO.Text = reader["po_number"].ToString();
                                lblVendor.Text = reader["vendor_name"].ToString();
                                DateTime tgl = Convert.ToDateTime(reader["created_at"]);
                                lblDibuat.Text = tgl.ToString("d MMMM 'Pukul' HH.mm", new System.Globalization.CultureInfo("id-ID"));
                                string status = reader["status"].ToString();
                                lblStatus.Text = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(status);
                                AturWarnaStatus(status);

                                decimal total = Convert.ToDecimal(reader["total_amount"]);
                                decimal sub = Convert.ToDecimal(reader["subtotal"]);
                                decimal ppn = Convert.ToDecimal(reader["ppn"]);

                                lblDetailTotal.Text = "Rp. " + total.ToString("N0");
                                lblDetailSubtotal.Text = "Rp. " + sub.ToString("N2");
                                lblDetailPPn.Text = "Rp. " + ppn.ToString("N2");
                            }
                        }
                    }

                    // --- QUERY 2: ITEM (GRID) ---
                    string queryItems = @"SELECT i.name AS nama_barang, pi.qty, pi.price AS harga_satuan, pi.subtotal FROM purchase_items pi JOIN ingredients i ON pi.ingredient_id = i.id WHERE pi.purchase_id = @id";

                    using (NpgsqlCommand cmdItem = new NpgsqlCommand(queryItems, conn))
                    {
                        cmdItem.Parameters.AddWithValue("@id", poId);
                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmdItem))
                        {
                            DataTable dtItems = new DataTable();
                            da.Fill(dtItems);
                            dgvDetailItemPO.DataSource = dtItems;
                            FormatGridDetail();
                        }
                    }
                }

                gbDetailItemPO.Visible = true;
                gbDetailItemPO.BringToFront();

                // PANGGIL LOGIKA RESIZING DINAMIS
                UpdateDetailLayoutPosisi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat detail: " + ex.Message);
            }
        }

        private void FormatGridDetail()
        {
            dgvDetailItemPO.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Baris ini membuat teks isi sel rata tengah
            dgvDetailItemPO.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            if (dgvDetailItemPO.Columns.Contains("nama_barang")) dgvDetailItemPO.Columns["nama_barang"].HeaderText = "Nama Item";
            if (dgvDetailItemPO.Columns.Contains("qty")) dgvDetailItemPO.Columns["qty"].HeaderText = "Qty";
            if (dgvDetailItemPO.Columns.Contains("harga_satuan")) dgvDetailItemPO.Columns["harga_satuan"].HeaderText = "Harga";
            if (dgvDetailItemPO.Columns.Contains("subtotal")) dgvDetailItemPO.Columns["subtotal"].HeaderText = "Subtotal";

            if (dgvDetailItemPO.Columns.Contains("harga_satuan")) dgvDetailItemPO.Columns["harga_satuan"].DefaultCellStyle.Format = "N0";
            if (dgvDetailItemPO.Columns.Contains("subtotal")) dgvDetailItemPO.Columns["subtotal"].DefaultCellStyle.Format = "N0";

            dgvDetailItemPO.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvDetailItemPO.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            if (dgvDetailItemPO.Columns.Contains("nama_barang")) dgvDetailItemPO.Columns["nama_barang"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            dgvDetailItemPO.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvDetailItemPO.ClearSelection();
        }

        private void AturWarnaStatus(string status)
        {
            status = status.ToLower();
            lblStatus.ForeColor = Color.White;
            switch (status)
            {
                case "draft": pnlStatus.FillColor = Color.Gray; break;
                case "pending": pnlStatus.FillColor = Color.Orange; break;
                case "diproses": pnlStatus.FillColor = Color.DodgerBlue; break;
                case "selesai": pnlStatus.FillColor = Color.SeaGreen; break;
                default: pnlStatus.FillColor = Color.Black; break;
            }
        }

        private void UpdateDetailLayoutPosisi()
        {
            // 1. Hitung Tinggi Konten Grid (+ Header + sedikit padding)
            int gridContentHeight = dgvDetailItemPO.ColumnHeadersHeight +
                                    dgvDetailItemPO.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + 2;

            // 2. Set Tinggi Grid & Container
            dgvDetailItemPO.Height = gridContentHeight;
            gbContainerItemPO.Height = gridContentHeight + 20; // Tambah margin container

            // 3. Geser Footer (pnlFooterDetail)
            pnlFooterDetail.Location = new Point(pnlFooterDetail.Location.X,
                                                 gbContainerItemPO.Location.Y + gbContainerItemPO.Height + GAP);

            // 4. Hitung Total Tinggi
            int newTotalHeight = pnlFooterDetail.Location.Y + pnlFooterDetail.Height + 30;

            // 5. Cek Limit Size (622 - 899)
            if (newTotalHeight < MIN_DETAIL_HEIGHT)
            {
                gbDetailItemPO.Height = MIN_DETAIL_HEIGHT;
            }
            else if (newTotalHeight > MAX_HEIGHT)
            {
                gbDetailItemPO.Height = MAX_HEIGHT;

                // Jika mentok Max, atur agar grid/container scrollable atau pas di sisa ruang
                int availableSpace = MAX_HEIGHT - pnlFooterDetail.Height - gbContainerItemPO.Location.Y - 40;
                gbContainerItemPO.Height = availableSpace;
                dgvDetailItemPO.Height = availableSpace - 20; // Kurangi sedikit agar muat di container

                pnlFooterDetail.Location = new Point(pnlFooterDetail.Location.X,
                                                     gbContainerItemPO.Location.Y + gbContainerItemPO.Height + GAP);
            }
            else
            {
                gbDetailItemPO.Height = newTotalHeight;
            }
        }
    }
}