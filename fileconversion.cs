using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace fileconversion
{
    public partial class UI : Form
    {

        private String[] sourceFiles;
        private char carriageReturn = (char)0xD;
        private char backSlash = (char)0x5C;

        public UI()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void convToCSV_Click(object sender, EventArgs e)
        {
            String output = String.Empty;
            String filter = "ecu files (*.ecu)|*.ecu";

            if (openFile(filter))
            {
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    String[] filePath = sourceFiles[i].Split(backSlash);
                    output = output + filePath[filePath.Length - 1] + carriageReturn;
                    new XmlToCSV(sourceFiles[i], XmlToCSV.conversonType.csvOnly);
                    fileNames.Text = output;
                    this.Update();
                }
                
            }
 
        }


        private void cleanAndCompress_Click(object sender, EventArgs e)
        {

            String output = String.Empty;
            String filter = "ecu files (*.ecu)|*.ecu";

            if (openFile(filter))
            {
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    String[] filePath = sourceFiles[i].Split(backSlash);
                    output = output + filePath[filePath.Length - 1] + carriageReturn;
                    new XmlToCSV(sourceFiles[i], XmlToCSV.conversonType.compressCSV);
                    fileNames.Text = output;
                    this.Update();
                }
            }

        }

        private void decompress_Click(object sender, EventArgs e)
        {

            String filter = "Compressed ecu files (*.cmp)|*.cmp";
            String output = String.Empty;

            if (openFile(filter))
            {
                for (int i = 0; i < sourceFiles.Length; i++)
                {
                    String[] filePath = sourceFiles[i].Split(backSlash);
                    output = output + filePath[filePath.Length - 1] + carriageReturn;
                    new XmlToCSV(sourceFiles[i], XmlToCSV.conversonType.decompress);
                    fileNames.Text = output;
                    this.Update();
                }
            }

        }

        private bool openFile(String filter)
        {

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                ofd.Filter = filter;
                ofd.FilterIndex = 1;
                ofd.RestoreDirectory = true;
                ofd.Multiselect = true;

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    sourceFiles = ofd.FileNames;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
        }
    }
}
