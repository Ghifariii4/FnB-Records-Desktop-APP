using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FnB_Records
{
    public partial class UC_Analisis : UserControl
    {
        public UC_Analisis()
        {
            InitializeComponent();
        }


        private async Task<string> AskGemini(string prompt)
        {
            string apiKey = "AIzaSyA2olYfVMSlneRcFsz9Chff3Pyq7FN5QV4";

            string url =
                $"https://generativelanguage.googleapis.com/v1/models/gemini-1.5-flash:generateContent?key={apiKey}";

            var payload = new
            {
                contents = new[]
                {
            new {
                role = "user",
                parts = new[] {
                    new { text = prompt }
                }
            }
        }
            };

            string jsonData = JsonConvert.SerializeObject(payload);

            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, content);
                string result = await response.Content.ReadAsStringAsync();

                dynamic json = JsonConvert.DeserializeObject(result);

                try
                {
                    if (json.error != null)
                        return "Error: " + json.error.message;

                    return json.candidates[0].content.parts[0].text.ToString();
                }
                catch
                {
                    return "API Response:\n" + result;
                }
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtisipromt_TextChanged(object sender, EventArgs e)
        {

        }
        private async void btnsend_Click(object sender, EventArgs e)
        {
            string prompt = txtisipromt.Text.Trim();

            if (string.IsNullOrEmpty(prompt))
            {
                MessageBox.Show("Isi prompt dulu!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Tampilkan prompt user
            richTextBox1.AppendText("Anda: " + prompt + "\n\n");

            // Clear textbox prompt
            txtisipromt.Clear();
            txtisipromt.Focus();

            // Placeholder loading
            richTextBox1.AppendText("AI Sedang menjawab...\n\n");

            // Panggil AI
            string response = await AskGemini(prompt);

            // Hapus placeholder
            richTextBox1.Text = richTextBox1.Text.Replace("AI Sedang menjawab...\n\n", "");

            // Tampilkan jawaban AI
            richTextBox1.AppendText("AI: " + response + "\n\n");
        }

    }
}

