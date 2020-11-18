using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace Semáforo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {



        public static class K8055
        {
            [DllImport("K8055D.dll")]
            public static extern int OpenDevice(int CardAddress);
            [DllImport("K8055D.dll")]
            public static extern void CloseDevice();
            [DllImport("K8055D.dll")]
            public static extern void SetDigitalChannel(int Channel);
            [DllImport("K8055D.dll")]
            public static extern bool ReadDigitalChannel(int Channel);
        }

        public MainWindow()
        {
            //InitializeComponent();
            K8055.OpenDevice(0); //Open communication with K8055 that has the device address 0
            K8055.SetDigitalChannel(1); //Sets digital output channel 1 to 'ON'
            K8055.SetDigitalChannel(6); //Sets digital output channel 6 to 'ON'
            Console.WriteLine(K8055.ReadDigitalChannel(2)); //Reads digital input channel and prints in console
            //K8055.CloseDevice(); //Closes communication with the K8055
        }

    }
}
