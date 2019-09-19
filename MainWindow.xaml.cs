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
using Microsoft.Win32;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

/* 
 * Main Window, invokes the usercontrols. 
 */
namespace PM
{
    
    public partial class MainWindow : Window  
    {
        ContentControl view1 = new ProjectsList();
        ContentControl view2 = new MergeFiles();
        ContentControl view3 = new BeaconSpecs();
        ContentControl view4 = new BeaconIpAddress();

        /*
         * Constructor for the window 
         */
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            this.contentControl.Content = view1;
        }

        /*
         * Event handler for clicking the "Project List" button
         */
        private void projectsList_Click(object sender, RoutedEventArgs e)
        {
            if (contentControl.Content is ProjectsList)
            {
                return;
            }
            else
            {
                contentControl.Content = view1;
            }
            
        }

        /*
         * Event handler for clicking the "Beacon Specs" button
         */
        private void beaconSpecs_Click(object sender, RoutedEventArgs e)
        {
            if (contentControl.Content is BeaconSpecs)
            {
                return;
            }
            else
            {
                contentControl.Content = view3;
            }
        }

        /*
         * Event handler for clicking the "Project Details" button
         */
        private void projectDetails_Click(object sender, RoutedEventArgs e)
        {
            if (contentControl.Content is ProjectDetails)
            {
                return;
            }
            else
            {
                contentControl.Content = new ProjectDetails(((ProjectsList)(view1)).SelectedProject);
            }
        }

        /*
         * Event handler for clicking the "Merge Files" button
         */
        private void mergeFiles_Click(object sender, RoutedEventArgs e)
        {
            if (contentControl.Content is MergeFiles)
            {
                return;
            }
            else
            {
                contentControl.Content = view2;
            }
        }

        /*
         * Event handler for clicking the "Beacon Ip Address" button
         */
        private void beaconIpAddress_Click(object sender, RoutedEventArgs e)
        {
            if (contentControl.Content is BeaconIpAddress)
            {
                return;
            }
            else
            {
                contentControl.Content = view4;
            }
        }
    }
}