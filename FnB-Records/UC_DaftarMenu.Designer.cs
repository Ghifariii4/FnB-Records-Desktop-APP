namespace FnB_Records
{
    partial class UC_DaftarMenu
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
            lblDaftarMenuRestoran = new Label();
            flpDaftarMenu = new FlowLayoutPanel();
            guna2Panel1 = new Guna.UI2.WinForms.Guna2Panel();
            guna2vScrollBar1 = new Guna.UI2.WinForms.Guna2VScrollBar();
            guna2Panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lblDaftarMenuRestoran
            // 
            lblDaftarMenuRestoran.AutoSize = true;
            lblDaftarMenuRestoran.Dock = DockStyle.Top;
            lblDaftarMenuRestoran.Font = new Font("Manrope ExtraBold", 47.9999962F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblDaftarMenuRestoran.ForeColor = Color.FromArgb(45, 45, 45);
            lblDaftarMenuRestoran.Location = new Point(0, 0);
            lblDaftarMenuRestoran.Margin = new Padding(3, 0, 3, 30);
            lblDaftarMenuRestoran.Name = "lblDaftarMenuRestoran";
            lblDaftarMenuRestoran.Size = new Size(423, 87);
            lblDaftarMenuRestoran.TabIndex = 2;
            lblDaftarMenuRestoran.Text = "Daftar Menu";
            // 
            // flpDaftarMenu
            // 
            flpDaftarMenu.AutoScroll = true;
            flpDaftarMenu.BackColor = Color.Transparent;
            flpDaftarMenu.Dock = DockStyle.Fill;
            flpDaftarMenu.FlowDirection = FlowDirection.TopDown;
            flpDaftarMenu.Location = new Point(5, 5);
            flpDaftarMenu.Margin = new Padding(0);
            flpDaftarMenu.Name = "flpDaftarMenu";
            flpDaftarMenu.Padding = new Padding(10);
            flpDaftarMenu.Size = new Size(1262, 917);
            flpDaftarMenu.TabIndex = 3;
            flpDaftarMenu.WrapContents = false;
            // 
            // guna2Panel1
            // 
            guna2Panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            guna2Panel1.BackColor = Color.Transparent;
            guna2Panel1.BorderRadius = 15;
            guna2Panel1.Controls.Add(guna2vScrollBar1);
            guna2Panel1.Controls.Add(flpDaftarMenu);
            guna2Panel1.CustomizableEdges = customizableEdges1;
            guna2Panel1.FillColor = Color.FromArgb(212, 122, 71);
            guna2Panel1.Location = new Point(20, 120);
            guna2Panel1.Name = "guna2Panel1";
            guna2Panel1.Padding = new Padding(5);
            guna2Panel1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Panel1.Size = new Size(1272, 927);
            guna2Panel1.TabIndex = 4;
            // 
            // guna2vScrollBar1
            // 
            guna2vScrollBar1.BindingContainer = flpDaftarMenu;
            guna2vScrollBar1.BorderRadius = 8;
            guna2vScrollBar1.Dock = DockStyle.Right;
            guna2vScrollBar1.FillColor = Color.Transparent;
            guna2vScrollBar1.InUpdate = false;
            guna2vScrollBar1.LargeChange = 10;
            guna2vScrollBar1.Location = new Point(1249, 5);
            guna2vScrollBar1.Name = "guna2vScrollBar1";
            guna2vScrollBar1.ScrollbarSize = 18;
            guna2vScrollBar1.Size = new Size(18, 917);
            guna2vScrollBar1.TabIndex = 5;
            guna2vScrollBar1.ThumbColor = Color.FromArgb(45, 45, 45);
            guna2vScrollBar1.Scroll += guna2vScrollBar1_Scroll;
            // 
            // UC_DaftarMenu
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            Controls.Add(guna2Panel1);
            Controls.Add(lblDaftarMenuRestoran);
            Name = "UC_DaftarMenu";
            Padding = new Padding(0, 0, 295, 0);
            Size = new Size(1604, 1067);
            Load += UC_DaftarMenu_Load;
            guna2Panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblDaftarMenuRestoran;
        private FlowLayoutPanel flpDaftarMenu;
        private Guna.UI2.WinForms.Guna2Panel guna2Panel1;
        private Guna.UI2.WinForms.Guna2VScrollBar guna2vScrollBar1;
    }
}
