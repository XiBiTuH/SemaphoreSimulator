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
using System.Diagnostics;
using System.Windows.Threading;

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
         *
         *  [ ] < ----- Car           sensor (Input Digital 1)
         *  [ ] < ----- Car           sensor (Input Digital 2)
         *  [ ] < ----- Pawn button   sensor (Input Digital 3)
         *  [ ] < ----- Pawn button   sensor (Input Digital 4)
         *  [ ] < ----- Police button sensor (Input Digital 5)
         *
         *
         *
         * Cycle Flag
         * When the Flag is equal to 0, the semaphore B will be GREEN, A and D will be RED.
         * When the Flag is equal to 1, the semaphores A and D will be GREEN, B will be RED.
         */


        // Class variables
        int cycle_flag = -1;                                                       // Cicle flag
        bool pawn_ten_flag = false;                                                // Pawn  flag tag
        int mode = 1;                                                              // Default mode [1 -> normal | 2 -> automatic]         
        bool policia = false;                                                      // Call the police flag
        int valores_sensores = -1;                                                 // Sensors value at the moment
        DispatcherTimer timer_sensores;                                            // Timer 
        bool[] sensores_status = new bool[] { false, false, false, false };


        // Constants
        const int debungTime = 2;                                                  // Debug 
        const int CICLE_DEFAULT = -1;                                              // Cicle default constant
        const int PAWN_PRIORITY = 0;                                               // Pawn has priority constant
        const int CAR_PRIORITY = 1;                                                // Car has priority constant
        const int INTERMITENTE = 2;                                                // Intermitente


        //Colors variables
        SolidColorBrush verde = null;                                              // Green
        SolidColorBrush vermelho = null;                                           // Red
        SolidColorBrush amarelo = null;                                            // Yellow/Orange    
        SolidColorBrush branco = null;                                             // Nothing


        // Dashboard Class
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
            [DllImport("K8055D.dll")]
            public static extern void ClearAllDigital();
            [DllImport("K8055D.dll")]
            public static extern int ReadAllDigital();
        }


        // This function will run when the program initiates. ONLY
        public MainWindow()
        {

            // Colors
            this.verde = new SolidColorBrush(Colors.Green);
            this.vermelho = new SolidColorBrush(Colors.Red);
            this.amarelo = new SolidColorBrush(Colors.Orange);
            this.branco = new SolidColorBrush(Colors.White);


            // Timer Set-Up
            timer_sensores = new DispatcherTimer();                 // Initialize timers
            timer_sensores.Interval = TimeSpan.FromSeconds(15);     // Set the period time
            timer_sensores.Tick += sensores;                        // Set up the method
            timer_sensores.Start();                                 // Start the timer            


            // Initialize the components
            InitializeComponent();

            // Open the device
            K8055.OpenDevice(0);

            // Default mode is 1, so it'll be normal mode and 
            maintence(-1);
        }


        // Sensors listener
        private void sensores(object sender, EventArgs e)
        {

            // Normal mode
            if (mode == 1)
            {
                if (K8055.ReadDigitalChannel(5) || check_Time())                   // Time/Sensor check
                {
                    timer_sensores.Interval = TimeSpan.FromSeconds(4);             // Change the span time
                    maintence(INTERMITENTE);                                       // Set the mode
                }
                else
                {
                    maintence(this.cycle_flag);                                    // Set the normal cycle
                    timer_sensores.Interval = TimeSpan.FromSeconds(15);            // Set the span time
                }
            }
            else
            {                                                                 // Sensor mode
                bool aux_sensor;


                if (K8055.ReadDigitalChannel(5) || check_Time())                   // Time/Police button check
                {
                    maintence(INTERMITENTE);                                       // Set the mode
                    valores_sensores = K8055.ReadAllDigital();                     // Update the variable
                    return;                                                        // End it here
                }
                if (valores_sensores != K8055.ReadAllDigital() && !pawn_ten_flag)  // If there's any change
                {

                    aux_sensor = false;                                         // Set aux sensor to false

                    for (int i = 0; i < 4; i += 2)                              // Loop in pair
                    {
                        int channel = i + 1;                                    // Get next channel

                        if (sensores_status[i] && sensores_status[i + 1])       // Compare variables, if it was a True | True
                            aux_sensor = true;                                  // Update our auxiliar variable

                        if (sensores_status[i] != K8055.ReadDigitalChannel(channel))  // Compare channel
                            sensores_status[i] = !sensores_status[i];                 // Update variables  

                        if (sensores_status[i + 1] != K8055.ReadDigitalChannel(channel + 1))  // Compate channel
                            sensores_status[i + 1] = !sensores_status[i + 1];                 // Update variables


                        if (sensores_status[i] != sensores_status[i + 1] && !aux_sensor)    // There's at least one car
                        {
                            // Cars
                            if (i == 0)
                            {
                                // Cars go by
                                if (cycle_flag != 0)
                                    maintence(CAR_PRIORITY); // Car got priority
                            }
                            else
                            {
                                // Pawns go by
                                if (cycle_flag != 1)
                                    maintence(PAWN_PRIORITY); // Pawn got priority
                            }
                        }
                        else if (!sensores_status[i] && !sensores_status[i + 1]) //   False | False sensors
                        {
                            // Cars 
                            if (i == 0)
                            {
                                // Pawn go by
                                if (cycle_flag != 1)
                                    maintence(PAWN_PRIORITY); // Pawn got priority
                            }
                        }
                    }
                    valores_sensores = K8055.ReadAllDigital();  // Update sensors total value
                }
                else    // If there're no changes
                {
                    if (cycle_flag != 1)
                    {
                        bool[] false_arr = new bool[] { false, false, false, false, false };
                        int arr1TrueCount = false_arr.Count(b => b);
                        int arr2TrueCount = sensores_status.Count(b => b);

                        if (arr1TrueCount == arr2TrueCount)
                        {
                            maintence(0);
                        }
                    }
                }

            }
        }


        // Clear semaphores
        private void clearAllSemaphores()
        {
            B_verde_1.Fill = branco;
            B_verde_2.Fill = branco;
            B_vermelho_1.Fill = branco;
            B_vermelho_2.Fill = branco;
            B_amarelo_1.Fill = branco;
            B_amarelo_2.Fill = branco;

            A_verde_1.Fill = branco;
            A_verde_2.Fill = branco;
            A_vermelho_1.Fill = branco;
            A_vermelho_2.Fill = branco;
            A_amarelo_1.Fill = branco;
            A_amarelo_2.Fill = branco;

            D_verde.Fill = branco;
            D_vermelho.Fill = branco;
        }


        // Sempahores management
        public async void maintence(int flag)
        {
            // Switch case (flag)
            switch (flag)
            {
                case CICLE_DEFAULT:                 // This is the INITIAL MOMENT!
                    K8055.ClearAllDigital();        // Clear the digital values
                    clearAllSemaphores();           // Clear semaphores 
                    this.cycle_flag = 0;            // Update flag --> 0

                    K8055.SetDigitalChannel(4);   // Semaphore B --> GREEN
                    B_verde_1.Fill = B_verde_2.Fill = verde; // Color

                    K8055.SetDigitalChannel(3);   // Semaphore A --> RED
                    A_vermelho_1.Fill = A_vermelho_2.Fill = vermelho; // Color

                    K8055.SetDigitalChannel(8);   // Semaphore D --> RED
                    D_vermelho.Fill = vermelho;   // Color

                    await Task.Delay(5000);  // Delay 5 seconds
                    break;                   // break l

                case PAWN_PRIORITY:              // In this particular case, we need to consider some situations [B IS GREEN | OTHERS ARE RED]
                    this.cycle_flag = 1;          // Update flag --> 1
                    pawn_ten_flag = true;         // Pawns got priority, so update flag
                    await Task.Delay(5000);       // Flag
                    K8055.ClearDigitalChannel(4); // Clear the channel
                    B_verde_1.Fill = B_verde_2.Fill = branco; // Color

                    K8055.SetDigitalChannel(5);   // Turn the semaphore B yellow, the others stay RED
                    B_amarelo_1.Fill = B_amarelo_2.Fill = amarelo;

                    await Task.Delay(2000);       // Delay of 1.2 seconds
                    K8055.ClearDigitalChannel(5); // Clear the channel 
                    B_amarelo_2.Fill = B_amarelo_1.Fill = branco; // Color

                    K8055.SetDigitalChannel(6);   // Turn the semaphore B RED, the others stay RED
                    B_vermelho_1.Fill = B_vermelho_2.Fill = vermelho; // Color

                    await Task.Delay(2000);       // Delay of 1.5 seconds

                    K8055.ClearDigitalChannel(3); // Clear the channel
                    A_vermelho_1.Fill = A_vermelho_2.Fill = branco; // Color

                    K8055.SetDigitalChannel(1);   // Semaphore A --> GREEN
                    A_verde_1.Fill = A_verde_2.Fill = verde; // Color

                    K8055.ClearDigitalChannel(8); // Clear the channel
                    D_vermelho.Fill = branco;     // Color

                    K8055.SetDigitalChannel(7);   // Semaphore D --> GREEN
                    D_verde.Fill = verde;         // Color
                    if (mode == 2)                // Aumatic mode
                        await Task.Delay(6000);   // Delay of 6 seconds
                    else                          // Normal mode
                        await Task.Delay(15000);  // Delay of 15 seconds

                    pawn_ten_flag = false;        // Pawn loose priority
                    break;                        // break

                case CAR_PRIORITY:// In this particular case, we need to consider some situations [B IS RED | OTHERS ARE GREEN]
                    if (pawn_ten_flag == false)
                    {
                        this.cycle_flag = 0;          // Update flag --> 0
                        await Task.Delay(5000);

                        K8055.ClearDigitalChannel(1); // Clear the channel
                        A_verde_1.Fill = A_verde_2.Fill = branco;

                        K8055.SetDigitalChannel(2);   // Turn the semaphore A yellow 
                        A_amarelo_1.Fill = A_amarelo_2.Fill = amarelo;

                        K8055.ClearDigitalChannel(7); // Clear the channel
                        D_verde.Fill = branco;

                        K8055.SetDigitalChannel(8);   // Turn the semaphore D red
                        D_vermelho.Fill = vermelho;

                        await Task.Delay(2000);       // Delay of 1.2 seconds

                        K8055.ClearDigitalChannel(2); // Clear the channel
                        A_amarelo_1.Fill = A_amarelo_2.Fill = branco;

                        K8055.SetDigitalChannel(3);   // Semaphore A --> RED
                        A_vermelho_1.Fill = A_vermelho_2.Fill = vermelho;

                        await Task.Delay(2000);       // Delay of 1.5 seconds

                        K8055.ClearDigitalChannel(6); // Clear the channel
                        B_vermelho_1.Fill = B_vermelho_2.Fill = branco;

                        K8055.SetDigitalChannel(4);   // Semaphore B --> GREEN
                        B_verde_1.Fill = B_verde_2.Fill = verde;
                        await Task.Delay(15000);
                    }
                    break;
                case INTERMITENTE:
                    this.cycle_flag = -1;
                    K8055.ClearAllDigital();
                    clearAllSemaphores();

                    await Task.Delay(2000);       // Delay of 2
                    K8055.SetDigitalChannel(5);
                    B_amarelo_1.Fill = B_amarelo_2.Fill = amarelo;

                    K8055.SetDigitalChannel(2);
                    A_amarelo_1.Fill = A_amarelo_2.Fill = amarelo;

                    await Task.Delay(2000);       // Delay of 2

                    if (K8055.ReadDigitalChannel(5) || cycle_flag == -1)
                    {
                        clearAllSemaphores();
                        K8055.ClearAllDigital();
                    }
                    break;

                default:                          // Blink the semaphores A and B, D must have nothing
                    K8055.ClearAllDigital();
                    await Task.Delay(2000);       // Delay of 2
                    K8055.SetDigitalChannel(5);
                    K8055.SetDigitalChannel(2);
                    await Task.Delay(2000);       // Delay of 2
                    K8055.ClearAllDigital();
                    break;
            }

        }

        // Returns true
        public bool check_Time()
        {
            String time = DateTime.Now.ToString("HH:mm:ss");
            if (DateTime.Parse(time) > DateTime.Parse("00:00:00") && DateTime.Parse(time) < DateTime.Parse("06:00:00"))
                return true;
            return false;
        }

        //Botão de Modo Normal
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mode = 1;
            timer_sensores.Interval = TimeSpan.FromSeconds(15);
        }

        //Botõ de Modo Automático
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            mode = 2;
            timer_sensores.Interval = TimeSpan.FromSeconds(5);
        }
    }
}
