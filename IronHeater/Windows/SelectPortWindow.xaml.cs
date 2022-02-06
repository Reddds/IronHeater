using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using System.Windows.Shapes;

namespace IronHeater.Windows
{
    /// <summary>
    /// Interaction logic for SelectPortWindow.xaml
    /// </summary>
    public partial class SelectPortWindow : Window
    {
        public string? SelectedPort = null;
        public SelectPortWindow(string[] ports)
        {
            InitializeComponent();


            CbPorts.ItemsSource = ports;
        }

        private void BOk_Click(object sender, RoutedEventArgs e)
        {
            if(CbPorts.SelectedItem == null)
            {
                MessageBox.Show("Выберите порт!");
                return;
            }

            SelectedPort = (string)CbPorts.SelectedValue;
            DialogResult = true;
        }
    }
}
