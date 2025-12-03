namespace FnB_Records
{
    partial class Vendor
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
            label1 = new Label();
            guna2GroupBox1 = new Guna.UI2.WinForms.Guna2GroupBox();
            label2 = new Label();
            panel1 = new Panel();
            guna2GroupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Manrope ExtraBold", 26.2499962F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.ForeColor = Color.Black;
            label1.Location = new Point(0, 0);
            label1.Name = "label1";
            label1.Size = new Size(253, 48);
            label1.TabIndex = 0;
            label1.Text = "Kelola Vendor";
            // 
            // guna2GroupBox1
            // 
            guna2GroupBox1.BorderColor = Color.Transparent;
            guna2GroupBox1.Controls.Add(label2);
            guna2GroupBox1.Controls.Add(label1);
            guna2GroupBox1.CustomBorderColor = Color.Transparent;
            guna2GroupBox1.CustomizableEdges = customizableEdges1;
            guna2GroupBox1.FillColor = Color.Transparent;
            guna2GroupBox1.Font = new Font("Manrope", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            guna2GroupBox1.ForeColor = Color.Transparent;
            guna2GroupBox1.Location = new Point(12, 12);
            guna2GroupBox1.Name = "guna2GroupBox1";
            guna2GroupBox1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2GroupBox1.Size = new Size(1186, 99);
            guna2GroupBox1.TabIndex = 1;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Inter", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.FromArgb(45, 45, 45);
            label2.Location = new Point(3, 48);
            label2.Name = "label2";
            label2.Size = new Size(320, 22);
            label2.TabIndex = 2;
            label2.Text = "Kelola daftar vendor dan suplier bahan baku";
            // 
            // panel1
            // 
            panel1.Location = new Point(15, 149);
            panel1.Name = "panel1";
            panel1.Size = new Size(200, 100);
            panel1.TabIndex = 2;
            // 
            // Vendor
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1210, 710);
            Controls.Add(panel1);
            Controls.Add(guna2GroupBox1);
            Font = new Font("Manrope", 8.999999F, FontStyle.Regular, GraphicsUnit.Point, 0);
            FormBorderStyle = FormBorderStyle.None;
            Name = "Vendor";
            Text = " ";
            guna2GroupBox1.ResumeLayout(false);
            guna2GroupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label label1;
        private Guna.UI2.WinForms.Guna2GroupBox guna2GroupBox1;
        private Label label2;
        private Panel panel1;
    }
}