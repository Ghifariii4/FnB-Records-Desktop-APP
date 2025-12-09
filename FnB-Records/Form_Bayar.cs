using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace FnB_Records
{
    public partial class Form_Bayar : Form
    {
        // Property untuk mengembalikan hasil ke Mode_POS
        public bool IsPaid { get; private set; } = false;
        public double CashGiven { get; private set; } = 0;
        public double ChangeDue { get; private set; } = 0;
        
        private double _totalTagihan;
        private string _metode;

        // Constructor menerima Total dan Metode
        public Form_Bayar(double total, string metode)
        {
            InitializeComponent();
            _totalTagihan = total;
            _metode = metode;
        }

        private void Form_Bayar_Load(object sender, EventArgs e)
        {
            lblTotalTagihan.Text = "Total: Rp " + _totalTagihan.ToString("N0");
            AturTampilanMetode();
        }

        private void AturTampilanMetode()
        {
            // Reset Tampilan
            pnlCash.Visible = false;
            pnlNonTunai.Visible = false;
            btnKonfirmasi.Enabled = false; // Default mati sebelum lunas

            switch (_metode)
            {
                case "Cash":
                    pnlCash.Visible = true;
                    txtUangDiterima.Focus();
                    lblInstruksi.Text = "Masukkan jumlah uang tunai.";
                    break;

                case "QRIS":
                    pnlNonTunai.Visible = true;

                    // [UPDATE] Panggil gambar yang baru saja Anda masukkan
                    // Pastikan nama 'Qris_Sample' SAMA PERSIS dengan nama file di Resources tadi
                    picQris.Image = Properties.Resources.Qris_Sample;

                    lblInstruksi.Text = "Scan QRIS di atas.\nMenunggu pembayaran...";

                    // Simulasi Otomatis Lunas dalam 2 detik (Opsional)
                    Timer t = new Timer();
                    t.Interval = 7000;
                    t.Tick += (s, ev) => { 
                        lblInstruksi.Text = "Pembayaran QRIS Berhasil!"; 
                        lblInstruksi.ForeColor = Color.Green;
                        btnKonfirmasi.Enabled = true; 
                        t.Stop();
                    };
                    t.Start();
                    break;

                case "Virtual Account":
                    pnlNonTunai.Visible = true;
                    // Generate nomor VA acak
                    string vaNum = "8800" + DateTime.Now.ToString("fff") + "123456";
                    lblInstruksi.Text = $"Nomor VA (BCA):\n{vaNum}\n\nSilakan transfer sesuai nominal.";
                    btnKonfirmasi.Enabled = true; // Anggap user sudah transfer
                    break;

                case "Debit":
                    pnlNonTunai.Visible = true;
                    lblInstruksi.Text = "Gesek kartu pada mesin EDC.\nMasukkan PIN.";
                    btnKonfirmasi.Enabled = true;
                    break;
            }
        }

        // Logika Hitung Kembalian (Khusus Cash)
        private void txtUangDiterima_TextChanged(object sender, EventArgs e)
        {
            if (_metode != "Cash") return;

            // Parsing uang (Hapus Rp/Titik jika ada)
            string clean = txtUangDiterima.Text.Replace(".", "").Replace("Rp", "").Trim();
            
            if (double.TryParse(clean, out double bayar))
            {
                CashGiven = bayar;
                double kembalian = bayar - _totalTagihan;

                if (kembalian >= 0)
                {
                    lblKembalian.Text = "Kembalian: Rp " + kembalian.ToString("N0");
                    lblKembalian.ForeColor = Color.Green;
                    ChangeDue = kembalian;
                    btnKonfirmasi.Enabled = true; // Uang cukup, tombol hidup
                }
                else
                {
                    lblKembalian.Text = "Kurang: Rp " + Math.Abs(kembalian).ToString("N0");
                    lblKembalian.ForeColor = Color.Red;
                    btnKonfirmasi.Enabled = false; // Uang kurang, tombol mati
                }
            }
            else
            {
                btnKonfirmasi.Enabled = false;
            }
        }

        private void btnKonfirmasi_Click(object sender, EventArgs e)
        {
            if (_metode != "Cash")
            {
                // Jika non-tunai, anggap bayar pas
                CashGiven = _totalTagihan;
                ChangeDue = 0;
            }

            IsPaid = true;
            this.DialogResult = DialogResult.OK; // Memberi sinyal OK ke Form POS
            this.Close();
        }

        private void btnBatal_Click(object sender, EventArgs e)
        {
            IsPaid = false;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
