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
using Microsoft.WindowsAPICodePack.Shell;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

namespace PM
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BeaconSpecs : UserControl
    {
        private List<Beacon> beacons;

        private string PROJECT_FOLDER;
        private string SPEC_FILE_PATH;

        private string selectedFilePath;
        private string selectedImagePath;

        public BeaconSpecs()
        {
            InitializeComponent();

            PROJECT_FOLDER = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName;
            SPEC_FILE_PATH = PROJECT_FOLDER + "\\beacon.json";

            LoadBeacons();
        }

        public class Beacon
        {
            public string Title { get; set; }
            public BitmapSource BeaconImage { get; set; }
            public string Completion { get; set; }
            public string Degree { get; set; }
            public string Radius { get; set; }
            public string FamilyPath { get; set; }
        }

        private void LoadBeacons()
        {
            beacons = new List<Beacon>();

            if (File.Exists(SPEC_FILE_PATH))
            {
                var lines = File.ReadLines(SPEC_FILE_PATH);
                beacons = JsonConvert.DeserializeObject<List<Beacon>>(lines.First());
            }

            listBox.ItemsSource = beacons;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
                Beacon p = btn.DataContext as Beacon;
                Console.WriteLine(p.FamilyPath);
                Process.Start(p.FamilyPath);
            }
        }
        private void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
                Beacon p = btn.DataContext as Beacon;
                beacons.Remove(p);
                listBox.Items.Refresh();

                File.WriteAllLines(SPEC_FILE_PATH, new string[] { JsonConvert.SerializeObject(beacons) });
            }
        }

        private void Select_File_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "rfa files (*.rfa)|*.rfa";
            dialog.InitialDirectory = PROJECT_FOLDER;
            dialog.Title = "Select a Rivet file";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BTN_SelectFile.Content = "Selected";
                selectedFilePath = dialog.FileName;
            }
        }

        private void Select_Image_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "png files (*.png)|*.png";
            dialog.InitialDirectory = PROJECT_FOLDER + "\\Resources";
            dialog.Title = "Select a png file";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                BTN_SelectImage.Content = "Selected";
                selectedImagePath = dialog.FileName;
            }
        }

        private void Add_Button_Click(object sender, RoutedEventArgs e)
        {
            Beacon beacon = new Beacon();
            beacon.Title = TB_Type.Text;
            beacon.Degree = TB_Degree.Text;
            beacon.Radius = TB_Radius.Text;
            beacon.Completion = TB_Completion.Text;
            beacon.BeaconImage = new BitmapImage(new Uri(selectedImagePath));
            beacon.FamilyPath = selectedFilePath;
            beacons.Add(beacon);
            listBox.Items.Refresh();

            File.WriteAllLines(SPEC_FILE_PATH, new string[] { JsonConvert.SerializeObject(beacons) });

            BTN_SelectFile.Content = "Select File";
            BTN_SelectImage.Content = "Select Image";
            TB_Type.Clear();
            TB_Degree.Clear();
            TB_Radius.Clear();
            TB_Completion.Clear();
            selectedFilePath = null;
            selectedImagePath = null;
        }
    }
}
