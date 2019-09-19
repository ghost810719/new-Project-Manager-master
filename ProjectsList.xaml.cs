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
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Shell;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json;

namespace PM
{
    public partial class ProjectsList : System.Windows.Controls.UserControl
    {
        /* 
         * Global variable for the Project (folder) selected
         */
        public ProjectFiles SelectedProject { get; set; }

        public string FileAttributePath { get; set; }
        public Dictionary<string, FileAttribute> AllFileAttributes { get; set; }
        public bool SuppressChanged { get; set; }

        /*
         * Constructor for the project list
         */
        public ProjectsList()
        {
            DataContext = this;
            InitializeComponent();
        }

        public class FileAttribute
        {
            public bool Status { get; set; }
            public bool DesignBeacon { get; set; }
            public bool SetBeacon { get; set; }
            public bool Complete { get; set; }
            public string Remark { get; set; }
        }

        /*
         * Establishing what each project (file) consists of
         */
        public class ProjectFiles
        {
            public string Title { get; set; }
            public string FullPath { get; set; }
            public System.IO.FileAttributes Attr { get; set; }
            public DateTime AccsTime { get; set; }
            public BitmapSource Icon { get; set; }
            public BitmapSource RenderedImg { get; set; }
            public int BeaconCount { get; set; }
            public int Progress { get; set; }
            public FileAttribute FileAttribute { get; set; }
        }

        /* Updates the listview to show the files in the current directory */
        private void updateFiles(string proj = null)
        {
            Random rnd = new Random();
            if (String.IsNullOrEmpty(proj) || proj == "Please Select")
            {
                proj = projectSelect.SelectedItem as string;
                if (String.IsNullOrEmpty(proj) || proj == "Please Select")
                {
                    listView33.ItemsSource = null;
                    return;
                }
            }
            ObservableCollection<ProjectFiles> lopf = new ObservableCollection<ProjectFiles>();
            var fileNames = Directory.GetFiles(proj, "*.rvt");
            foreach (string fn in fileNames)
            {
                FileInfo fi = new FileInfo(fn);
                var sysicon = System.Drawing.Icon.ExtractAssociatedIcon(fi.FullName);
                var bmpSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                        sysicon.Handle,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                sysicon.Dispose();
                ShellFile sf = ShellFile.FromFilePath(fi.FullName);
                //sf.Thumbnail.FormatOption = ShellThumbnailFormatOption.ThumbnailOnly;
                var renderedImg = sf.Thumbnail.ExtraLargeBitmapSource;
                var bc = 0;

                try
                {
                    Console.WriteLine("Trying to load XML for {0}......", fi.Name);
                    XDocument objDoc = XDocument.Load(fi.FullName.Replace(".rvt", ".xml"));
                    if (objDoc.Descendants("BeaconFamily").Any())
                    {
                        foreach (var fam in objDoc.Descendants("BeaconFamily"))
                        {
                            string famName = fam.Value;
                            foreach (var BType in fam.Descendants("BeaconType"))
                            {
                                string typeName = BType.Value;
                                if (BType.Descendants("Instance").Any())
                                {

                                    foreach (var BInstance in BType.Descendants("Instance"))
                                    {
                                        ++bc;

                                        XElement height = BInstance.Element("BeaconHeight");
                                        XElement level = BInstance.Element("BeaconLevel");
                                    }
                                }
                                else { Console.WriteLine("No instances of {0} in {1}", typeName, fi.Name); }
                            }
                        }
                    }
                    else { Console.WriteLine("No BeaconFamily in {0}", fi.Name); }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Error Finding XML");
                }

                lopf.Add(new ProjectFiles() { Title = fi.Name, FullPath = fi.FullName, Attr = fi.Attributes, AccsTime = fi.LastWriteTime, Icon = bmpSrc, RenderedImg = renderedImg, BeaconCount = bc, Progress = rnd.Next(0, 100) });

            }

            string fname = proj + "/file_attributes.xml";
            if (fname != FileAttributePath)
            {
                FileAttributePath = fname;
                AllFileAttributes = new Dictionary<string, FileAttribute>();
                if (File.Exists(FileAttributePath))
                {
                    var lines = File.ReadLines(FileAttributePath);
                    AllFileAttributes = JsonConvert.DeserializeObject<Dictionary<string, FileAttribute>>(lines.First());
                }
                foreach (var projectFile in lopf)
                {
                    if (!AllFileAttributes.ContainsKey(projectFile.Title))
                    {
                        AllFileAttributes[projectFile.Title] = new FileAttribute();
                    }
                }
                File.WriteAllLines(FileAttributePath, new string[] { JsonConvert.SerializeObject(AllFileAttributes) });
            }
            listView33.ItemsSource = lopf;
        }

        /*
         * Event Handler to show all files in the folder when the "See All Files" button is clicked 
         */
        private void SeeAllFiles_Click(object sender, RoutedEventArgs e)
        {
            lbFiles.Items.Clear();

            string proj;
            proj = projectSelect.SelectedItem as string;
            if (String.IsNullOrEmpty(proj) || proj == "Please Select")
            {
                return;
            }
            var fileNames = Directory.GetFiles(proj, "*");

            foreach (string fn in fileNames)
            {
                FileInfo fi = new FileInfo(fn);
                lbFiles.Items.Add(fi.Name);
            }
        }

