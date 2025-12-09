namespace FnB_Records
{
    partial class UC_CartItem
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            picThumb = new Guna.UI2.WinForms.Guna2PictureBox();
            lblCartNama = new Label();
            lblCartHarga = new Label();
            lblQty = new Label();
            btnHapus = new Guna.UI2.WinForms.Guna2CircleButton();
            ((System.ComponentModel.ISupportInitialize)picThumb).BeginInit();
            SuspendLayout();
            // 
            // picThumb
            // 
            picThumb.BackColor = Color.Transparent;
            picThumb.BorderRadius = 15;
            picThumb.CustomizableEdges = customizableEdges1;
            picThumb.ImageRotate = 0F;
            picThumb.Location = new Point(15, 12);
            picThumb.Name = "picThumb";
            picThumb.ShadowDecoration.BorderRadius = 15;
            picThumb.ShadowDecoration.Color = Color.FromArgb(212, 122, 71);
            picThumb.ShadowDecoration.CustomizableEdges = customizableEdges2;
            picThumb.ShadowDecoration.Enabled = true;
            picThumb.Size = new Size(50, 50);
            picThumb.SizeMode = PictureBoxSizeMode.Zoom;
            picThumb.TabIndex = 5;
            picThumb.TabStop = false;
            // 
            // lblCartNama
            // 
            lblCartNama.AutoSize = true;
            lblCartNama.Font = new Font("Manrope", 11.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblCartNama.ForeColor = Color.Black;
            lblCartNama.Location = new Point(80, 12);
            lblCartNama.Name = "lblCartNama";
            lblCartNama.Size = new Size(100, 22);
            lblCartNama.TabIndex = 6;
            lblCartNama.Text = "Nama Menu";
            // 
            // lblCartHarga
            // 
            lblCartHarga.AutoSize = true;
            lblCartHarga.Font = new Font("Manrope", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblCartHarga.ForeColor = Color.FromArgb(212, 122, 71);
            lblCartHarga.Location = new Point(80, 34);
            lblCartHarga.Name = "lblCartHarga";
            lblCartHarga.Size = new Size(86, 18);
            lblCartHarga.TabIndex = 7;
            lblCartHarga.Text = "Rp 000.000";
            // 
            // lblQty
            // 
            lblQty.AutoSize = true;
            lblQty.Font = new Font("Manrope", 11.999999F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblQty.ForeColor = Color.Black;
            lblQty.Location = new Point(48, 12);
            lblQty.Name = "lblQty";
            lblQty.Size = new Size(26, 22);
            lblQty.TabIndex = 8;
            lblQty.Text = "1x";
            // 
            // btnHapus
            // 
            btnHapus.CheckedState.FillColor = Color.FromArgb(45, 45, 45);
            btnHapus.CheckedState.Image = Properties.Resources._87;
            btnHapus.Cursor = Cursors.Hand;
            btnHapus.DisabledState.BorderColor = Color.DarkGray;
            btnHapus.DisabledState.CustomBorderColor = Color.DarkGray;
            btnHapus.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnHapus.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnHapus.FillColor = Color.FromArgb(211, 47, 47);
            btnHapus.Font = new Font("Segoe UI", 9F);
            btnHapus.ForeColor = Color.White;
            btnHapus.Image = Properties.Resources._88;
            btnHapus.ImageOffset = new Point(1, 0);
            btnHapus.ImageSize = new Size(25, 25);
            btnHapus.Location = new Point(225, 18);
            btnHapus.Name = "btnHapus";
            btnHapus.ShadowDecoration.CustomizableEdges = customizableEdges3;
            btnHapus.ShadowDecoration.Mode = Guna.UI2.WinForms.Enums.ShadowMode.Circle;
            btnHapus.Size = new Size(42, 40);
            btnHapus.TabIndex = 9;
            btnHapus.Click += btnHapus_Click;
            // 
            // UC_CardItem
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(btnHapus);
            Controls.Add(lblQty);
            Controls.Add(lblCartHarga);
            Controls.Add(lblCartNama);
            Controls.Add(picThumb);
            Name = "UC_CardItem";
            Size = new Size(280, 81);
            ((System.ComponentModel.ISupportInitialize)picThumb).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2PictureBox picThumb;
        private Label lblCartNama;
        private Label lblCartHarga;
        private Label lblQty;
        private Guna.UI2.WinForms.Guna2CircleButton btnHapus;
    }
}
