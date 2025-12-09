namespace FnB_Records
{
    partial class Form_Verifikasi
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges3 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges4 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges5 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges6 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            txtPassword = new Guna.UI2.WinForms.Guna2TextBox();
            lbl01 = new Label();
            btnKonfirmasi = new Guna.UI2.WinForms.Guna2Button();
            btnBatal = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // txtPassword
            // 
            txtPassword.BorderColor = Color.Gainsboro;
            txtPassword.BorderRadius = 5;
            txtPassword.Cursor = Cursors.IBeam;
            txtPassword.CustomizableEdges = customizableEdges1;
            txtPassword.DefaultText = "";
            txtPassword.DisabledState.BorderColor = Color.FromArgb(208, 208, 208);
            txtPassword.DisabledState.FillColor = Color.FromArgb(226, 226, 226);
            txtPassword.DisabledState.ForeColor = Color.FromArgb(138, 138, 138);
            txtPassword.DisabledState.PlaceholderForeColor = Color.FromArgb(138, 138, 138);
            txtPassword.FocusedState.BorderColor = Color.FromArgb(94, 148, 255);
            txtPassword.Font = new Font("Inter SemiBold", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            txtPassword.ForeColor = Color.FromArgb(45, 45, 45);
            txtPassword.HoverState.BorderColor = Color.FromArgb(94, 148, 255);
            txtPassword.Location = new Point(67, 158);
            txtPassword.Margin = new Padding(9);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.PlaceholderForeColor = Color.FromArgb(45, 45, 45);
            txtPassword.PlaceholderText = "";
            txtPassword.SelectedText = "";
            txtPassword.ShadowDecoration.CustomizableEdges = customizableEdges2;
            txtPassword.ShadowDecoration.Depth = 0;
            txtPassword.Size = new Size(550, 51);
            txtPassword.TabIndex = 46;
            // 
            // lbl01
            // 
            lbl01.AutoSize = true;
            lbl01.Font = new Font("Manrope ExtraBold", 20.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbl01.ForeColor = Color.White;
            lbl01.Location = new Point(210, 90);
            lbl01.Name = "lbl01";
            lbl01.Size = new Size(270, 37);
            lbl01.TabIndex = 45;
            lbl01.Text = "Masukan Password";
            // 
            // btnKonfirmasi
            // 
            btnKonfirmasi.BorderRadius = 5;
            btnKonfirmasi.Cursor = Cursors.Hand;
            btnKonfirmasi.CustomizableEdges = customizableEdges3;
            btnKonfirmasi.DisabledState.BorderColor = Color.DarkGray;
            btnKonfirmasi.DisabledState.CustomBorderColor = Color.DarkGray;
            btnKonfirmasi.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnKonfirmasi.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnKonfirmasi.FillColor = Color.FromArgb(46, 125, 50);
            btnKonfirmasi.Font = new Font("Manrope ExtraBold", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnKonfirmasi.ForeColor = Color.White;
            btnKonfirmasi.Location = new Point(388, 258);
            btnKonfirmasi.Name = "btnKonfirmasi";
            btnKonfirmasi.ShadowDecoration.CustomizableEdges = customizableEdges4;
            btnKonfirmasi.Size = new Size(229, 51);
            btnKonfirmasi.TabIndex = 58;
            btnKonfirmasi.Text = "Konfirmasi";
            btnKonfirmasi.Click += btnKonfirmasi_Click;
            // 
            // btnBatal
            // 
            btnBatal.BorderRadius = 5;
            btnBatal.Cursor = Cursors.Hand;
            btnBatal.CustomizableEdges = customizableEdges5;
            btnBatal.DisabledState.BorderColor = Color.DarkGray;
            btnBatal.DisabledState.CustomBorderColor = Color.DarkGray;
            btnBatal.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            btnBatal.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            btnBatal.FillColor = Color.FromArgb(211, 47, 47);
            btnBatal.Font = new Font("Manrope ExtraBold", 15.7499981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnBatal.ForeColor = Color.White;
            btnBatal.Location = new Point(67, 258);
            btnBatal.Name = "btnBatal";
            btnBatal.ShadowDecoration.CustomizableEdges = customizableEdges6;
            btnBatal.Size = new Size(229, 51);
            btnBatal.TabIndex = 57;
            btnBatal.Text = "Batal";
            btnBatal.Click += btnBatal_Click;
            // 
            // Form_Verifikasi
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(212, 122, 71);
            ClientSize = new Size(687, 371);
            Controls.Add(btnKonfirmasi);
            Controls.Add(btnBatal);
            Controls.Add(txtPassword);
            Controls.Add(lbl01);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Form_Verifikasi";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Form_Verifikasi";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Guna.UI2.WinForms.Guna2TextBox txtPassword;
        private Label lbl01;
        private Guna.UI2.WinForms.Guna2Button btnKonfirmasi;
        private Guna.UI2.WinForms.Guna2Button btnBatal;
    }
}