        /* 
         * Opens up the Revit file when the users double click the item.
         */
        private void ListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender != null)
            {
                System.Windows.Controls.ListBox lb = (System.Windows.Controls.ListBox)sender;
                ProjectFiles p = (ProjectFiles)lb.SelectedItem;
                if (p != null)
                {
                    Process.Start(p.FullPath);
                    updateLastOpenedFile(p);
                }
            }
            updateFiles();
        }

        /* 
         * Update the panel on the right hand side to show most recently selected file
         */
        private void updateMostRecentFile(ProjectFiles p)
        {
            RFImage.Source = p.RenderedImg;
            RFLabel1.Text = p.Title;
            RFLabel2.Content = p.AccsTime;
            //RFLabel3.Content = p.Progress;
            RFLabel4.Content = String.Format("Number of Beacons: {0}", p.BeaconCount);

            SuppressChanged = true;
            CB_Status.IsChecked = AllFileAttributes[p.Title].Status;
            CB_Design_Beacon.IsChecked = AllFileAttributes[p.Title].DesignBeacon;
            CB_Set_Beacon.IsChecked = AllFileAttributes[p.Title].SetBeacon;
            CB_Complete.IsChecked = AllFileAttributes[p.Title].Complete;
            TB_Remark.Text = AllFileAttributes[p.Title].Remark;
            SuppressChanged = false;
            //pbStatus.Value = p.Progress;
        }

        /* 
         * Update the panel on the right hand side to show most recently opened file
         */
        private void updateLastOpenedFile(ProjectFiles p)
        {
            Pooplabel1.Content = p.Title;
        }

        /*
         * opens the file when users click on the open button
         */
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender != null)
            {
                System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
                ProjectFiles p = btn.DataContext as ProjectFiles;
                Process.Start(p.FullPath);
                updateLastOpenedFile(p);
            }
            updateFiles();
        }

        /* 
         * Deletes the file when users click on the delete button 
         */
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                try
                {
                    System.Windows.Controls.Button btn = (System.Windows.Controls.Button)sender;
                    ProjectFiles p = btn.DataContext as ProjectFiles;
                    File.Delete(p.FullPath);
                }
                catch (IOException iox)
                {
                    Console.WriteLine(iox.Message);
                }
            }
            updateFiles();
        }

        private void CheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (SuppressChanged || AllFileAttributes == null || SelectedProject == null) return;
            AllFileAttributes[SelectedProject.Title].Status = CB_Status.IsChecked.Value;
            AllFileAttributes[SelectedProject.Title].DesignBeacon = CB_Design_Beacon.IsChecked.Value;
            AllFileAttributes[SelectedProject.Title].SetBeacon = CB_Set_Beacon.IsChecked.Value;
            AllFileAttributes[SelectedProject.Title].Complete = CB_Complete.IsChecked.Value;
            File.WriteAllLines(FileAttributePath, new string[] { JsonConvert.SerializeObject(AllFileAttributes) });
        }

        private void Remark_TextBox_Changed(object sender, RoutedEventArgs e)
        {
            if (SuppressChanged || AllFileAttributes == null) return;
            AllFileAttributes[SelectedProject.Title].Remark = TB_Remark.Text;
            File.WriteAllLines(FileAttributePath, new string[] { JsonConvert.SerializeObject(AllFileAttributes) });
        }

        /*
         * Event handler for project select
         */
        private void projectSelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // ... Get the ComboBox.
            var comboBox = sender as System.Windows.Controls.ComboBox;

            // ... Set SelectedItem as Window Title.
            string value = comboBox.SelectedItem as string;
            updateFiles(value);
        }

        /*
         * loads the project select
         */
        private void projectSelect_Loaded(object sender, RoutedEventArgs e)
        {
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string[] subdirectories = Directory.GetDirectories(desktop);


            List<string> data = new List<string>();
            data.Add("Please Select");
            data.AddRange(subdirectories);

            // ... Get the ComboBox reference.
            var comboBox = sender as System.Windows.Controls.ComboBox;

            // ... Assign the ItemsSource to the List.
            comboBox.ItemsSource = data;

            // ... Make the first item selected.
            comboBox.SelectedIndex = 0;
        }

        /*
         * Event handler for project selection changed
         */
        private void listView33_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender != null)
            {
                System.Windows.Controls.ListBox lb = (System.Windows.Controls.ListBox)sender;
                ProjectFiles p = (ProjectFiles)lb.SelectedItem;
                if (p != null)
                {
                    SelectedProject = p;
                    updateMostRecentFile(p);
                }
            }
        }

        /*
         * Event handler for adding new Revit file to project folder, needs improvement
         * TODO: Make the file usable 
         */
        private void AddToProject_Click(object sender, RoutedEventArgs e)
        {
            string proj;
            proj = projectSelect.SelectedItem as string;
            if (String.IsNullOrEmpty(proj) || proj == "Please Select")
            {
                System.Windows.MessageBox.Show("Please Select a Project!");
            }
            else
            {
                var fn = NewFileName.Text;
                if (fn == null)
                {
                    fn = System.IO.Path.GetRandomFileName();
                }
                try
                {
                    File.Create(proj + @"\" + fn + ".rvt");
                    AllFileAttributes[fn + ".rvt"] = new FileAttribute();
                    updateFiles();
                }
                catch
                {
                    System.Windows.MessageBox.Show("Error", "Cannot Create File!");
                }
            }
        }

        private void lbFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
    

