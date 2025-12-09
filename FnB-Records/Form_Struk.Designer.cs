namespace FnB_Records
{
    partial class Form_Struk
    {
        private System.ComponentModel.IContainer components = null;

        // [TAMBAHAN 1] Variabel Elipse untuk membuat form rounded
        private Guna.UI2.WinForms.Guna2Elipse guna2Elipse1;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            guna2Elipse1 = new Guna.UI2.WinForms.Guna2Elipse(components);
            label1 = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            label6 = new Label();
            label7 = new Label();
            label5 = new Label();
            label21 = new Label();
            label20 = new Label();
            label19 = new Label();
            label17 = new Label();
            lblwaktu = new Label();
            lblnofaktur = new Label();
            label18 = new Label();
            label16 = new Label();
            lblpajak = new Label();
            label14 = new Label();
            lblsubtotal = new Label();
            lbltunai = new Label();
            label9 = new Label();
            label12 = new Label();
            lblkasir = new Label();
            lbluserid = new Label();
            lblstruk = new Label();
            label26 = new Label();
            guna2PictureBox1 = new Guna.UI2.WinForms.Guna2PictureBox();
            label2 = new Label();
            panel1 = new Panel();
            lblUangDiterima = new Label();
            label8 = new Label();
            lblUangkembali = new Label();
            label3 = new Label();
            lbltotal = new Label();
            flowLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)guna2PictureBox1).BeginInit();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // guna2Elipse1
            // 
            guna2Elipse1.BorderRadius = 20;
            guna2Elipse1.TargetControl = this;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(3, 45);
            label1.Name = "label1";
            label1.Size = new Size(37, 15);
            label1.TabIndex = 26;
            label1.Text = "24000";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.AutoSize = true;
            flowLayoutPanel1.Controls.Add(label6);
            flowLayoutPanel1.Controls.Add(label7);
            flowLayoutPanel1.Controls.Add(label5);
            flowLayoutPanel1.Controls.Add(label1);
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Location = new Point(23, 300);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(374, 67);
            flowLayoutPanel1.TabIndex = 70;
            flowLayoutPanel1.WrapContents = false;
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(3, 0);
            label6.Name = "label6";
            label6.Size = new Size(95, 15);
            label6.TabIndex = 24;
            label6.Text = "UHT Milk 250 Ml";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(3, 15);
            label7.Name = "label7";
            label7.Size = new Size(31, 15);
            label7.TabIndex = 25;
            label7.Text = "8000";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(3, 30);
            label5.Name = "label5";
            label5.Size = new Size(18, 15);
            label5.TabIndex = 23;
            label5.Text = "3x";
            // 
            // label21
            // 
            label21.AutoSize = true;
            label21.Font = new Font("Courier New", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label21.Location = new Point(12, 173);
            label21.Name = "label21";
            label21.Size = new Size(350, 60);
            label21.TabIndex = 66;
            label21.Text = "Terima Kasih\r\n\r\nPembelian Anda GRATIS\r\napabila ditagih lebih atau tidak  mendapat struk.";
            label21.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label20
            // 
            label20.AutoSize = true;
            label20.Location = new Point(-1, 131);
            label20.Name = "label20";
            label20.Size = new Size(375, 15);
            label20.TabIndex = 65;
            label20.Text = " _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(0, -6);
            label19.Name = "label19";
            label19.Size = new Size(375, 15);
            label19.TabIndex = 64;
            label19.Text = " _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(17, 269);
            label17.Name = "label17";
            label17.Size = new Size(375, 15);
            label17.TabIndex = 63;
            label17.Text = " _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";
            // 
            // lblwaktu
            // 
            lblwaktu.AutoSize = true;
            lblwaktu.Font = new Font("Courier New", 9F);
            lblwaktu.Location = new Point(140, 254);
            lblwaktu.Name = "lblwaktu";
            lblwaktu.Size = new Size(140, 15);
            lblwaktu.TabIndex = 50;
            lblwaktu.Text = "18 Okt '25 19:30 PM";
            // 
            // lblnofaktur
            // 
            lblnofaktur.AutoSize = true;
            lblnofaktur.Font = new Font("Courier New", 9F);
            lblnofaktur.Location = new Point(151, 231);
            lblnofaktur.Name = "lblnofaktur";
            lblnofaktur.Size = new Size(119, 15);
            lblnofaktur.TabIndex = 48;
            lblnofaktur.Text = "TRJ-20250910-001";
            lblnofaktur.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(18, 206);
            label18.Name = "label18";
            label18.Size = new Size(375, 15);
            label18.TabIndex = 62;
            label18.Text = " _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Font = new Font("Courier New", 12F, FontStyle.Bold);
            label16.Location = new Point(4, 110);
            label16.Name = "label16";
            label16.Size = new Size(58, 18);
            label16.TabIndex = 60;
            label16.Text = "Total";
            // 
            // lblpajak
            // 
            lblpajak.AutoSize = true;
            lblpajak.Font = new Font("Courier New", 9F);
            lblpajak.Location = new Point(276, 47);
            lblpajak.Name = "lblpajak";
            lblpajak.Size = new Size(35, 15);
            lblpajak.TabIndex = 59;
            lblpajak.Text = "IDR0";
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Font = new Font("Courier New", 9F);
            label14.Location = new Point(4, 47);
            label14.Name = "label14";
            label14.Size = new Size(70, 15);
            label14.TabIndex = 58;
            label14.Text = "Pajak 10%";
            // 
            // lblsubtotal
            // 
            lblsubtotal.AutoSize = true;
            lblsubtotal.Font = new Font("Courier New", 9F);
            lblsubtotal.Location = new Point(276, 26);
            lblsubtotal.Name = "lblsubtotal";
            lblsubtotal.Size = new Size(70, 15);
            lblsubtotal.TabIndex = 57;
            lblsubtotal.Text = "IDR34,000";
            // 
            // lbltunai
            // 
            lbltunai.AutoSize = true;
            lbltunai.Font = new Font("Courier New", 9F);
            lbltunai.Location = new Point(298, 466);
            lbltunai.Name = "lbltunai";
            lbltunai.Size = new Size(70, 15);
            lbltunai.TabIndex = 55;
            lbltunai.Text = "IDR50,000";
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Font = new Font("Courier New", 9F);
            label9.Location = new Point(26, 466);
            label9.Name = "label9";
            label9.Size = new Size(35, 15);
            label9.TabIndex = 54;
            label9.Text = "Cash";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Font = new Font("Courier New", 9F);
            label12.Location = new Point(4, 29);
            label12.Name = "label12";
            label12.Size = new Size(63, 15);
            label12.TabIndex = 56;
            label12.Text = "Subtotal";
            // 
            // lblkasir
            // 
            lblkasir.AutoSize = true;
            lblkasir.Font = new Font("Courier New", 9F);
            lblkasir.Location = new Point(87, 190);
            lblkasir.Name = "lblkasir";
            lblkasir.Size = new Size(105, 15);
            lblkasir.TabIndex = 53;
            lblkasir.Text = "Firdaus Ramdan";
            // 
            // lbluserid
            // 
            lbluserid.AutoSize = true;
            lbluserid.Font = new Font("Courier New", 9F);
            lbluserid.Location = new Point(22, 190);
            lbluserid.Name = "lbluserid";
            lbluserid.Size = new Size(49, 15);
            lbluserid.TabIndex = 52;
            lbluserid.Text = "USR004";
            // 
            // lblstruk
            // 
            lblstruk.AutoSize = true;
            lblstruk.Font = new Font("Courier New", 9F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblstruk.Location = new Point(151, 126);
            lblstruk.Name = "lblstruk";
            lblstruk.Size = new Size(112, 16);
            lblstruk.TabIndex = 49;
            lblstruk.Text = "Struk Pembelian";
            // 
            // label26
            // 
            label26.AutoSize = true;
            label26.Location = new Point(0, 146);
            label26.Name = "label26";
            label26.Size = new Size(375, 15);
            label26.TabIndex = 69;
            label26.Text = " _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _";
            // 
            // guna2PictureBox1
            // 
            guna2PictureBox1.BackgroundImage = Properties.Resources.Logo_FnB_Records_removebg_preview;
            guna2PictureBox1.BackgroundImageLayout = ImageLayout.Stretch;
            guna2PictureBox1.CustomizableEdges = customizableEdges1;
            guna2PictureBox1.Image = Properties.Resources.Logo_FnB_Records_removebg_preview;
            guna2PictureBox1.ImageRotate = 0F;
            guna2PictureBox1.Location = new Point(57, 44);
            guna2PictureBox1.Name = "guna2PictureBox1";
            guna2PictureBox1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2PictureBox1.Size = new Size(80, 80);
            guna2PictureBox1.TabIndex = 71;
            guna2PictureBox1.TabStop = false;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("League Spartan", 27.7499962F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.FromArgb(212, 122, 71);
            label2.Location = new Point(127, 54);
            label2.Name = "label2";
            label2.Size = new Size(220, 55);
            label2.TabIndex = 72;
            label2.Text = "FnB Records";
            // 
            // panel1
            // 
            panel1.Controls.Add(lblUangDiterima);
            panel1.Controls.Add(label8);
            panel1.Controls.Add(lblUangkembali);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(label20);
            panel1.Controls.Add(lbltotal);
            panel1.Controls.Add(label12);
            panel1.Controls.Add(label26);
            panel1.Controls.Add(label21);
            panel1.Controls.Add(lblsubtotal);
            panel1.Controls.Add(label19);
            panel1.Controls.Add(label14);
            panel1.Controls.Add(lblpajak);
            panel1.Controls.Add(label16);
            panel1.Location = new Point(23, 391);
            panel1.Name = "panel1";
            panel1.Size = new Size(375, 267);
            panel1.TabIndex = 73;
            // 
            // lblUangDiterima
            // 
            lblUangDiterima.AutoSize = true;
            lblUangDiterima.Font = new Font("Courier New", 9F);
            lblUangDiterima.Location = new Point(276, 66);
            lblUangDiterima.Name = "lblUangDiterima";
            lblUangDiterima.Size = new Size(35, 15);
            lblUangDiterima.TabIndex = 73;
            lblUangDiterima.Text = "IDR0";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Font = new Font("Courier New", 9F);
            label8.Location = new Point(4, 66);
            label8.Name = "label8";
            label8.Size = new Size(98, 15);
            label8.TabIndex = 72;
            label8.Text = "Uang Diterima";
            // 
            // lblUangkembali
            // 
            lblUangkembali.AutoSize = true;
            lblUangkembali.Font = new Font("Courier New", 9F);
            lblUangkembali.Location = new Point(276, 85);
            lblUangkembali.Name = "lblUangkembali";
            lblUangkembali.Size = new Size(35, 15);
            lblUangkembali.TabIndex = 71;
            lblUangkembali.Text = "IDR0";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Courier New", 9F);
            label3.Location = new Point(4, 85);
            label3.Name = "label3";
            label3.Size = new Size(91, 15);
            label3.TabIndex = 70;
            label3.Text = "Uang Kembali";
            // 
            // lbltotal
            // 
            lbltotal.AutoSize = true;
            lbltotal.Font = new Font("Courier New", 12F, FontStyle.Bold);
            lbltotal.Location = new Point(248, 113);
            lbltotal.Name = "lbltotal";
            lbltotal.Size = new Size(98, 18);
            lbltotal.TabIndex = 68;
            lbltotal.Text = "IDR34,000";
            // 
            // Form_Struk
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoSize = true;
            ClientSize = new Size(414, 704);
            Controls.Add(guna2PictureBox1);
            Controls.Add(label2);
            Controls.Add(flowLayoutPanel1);
            Controls.Add(label17);
            Controls.Add(lblwaktu);
            Controls.Add(lblnofaktur);
            Controls.Add(label18);
            Controls.Add(lblkasir);
            Controls.Add(lbluserid);
            Controls.Add(lblstruk);
            Controls.Add(panel1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "Form_Struk";
            Text = "Form_Struk";
            Load += Form_Struk_Load;
            flowLayoutPanel1.ResumeLayout(false);
            flowLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)guna2PictureBox1).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label lblwaktu;
        private System.Windows.Forms.Label lblnofaktur;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label lblpajak;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label lblsubtotal;
        private System.Windows.Forms.Label lbltunai;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label lblkasir;
        private System.Windows.Forms.Label lbluserid;
        private System.Windows.Forms.Label lblstruk;
        private System.Windows.Forms.Label label26;
        private Guna.UI2.WinForms.Guna2PictureBox guna2PictureBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lbltotal;
        private System.Windows.Forms.Label lblUangkembali;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblUangDiterima;
        private System.Windows.Forms.Label label8;
    }
}