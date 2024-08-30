using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace mergevehiclesfivem
{
    public partial class Form1 : Form
    {
        private string vehicleFolderPath = string.Empty;
        private string outputFolderPath = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the folder containing the vehicles";
                folderDialog.ShowNewFolderButton = false;
                folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    vehicleFolderPath = folderDialog.SelectedPath;
                    MessageBox.Show($"Vehicle folder selected: {vehicleFolderPath}", "Folder Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select the output folder";
                folderDialog.ShowNewFolderButton = true;
                folderDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    outputFolderPath = folderDialog.SelectedPath;
                    MessageBox.Show($"Output folder selected: {outputFolderPath}", "Folder Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(vehicleFolderPath) || string.IsNullOrEmpty(outputFolderPath))
            {
                MessageBox.Show("Please select both folders before proceeding.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Please enter a valid name for the folder");
                return;
            }

            string mergedFolder = Path.Combine(outputFolderPath, textBox1.Text);
            string streamFolder = Path.Combine(mergedFolder, "stream");
            string dataFolder = Path.Combine(mergedFolder, "data");

            if (!Directory.Exists(mergedFolder))
            {
                Directory.CreateDirectory(mergedFolder);
            }

            if (!Directory.Exists(streamFolder))
            {
                Directory.CreateDirectory(streamFolder);
            }

            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }

            GenerateManifest(mergedFolder);

            foreach (var vehicleDir in Directory.GetDirectories(vehicleFolderPath))
            {
                string vehicleName = Path.GetFileName(vehicleDir);
                string vehicleStreamFolder = Path.Combine(streamFolder, vehicleName);
                string vehicleDataFolder = Path.Combine(dataFolder, vehicleName);

                Directory.CreateDirectory(vehicleStreamFolder);
                Directory.CreateDirectory(vehicleDataFolder);

                foreach (var subDir in Directory.GetDirectories(vehicleDir))
                {
                    string subDirName = Path.GetFileName(subDir).ToLower();

                    if (subDirName == "stream")
                    {
                        ProcessStreamDirectory(subDir, vehicleStreamFolder);
                    }
                    else
                    {
                        ProcessMetaDirectory(subDir, vehicleDataFolder);
                    }
                }

                foreach (var file in Directory.GetFiles(vehicleDir))
                {
                    string fileName = Path.GetFileName(file);
                    string extension = Path.GetExtension(file).ToLower();
                    if (extension == ".meta")
                    {
                        File.Copy(file, Path.Combine(vehicleDataFolder, fileName), true);
                    }
                    else
                    {
                        File.Copy(file, Path.Combine(vehicleStreamFolder, fileName), true);
                    }
                }
            }

            MessageBox.Show("Merge completed. All files have been combined.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ProcessStreamDirectory(string currentDir, string vehicleStreamFolder)
        {
            foreach (var file in Directory.GetFiles(currentDir))
            {
                string fileName = Path.GetFileName(file);
                string extension = Path.GetExtension(file).ToLower();

                File.Copy(file, Path.Combine(vehicleStreamFolder, fileName), true);
            }

            foreach (var subDir in Directory.GetDirectories(currentDir))
            {
                ProcessStreamDirectory(subDir, vehicleStreamFolder);
            }
        }

        private void ProcessMetaDirectory(string currentDir, string vehicleMetaFolder)
        {
            foreach (var file in Directory.GetFiles(currentDir))
            {
                string fileName = Path.GetFileName(file);
                string extension = Path.GetExtension(file).ToLower();

                File.Copy(file, Path.Combine(vehicleMetaFolder, fileName), true);
            }

            foreach (var subDir in Directory.GetDirectories(currentDir))
            {
                ProcessStreamDirectory(subDir, vehicleMetaFolder);
            }
        }

        private void GenerateManifest(string outputFolder)
        {
            string manifestFile = Path.Combine(outputFolder, "fxmanifest.lua");

            using (StreamWriter writer = new StreamWriter(manifestFile))
            {
                writer.WriteLine("fx_version 'adamant'");
                writer.WriteLine("game 'gta5'");
                writer.WriteLine("");
                writer.WriteLine("files {\r\n'data/**/*.meta',\r\n}");
                writer.WriteLine("");
                writer.WriteLine("data_file 'HANDLING_FILE' 'data/**/handling.meta'");
                writer.WriteLine("data_file 'VEHICLE_LAYOUTS_FILE' 'data/**/vehiclelayouts.meta'");
                writer.WriteLine("data_file 'VEHICLE_METADATA_FILE' 'data/**/vehicles.meta'");
                writer.WriteLine("data_file 'CARCOLS_FILE' 'data/**/carcols.meta'");
                writer.WriteLine("data_file 'VEHICLE_VARIATION_FILE' 'data/**/carvariations.meta'");
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Console.WriteLine(textBox1.Text);
        }
    }
}
