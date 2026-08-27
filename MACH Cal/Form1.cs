using System;
using System.Windows.Forms;
using System.Globalization;

namespace MACH_Cal
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        // Robust parsing helper: trims, accepts comma or dot decimal separator, uses invariant culture
        private bool TryParseDouble(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text))
                return false;

            string s = text.Trim();
            // try invariant first
            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
                return true;

            // try current culture
            if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.CurrentCulture, out value))
                return true;

            // replace comma with dot and try invariant
            string alt = s.Replace(',', '.');
            if (double.TryParse(alt, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
                return true;

            return false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Parse inputs and update label5 and label7/label9. No message boxes.
            bool ok1 = TryParseDouble(textBox1.Text, out double input1);
            bool ok2 = TryParseDouble(textBox2.Text, out double input2);
            // optional override value from textBox4
            bool ok4 = TryParseDouble(textBox4.Text, out double input4);

            // label5: compute 4*80 / input1 (require input1 != 0)
            if (!ok1 || input1 == 0.0)
            {
                label5.Text = string.Empty;
            }
            else
            {
                double result = 4 * 80 / input1; // 320 / input1
                label5.Text = result.ToString("F3");
            }

            // For label7/label9 we may override input1 with input4 when provided.
            // Determine the base value: prefer textBox4 if it contains a valid number.
            bool haveBase = ok4 || ok1;
            double baseValue = ok4 ? input4 : input1;

            // label7: compute baseValue - input2 (require baseValue and input2 parsed)
            if (!haveBase || !ok2)
            {
                label7.Text = string.Empty;
                label9.Text = string.Empty;
            }
            else
            {
                double diff = baseValue - input2;
                label7.Text = diff.ToString("F3");

                // label9: half of label7 (diff / 2)
                double half = diff / 2.0;
                label9.Text = half.ToString("F3");
            }

            // --- Travel indicator: compute full dial revolutions using textbox3 as travel
            bool ok3 = TryParseDouble(textBox3.Text, out double travelValue);
            // default units per revolution is 1.0; allow override from textBox5 if present and valid
            double unitsPerRevolution = 1.0;
            var tb5found = this.Controls.Find("textBox5", true);
            if (tb5found.Length > 0 && tb5found[0] is TextBox tb5)
            {
                if (TryParseDouble(tb5.Text, out double parsedUnits) && parsedUnits > 0)
                    unitsPerRevolution = parsedUnits;
            }

            if (!ok3 || travelValue <= 0 || unitsPerRevolution <= 0)
            {
                label11.Text = string.Empty;
                // clear label14 if present
                var foundClear = this.Controls.Find("label14", true);
                if (foundClear.Length > 0 && foundClear[0] is Label lblClear)
                    lblClear.Text = string.Empty;
            }
            else
            {
                long fullRevs = (long)Math.Floor(travelValue / unitsPerRevolution);
                label11.Text = $"Full Revolutions: {fullRevs}";
                // update remaining travel (distance not making a full revolution) in label14 if it exists
                double remainder = travelValue - fullRevs * unitsPerRevolution;
                var found = this.Controls.Find("label14", true);
                if (found.Length > 0 && found[0] is Label lbl14)
                {
                    lbl14.Text = $"Travel Remaining {remainder:F3}";
                }
            }
        }
    }
}
