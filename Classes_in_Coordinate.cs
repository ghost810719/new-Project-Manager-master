using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.IO;
using System.Net.NetworkInformation;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WinSCP;
using LLP_API;
using ZXing;
using ZXing.QrCode;
using ZXing.QrCode.Internal;

namespace PM
{
    static class Project
    {
        static public string Path;
    }

    class BeaconData
    {
        public int Number { get; set; }
        public class Beacon
        {
            public string Coordinate_X { get; set; }
            public string Coordinate_Y { get; set; }
            public string Level { get; set; }
            public string UUID { get; set; }
            public string Coverage { get; set; }
            public bool Used { get; set; }
        }
        static public List<Beacon> Beacons;

        public BeaconData()
        {
            Beacons = new List<Beacon>();
        }

        public void WhichData(string uuid)
        {
            Number = -1;
            for (int i = 0; i < Beacons.Count; i++)
            {
                if (Beacons[i].UUID == uuid)
                {
                    Number = i;
                }
            }
        }

        public void Used()
        {
            foreach (string uuid in UsedData.UsedList)
            {
                foreach (var beacon in Beacons)
                {
                    if (beacon.UUID == uuid)
                    {
                        beacon.Used = true;
                    }
                }
            }
        }

        public IEnumerable<string> AvailableList()
        {
            var query = from beacon in Beacons
                        where beacon.Used == false
                        select beacon.UUID;
            return query;
        }
    }

    class Json
    {
        private string _path;
        private string _content;
        private JObject _beaconData;
        public bool Exist { get; set; }

        public Json()
        {
            _path = Project.Path + "_ForLBeacon.xml";
            if (File.Exists(_path))
            {
                Exist = true;
                _content = File.ReadAllText(_path);
            }
            else
            {
                Exist = false;
            }
        }

        public void Transfer()
        {
            _beaconData = JObject.Parse(_content);

            foreach (var data in _beaconData["features"])
            {
                BeaconData.Beacon beacon = new BeaconData.Beacon();
                beacon.Coordinate_X = (string)data["geometry"]["coordinates"][0];
                beacon.Coordinate_Y = (string)data["geometry"]["coordinates"][1];
                beacon.Level = (string)data["properties"]["Level"];
                beacon.UUID = (string)data["properties"]["GUID"];
                beacon.Coverage = (string)data["properties"]["Type"];
                beacon.Used = false;
                BeaconData.Beacons.Add(beacon);
            }
        }
    }

    class Xml
    {
        private string _path;
        private XDocument _xdoc;
        public bool Exist { get; set; }

        public Xml()
        {
            _path = Project.Path + "_ForLBeacon.xml";
            if (File.Exists(_path))
            {
                Exist = true;
                _xdoc = XDocument.Load(_path);
            }
            else
            {
                Exist = false;
            }
        }

        public void Transfer()
        {
            var nodes = _xdoc.Descendants("features");

            foreach (var node in nodes)
            {
                BeaconData.Beacon beacon = new BeaconData.Beacon();
                var coordinates = node.Element("geometry").Elements("coordinates").ToList();
                beacon.Coordinate_X = (string)coordinates[0];
                beacon.Coordinate_Y = (string)coordinates[1];
                beacon.Level = (string)node.Element("properties").Element("Level");
                beacon.UUID = (string)node.Element("properties").Element("GUID");
                beacon.Coverage = (string)node.Element("properties").Element("Type");
                beacon.Used = false;
                BeaconData.Beacons.Add(beacon);
            }

        }
    }

    class Config
    {
        private string _path;
        public List<string> ConfText = new List<string>();

        public Config()
        {
            _path = Project.Path + "_Config.txt";
            if (!File.Exists(_path))
            {
                File.Copy("..\\..\\config.conf", _path);
            }

            

            using (StreamReader sr = new StreamReader(_path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    ConfText.Add(s);
                }
            }
        }

        public void Change(int index)
        {
            ConfText[0] = "coordinate_X=" + BeaconData.Beacons[index].Coordinate_X;
            ConfText[1] = "coordinate_Y=" + BeaconData.Beacons[index].Coordinate_Y;
            ConfText[2] = "coordinate_Z=" + BeaconData.Beacons[index].Level.Substring(3);
            ConfText[9] = "RSSI_coverage=" + BeaconData.Beacons[index].Coverage.Substring(0, 2);
            ConfText[10] = "uuid=" + BeaconData.Beacons[index].UUID;
            ConfText[11] = "init=1";
        }

