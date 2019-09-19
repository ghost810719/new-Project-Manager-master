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
using System.Text.RegularExpressions;

namespace PM
{
    public partial class MergeFiles : System.Windows.Controls.UserControl
    {
        private string PROJECT_FOLDER;
        private XmlDocument outputXml;
        private List<Beacon> beacons;

        /*
         * Constructor for the beacon ip address
         */
        public MergeFiles()
        {
            
            DataContext = this;
            InitializeComponent();


            PROJECT_FOLDER = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName;
        }

        private void Revit_Select_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "xml files (*.xml)|*.xml";
            dialog.InitialDirectory = PROJECT_FOLDER;
            dialog.Title = "Select a XML file";
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                
                string path = dialog.FileName;               
                TB_Rivet_Path.Text = path;                
                TB_Rivet_Content.Text = File.ReadAllText(path);
                Merge_Files();
            }

        }

        private void Output_Select_Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                TB_Output_Path.Text = dialog.SelectedPath;
            }
        }

        /* convert xml to config.conf */
        private void Merge_Files()
        {
            string aPath = TB_Rivet_Path.Text;
            XmlDocument aDoc = null;           

            if (aPath != null && aPath.Length > 0)
            {
                aDoc = LoadXmlDocument(aPath);
            }            
            
            if (aDoc != null)
            {                
                // merge
                string content = "";
                int index = 0;
                beacons = new List<Beacon>();
             

                 foreach (XmlNode node in aDoc["abc"].ChildNodes) 
                 {                    
                    
                    Beacon beacon = new Beacon();

                    try
                    {
                        var coords = node.SelectNodes("geometry/coordinates");
                        string x = node["geometry"]["coordinates"].InnerText.Substring(0, 10);                                          
                        beacon.x = x;
                        string y = node["geometry"]["coordinates"].NextSibling.InnerText.Substring(0, 9);
                        beacon.y = y;


                        string level = node.SelectSingleNode("properties/Level").InnerText;
                        string z = Regex.Match(level, @"B\d+").Value;
                        if (z == null || z.Length == 0)
                        {
                            z = Regex.Match(level, @"\d+").Value;
                        }
                        else
                        {
                            z = "-" + Regex.Match(z, @"\d+").Value;
                        }
                        beacon.z = z;
                    }
                    catch (Exception e)
                    {
                        beacon.x = beacon.y = beacon.z = "0";
                    }
                    beacon.area_id = TB_AreaID.Text;
                    beacon.scan_mac_prefix = TB_ScanMacPrefix.Text;
                    beacon.gateway_addr = TB_GatewayAddr.Text;

                    beacons.Add(beacon);

                    content += "#" + index + "\n" + beacon + "\n\n";
                    index += 1;
                    Console.WriteLine("1");
                }

                TB_Output_Content.Text = content;
            }
        }

        private XmlDocument LoadXmlDocument(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            return doc;
        }

        private string XmlToString(XmlDocument doc)
        {
            var stringBuilder = new StringBuilder();
            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                doc.Save(xmlWriter);
            }
            return stringBuilder.ToString();
        }

        private void Merge_Button_Click(object sender, RoutedEventArgs e)
        {
            string path = TB_Output_Path.Text;

            if (path == null || path.Length <= 0)
            {
                return;
            }

            int index = 1;
            foreach (Beacon beacon in beacons)
            {
                Directory.CreateDirectory(path + "/" + index);
                File.WriteAllText(path + "/" + index + "/config.conf", beacon.ToString());
                index += 1;
            }
        }

        /* Lbeacon config */
        class Beacon
        {
            public string area_id;
            public string x;
            public string y;
            public string z;
            public int lowest_basement_level = 20;
            public int advertise_dongle_id = 0;
            public int advertise_interval_in_uints_0625_ms = 480;
            public int advertise_rssi_value = -50;
            public int scan_dongle_id = 1;
            public int scan_rssi_coverage = -100;
            public string scan_mac_prefix = "1,C1:,";
            public string gateway_addr;
            public int gateway_port = 8888;
            public int local_client_port = 9999;

            public override string ToString()
            {
                return String.Join(
                    Environment.NewLine,
                    "area_id=" + area_id,
                    "coordinate_X=" + x,
                    "coordinate_Y=" + y,
                    "coordinate_Z=" + z,
                    "lowest_basement_level=" + lowest_basement_level,
                    "advertise_dongle_id=" + advertise_dongle_id,
                    "advertise_interval_in_uints_0625_ms=" + advertise_interval_in_uints_0625_ms,
                    "advertise_rssi_value=" + advertise_rssi_value,
                    "scan_dongle_id=" + scan_dongle_id,
                    "scan_rssi_coverage=" + scan_rssi_coverage,
                    "scan_mac_prefix=" + scan_mac_prefix,
                    "gateway_addr=" + gateway_addr,
                    "gateway_port=" + gateway_port,
                    "local_client_port=" + local_client_port);
            }
        }
    }
}
