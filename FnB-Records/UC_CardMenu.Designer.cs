namespace FnB_Records
{
    partial class UC_CardMenu
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // [TAMBAHAN 1] Variabel Elipse
        private Guna.UI2.WinForms.Guna2Elipse elipseCard;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            elipseCard = new Guna.UI2.WinForms.Guna2Elipse(components);
            lblNama = new Label();
            lblHarga = new Label();
            picProduk = new Guna.UI2.WinForms.Guna2PictureBox();
            btTambah = new Guna.UI2.WinForms.Guna2CircleButton();
            lblStok = new Label();
            ((System.ComponentModel.ISupportInitialize)picProduk).BeginInit();
            SuspendLayout();
            // 
            // elipseCard
            // 
            elipseCard.BorderRadius = 25;
            elipseCard.TargetControl = this;
            // 
            // lblNama
            // 
            lblNama.AutoSize = true;
            lblNama.Font = new Font("Manrope", 17.9999981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblNama.ForeColor = Color.Black;
            lblNama.Location = new Point(24, 299);
            lblNama.Name = "lblNama";
            lblNama.Size = new Size(152, 33);
            lblNama.TabIndex = 2;
            lblNama.Text = "Nama Menu";
            // 
            // lblHarga
            // 
            lblHarga.AutoSize = true;
            lblHarga.Font = new Font("Manrope", 14.2499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblHarga.ForeColor = Color.FromArgb(212, 122, 71);
            lblHarga.Location = new Point(24, 332);
            lblHarga.Name = "lblHarga";
            lblHarga.Size = new Size(125, 26);
            lblHarga.TabIndex = 3;
            lblHarga.Text = "Rp 000.000";
            // 
            // picProduk
            // 
            picProduk.BackColor = Color.Transparent;
            picProduk.BorderRadius = 15;
            picProduk.CustomizableEdges = customizableEdges5;
            picProduk.ImageRotate = 0F;
            picProduk.Location = new Point(24, 29);
            picProduk.Name = "picProduk";
            picProduk.ShadowDecoration.BorderRadius = 15;
            picProduk.ShadowDecoration.Color = Color.FromArgb(212, 122, 71);
            picProduk.ShadowDecoration.CustomizableEdges = customizableEdges6;
            picProduk.ShadowDecoration.Enabled = true;
            picProduk.Size = new Size(250, 250);
            picProduk.SizeMode = PictureBoxSizeMode.Zoom;
            picProduk.TabIndex = 4;
            picProduk.TabStop = false;
            // 
            // btTambah
            // 
            btTambah.CheckedState.FillColor = Color.FromArgb(45, 45, 45);
            btTambah.CheckedState.Image = Properties.Resources.Plus_Icon_Black;
            btTambah.Cursor = Cursors.Hand;
            btTambah.DisabledState.BorderColor = Color.DarkGray;
            btTambah.DisabledState.CustomBorderColor = Color.DarkGray;
            btTambah.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btTambah.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btTambah.FillColor = Color.FromArgb(212, 122, 71);
            btTambah.Font = new Font("Segoe UI", 9F);
            btTambah.ForeColor = Color.White;
            btTambah.Image = Properties.Resources.Plus_icon_White;
            btTambah.ImageOffset = new Point(1, 0);
            btTambah.ImageSize = new Size(25, 25);
            btTambah.Location = new Point(232, 357);
            btTambah.Name = "btTambah";
            btTambah.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btTambah.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            btTambah.Size = new Size(42, 40);
            btTambah.TabIndex = 5;
            btTambah.Click += btTambah_Click;
            // 
            // lblStok
            // 
            lblStok.AutoSize = true;
            lblStok.Font = new Font("Manrope", 14.2499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblStok.ForeColor = Color.Black;
            lblStok.Location = new Point(24, 371);
            lblStok.Name = "lblStok";
            lblStok.Size = new Size(78, 26);
            lblStok.TabIndex = 6;
            lblStok.Text = "Stok: 0";
            // 
            // UC_CardMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(227, 201, 189);
            Controls.Add(lblStok);
            Controls.Add(btTambah);
            Controls.Add(picProduk);
            Controls.Add(lblHarga);
            Controls.Add(lblNama);
            Name = "UC_CardMenu";
            Size = new Size(306, 421);
            ((System.ComponentModel.ISupportInitialize)picProduk).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblNama;
        private System.Windows.Forms.Label lblHarga;
        private Guna.UI2.WinForms.Guna2PictureBox picProduk;
        private Guna.UI2.WinForms.Guna2CircleButton btTambah;
        private Label lblStok;
    }
}