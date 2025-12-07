namespace FnB_Records
{
    partial class UC_Panel_MenuTerlaris
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
            lblNamaMenuTerlaris = new Label();
            lblJumlahMenuTerjual = new Label();
            SuspendLayout();
            // 
            // lblNamaMenuTerlaris
            // 
            lblNamaMenuTerlaris.AutoSize = true;
            lblNamaMenuTerlaris.Font = new Font("Manrope ExtraBold", 23.9999981F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblNamaMenuTerlaris.ForeColor = Color.Black;
            lblNamaMenuTerlaris.Location = new Point(19, 23);
            lblNamaMenuTerlaris.Name = "lblNamaMenuTerlaris";
            lblNamaMenuTerlaris.Size = new Size(321, 44);
            lblNamaMenuTerlaris.TabIndex = 1;
            lblNamaMenuTerlaris.Text = "Nama Menu terlaris";
            // 
            // lblJumlahMenuTerjual
            // 
            lblJumlahMenuTerjual.AutoSize = true;
            lblJumlahMenuTerjual.Font = new Font("Inter", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            lblJumlahMenuTerjual.ForeColor = Color.FromArgb(45, 45, 45);
            lblJumlahMenuTerjual.Location = new Point(18, 84);
            lblJumlahMenuTerjual.Name = "lblJumlahMenuTerjual";
            lblJumlahMenuTerjual.Size = new Size(135, 27);
            lblJumlahMenuTerjual.TabIndex = 3;
            lblJumlahMenuTerjual.Text = "0 porsi terjual";
            // 
            // UC_Panel_MenuTerlaris
            // 
            AutoScaleDimensions = new SizeF(8F, 18F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(249, 247, 245);
            Controls.Add(lblJumlahMenuTerjual);
            Controls.Add(lblNamaMenuTerlaris);
            Font = new Font("Manrope", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Margin = new Padding(3, 4, 3, 4);
            Name = "UC_Panel_MenuTerlaris";
            Size = new Size(710, 153);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblNamaMenuTerlaris;
        private Label lblJumlahMenuTerjual;
    }
}
