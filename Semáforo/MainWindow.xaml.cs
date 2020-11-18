using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
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

        /*
         * 
         *  [ ] < ----- 8 (Red    of Semaphore D)
         *  [ ] < ----- 7 (Green  of Semaphore D)
         *  [ ] < ----- 6 (Red    of Semaphore B)
         *  [ ] < ----- 5 (Yellow of Semaphore B)
         *  [ ] < ----- 4 (Green  of Semaphore B)
         *  [ ] < ----- 3 (Red    of Semaphore A)
         *  [ ] < ----- 2 (Yellow of Semaphore A)
         *  [ ] < ----- 1 (Green  of Semaphore A)
         *
         */



        /*
         * Cycle Flag
         * When the Flag is equal to 0, the semaphore B will be GREEN, A and D will be RED.
         * When the Flag is equal to 1, the semaphores A and D will be GREEN, B will be RED.
         */
        int cycle_flag = 0;

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
            [DllImport("K8055D.dll")]
            public static extern void ClearDigitalChannel(int Channel);
            [DllImport("K80055D.dll")]
            public static extern void ClearAllDigital();
        }

        // This function will run when the program initiates. ONLY
        public MainWindow()
        {
            InitializeComponent();
            // Here, we just need to open the device
            // Open communication with K8055 that has the device address 0
            K8055.OpenDevice(0);
            maintence(this.cycle_flag);
            
            //K8055.CloseDevice(); //Closes communication with the K8055
        }

        /*
         * 
         */
        public void maintence(int flag)
        {

            switch (flag)
            {
                case -1:                          // This is the INITIAL MOMENT!
                    K8055.SetDigitalChannel(4);   // Semaphore B --> GREEN
                    K8055.SetDigitalChannel(3);   // Semaphore A --> RED
                    K8055.SetDigitalChannel(8);   // Semaphore D --> RED
                    this.cycle_flag = 0;          // Update flag --> 0
                    //Thread.Sleep(20000);          // Delay 20 seconds
                    break;

                case  0:                          // In this particular case, we need to consider some situations [B IS GREEN | OTHERS ARE RED]
                    K8055.ClearDigitalChannel(4); // Clear the channel
                    K8055.SetDigitalChannel(5);   // Turn the semaphore B yellow, the others stay RED
                    Thread.Sleep(1200);          // Delay of 1.2 seconds
                    K8055.ClearDigitalChannel(5); // Clear the channel 
                    K8055.SetDigitalChannel(6);   // Turn the semaphore B RED, the others stay RED
                    Thread.Sleep(1500);           // Delay of 1.5 seconds
                    K8055.ClearDigitalChannel(3); // Clear the channel
                    K8055.SetDigitalChannel(1);   // Semaphore A --> GREEN
                    K8055.ClearDigitalChannel(8); // Clear the channel
                    K8055.SetDigitalChannel(7);   // Semaphore D --> GREEN
                    this.cycle_flag = 1;          // Update flag --> 1
                    //Thread.Sleep(20000);          // Delay 20 seconds
                    break;

                case  1:                          // In this particular case, we need to consider some situations [B IS RED | OTHERS ARE GREEN]
                    K8055.ClearDigitalChannel(4); // Clear the channel
                    K8055.SetDigitalChannel(5);   // Turn the semaphore A yellow 
                    K8055.ClearDigitalChannel(7); // Clear the channel
                    K8055.SetDigitalChannel(8);   // Turn the semaphore D red
                    //Thread.Sleep(1200);           // Delay of 1.2 seconds
                    K8055.ClearDigitalChannel(2); // Clear the channel
                    K8055.SetDigitalChannel(3);   // Semaphore A --> RED
                    //Thread.Sleep(1500);           // Delay of 1.5 seconds
                    K8055.ClearDigitalChannel(6); // Clear the channel
                    K8055.SetDigitalChannel(4);   // Semaphore B --> GREEN
                    this.cycle_flag = 0;          // Update flag --> 0
                    //Thread.Sleep(20000);          // Delay 20 seconds
                    break;

                default:                        // Blink the semaphores A and B, D must have nothing

                    break;
            }
            
        }


        public bool check_Time()
        {
            String time = DateTime.Now.ToString("HH:mm:ss");
            if(DateTime.Parse(time) < DateTime.Parse("06:00:00") && DateTime.Parse(time) > DateTime.Parse("00:00:00"))
                return true;
            return false;
        }

    }
}
