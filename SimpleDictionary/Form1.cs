using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleDictionary
{
    public partial class Form1 : Form
    {
        private bool isHasDataFile, isHasImageFolder, isJustCreateFile = false;
        private String currentPath;
        private String dataFileName = "\\data.txt";
        private String imageFolder = "\\images";

        private String statusMessage;
        private Dictionary<String, String> dict;

        public Form1()
        {
            InitializeComponent();
            initApplication();
            readData();
        }

        public void initApplication()
        {
            // Check enough file and folder, if not have, create it
            // File: config.txt, data.txt, log.txt

            // Get current path
            currentPath = System.IO.Path.GetDirectoryName(Application.ExecutablePath);

            // Check have data.txt file
            isHasDataFile = File.Exists(currentPath + dataFileName);
            isHasImageFolder = Directory.Exists(currentPath + imageFolder);

            // Create file if not exist
            if (!isHasDataFile)
            {
                File.Create(currentPath + dataFileName);
                isJustCreateFile = true;
            }

            // Create image folder if not exist
            if (!isHasImageFolder)
            {
                Directory.CreateDirectory(currentPath + imageFolder);
            }

            // Init dictionary
            dict = new Dictionary<string, string>();

            statusMessage = "Program init successfully";
            setStatusMessage(statusMessage);
        }

        public void readData()
        {
            if (isJustCreateFile)
            {
                return;
            }

            String filePath = currentPath + dataFileName;
            String[] data;
            String word, meaning;
            // Read data from file
            using (FileStream fs = File.Open(filePath, FileMode.Open))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs))
                    {
                        string str;
                        while ((str = sr.ReadLine()) != null)
                        {
                            // Split data into dictionary
                            // Data structure: word@meaning
                            data = str.Split('@');
                            word = data[0];
                            meaning = data[1];
                            if (!dict.ContainsKey(word))
                            {
                                dict.Add(word, meaning);
                            }
                        }
                    }
                }
            }

            // Set dataset for listbox
            if(dict.Count > 0)
            {
                lsbWord.DataSource = new BindingSource(dict, null);
                lsbWord.DisplayMember = "Key";
                lsbWord.ValueMember = "Value";
            }
        }

        public void writeData()
        {
            var filePath = currentPath + dataFileName;
            // Delete old file
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Open file and write data to.
            using (var sw = new StreamWriter(filePath, true))
            {
                foreach (var item in dict)
                {
                    // Get key & value from dict
                    var key = item.Key;
                    var value = item.Value;

                    // Write to file
                    sw.WriteLine(key + "@" + value);
                }
            }
        }

        public void translate()
        {
            // Get selected value value
            String value = lsbWord.SelectedValue.ToString();

            // Split into meaning and picture ("#")
            String[] data = value.Split('#');

            if (data != null && data.Length == 2)
            {
                // Fill to textarea
                rtbMeaning.Text = data[0];

                // Set picture
                if (File.Exists(currentPath + imageFolder + "\\" + data[1]))
                {
                    ptbMeaning.Image = Image.FromFile(currentPath + imageFolder + "\\" + data[1]);
                }
            }
            else
            {
                MessageBox.Show("Something went wrong");
            }

        }

        private void btnTranslate_Click(object sender, EventArgs e)
        {
            translate();
        }

        private void btnSaveData_Click(object sender, EventArgs e)
        {
            writeData();
            // Set status message
            setStatusMessage("Save success!");
            MessageBox.Show("Save data success");

        }

        private void btnAddBrowsePicture_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;...";
            dialog.InitialDirectory = @"C:\";
            dialog.Title = "Please select an image file for the new word.";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                // Set file path
                txtAddPicturePath.Text = dialog.FileName;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Check data valid (has word, has meaning, has file in path)
            bool isValid = false;
            String word, meaning, picture;

            word = txtAddWord.Text;
            meaning = rtbAddMeaning.Text;
            
            if(!String.IsNullOrEmpty(word) && !String.IsNullOrEmpty(meaning))
            {
                isValid = true;
            }

            if (File.Exists(txtAddPicturePath.Text))
            {
                isValid = true;
            }

            if (!isValid)
            {
                MessageBox.Show("Data is not valid!");
                return;
            }

            // Coppy image to folder
            picture = System.IO.Path.GetFileName(txtAddPicturePath.Text);
            var imgFolder = currentPath + imageFolder + "\\";
            System.IO.File.Copy(txtAddPicturePath.Text, imgFolder + picture, true);

            // Add to dict
           
            String line = word + "@" + meaning + "#" + picture;
            if (dict.ContainsKey(word))
            {
                dict.Remove(word);
            }


            dict.Add(word, meaning + "#" + picture);

            // Call write data
            writeData();

            MessageBox.Show("Add word success!!");
        }

        private void btnResetDefault_Click(object sender, EventArgs e)
        {
            initApplication();
            readData();
        }

        public void setStatusMessage(String message)
        {
            lbStatus.Text = message;
        }
    }
}