        public void Write()
        {
            using (StreamWriter sw = File.CreateText(_path))
            {
                foreach (string s in ConfText)
                {
                    sw.WriteLine(s);
                }
            }
        }
    }

    class UsedData
    {
        private string _path;
        static public List<string> UsedList;

        public UsedData()
        {
            UsedList = new List<string>();
            _path = Project.Path + "_UsedList.txt";
            if (!File.Exists(_path))
            {
                using (File.Create(_path)) { }
            }

            using (StreamReader sr = new StreamReader(_path))
            {
                string s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    UsedList.Add(s);
                }
            }
        }

        public void Add(string uuid)
        {
            UsedList.Add(uuid);

            using (StreamWriter sw = File.AppendText(_path))
            {
                sw.WriteLine(uuid);
            }
        }
    }

    class SCP
    {
        SessionOptions sessionOptions;

        public SCP(string hostName, string userName, string passWord)
        {
            sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Scp,
                HostName = hostName,
                UserName = userName,
                Password = passWord
            };
            sessionOptions.GiveUpSecurityAndAcceptAnySshHostKey = true;
            string file = Project.Path + "_Config.txt";

            using (Session session = new Session())
            {
                session.Open(sessionOptions);

                TransferOptions transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Automatic;

                TransferOperationResult transferResult;
                transferResult =
                    session.PutFiles(file, "/home/pi/LBeacon/config/config.conf", false, transferOptions);

                transferResult.Check();

                foreach (TransferEventArgs transfer in transferResult.Transfers)
                {
                    MessageBox.Show("Upload of " + transfer.FileName + " succeeded");
                }
            }
        }
    }

    class PingTest
    {
        public PingTest(string hostname)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            options.DontFragment = true;

            string data = "This is a ping test.";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 60;
            try
            {
                PingReply reply = pingSender.Send(hostname, timeout, buffer, options);
                if (reply.Status == IPStatus.Success)
                {
                    MessageBox.Show("Success!");
                }
            }
            catch
            {
                MessageBox.Show("Time out!");
            }
        }
    }

    class NetAPI
    {
        public NetAPI()
        {
            ServerAPI serverAPI = new ServerAPI("http://10.32.0.170/", "149ddd95-e979-4a3c-8d73-465946524b1b", "sroGVr%2bqScDC02EW2EOpcBPaYVAwhP35L7gDmLHBo0TgEU4YkvcMxv%2fNzx30MYBQFFRcshu0cjC2J6yZ47s6n%2bDm%2bRLWGGB9Lhv4T4sBhHb9B1y9bz6QHtJJxcRFYqk77CbtqATtR960sfGb8hlz2%2bXwTuBVKK5pjUr5fIIs%2fVTpi15atPHYRxIKizPoRlWY");

            List<BeaconInformation> beacons = new List<BeaconInformation>();
            beacons.Add(new BeaconInformation
            {
                Id = Guid.NewGuid(),
                Longitude = 123.45645,
                Latitude = 4654.12313,
                Floor = "",
                LaserPointerInformationId = Guid.NewGuid(),
                Position = ""
            });

            serverAPI.AddBeaconInformations(beacons);

            List<LaserPointerInformation> laserPointers = new List<LaserPointerInformation>();
            laserPointers.Add(new LaserPointerInformation
            {
                Id = Guid.NewGuid(),
                FaceLatitude = 123,
                FaceLongitude = 123,
                Latitude = 123,
                Longitude = 123,
                Floor = "",
                Position = ""
            });

            serverAPI.AddLaserPointerInformations(laserPointers);
        }
    }

    class LaserPointerData
    {
        public List<LaserPointerInformation> LaserPointers;

        public LaserPointerData()
        {
            LaserPointers = new List<LaserPointerInformation>();
        }

    }

    class QRCode
    {
        public string _path;
        private Image _image;
        private int _x, _y;

        private static BarcodeWriter _barcode = new BarcodeWriter()
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions()
            {
                ErrorCorrection = ErrorCorrectionLevel.H,
                Margin = 0,
                Width = 120,
                Height = 120,
                CharacterSet = "UTF-8"
            }
        };

        public QRCode()
        {
            _path = Project.Path + "_QRCode.png";
        }

        public void SavePicture(string content)
        {
            Bitmap qrcode = _barcode.Write(content);
            _image = qrcode;
        }

        public void PrintPage(object o, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(_image, 0, 0);
        }
    }
}