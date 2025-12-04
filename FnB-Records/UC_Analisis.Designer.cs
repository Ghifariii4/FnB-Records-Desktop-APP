namespace FnB_Records
{
    partial class UC_Analisis
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
            groupBox1 = new GroupBox();
            pictureBox1 = new PictureBox();
            listView1 = new ListView();
            btnsend = new Button();
            txtisipromt = new TextBox();
            richTextBox1 = new RichTextBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.BackColor = Color.DarkGray;
            groupBox1.Controls.Add(pictureBox1);
            groupBox1.Controls.Add(listView1);
            groupBox1.Controls.Add(btnsend);
            groupBox1.Controls.Add(txtisipromt);
            groupBox1.Controls.Add(richTextBox1);
            groupBox1.Location = new Point(414, 224);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(576, 743);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.White;
            pictureBox1.Location = new Point(441, 13);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(115, 75);
            pictureBox1.TabIndex = 4;
            pictureBox1.TabStop = false;
            // 
            // listView1
            // 
            listView1.Location = new Point(47, 94);
            listView1.Name = "listView1";
            listView1.Size = new Size(490, 87);
            listView1.TabIndex = 3;
            listView1.UseCompatibleStateImageBehavior = false;
            // 
            // btnsend
            // 
            btnsend.Location = new Point(208, 628);
            btnsend.Name = "btnsend";
            btnsend.Size = new Size(170, 51);
            btnsend.TabIndex = 2;
            btnsend.Text = "button1";
            btnsend.UseVisualStyleBackColor = true;
            btnsend.Click += btnsend_Click;
            // 
            // txtisipromt
            // 
            txtisipromt.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtisipromt.Location = new Point(27, 541);
            txtisipromt.Name = "txtisipromt";
            txtisipromt.Size = new Size(529, 37);
            txtisipromt.TabIndex = 1;
            txtisipromt.TextChanged += txtisipromt_TextChanged;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(27, 197);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(529, 316);
            richTextBox1.TabIndex = 0;
            richTextBox1.Text = "";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // UC_Analisis
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(212, 122, 71);
            Controls.Add(groupBox1);
            Name = "UC_Analisis";
            Size = new Size(2309, 1712);
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private GroupBox groupBox1;
        private TextBox txtisipromt;
        private RichTextBox richTextBox1;
        private PictureBox pictureBox1;
        private ListView listView1;
        private Button btnsend;
    }
}
