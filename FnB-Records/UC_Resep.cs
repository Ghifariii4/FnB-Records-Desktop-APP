using FnB_Records.Koneksi_DB;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class UC_Resep : UserControl
    {
        // --- 1. MODEL & VARIABEL ---
        private class IngredientItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
            public string Unit { get; set; }
            public string Display => $"{Name} (Rp {Price:#,##0}/{Unit})";
        }

        private readonly Koneksi db = new Koneksi();
        private List<IngredientItem> listBahanMaster = new List<IngredientItem>();

        private const double SUGGESTED_MULTIPLIER = 3.0; // Margin x3
        private int currentUserId => Login.GlobalSession.CurrentUserId;
        private int? editingRecipeId = null;

        public UC_Resep()
        {
            InitializeComponent();
            gbResepPopUp.BackColor = Color.White;
        }

        // --- 2. SETUP EVENT & LOAD ---
        private void AttachEvents()
        {
            btnTambahResep.Click += BtnTambahResep_Click;
            btnTambahBahan.Click += BtnTambahBahan_Click;
            btnSimpanPopUpResep.Click += BtnSimpanPopUpResep_Click;
            btnBatalPopUpResep.Click += BtnBatalPopUpResep_Click;
            btnClosePopUpResep.Click += BtnBatalPopUpResep_Click;

            txtCariResep.TextChanged += TxtCariResep_TextChanged;
            txtInputJumlahPorsi.TextChanged += AnyInputAffectingHpp_Changed;
            txtInputHargaJualTarget.TextChanged += AnyInputAffectingHpp_Changed;

            dgvResepMenu.CellContentClick += DgvResepMenu_CellContentClick;
            dgvResepMenu.CellDoubleClick += DgvResepMenu_CellDoubleClick;
        }

        private void UC_Resep_Load(object sender, EventArgs e)
        {
            if (currentUserId == 0) Login.GlobalSession.CurrentUserId = 1;

            AttachEvents();
            LoadBahanMasterFromDB();
            SetupGridColumns();
            LoadRecipes();
        }

        // --- 3. LOAD DATA MASTER BAHAN ---
        private void LoadBahanMasterFromDB()
        {
            listBahanMaster.Clear();
            try
            {
                using (var conn = db.GetKoneksi())
                {
                    string sql = "SELECT id, name, price, unit FROM ingredients WHERE user_id = @uid ORDER BY name ASC";
                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("uid", currentUserId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                listBahanMaster.Add(new IngredientItem
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.GetDouble(2),
                                    Unit = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal memuat bahan baku: " + ex.Message); }
        }

        // --- 4. LOGIKA TAMBAH BAHAN KE PANEL ---
        private void BtnTambahBahan_Click(object sender, EventArgs e)
        {
            AddBahanRow();
        }

        private void AddBahanRow(int? selectedBahanId = null, double qtyValue = 0)
        {
            var rowPanel = new Panel
            {
                Width = flowPanelBahan.Width - 25,
                Height = 45,
                Margin = new Padding(0, 0, 0, 5)
            };

            var cb = new Guna.UI2.WinForms.Guna2ComboBox
            {
                Name = "cbPilihBahan",
                DataSource = new BindingList<IngredientItem>(listBahanMaster),
                DisplayMember = "Display",
                ValueMember = "Id",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 260,
                Height = 36,
                Location = new Point(5, 5),
                BorderRadius = 5
            };

            if (selectedBahanId.HasValue) cb.SelectedValue = selectedBahanId.Value;
            else cb.SelectedIndex = -1;

            var txtQty = new Guna.UI2.WinForms.Guna2TextBox
            {
                Name = "txtInputJumlahBahan",
                Width = 100,
                Height = 36,
                Location = new Point(275, 5),
                PlaceholderText = "Qty",
                BorderRadius = 5
            };
            if (qtyValue > 0) txtQty.Text = qtyValue.ToString();

            var btnDel = new Guna.UI2.WinForms.Guna2Button
            {
                Text = "X",
                Width = 40,
                Height = 36,
                Location = new Point(385, 5),
                FillColor = Color.FromArgb(255, 192, 192),
                ForeColor = Color.Red,
                BorderRadius = 5,
                Cursor = Cursors.Hand
            };

            cb.SelectedIndexChanged += (s, e) => RecalculateHpp();
            txtQty.TextChanged += (s, e) => RecalculateHpp();

            btnDel.Click += (s, e) =>
            {
                flowPanelBahan.Controls.Remove(rowPanel);
                rowPanel.Dispose();
                RecalculateHpp();
            };

            rowPanel.Controls.Add(cb);
            rowPanel.Controls.Add(txtQty);
            rowPanel.Controls.Add(btnDel);

            flowPanelBahan.Controls.Add(rowPanel);
        }

        // --- 5. PERHITUNGAN HPP & PROFIT ---
        private void AnyInputAffectingHpp_Changed(object sender, EventArgs e) => RecalculateHpp();

        private void RecalculateHpp()
        {
            double totalCost = 0.0;

            foreach (Panel p in flowPanelBahan.Controls.OfType<Panel>())
            {
                var cb = p.Controls.OfType<Guna.UI2.WinForms.Guna2ComboBox>().FirstOrDefault();
                var txt = p.Controls.OfType<Guna.UI2.WinForms.Guna2TextBox>().FirstOrDefault();

                if (cb?.SelectedItem is IngredientItem ing && txt != null)
                {
                    if (double.TryParse(txt.Text.Replace(",", "."),
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double qty))
                    {
                        totalCost += ing.Price * qty;
                    }
                }
            }

            // Total HPP (bukan per porsi)
            lblHPP.Text = $"Rp {totalCost:#,##0}";

            // HPP per porsi untuk perhitungan
            int servings = 1;
            int.TryParse(txtInputJumlahPorsi.Text, out servings);
            if (servings <= 0) servings = 1;

            double hppPerServing = totalCost / servings;

            // Hitung Saran Harga
            double suggested = hppPerServing * SUGGESTED_MULTIPLIER;
            lblHargaJualDisarankan.Text = $"Rp {suggested:#,##0}";

            // Hitung Profit
            double target = 0;
            double.TryParse(txtInputHargaJualTarget.Text.Replace(".", "").Replace(",", ""), out target);

            double profit = target - hppPerServing;
            double margin = target > 0 ? (profit / target * 100) : 0;

            lblEstimasiProfit.Text = $"Estimasi Profit: Rp {profit:#,##0} ({margin:0.0}%)";
        }

        // --- 6. SIMPAN DATA (INSERT / UPDATE) ---
        private void BtnSimpanPopUpResep_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInputNamaResep.Text))
            {
                MessageBox.Show("Nama resep wajib diisi!", "Warning");
                return;
            }

            // Ambil HPP TOTAL (bukan per porsi)
            string hppText = lblHPP.Text.Replace("Rp", "").Replace(".", "").Replace(",", "").Trim();
            double hppTotal = double.TryParse(hppText, out double h) ? h : 0;

            double targetPrice = 0;
            double.TryParse(txtInputHargaJualTarget.Text.Replace(".", "").Replace(",", ""), out targetPrice);

            int servings = int.TryParse(txtInputJumlahPorsi.Text, out int s) && s > 0 ? s : 1;

            try
            {
                using (var conn = db.GetKoneksi())
                {
                    using (var trans = conn.BeginTransaction())
                    {
                        try
                        {
                            int recipeId = 0;

                            // A. Header Resep
                            if (editingRecipeId == null) // INSERT
                            {
                                string sqlResep = @"
                                    INSERT INTO recipes (user_id, name, description, serving_size, hpp, suggested_price, created_at, updated_at)
                                    VALUES (@uid, @name, @desc, @serv, @hpp, @price, NOW(), NOW()) 
                                    RETURNING id";

                                using (var cmd = new NpgsqlCommand(sqlResep, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@uid", currentUserId);
                                    cmd.Parameters.AddWithValue("@name", txtInputNamaResep.Text.Trim());
                                    cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(txtInputDeskripsi.Text) ? "" : txtInputDeskripsi.Text.Trim());
                                    cmd.Parameters.AddWithValue("@serv", servings);
                                    cmd.Parameters.AddWithValue("@hpp", hppTotal); // HPP TOTAL
                                    cmd.Parameters.AddWithValue("@price", targetPrice);

                                    recipeId = (int)cmd.ExecuteScalar();
                                }
                            }
                            else // UPDATE
                            {
                                recipeId = editingRecipeId.Value;
                                string sqlUpdate = @"
                                    UPDATE recipes 
                                    SET name=@name, description=@desc, serving_size=@serv, 
                                        hpp=@hpp, suggested_price=@price, updated_at=NOW() 
                                    WHERE id=@id AND user_id=@uid";

                                using (var cmd = new NpgsqlCommand(sqlUpdate, conn, trans))
                                {
                                    cmd.Parameters.AddWithValue("@name", txtInputNamaResep.Text.Trim());
                                    cmd.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(txtInputDeskripsi.Text) ? "" : txtInputDeskripsi.Text.Trim());
                                    cmd.Parameters.AddWithValue("@serv", servings);
                                    cmd.Parameters.AddWithValue("@hpp", hppTotal); // HPP TOTAL
                                    cmd.Parameters.AddWithValue("@price", targetPrice);
                                    cmd.Parameters.AddWithValue("@id", recipeId);
                                    cmd.Parameters.AddWithValue("@uid", currentUserId);
                                    cmd.ExecuteNonQuery();
                                }

                                // Hapus detail lama - TABEL YANG BENAR: recipe_items
                                using (var cmdDel = new NpgsqlCommand("DELETE FROM recipe_items WHERE recipe_id=@rid", conn, trans))
                                {
                                    cmdDel.Parameters.AddWithValue("@rid", recipeId);
                                    cmdDel.ExecuteNonQuery();
                                }
                            }

                            // B. Detail Bahan - TABEL: recipe_items, KOLOM: qty
                            foreach (Panel p in flowPanelBahan.Controls.OfType<Panel>())
                            {
                                var cb = p.Controls.OfType<Guna.UI2.WinForms.Guna2ComboBox>().FirstOrDefault();
                                var txt = p.Controls.OfType<Guna.UI2.WinForms.Guna2TextBox>().FirstOrDefault();

                                if (cb != null && cb.SelectedValue != null && txt != null)
                                {
                                    if (double.TryParse(txt.Text.Replace(",", "."),
                                        System.Globalization.NumberStyles.Any,
                                        System.Globalization.CultureInfo.InvariantCulture, out double qty) && qty > 0)
                                    {
                                        int ingredientId = (int)cb.SelectedValue;

                                        // PERBAIKAN: Gunakan recipe_items dan kolom qty
                                        string sqlIng = @"
                                            INSERT INTO recipe_items (recipe_id, ingredient_id, qty, created_at) 
                                            VALUES (@rid, @iid, @qty, NOW())";

                                        using (var cmdIng = new NpgsqlCommand(sqlIng, conn, trans))
                                        {
                                            cmdIng.Parameters.AddWithValue("@rid", recipeId);
                                            cmdIng.Parameters.AddWithValue("@iid", ingredientId);
                                            cmdIng.Parameters.AddWithValue("@qty", qty);
                                            cmdIng.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }

                            trans.Commit();
                            MessageBox.Show("✅ Resep berhasil disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            gbResepPopUp.Visible = false;
                            LoadRecipes();
                        }
                        catch (Exception ex)
                        {
                            trans.Rollback();
                            throw ex;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error menyimpan resep:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 7. LOAD DATA UNTUK EDIT ---
        private void LoadRecipeIntoPopup(int recipeId)
        {
            try
            {
                using (var conn = db.GetKoneksi())
                {
                    // Header
                    using (var cmd = new NpgsqlCommand("SELECT * FROM recipes WHERE id=@id", conn))
                    {
                        cmd.Parameters.AddWithValue("@id", recipeId);
                        using (var r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                            {
                                editingRecipeId = recipeId;
                                txtInputNamaResep.Text = r["name"].ToString();
                                txtInputDeskripsi.Text = r["description"].ToString();
                                txtInputJumlahPorsi.Text = r["serving_size"].ToString();
                                txtInputHargaJualTarget.Text = Convert.ToDouble(r["suggested_price"]).ToString("N0");
                            }
                        }
                    }

                    // Detail Bahan - TABEL YANG BENAR: recipe_items
                    flowPanelBahan.Controls.Clear();
                    string sqlDet = "SELECT ingredient_id, qty FROM recipe_items WHERE recipe_id=@rid";
                    using (var cmd = new NpgsqlCommand(sqlDet, conn))
                    {
                        cmd.Parameters.AddWithValue("@rid", recipeId);
                        using (var r = cmd.ExecuteReader())
                        {
                            bool ada = false;
                            while (r.Read())
                            {
                                AddBahanRow(r.GetInt32(0), r.GetDouble(1));
                                ada = true;
                            }
                            if (!ada) AddBahanRow();
                        }
                    }
                }
                lblResep.Text = "Edit Resep";
                gbResepPopUp.Visible = true;
                gbResepPopUp.BringToFront();
                RecalculateHpp();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal load resep:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 8. CONFIG GRID & LOAD DAFTAR RESEP ---
        private void SetupGridColumns()
        {
            dgvResepMenu.Columns.Clear();
            dgvResepMenu.Theme = Guna.UI2.WinForms.Enums.DataGridViewPresetThemes.Light;

            dgvResepMenu.Columns.Add("id", "ID");
            dgvResepMenu.Columns["id"].Visible = false;
            dgvResepMenu.Columns.Add("col_name", "Nama Resep");
            dgvResepMenu.Columns.Add("col_serving", "Porsi");
            dgvResepMenu.Columns.Add("col_hpp", "HPP Total");
            dgvResepMenu.Columns.Add("col_hpp_per_porsi", "HPP/Porsi");
            dgvResepMenu.Columns.Add("col_price", "Harga Jual");
            dgvResepMenu.Columns.Add("col_profit", "Profit/Porsi");

            var colEdit = new DataGridViewImageColumn
            {
                Name = "col_edit",
                HeaderText = "Edit",
                Width = 50,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Image = Properties.Resources.draw_1798785
            };

            var colDel = new DataGridViewImageColumn
            {
                Name = "col_delete",
                HeaderText = "Hapus",
                Width = 50,
                ImageLayout = DataGridViewImageCellLayout.Zoom,
                Image = Properties.Resources.trash_9787142
            };

            dgvResepMenu.Columns.Add(colEdit);
            dgvResepMenu.Columns.Add(colDel);
        }

        private void LoadRecipes(string search = "")
        {
            dgvResepMenu.Rows.Clear();
            try
            {
                using (var conn = db.GetKoneksi())
                {
                    string sql = @"
                        SELECT id, name, serving_size, hpp, suggested_price 
                        FROM recipes 
                        WHERE user_id = @uid AND name ILIKE @q 
                        ORDER BY created_at DESC";

                    using (var cmd = new NpgsqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", currentUserId);
                        cmd.Parameters.AddWithValue("@q", "%" + search + "%");

                        using (var r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                int idx = dgvResepMenu.Rows.Add();
                                dgvResepMenu.Rows[idx].Tag = r["id"];
                                dgvResepMenu.Rows[idx].Cells["col_name"].Value = r["name"];

                                int srv = r.GetInt32(2);
                                double hppTotal = r.GetDouble(3); // HPP Total
                                double hppPerPorsi = srv > 0 ? hppTotal / srv : 0;
                                double price = r.GetDouble(4);
                                double profit = price - hppPerPorsi;

                                dgvResepMenu.Rows[idx].Cells["col_serving"].Value = srv;
                                dgvResepMenu.Rows[idx].Cells["col_hpp"].Value = $"Rp {hppTotal:#,##0}";
                                dgvResepMenu.Rows[idx].Cells["col_hpp_per_porsi"].Value = $"Rp {hppPerPorsi:#,##0}";
                                dgvResepMenu.Rows[idx].Cells["col_price"].Value = $"Rp {price:#,##0}";
                                dgvResepMenu.Rows[idx].Cells["col_profit"].Value = $"Rp {profit:#,##0}";
                            }
                        }
                    }
                }
                dgvResepMenu.ClearSelection();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading recipes:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- 9. UTILITIES ---
        private void BtnTambahResep_Click(object sender, EventArgs e)
        {
            editingRecipeId = null;
            txtInputNamaResep.Clear();
            txtInputDeskripsi.Clear();
            txtInputJumlahPorsi.Text = "1";
            txtInputHargaJualTarget.Clear();

            flowPanelBahan.Controls.Clear();
            AddBahanRow();

            lblResep.Text = "Tambah Resep Baru";
            gbResepPopUp.Visible = true;
            gbResepPopUp.BringToFront();
            RecalculateHpp();
        }

        private void DgvResepMenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            int id = Convert.ToInt32(dgvResepMenu.Rows[e.RowIndex].Tag);

            if (dgvResepMenu.Columns[e.ColumnIndex].Name == "col_edit")
            {
                LoadRecipeIntoPopup(id);
            }
            else if (dgvResepMenu.Columns[e.ColumnIndex].Name == "col_delete")
            {
                if (MessageBox.Show("Hapus resep ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        using (var conn = db.GetKoneksi())
                        {
                            // Hapus detail dulu - TABEL YANG BENAR: recipe_items
                            using (var cmd = new NpgsqlCommand("DELETE FROM recipe_items WHERE recipe_id=@id", conn))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.ExecuteNonQuery();
                            }

                            // Hapus header
                            using (var cmd = new NpgsqlCommand("DELETE FROM recipes WHERE id=@id AND user_id=@uid", conn))
                            {
                                cmd.Parameters.AddWithValue("@id", id);
                                cmd.Parameters.AddWithValue("@uid", currentUserId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        MessageBox.Show("✅ Resep berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadRecipes();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error menghapus resep:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DgvResepMenu_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                LoadRecipeIntoPopup(Convert.ToInt32(dgvResepMenu.Rows[e.RowIndex].Tag));
            }
        }

        private void BtnBatalPopUpResep_Click(object sender, EventArgs e) => gbResepPopUp.Visible = false;
        private void TxtCariResep_TextChanged(object sender, EventArgs e) => LoadRecipes(txtCariResep.Text);
        private void dgvResepMenu_CellContentClick_1(object sender, DataGridViewCellEventArgs e) { }
    }
}