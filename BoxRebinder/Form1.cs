using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace BoxRebinder
{
    public partial class BoxRebinderForm : Form
    {
        string load_path;
        Configs configs;
        Dictionary<PictureBox, string> pbMap;
        List<PictureBox> pbList;
        Bitmap circleClear = Properties.Resources.Circle;
        Bitmap circleSelected = Properties.Resources.Circle2;
        Bitmap circleHovered = Properties.Resources.Circle3;


        bool _loaded;
        PictureBox lastBox;

        Dictionary<string, Bitmap> colorMap = new Dictionary<string, Bitmap>()
        {
            {"A", Properties.Resources.Circle_Green },
            {"B", Properties.Resources.Circle_Red },
            {"Z", Properties.Resources.Circle_Purple },
            {"CUp", Properties.Resources.Circle_Orange },
            {"CDown", Properties.Resources.Circle_Orange },
            {"CLeft", Properties.Resources.Circle_Orange },
            {"CRight", Properties.Resources.Circle_Orange },
            {"DUp", Properties.Resources.Circle_LightGray },
            {"DDown", Properties.Resources.Circle_LightGray },
            {"DLeft", Properties.Resources.Circle_LightGray },
            {"DRight", Properties.Resources.Circle_LightGray },
            {"Up", Properties.Resources.Circle_White },
            {"Left", Properties.Resources.Circle_White },
            {"Down", Properties.Resources.Circle_White },
            {"Right", Properties.Resources.Circle_White },
            {"Tilt" , Properties.Resources.Circle_Gray },
            {"Switch" , Properties.Resources.Circle_Gray },
            {"Light" , Properties.Resources.Circle_LightGray },
            {"L" , Properties.Resources.Circle_LightGray },
            {"R" , Properties.Resources.Circle_LightGray },
            {"Start" , Properties.Resources.Circle },
            {"X", Properties.Resources.Circle_FaintBlue },
            {"Y", Properties.Resources.Circle_FaintBlue }
        };

        public BoxRebinderForm()
        {
            InitializeComponent();

            _loaded = false;
            this.Activate();
        }

        private void LoadFile(string path)
        {
            load_path = path;

            pbList = new List<PictureBox>();
            pbMap = new Dictionary<PictureBox, string>();

            LoadConfigs();
            FindPBs();
            foreach (Config config in configs.configurations)
            {
                config.Connect();
            }
            LoadUI();

            _loaded = true;
        }

        private void LoadConfigs()
        {
            string configFileContent = File.ReadAllText(load_path);
            configs = JsonConvert.DeserializeObject<Configs>(configFileContent);

            foreach (Config config in configs.configurations)
            {
                cboConfigs.Items.Add(config.name);
            }
            cboConfigs.Text = cboConfigs.Items[0] as string;
        }

        private void FindPBs()
        {
            foreach (Control control in this.Controls)
            {
                if (!(control is PictureBox))
                    continue;

                PictureBox pb = control as PictureBox;
                string id = pb.Name.Split('_')[1];
                pbMap.Add(pb, "");
                pbList.Add(pb);
            }
        }

        private void LoadUI()
        {
            Config config = configs.configurations.Where(c => c.name == cboConfigs.Text).First();

            foreach (PictureBox pb in pbList)
            {
                if (!int.TryParse(pb.Name.Split('_')[1], out int id)) continue;
                pbMap[pb] = config.PinDict[id].Remove(0, 3);
                UpdateText(pb);
            }

            numX1.Value = config.x1Value;
            numY1.Value = config.y1Value;
            numX2.Value = config.x2Value;
            numY2.Value = config.y2Value;
            numTilt.Value = config.tiltValue;
            numSocd.Value = config.socdMode;
            cbSwitchDir.Checked = config.switchForDirections;
        }

        private void Save(string path)
        {
            Config config = configs.configurations.Where(c => c.name == cboConfigs.Text).First();

            foreach (PictureBox pb in pbList)
            {
                string input = pbMap[pb];
                int id = int.Parse(pb.Name.Split('_')[1]);
                string pin = "pin" + input;

                FieldInfo field = config.GetType().GetField(pin);
                field.SetValue(config, id);
            }

            config.x1Value = (int)numX1.Value;
            config.x2Value = (int)numX2.Value;
            config.y1Value = (int)numY1.Value;
            config.y2Value = (int)numY2.Value;
            config.tiltValue = (int)numTilt.Value;
            config.socdMode = (int)numSocd.Value;
            config.switchForDirections = cbSwitchDir.Checked;

            string json = JsonConvert.SerializeObject(configs, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        private void UpdateText(PictureBox pb)
        {
            pb.Invalidate();
        }

        private void pb_Click(object sender, EventArgs e)
        {
            if (!_loaded) return;

            PictureBox senderBox = sender as PictureBox;
            if (lastBox == null)
            {
                lastBox = senderBox;
                senderBox.Image = circleSelected;

            }
            else
            {
                string lastInput = pbMap[lastBox];
                string newInput = pbMap[senderBox];

                pbMap[lastBox] = newInput;
                pbMap[senderBox] = lastInput;

                UpdateText(lastBox);
                UpdateText(senderBox);

                lastBox.Image = circleClear;
                lastBox = null;
            }

        }

        private void pb_Paint(object sender, PaintEventArgs e)
        {
            if (!_loaded) return;

            PictureBox pb = sender as PictureBox;

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Font font = new Font("Arial", 12, FontStyle.Bold);
            Brush brush = Brushes.Black;

            bool res = pbMap.TryGetValue(pb, out string text);
            if (!res) return;

            if (text.Length > 6)
            {
                text = text.Remove(5);
            }

            SizeF f = e.Graphics.MeasureString(text, font);
            float newSize = font.Size;
            int max_width = 45;
            int max_height = 45;
            while (f.Width > max_width || f.Height > max_height)
            {
                newSize--;
                font = new Font(font.FontFamily, newSize, font.Style);
                f = e.Graphics.MeasureString(text, font);
            }

            if(text == "Z")
            {
                brush = Brushes.LightGray;
            }

            pb.BackgroundImage = colorMap[text];
            e.Graphics.DrawString(text, font, brush, new Point(32, 32), sf);
        }

        private void bttnSave_Click(object sender, EventArgs e)
        {
            if (!_loaded)
            {
                MessageBox.Show("Can't save a blank configuration.\nLoad a file first.");
                return;
            }
            DialogResult result = sfdSaveConfig.ShowDialog();
            if (result != DialogResult.OK) return;

            Save(sfdSaveConfig.FileName);
        }

        private void bttnLoad_Click(object sender, EventArgs e)
        {
            DialogResult result = ofdLoadConfig.ShowDialog();
            if (result != DialogResult.OK)
            {
                return;
            }

            LoadFile(ofdLoadConfig.FileName);
        }

        private void pb_MouseEnter(object sender, EventArgs e)
        {

        }
        private void pb_MouseLeave(object sender, EventArgs e)
        {

        }
    }
}
