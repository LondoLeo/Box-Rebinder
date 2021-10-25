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
    public partial class Form1 : Form
    {
        string load_path;
        Configs configs;
        Dictionary<PictureBox, string> pbMap;
        List<PictureBox> pbList;
        Bitmap circleClear = new Bitmap(Properties.Resources.Circle);
        Bitmap circleSelected = new Bitmap(Properties.Resources.Circle2);
        Bitmap circleHovered = new Bitmap(Properties.Resources.Circle3);


        bool loaded;
        PictureBox lastBox;

        public Form1()
        {
            InitializeComponent();

            loaded = false;
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

            loaded = true;
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
            if (!loaded) return;

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
            if (!loaded) return;

            PictureBox pb = sender as PictureBox;

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;
            sf.LineAlignment = StringAlignment.Center;
            Font font = new Font(FontFamily.GenericSansSerif, 12);
            Brush brush = Brushes.Black;

            bool res = pbMap.TryGetValue(pb, out string text);
            if (!res) return;

            SizeF f = e.Graphics.MeasureString(text, font);
            float newSize = font.Size;
            int max_width = 45;
            int max_height = 45;
            while(f.Width > max_width || f.Height > max_height)
            {
                newSize--;
                font = new Font(FontFamily.GenericSansSerif, newSize);
                f = e.Graphics.MeasureString(text, font);
            }


            e.Graphics.DrawString(text, font, brush, new Point(32, 32), sf);
        }

        private void bttnSave_Click(object sender, EventArgs e)
        {
            if (!loaded)
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
            PictureBox pb = sender as PictureBox;
            pb.BackgroundImage = circleHovered;
        }
        private void pb_MouseLeave(object sender, EventArgs e)
        {
            PictureBox pb = sender as PictureBox;
            pb.BackgroundImage = circleClear;
        }
    }
}
