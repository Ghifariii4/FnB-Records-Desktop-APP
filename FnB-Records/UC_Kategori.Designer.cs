namespace FnB_Records
{
    partial class UC_Kategori
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
            guna2GroupBox8 = new Guna.UI2.WinForms.Guna2GroupBox();
            lblKategori = new Label();
            flpCardMenu = new FlowLayoutPanel();
            guna2vScrollBar1 = new Guna.UI2.WinForms.Guna2VScrollBar();
            guna2GroupBox8.SuspendLayout();
            SuspendLayout();
            // 
            // guna2GroupBox8
            // 
            guna2GroupBox8.BackColor = Color.Transparent;
            guna2GroupBox8.BorderColor = Color.FromArgb(45, 45, 45);
            guna2GroupBox8.BorderRadius = 15;
            guna2GroupBox8.BorderThickness = 0;
            guna2GroupBox8.Controls.Add(lblKategori);
            guna2GroupBox8.CustomBorderColor = Color.FromArgb(45, 45, 45);
            guna2GroupBox8.CustomBorderThickness = new Padding(0, 0, 0, 3);
            customizableEdges1.BottomLeft = false;
            customizableEdges1.BottomRight = false;
            guna2GroupBox8.CustomizableEdges = customizableEdges1;
            guna2GroupBox8.Dock = DockStyle.Top;
            guna2GroupBox8.FillColor = Color.FromArgb(212, 122, 71);
            guna2GroupBox8.Font = new Font("Segoe UI", 9F);
            guna2GroupBox8.ForeColor = Color.FromArgb(125, 137, 149);
            guna2GroupBox8.Location = new Point(0, 0);
            guna2GroupBox8.Name = "guna2GroupBox8";
            guna2GroupBox8.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2GroupBox8.Size = new Size(1262, 86);
            guna2GroupBox8.TabIndex = 2;
            // 
            // lblKategori
            // 
            lblKategori.AutoSize = true;
            lblKategori.Font = new Font("Manrope ExtraBold", 23.9999981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblKategori.ForeColor = Color.White;
            lblKategori.Location = new Point(16, 18);
            lblKategori.Margin = new Padding(3, 0, 3, 30);
            lblKategori.Name = "lblKategori";
            lblKategori.Size = new Size(150, 44);
            lblKategori.TabIndex = 2;
            lblKategori.Text = "kategori";
            // 
            // flpCardMenu
            // 
            flpCardMenu.AutoScroll = true;
            flpCardMenu.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpCardMenu.BackColor = Color.Transparent;
            flpCardMenu.Dock = DockStyle.Fill;
            flpCardMenu.Location = new Point(0, 86);
            flpCardMenu.Margin = new Padding(0);
            flpCardMenu.Name = "flpCardMenu";
            flpCardMenu.Padding = new Padding(3);
            flpCardMenu.Size = new Size(1262, 814);
            flpCardMenu.TabIndex = 3;
            // 
            // guna2vScrollBar1
            // 
            guna2vScrollBar1.BackColor = Color.Transparent;
            guna2vScrollBar1.BindingContainer = flpCardMenu;
            guna2vScrollBar1.BorderRadius = 8;
            guna2vScrollBar1.Dock = DockStyle.Right;
            guna2vScrollBar1.FillColor = Color.Transparent;
            guna2vScrollBar1.InUpdate = false;
            guna2vScrollBar1.LargeChange = 10;
            guna2vScrollBar1.Location = new Point(1244, 86);
            guna2vScrollBar1.Name = "guna2vScrollBar1";
            guna2vScrollBar1.ScrollbarSize = 18;
            guna2vScrollBar1.Size = new Size(18, 814);
            guna2vScrollBar1.TabIndex = 6;
            guna2vScrollBar1.ThumbColor = Color.FromArgb(212, 122, 71);
            // 
            // UC_Kategori
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            AutoSizeMode = AutoSizeMode.GrowAndShrink;
            BackColor = Color.Transparent;
            Controls.Add(guna2vScrollBar1);
            Controls.Add(flpCardMenu);
            Controls.Add(guna2GroupBox8);
            Margin = new Padding(0, 0, 0, 50);
            Name = "UC_Kategori";
            Size = new Size(1262, 900);
            guna2GroupBox8.ResumeLayout(false);
            guna2GroupBox8.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Guna.UI2.WinForms.Guna2GroupBox guna2GroupBox8;
        private Label lblKategori;
        private FlowLayoutPanel flpCardMenu;
        private Guna.UI2.WinForms.Guna2VScrollBar guna2vScrollBar1;
    }
}
