using Autodesk.Revit.UI;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FamilyCompactor
{
    /// <summary>
    /// Interaction logic for FileOpenWindow.xaml
    /// </summary>
    public partial class FileOpenWindow : Window
    {
        private List<string> filesList = new List<string>();

        public List<string> FilesList
        {
            get { return filesList; }
        }

        public FileOpenWindow(string title)
        {
            InitializeComponent();
            Title = title;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                Filter = "Revit family (*.rfa)|*.rfa|All files (*.*)|*.*",
                Multiselect = true,
                CheckFileExists = true
            };
            if(ofd.ShowDialog(this) == true)
                foreach(string fileName in ofd.FileNames)
                    txtFileNames.Text += fileName + '\n';
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            txtFileNames.Text = txtFileNames.Text.Replace("\r\n", "\n");
            string[] lines = txtFileNames.Text.Split('\n');
            foreach(string line in lines)
                filesList.Add(line);
            if (filesList.Count > 0)
                DialogResult = true;
            else
                DialogResult = false;
            //Close();
        }
    }
}
