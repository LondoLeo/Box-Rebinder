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
        string default_path = @"G:/configuration.json";
        Configs configs;
        Dictionary<Label, PictureBox> labelPBDict;
        Dictionary<PictureBox, Label> PBLabelDict;
        Bitmap circleClear = new Bitmap(Properties.Resources.Circle);
        Bitmap circleSelected = new Bitmap(Properties.Resources.Circle2);

        Dictionary<string, int> pinDict = new Dictionary<string, int>();
        List<Label> labelList = new List<Label>();

        bool loaded;

        Label lastLabel = null;

        public Form1()
        {
            InitializeComponent();

            foreach (Control label in this.Controls)
            {
                if (!(label is Label))
                    continue;
                if (!label.Name.StartsWith("label_"))
                    continue;

                label.Text = "";
            }
            loaded = false;
            this.Activate();
        }

        private void LoadFile(string path)
        {
            load_path = path;
            labelPBDict = new Dictionary<Label, PictureBox>();
            PBLabelDict = new Dictionary<PictureBox, Label>();

            LoadConfigs();
            FindLabels();
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

        private void FindLabels()
        {
            foreach (Control label in this.Controls)
            {
                if (!(label is Label))
                    continue;
                if (!label.Name.StartsWith("label_"))
                    continue;

                labelList.Add(label as Label);

                string id = label.Name.Split('_')[1];
                PictureBox pb = this.Controls.Find("pb_" + id, true).First() as PictureBox;

                labelPBDict.Add(label as Label, pb);
                PBLabelDict.Add(pb, label as Label);
            }
        }

        private void LoadUI()
        {
            Config config = configs.configurations.Where(c => c.name == cboConfigs.Text).First();

            foreach (Label label in labelList)
            {
                if (!int.TryParse(label.Name.Split('_')[1], out int id)) continue;

                label.Text = config.PinDict[id].Remove(0, 3);
                label.TextAlign = ContentAlignment.MiddleCenter;

                PictureBox pb = labelPBDict[label];
                label.Location = new Point(pb.Location.X + 15, pb.Location.Y + 25);
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

            foreach (Control control in this.Controls)
            {
                if (!(control is Label)) continue;
                Label label = control as Label;
                if (!label.Name.StartsWith("label_")) continue;

                int id = int.Parse(label.Name.Split('_')[1]);
                string pin = "pin" + label.Text;

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

        private void pb_Click(object sender, EventArgs e)
        {
            if (!loaded) return;

            PictureBox senderBox = sender as PictureBox;
            if (lastLabel == null)
            {
                lastLabel = PBLabelDict[senderBox];
                senderBox.Image = circleSelected;

            }
            else
            {
                Label newLabel = PBLabelDict[senderBox];
                string lastInput = lastLabel.Text;
                string newInput = newLabel.Text;
                newLabel.Text = lastInput;
                lastLabel.Text = newInput;

                labelPBDict[lastLabel].Image = circleClear;
                lastLabel = null;
            }

        }

        private void label_Click(object sender, EventArgs e)
        {
            if (!loaded) return;

            Label sendingLabel = sender as Label;
            bool result = labelPBDict.TryGetValue(sendingLabel, out PictureBox pb);

            if (!result) return;

            pb_Click(pb, e);
        }

        private void pb_Paint(object sender, PaintEventArgs e)
        {
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
    }
}
