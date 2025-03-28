using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using RestSharp;
using Newtonsoft.Json;

namespace RedcapCSharpApiExamples
{
    public static class MyClass
    {
        // API token and URI for submitting a record
        private static readonly string token = "########";
        private static readonly Uri apiUri = new Uri("#########");

        public static async Task<RestResponse> SubmitRecordAsync(string mappedAsset, string changes)
        {
            try
            {
                // Build a new record object for a repeating instrument.
                // Fields: record_id, redcap_repeat_instrument, redcap_repeat_instance, itassets, changes, date, it_changes_complete
                var record = new
                {
                    record_id = DateTime.Now.Ticks.ToString(),
                    redcap_repeat_instrument = "it_changes", // must match the unique form name in REDCap
                    redcap_repeat_instance = "1",             // set instance to "1" (or increment for subsequent entries)
                    itassets = mappedAsset,                   // use the mapped value ("1", "2", or "3")
                    changes = changes,
                    date = DateTime.Now.ToString("yyyy-MM-dd"),
                    it_changes_complete = "0"
                };

                // REDCap expects an array of record objects (even if it's just one record)
                string jsonData = JsonConvert.SerializeObject(new[] { record });

                // Create the RestSharp client and request.
                var client = new RestClient(apiUri);
                var request = new RestRequest();
                request.Method = Method.Post; // Set HTTP method to POST

                // Add parameters for the API call
                request.AddParameter("token", token);
                request.AddParameter("content", "record");
                request.AddParameter("format", "json");
                request.AddParameter("type", "flat");
                request.AddParameter("data", jsonData);
                request.AddParameter("returnFormat", "json");

                // Execute the API call asynchronously.
                var response = await client.ExecuteAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Submission Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }
    }

    public class MainForm : Form
    {
        private ComboBox cmbItAssets;
        private TextBox txtChanges;
        private Button btnSubmit;
        private TextBox txtResponse;

        public MainForm()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            this.Text = "Submit Changelog";
            this.Width = 600;
            this.Height = 400;

            // Label and ComboBox for IT Asset selection
            Label lblAsset = new Label() { Text = "IT Asset:", Left = 10, Top = 20, Width = 100 };
            cmbItAssets = new ComboBox() { Left = 120, Top = 20, Width = 200 };
            cmbItAssets.Items.AddRange(new string[] { "Chris", "Charles", "Kent" });
            cmbItAssets.SelectedIndex = 0;

            // Label and TextBox for changes description
            Label lblChanges = new Label() { Text = "Changes:", Left = 10, Top = 60, Width = 100 };
            txtChanges = new TextBox() { Left = 120, Top = 60, Width = 400, Height = 100, Multiline = true };

            // Button to submit the new record
            btnSubmit = new Button() { Text = "Submit Record", Left = 120, Top = 180, Width = 150 };
            btnSubmit.Click += async (sender, e) => await BtnSubmit_Click(sender, e);

            // TextBox to display the API response
            txtResponse = new TextBox()
            {
                Left = 10,
                Top = 220,
                Width = 560,
                Height = 100,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both
            };

            // Add controls to the form.
            this.Controls.Add(lblAsset);
            this.Controls.Add(cmbItAssets);
            this.Controls.Add(lblChanges);
            this.Controls.Add(txtChanges);
            this.Controls.Add(btnSubmit);
            this.Controls.Add(txtResponse);
        }

        private async Task BtnSubmit_Click(object sender, EventArgs e)
        {
            btnSubmit.Enabled = false;
            txtResponse.Text = "Submitting record...";
            try
            {
                // Get the selected asset and map it to its corresponding number.
                string selectedAsset = cmbItAssets.SelectedItem.ToString();
                string mappedValue = selectedAsset switch
                {
                    "Chris" => "1",
                    "Charles" => "2",
                    "Kent" => "3",
                    _ => "0"
                };

                string changes = txtChanges.Text.Trim();
                if (string.IsNullOrEmpty(changes))
                {
                    MessageBox.Show("Please enter a description for the changes.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    btnSubmit.Enabled = true;
                    return;
                }

                // Submit the record using the mapped value.
                var response = await MyClass.SubmitRecordAsync(mappedValue, changes);
                if (response.IsSuccessful)
                {
                    txtResponse.Text = "Record submitted successfully:\r\n" + response.Content;
                }
                else
                {
                    txtResponse.Text = "Error submitting record:\r\n" + response.Content;
                }
            }
            catch (Exception ex)
            {
                txtResponse.Text = "Exception: " + ex.Message;
            }
            finally
            {
                btnSubmit.Enabled = true;
            }
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Standard Windows Forms initialization.
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
