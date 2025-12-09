using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections.Generic;

namespace FnB_Records
{
    public partial class Form_Struk : Form
    {
        public Form_Struk()
        {
            InitializeComponent();
        }

        // --- DEFINISI CLASS DATA STRUK ---
        public class ReceiptItem
        {
            public string Name { get; set; }
            public int Qty { get; set; }
            public double Price { get; set; }
            public double Total => Qty * Price;
        }

        public static class CurrentReceipt
        {
            public static string NoFaktur { get; set; }
            public static string KasirName { get; set; }
            public static string UserId { get; set; }
            public static DateTime WaktuTransaksi { get; set; }

            public static List<ReceiptItem> Items { get; set; } = new List<ReceiptItem>();

            public static double Subtotal { get; set; }
            public static double Tax { get; set; }
            public static double Discount { get; set; }
            public static double GrandTotal { get; set; }
            public static double CashPaid { get; set; }
            public static double ChangeDue { get; set; }

            public static void Clear()
            {
                Items.Clear();
                Subtotal = 0; Tax = 0; Discount = 0; GrandTotal = 0;
                CashPaid = 0; ChangeDue = 0;
            }
        }

        private void Form_Struk_Load(object sender, EventArgs e)
        {
            LoadDataTransaksi();
        }

        private void LoadDataTransaksi()
        {
            // 1. SET HEADER
            lblnofaktur.Text = CurrentReceipt.NoFaktur;
            lblwaktu.Text = CurrentReceipt.WaktuTransaksi.ToString("dd MMM yy HH:mm tt");
            lblkasir.Text = CurrentReceipt.KasirName;
            lbluserid.Text = CurrentReceipt.UserId;

            // 2. POPULATE ITEM (Dynamic List)
            int marginSpace = 20;

            flowLayoutPanel1.Controls.Clear();

            foreach (var item in CurrentReceipt.Items)
            {
                Panel pnlItem = new Panel();
                pnlItem.Width = flowLayoutPanel1.Width - 5;
                pnlItem.Height = 45;
                pnlItem.Margin = new Padding(0, 0, 0, 5);

                Label lblNama = new Label();
                lblNama.Text = item.Name;
                lblNama.Font = new Font("Courier New", 9, FontStyle.Bold);
                lblNama.Location = new Point(0, 0);
                lblNama.AutoSize = true;

                Label lblDetail = new Label();
                lblDetail.Text = $"{item.Qty}x {item.Price:N0}";
                lblDetail.Font = new Font("Courier New", 9);
                lblDetail.Location = new Point(0, 20);
                lblDetail.AutoSize = true;

                Label lblTotalItem = new Label();
                lblTotalItem.Text = item.Total.ToString("N0");
                lblTotalItem.Font = new Font("Courier New", 9);
                lblTotalItem.AutoSize = false;
                lblTotalItem.Width = 100;
                lblTotalItem.TextAlign = ContentAlignment.MiddleRight;
                lblTotalItem.Location = new Point(pnlItem.Width - 105, 20);

                pnlItem.Controls.Add(lblNama);
                pnlItem.Controls.Add(lblDetail);
                pnlItem.Controls.Add(lblTotalItem);

                flowLayoutPanel1.Controls.Add(pnlItem);
            }

            // 3. AUTO ADJUST LAYOUT 
            panel1.Top = flowLayoutPanel1.Bottom + marginSpace;
            this.Height = panel1.Bottom + 50;

            // 4. SET TOTALS (Footer)
            lblsubtotal.Text = "IDR " + CurrentReceipt.Subtotal.ToString("N0");
            lblpajak.Text = "IDR " + CurrentReceipt.Tax.ToString("N0");
            lbltotal.Text = "IDR " + CurrentReceipt.GrandTotal.ToString("N0");

            // --- [UPDATE: ISI SEMUA LABEL PEMBAYARAN] ---
            lblUangDiterima.Text = "IDR " + CurrentReceipt.CashPaid.ToString("N0");
            lblUangkembali.Text = "IDR " + CurrentReceipt.ChangeDue.ToString("N0");

            // Note: lbltunai sepertinya redundant (dobel) dengan lblUangDiterima
            // Jika Anda mau tetap pakai lbltunai untuk info lain silakan, 
            // tapi biasanya cukup satu label "Uang Diterima".
            lbltunai.Text = "IDR " + CurrentReceipt.CashPaid.ToString("N0");
        }

        private void label26_Click(object sender, EventArgs e)
        {
            // Kosong
        }
    }
}