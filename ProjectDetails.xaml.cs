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
using System.Diagnostics;
using System.Collections.ObjectModel;
using Microsoft.WindowsAPICodePack.Shell;
using System.Xml;
using System.Xml.Linq;

namespace PM
{
    /// <summary>
    /// Interaction logic for ProjectDetails.xaml
    /// </summary>
    
    public partial class ProjectDetails : UserControl
    {
        private ContentControl _coordinate;
        public ProjectDetails(ProjectsList.ProjectFiles p)
        {
            InitializeComponent();
            if (p != null)
            {
                lbl1.Content = p.Title;
                lbl2.Content = p.AccsTime;
                lbl3.Content = p.Attr;
                lbl4.Content = p.BeaconCount;
                _coordinate = new Coordinate(p);
            }
        }

        private void btnCoordinate_Click(object sender, RoutedEventArgs e)
        {
            if (_coordinate != null)
            {
                this.Content = _coordinate;
            }
        }
    }
    
}
