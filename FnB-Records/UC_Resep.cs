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
        // Model untuk ingredient (data sementara - nanti akan diganti dengan UC_BahanBaku)
        private class Ingredient
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
            public string Unit { get; set; }
            public string Display => $"{Name} (Rp {Price:#,##0}/{Unit})";
            public override string ToString() => Display;
        }

        private readonly Koneksi db = new Koneksi();
        private readonly List<Ingredient> bahanList = new List<Ingredient>();
        private const double SUGGESTED_MULTIPLIER = 3.0;
        private int currentUserId = 1;
        private int? editingRecipeId = null;
        private FlowLayoutPanel pnlBahanRows;

        public UC_Resep()
        {
            InitializeComponent();
            AttachEvents();
        }

        private void AttachEvents()
        {
            // Button events
            btnTambahResep.Click += BtnTambahResep_Click;
            btnTambahBahan.Click += BtnTambahBahan_Click;
            btnSimpanPopUpResep.Click += BtnSimpanPopUpResep_Click;
            btnBatalPopUpResep.Click += BtnBatalPopUpResep_Click;
            btnClosePopUpResep.Click += BtnBatalPopUpResep_Click;

            // TextBox events
            txtCariResep.TextChanged += TxtCariResep_TextChanged;
            txtInputJumlahPorsi.TextChanged += AnyInputAffectingHpp_Changed;
            txtInputHargaJualTarget.TextChanged += AnyInputAffectingHpp_Changed;

            // DataGridView events
            dgvResepMenu.CellContentClick += DgvResepMenu_CellContentClick;
            dgvResepMenu.CellDoubleClick += DgvResepMenu_CellDoubleClick;
        }

        private void UC_Resep_Load(object sender, EventArgs e)
        {
            LoadBahanMaster();
            CreatePanelBahanRows();
            EnsureGridColumns();
            LoadRecipes();
            // Jangan tambah row default di sini, biarkan kosong
        }

        #region Load Bahan Master (Data Sementara - nanti diganti UC_BahanBaku)
        private void LoadBahanMaster()
        {
            bahanList.Clear();

            // DATA SEMENTARA - Nanti akan diganti dengan data dari UC_BahanBaku
            // Format: Id, Name, Price, Unit
            bahanList.Add(new Ingredient { Id = 1, Name = "Tepung Terigu", Price = 12000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 2, Name = "Gula Pasir", Price = 14000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 3, Name = "Telur Ayam", Price = 2000, Unit = "pcs" });
            bahanList.Add(new Ingredient { Id = 4, Name = "Mentega", Price = 35000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 5, Name = "Susu Cair", Price = 15000, Unit = "liter" });
            bahanList.Add(new Ingredient { Id = 6, Name = "Garam", Price = 5000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 7, Name = "Baking Powder", Price = 25000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 8, Name = "Vanilla Extract", Price = 45000, Unit = "botol" });
            bahanList.Add(new Ingredient { Id = 9, Name = "Coklat Bubuk", Price = 50000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 10, Name = "Keju Parut", Price = 80000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 11, Name = "Minyak Goreng", Price = 18000, Unit = "liter" });
            bahanList.Add(new Ingredient { Id = 12, Name = "Bawang Merah", Price = 40000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 13, Name = "Bawang Putih", Price = 35000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 14, Name = "Cabai Merah", Price = 45000, Unit = "kg" });
            bahanList.Add(new Ingredient { Id = 15, Name = "Daging Ayam", Price = 35000, Unit = "kg" });
        }
        #endregion

        #region Dynamic Bahan Rows
        private void CreatePanelBahanRows()
        {
            // Hapus panel lama jika ada
            if (pnlBahanRows != null)
            {
                gbResepPopUp.Controls.Remove(pnlBahanRows);
                pnlBahanRows.Dispose();
            }

            // Buat panel baru untuk rows bahan
            pnlBahanRows = new FlowLayoutPanel
            {
                Name = "pnlBahanRows",
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                AutoScroll = true,
                WrapContents = false,
                Location = new Point(17, 500),
                Width = 504,
                MaximumSize = new Size(504, 250),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            gbResepPopUp.Controls.Add(pnlBahanRows);
            pnlBahanRows.BringToFront();
        }

        private void BtnTambahBahan_Click(object sender, EventArgs e)
        {
            AddBahanRow();
        }

        private void AddBahanRow(int? selectedBahanId = null, string qtyText = "")
        {
            if (pnlBahanRows == null) CreatePanelBahanRows();

            var rowPanel = new Panel
            {
                Width = pnlBahanRows.Width - 6,
                Height = 40,
                Margin = new Padding(3, 3, 3, 3)
            };

            // ComboBox Bahan
            var cb = new ComboBox
            {
                Name = "cbPilihBahan",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Left = 0,
                Top = 5,
                Width = 280,
                Font = new Font("Manrope", 11.999999F)
            };

            cb.DataSource = new BindingList<Ingredient>(bahanList.ToList());
            cb.DisplayMember = "Display";
            cb.ValueMember = "Id";

            if (selectedBahanId.HasValue)
            {
                var idx = bahanList.FindIndex(x => x.Id == selectedBahanId.Value);
                if (idx >= 0) cb.SelectedIndex = idx;
            }

            // TextBox Jumlah
            var txtQty = new TextBox
            {
                Name = "txtInputJumlahBahan",
                Left = cb.Right + 8,
                Top = 5,
                Width = 130,
                Text = qtyText ?? "",
                Font = new Font("Manrope", 11.999999F),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Button Hapus
            var btnDel = new Button
            {
                Name = "btnHapusBahan",
                Text = "✕",
                Width = 40,
                Height = 30,
                Left = txtQty.Right + 8,
                Top = 4,
                Font = new Font("Manrope", 11F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(249, 247, 245),
                FlatStyle = FlatStyle.Flat
            };
            btnDel.FlatAppearance.BorderColor = Color.Gainsboro;

            // Events
            cb.SelectedIndexChanged += (s, e) => RecalculateHpp();
            txtQty.TextChanged += (s, e) => RecalculateHpp();
            btnDel.Click += (s, e) =>
            {
                if (pnlBahanRows.Controls.Count <= 1)
                {
                    MessageBox.Show("Minimal harus ada 1 bahan!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                pnlBahanRows.Controls.Remove(rowPanel);
                rowPanel.Dispose();
                UpdateDeleteButtonsState();
                RecalculateHpp();
            };

            rowPanel.Controls.Add(cb);
            rowPanel.Controls.Add(txtQty);
            rowPanel.Controls.Add(btnDel);

            pnlBahanRows.Controls.Add(rowPanel);
            UpdateDeleteButtonsState();
        }

        private void UpdateDeleteButtonsState()
        {
            if (pnlBahanRows == null) return;

            bool disable = pnlBahanRows.Controls.Count <= 1;
            foreach (Panel p in pnlBahanRows.Controls.OfType<Panel>())
            {
                var btn = p.Controls.OfType<Button>().FirstOrDefault(x => x.Name == "btnHapusBahan");
                if (btn != null) btn.Enabled = !disable;
            }
        }
        #endregion

        #region HPP Calculations
        private void AnyInputAffectingHpp_Changed(object sender, EventArgs e)
        {
            RecalculateHpp();
        }

        private void RecalculateHpp()
        {
            double totalCost = 0.0;

            if (pnlBahanRows != null)
            {
                foreach (Panel p in pnlBahanRows.Controls.OfType<Panel>())
                {
                    var cb = p.Controls.OfType<ComboBox>().FirstOrDefault(c => c.Name == "cbPilihBahan");
                    var txt = p.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == "txtInputJumlahBahan");

                    if (cb?.SelectedItem is Ingredient ing && txt != null)
                    {
                        if (double.TryParse(txt.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double qty))
                        {
                            totalCost += ing.Price * qty;
                        }
                    }
                }
            }

            // Jumlah porsi
            int servings = 1;
            if (!string.IsNullOrWhiteSpace(txtInputJumlahPorsi.Text))
            {
                int.TryParse(txtInputJumlahPorsi.Text, out servings);
            }
            if (servings <= 0) servings = 1;

            // HPP per porsi
            double hppPerServing = servings > 0 ? (totalCost / servings) : 0.0;
            lblHPP.Text = $"{hppPerServing:#,##0}";

            // Harga jual disarankan (3x HPP)
            double suggested = hppPerServing * SUGGESTED_MULTIPLIER;
            lblHargaJualDisarankan.Text = $"{suggested:#,##0}";

            // Estimasi profit berdasarkan harga jual target
            double targetPrice = suggested; // Default gunakan suggested

            if (!string.IsNullOrWhiteSpace(txtInputHargaJualTarget.Text))
            {
                if (double.TryParse(txtInputHargaJualTarget.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double t))
                {
                    targetPrice = t;
                }
            }

            double profit = targetPrice - hppPerServing;
            double marginPct = targetPrice != 0 ? (profit / targetPrice * 100.0) : 0.0;

            lblEstimasiProfit.Text = $"Estimasi profit: {profit:#,##0} ({marginPct:0.##}% margin)";
        }
        #endregion

        #region DataGridView Setup & Load
        private void EnsureGridColumns()
        {
            dgvResepMenu.Columns.Clear();
            dgvResepMenu.Rows.Clear();

            dgvResepMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_name",
                HeaderText = "Nama Resep",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dgvResepMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_serving",
                HeaderText = "Porsi",
                Width = 90
            });

            dgvResepMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_hpp",
                HeaderText = "HPP/Porsi",
                Width = 120
            });

            dgvResepMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_suggested",
                HeaderText = "Harga Jual",
                Width = 120
            });

            dgvResepMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_profit",
                HeaderText = "Est. Profit",
                Width = 140
            });

            // Column Edit
            var colEdit = new DataGridViewImageColumn
            {
                Name = "col_edit",
                HeaderText = "Edit",
                Width = 60,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            dgvResepMenu.Columns.Add(colEdit);

            // Column Delete
            var colDelete = new DataGridViewImageColumn
            {
                Name = "col_delete",
                HeaderText = "Hapus",
                Width = 60,
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            dgvResepMenu.Columns.Add(colDelete);

            dgvResepMenu.RowHeadersVisible = false;
            dgvResepMenu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvResepMenu.AllowUserToAddRows = false;
        }

        private void LoadRecipes(string search = "")
        {
            dgvResepMenu.Rows.Clear();

            DataTable dt = new DataTable();
            try
            {
                using (var conn = db.GetKoneksi())
                using (var cmd = new NpgsqlCommand(@"
                    SELECT id, name, serving_size,
                           COALESCE(hpp,0) as hpp_total,
                           CASE WHEN serving_size>0 THEN COALESCE(hpp,0)/serving_size ELSE 0 END AS hpp_per_serving,
                           COALESCE(suggested_price,0) as suggested_price
                    FROM recipes
                    WHERE user_id = @uid AND (name ILIKE @q OR COALESCE(description,'') ILIKE @q)
                    ORDER BY created_at DESC", conn))
                using (var da = new NpgsqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("uid", currentUserId);
                    cmd.Parameters.AddWithValue("q", "%" + search + "%");
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recipes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (DataRow r in dt.Rows)
            {
                int idx = dgvResepMenu.Rows.Add();
                dgvResepMenu.Rows[idx].Tag = r["id"];
                dgvResepMenu.Rows[idx].Cells["col_name"].Value = r["name"]?.ToString();
                dgvResepMenu.Rows[idx].Cells["col_serving"].Value = $"{r["serving_size"]} porsi";

                double hppPer = Convert.ToDouble(r["hpp_per_serving"]);
                dgvResepMenu.Rows[idx].Cells["col_hpp"].Value = $"Rp {hppPer:#,##0}";

                double sug = Convert.ToDouble(r["suggested_price"]);
                dgvResepMenu.Rows[idx].Cells["col_suggested"].Value = $"Rp {sug:#,##0}";

                double profit = sug - hppPer;
                double marginPct = sug == 0 ? 0 : (profit / sug * 100);
                dgvResepMenu.Rows[idx].Cells["col_profit"].Value = $"Rp {profit:#,##0} ({marginPct:0.##}%)";
            }

            dgvResepMenu.ClearSelection();
        }
        #endregion

        #region DataGridView Events
        private void DgvResepMenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            var col = dgvResepMenu.Columns[e.ColumnIndex];
            var row = dgvResepMenu.Rows[e.RowIndex];

            if (row.Tag == null || !int.TryParse(row.Tag.ToString(), out int recipeId)) return;

            if (col.Name == "col_edit")
            {
                LoadRecipeIntoPopup(recipeId);
            }
            else if (col.Name == "col_delete")
            {
                var result = MessageBox.Show("Apakah Anda yakin ingin menghapus resep ini?",
                    "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    DeleteRecipe(recipeId);
                    LoadRecipes(txtCariResep.Text);
                }
            }
        }

        private void DgvResepMenu_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var tag = dgvResepMenu.Rows[e.RowIndex].Tag;
            if (tag != null && int.TryParse(tag.ToString(), out int recipeId))
            {
                LoadRecipeIntoPopup(recipeId);
            }
        }
        #endregion

        #region CRUD Operations
        private void LoadRecipeIntoPopup(int recipeId)
        {
            try
            {
                DataTable dt = new DataTable();
                using (var conn = db.GetKoneksi())
                using (var cmd = new NpgsqlCommand(
                    "SELECT id, name, description, serving_size, hpp, suggested_price FROM recipes WHERE id=@id AND user_id=@uid LIMIT 1", conn))
                using (var da = new NpgsqlDataAdapter(cmd))
                {
                    cmd.Parameters.AddWithValue("id", recipeId);
                    cmd.Parameters.AddWithValue("uid", currentUserId);
                    da.Fill(dt);
                }

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Resep tidak ditemukan.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var r = dt.Rows[0];
                editingRecipeId = recipeId;

                txtInputNamaResep.Text = r["name"]?.ToString() ?? "";
                txtInputDeskripsi.Text = r["description"]?.ToString() ?? "";
                txtInputJumlahPorsi.Text = r["serving_size"]?.ToString() ?? "1";

                double suggestedPrice = r["suggested_price"] != DBNull.Value ? Convert.ToDouble(r["suggested_price"]) : 0;
                txtInputHargaJualTarget.Text = suggestedPrice > 0 ? suggestedPrice.ToString("0.##") : "";

                // Reset bahan ke default 1 row
                // Karena tidak menyimpan detail bahan, saat edit hanya load data umum resep
                pnlBahanRows.Controls.Clear();
                AddBahanRow();

                RecalculateHpp();
                gbResepPopUp.Visible = true;
                lblResep.Text = "Edit Resep";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal memuat resep: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DeleteRecipe(int recipeId)
        {
            try
            {
                using (var conn = db.GetKoneksi())
                using (var cmd = new NpgsqlCommand("DELETE FROM recipes WHERE id = @rid AND user_id = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("rid", recipeId);
                    cmd.Parameters.AddWithValue("uid", currentUserId);
                    cmd.ExecuteNonQuery();
                }

                MessageBox.Show("Resep berhasil dihapus.", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menghapus resep: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnSimpanPopUpResep_Click(object sender, EventArgs e)
        {
            string name = txtInputNamaResep.Text.Trim();
            string desc = txtInputDeskripsi.Text.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Nama resep harus diisi!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtInputNamaResep.Focus();
                return;
            }

            if (!int.TryParse(txtInputJumlahPorsi.Text, out int servings) || servings <= 0)
            {
                MessageBox.Show("Jumlah porsi harus diisi dengan angka yang valid!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtInputJumlahPorsi.Focus();
                return;
            }

            // Hitung HPP dari bahan-bahan yang dipilih (hanya untuk kalkulasi, tidak disimpan)
            double totalCost = 0.0;
            bool hasValidBahan = false;

            if (pnlBahanRows != null)
            {
                foreach (Panel p in pnlBahanRows.Controls.OfType<Panel>())
                {
                    var cb = p.Controls.OfType<ComboBox>().FirstOrDefault(c => c.Name == "cbPilihBahan");
                    var txt = p.Controls.OfType<TextBox>().FirstOrDefault(t => t.Name == "txtInputJumlahBahan");

                    if (cb?.SelectedItem is Ingredient ing && txt != null)
                    {
                        if (double.TryParse(txt.Text.Replace(",", "."),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double qty) && qty > 0)
                        {
                            totalCost += ing.Price * qty;
                            hasValidBahan = true;
                        }
                    }
                }
            }

            if (!hasValidBahan)
            {
                MessageBox.Show("Minimal harus ada 1 bahan dengan jumlah yang valid!", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Calculate HPP
            double hppTotal = totalCost;
            double hppPerServing = servings > 0 ? (hppTotal / servings) : 0;
            double suggested = hppPerServing * SUGGESTED_MULTIPLIER;

            // Jika user mengisi harga jual target, gunakan itu. Jika tidak, gunakan suggested
            double finalSuggestedPrice = suggested;
            if (!string.IsNullOrWhiteSpace(txtInputHargaJualTarget.Text))
            {
                if (double.TryParse(txtInputHargaJualTarget.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double target))
                {
                    finalSuggestedPrice = target;
                }
            }

            try
            {
                using (var conn = db.GetKoneksi())
                {
                    if (editingRecipeId.HasValue)
                    {
                        // Update existing recipe
                        using (var cmd = new NpgsqlCommand(@"
                            UPDATE recipes 
                            SET name=@name, description=@desc, serving_size=@serv, 
                                hpp=@hpp, suggested_price=@suggested, updated_at=NOW() 
                            WHERE id=@id AND user_id=@uid", conn))
                        {
                            cmd.Parameters.AddWithValue("name", name);
                            cmd.Parameters.AddWithValue("desc", string.IsNullOrWhiteSpace(desc) ? (object)DBNull.Value : desc);
                            cmd.Parameters.AddWithValue("serv", servings);
                            cmd.Parameters.AddWithValue("hpp", hppTotal);
                            cmd.Parameters.AddWithValue("suggested", finalSuggestedPrice);
                            cmd.Parameters.AddWithValue("id", editingRecipeId.Value);
                            cmd.Parameters.AddWithValue("uid", currentUserId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        // Insert new recipe
                        using (var cmd = new NpgsqlCommand(@"
                            INSERT INTO recipes(user_id, name, description, serving_size, hpp, suggested_price, created_at, updated_at)
                            VALUES(@uid,@name,@desc,@serv,@hpp,@suggested,NOW(),NOW())", conn))
                        {
                            cmd.Parameters.AddWithValue("uid", currentUserId);
                            cmd.Parameters.AddWithValue("name", name);
                            cmd.Parameters.AddWithValue("desc", string.IsNullOrWhiteSpace(desc) ? (object)DBNull.Value : desc);
                            cmd.Parameters.AddWithValue("serv", servings);
                            cmd.Parameters.AddWithValue("hpp", hppTotal);
                            cmd.Parameters.AddWithValue("suggested", finalSuggestedPrice);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                MessageBox.Show("Resep berhasil disimpan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                editingRecipeId = null;
                gbResepPopUp.Visible = false;
                LoadRecipes(txtCariResep.Text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan resep: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region UI Actions
        private void BtnTambahResep_Click(object sender, EventArgs e)
        {
            editingRecipeId = null;
            txtInputNamaResep.Text = "";
            txtInputDeskripsi.Text = "";
            txtInputJumlahPorsi.Text = "1";
            txtInputHargaJualTarget.Text = "";

            pnlBahanRows.Controls.Clear();
            AddBahanRow();

            RecalculateHpp();
            gbResepPopUp.Visible = true;
            lblResep.Text = "Tambah Resep Baru";
        }

        private void BtnBatalPopUpResep_Click(object sender, EventArgs e)
        {
            gbResepPopUp.Visible = false;
            editingRecipeId = null;
        }

        private void TxtCariResep_TextChanged(object sender, EventArgs e)
        {
            LoadRecipes(txtCariResep.Text);
        }
        #endregion
    }
}