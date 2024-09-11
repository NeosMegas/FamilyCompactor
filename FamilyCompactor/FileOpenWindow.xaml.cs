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

        public FileOpenWindow(string title, string lang)
        {
            InitializeComponent();
            Title = title;
            DataObject.AddPastingHandler(txtFileNames, txtFileNames_OnPaste);
            Resources.MergedDictionaries.Clear();
            ResourceDictionary rd = new ResourceDictionary()
            {
                Source = new Uri($@"pack://application:,,,/{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name};component/Resources/{lang.ToLower()}.xaml", UriKind.RelativeOrAbsolute)
            };
            Resources.MergedDictionaries.Add(rd);
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
            {
                foreach(string fileName in ofd.FileNames)
                    txtFileNames.Text += fileName + '\n';
                btnOk.Focus();
            }    
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            txtFileNames.Text = txtFileNames.Text.Replace("\r\n", "\n");
            string[] lines = txtFileNames.Text.Split(new[] { '\n'}, StringSplitOptions.RemoveEmptyEntries);
            foreach(string line in lines)
                filesList.Add(line);
            if (filesList.Count > 0)
                DialogResult = true;
            else
                DialogResult = false;
        }

        private void txtFileNames_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.SourceDataObject.GetDataPresent(DataFormats.Text, true))
                btnOk.Focus();
        }

    }
}
