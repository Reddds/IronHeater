using IronHeater.Windows;
using OxyPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

namespace IronHeater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SerialPort _serialPort;
        MainViewModel _mainModel;
        Stopwatch _watch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();

        }

        void OpenPort()
        {
            // Create a new SerialPort object with default settings.
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = Properties.Settings.Default.ArduinoPort;
            _serialPort.BaudRate = 9600;
            //_serialPort.Parity = SetPortParity(_serialPort.Parity);
            //_serialPort.DataBits = SetPortDataBits(_serialPort.DataBits);
            //_serialPort.StopBits = SetPortStopBits(_serialPort.StopBits);
            //_serialPort.Handshake = SetPortHandshake(_serialPort.Handshake);

            // Set the read/write timeouts
            _serialPort.ReadTimeout = 50;
            _serialPort.WriteTimeout = 50;

            _serialPort.DataReceived += _serialPort_DataReceived;

            _serialPort.Open();
        }

        readonly Regex reT = new(@"^t1=(?<Val1>[0-9\.]+)t2=(?<Val2>[0-9\.]+)$");

        readonly Regex reHeaterOn = new(@"^heaterOn$");
        readonly Regex reHeaterOff = new(@"^heaterOff$");


        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(10);

            SerialPort sp = (SerialPort)sender;
            while (true)
            {
                try
                {
                    string indata = sp.ReadLine().Trim();

                    string processed = $"Не распознано: {indata}";

                    var mT = reT.Match(indata);
                    if (mT.Success)
                    {
                        var t1Str = mT.Groups["Val1"].Value;
                        if (!float.TryParse(t1Str, System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out var t1))
                            throw new Exception($"Wrong t1 temp: '{t1Str}'");
                        var t2Str = mT.Groups["Val2"].Value;
                        if (!float.TryParse(t2Str, System.Globalization.NumberStyles.Float,
                                        System.Globalization.CultureInfo.InvariantCulture, out var t2))
                            throw new Exception($"Wrong t2 temp: '{t2Str}'");
                        processed = $"Temp 1: {t1}°C, Temp 2: {t2}°C";
                        //continue;
                        _mainModel.AddData(t1, t2, _watch.Elapsed);
                    }





                    Dispatcher.InvokeAsync(() =>
                    {
            


                        TbLog.Text += processed + "\n";
                        TbLog.ScrollToEnd();
                    });
                }
                catch (TimeoutException)
                {
                    return;
                }
                catch (Exception ex)
                {

                    throw;
                }

            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();
            if (ports == null || ports.Length == 0)
            {
                MessageBox.Show("Ни одного порта не найдено в системе! Сначала подключите контроллер.");
                Application.Current.Shutdown();
                return;
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.ArduinoPort) || !ports.Contains(Properties.Settings.Default.ArduinoPort))
            {
                var selectPortWin = new SelectPortWindow(ports);
                if (selectPortWin.ShowDialog() != true)
                {
                    Application.Current.Shutdown();
                    return;
                }

                Properties.Settings.Default.ArduinoPort = selectPortWin.SelectedPort;
                Properties.Settings.Default.Save();
            }



            OpenPort();

            _mainModel = new MainViewModel();
            DataContext = _mainModel;

            _watch.Start();
        }
    }


}
