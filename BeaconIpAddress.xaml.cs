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

namespace PM
{
    public partial class BeaconIpAddress : System.Windows.Controls.UserControl
    {
        private string selectedProject;

        private List<Gateway> gateways;
        private List<Beacon> beacons;

        public class Gateway
        {
            public string ip;
            public string serverIp;
            public List<Beacon> beacons;
        }

        public class Beacon
        {
            public string x;
            public string y;
            public string z;
            public string coords;
            public string id;
            public string ip;
            public Gateway gateway;
        }

        /*
         * Constructor for the beacon ip address
         */
        public BeaconIpAddress()
        {
            DataContext = this;
            InitializeComponent();
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
            updateGatewayAndBeacons(value);
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

        private void updateGatewayAndBeacons(string dirPath)
        {
            if (dirPath == selectedProject)
            {
                return;
            }
            else if (String.IsNullOrEmpty(dirPath) || dirPath == "Please Select")
            {
                updateGatewayAndBeacons(selectedProject);
                return;
            }

            //string gatewayFname = dirPath + "/gateway.xml";
            string gatewayFname = dirPath + "/gateways.xml";
            string beaconFname = dirPath + "/beacons.xml";
            if (!File.Exists(gatewayFname) || !File.Exists(beaconFname))
            {
                gatewaySelect.ItemsSource = null;
                //gatewaySelect2.ItemsSource = null;
                beaconIpSelect.ItemsSource = null;
                beaconsInGateway.Text = "";
                beaconIp.Text = "";
                return;
            }

            XmlDocument gatewayXmlDoc = new XmlDocument();
            gatewayXmlDoc.Load(gatewayFname);

            gateways = new List<Gateway>();
            List<string> gatewayNames = new List<string>();

            // read gateways
            foreach (XmlNode node in gatewayXmlDoc.SelectNodes("root/gateway"))
            {
                Gateway gateway = new Gateway();
                gateway.ip = node["properties"]["gateway_addr"].InnerText;
                gateway.serverIp = node["properties"]["sever_ip"].InnerText;
                gateway.beacons = new List<Beacon>();
                gatewayNames.Add(gateway.ip);
                gateways.Add(gateway);
            }

            gatewaySelect.ItemsSource = gatewayNames;
            //gatewaySelect2.ItemsSource = gatewayNames;

            XmlDocument beaconXmlDoc = new XmlDocument();
            beaconXmlDoc.Load(beaconFname);

            beacons = new List<Beacon>();
            List<string> beaconNames = new List<string>();

            // read beacons
            foreach (XmlNode node in beaconXmlDoc.SelectNodes("root/features"))
            {
                Beacon beacon = new Beacon();
                var coords = node.SelectNodes("geometry/coordinates");
                beacon.x = coords[0].InnerText;
                beacon.y = coords[1].InnerText;
                beacon.z = node["properties"]["Level"].InnerText;
                beacon.coords = "(" + beacon.x + ", " + beacon.y + ", " + beacon.z + ")";
                beaconNames.Add(beacon.coords);
                beacon.id = node["properties"]["Element_Id"]["IntegerValue"].InnerText;
                //beacon.ip = node["ip"].InnerText;
                foreach (Gateway gateway in gateways)
                {
                    if (node["properties"]["gateway_addr"].InnerText == gateway.ip)
                    {
                        beacon.gateway = gateway;
                        gateway.beacons.Add(beacon);
                        break;
                    }
                }
                beacons.Add(beacon);
            }

            beaconIpSelect.ItemsSource = beaconNames;
        }

        private void OnGatewaySelected(object sender, SelectionChangedEventArgs e)
        {
            Gateway selectedGateway = null;
            if (sender != null)
            {
                System.Windows.Controls.ComboBox cb = (System.Windows.Controls.ComboBox) sender;
                if (cb == null || cb.SelectedItem == null)
                {
                    beaconsInGateway.Text = "";
                    return;
                }
                string gatewayIp = cb.SelectedItem.ToString();
                foreach (Gateway gateway in gateways)
                {
                    if (gateway.ip == gatewayIp)
                    {
                        selectedGateway = gateway;
                        break;
                    }
                }
            }
            if (selectedGateway != null)
            {
                string text = "Server IP: " + selectedGateway.serverIp;
                text += "\nGateway IP: " + selectedGateway.ip;

                if (selectedGateway.beacons.Count == 0)
                {
                    text += "\n\nThis gateway has no beacon.";
                }
                else
                {
                    text += "\n\nBeacons:";
                    int index = 0;
                    foreach (Beacon beacon in selectedGateway.beacons)
                    {
                        index += 1;
                        text += "\n[" + index + "] ID=" + beacon.id + ", coords=" + beacon.coords;
                    }

                }
                beaconsInGateway.Text = text;
            }
        }

        private void OnBeaconSelected(object sender, SelectionChangedEventArgs e)
        {
            Beacon selectedBeacon = null;
            if (sender != null)
            {
                System.Windows.Controls.ComboBox cb = (System.Windows.Controls.ComboBox)sender;
                if (cb == null || cb.SelectedItem == null)
                {
                    beaconIp.Text = "";
                    return;
                }
                string beaconCoords = cb.SelectedItem.ToString();
                foreach (Beacon beacon in beacons)
                {
                    if (beacon.coords == beaconCoords)
                    {
                        selectedBeacon = beacon;
                        break;
                    }
                }
            }
            if (selectedBeacon != null)
            {
                string text = "Coords=" + selectedBeacon.coords;

                if (selectedBeacon.gateway == null)
                {
                    text += "\n\nThis beacon has no associated gateway.";
                }
                else
                {
                    text += "\n\nGateway IP: " + selectedBeacon.gateway.ip;
                }
                beaconIp.Text = text;
            }
        }
    }
}
