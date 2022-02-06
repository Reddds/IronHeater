using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// Interaction logic for ConnectingWindow.xaml
    /// </summary>
    public partial class ConnectingWindow : Window
    {
        private SerialPort _port;
        private bool _isTryConnect = true;
        public ConnectingWindow(SerialPort port)
        {
            InitializeComponent();

            _port = port;
            Title = $"Подключение к {port.PortName}...";

            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var connectTask = new Task(() =>
            {
                while (!_port.IsOpen)
                {
                    if (!_isTryConnect)
                        return;
                    try
                    {
                        _port.Open();
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                    Thread.Sleep(500);
                }
                Dispatcher.Invoke(() =>
                {
                    DialogResult = true;
                });
                
            });


            connectTask.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isTryConnect = false;
        }
    }
}
