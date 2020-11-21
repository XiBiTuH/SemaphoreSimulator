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
         */



        /*
         * Cycle Flag
         * When the Flag is equal to 0, the semaphore B will be GREEN, A and D will be RED.
         * When the Flag is equal to 1, the semaphores A and D will be GREEN, B will be RED.
         */
        int cycle_flag = -1;
        int mode = 1;   //Default 1
        bool policia = false;
        DispatcherTimer tim;
        int valores_sensores = -1;
        DispatcherTimer timer_sensores;
        DispatcherTimer tim_policia;
        bool[] sensores_status = new bool[] { false, false, false, false, false };


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



            //Timer relativamente ao semáforo temporário - corre a cada 20 segundos
            tim = new DispatcherTimer();
            tim.Interval = TimeSpan.FromSeconds(20);
            tim.Tick += semaforo_temporario;
            //tim.Start();
            

            //Timer relativamente á policia - Intermitente
            tim_policia = new DispatcherTimer();
            tim_policia.Interval = TimeSpan.FromSeconds(4);
            tim_policia.Tick += policia_intermitente;



            //Timer que está sempre a verificar as horas - corre a cada segundo
            DispatcherTimer timer_verificar_horas = new DispatcherTimer();
            timer_verificar_horas.Interval = TimeSpan.FromSeconds(1);
            timer_verificar_horas.Tick += verifica_horas;
            timer_verificar_horas.Start();



            //Verifica se houve alguma alteração nos sensores
            timer_sensores = new DispatcherTimer();
            timer_sensores.Interval = TimeSpan.FromSeconds(5);
            timer_sensores.Tick += sensores;
            timer_sensores.Start();


            InitializeComponent();
            // Here, we just need to open the device
            // Open communication with K8055 that has the device address 0
            K8055.OpenDevice(0);
            maintence(-1);


        }

        private void policia_intermitente(object sender, EventArgs e)
        {
            maintence(2);
        }



        /*
                            //Caso já esteja verde para os peoes
                            if (this.cycle_flag != 1)
                            
                                tim.Interval = TimeSpan.FromSeconds(5);

                                this.cycle_flag = 0;

                            }

                            if (mode == 2)
                            {
                                timer_sensores.Stop();
                                maintence(0);
                                timer_sensores.Start();
                                valores_sensores = 0;
                            }


                            break;
                            */


        //Listener dos sensores
        private void sensores(object sender, EventArgs e)
        {
            bool aux_sensor;

            Console.WriteLine("OLHA RAPOSO CHUPA TU");

            if (valores_sensores != K8055.ReadAllDigital())
            {

                aux_sensor = false;

                for (int i=1;i < 4; i+=2)
                {

                    if(sensores_status[i] && sensores_status[i + 1])
                    {
                        aux_sensor = true;
                    }

                    Debug.WriteLine("------------");
                    Debug.WriteLine("xp");

                    if (sensores_status[i] != K8055.ReadDigitalChannel(i))
                        sensores_status[i] = !sensores_status[i];
                    

                    if (sensores_status[i + 1] != K8055.ReadDigitalChannel(i+1))
                        sensores_status[i + 1] = !sensores_status[i + 1];


                    Debug.WriteLine(sensores_status[i]);
                    Debug.WriteLine(sensores_status[i + 1]);


                    

                    if (sensores_status[i] != sensores_status[i + 1] && !aux_sensor) // Existem carros em 1 dos lados
                    {
                        Debug.WriteLine("45");

                        if (i == 1)
                        {
                            //Se estivermos no modo automatico
                            if(mode == 2)
                            {

                                //Abre os automoveis
                                Debug.WriteLine("Abri os automoveis");
                                Debug.WriteLine("------------");
                                if(cycle_flag != 0)
                                    maintence(1);

                            }

                        }
                        else
                        {
                            //Abre os peões
                            if(mode == 1)
                            {

                            }
                            else
                            {
                                if(cycle_flag != 1)
                                    maintence(0);
                            }

                        }

                    }
                    else if(!sensores_status[i] && !sensores_status[i+1]) // false/false
                    {
                        if(i == 1)
                        {

                            if(mode == 2)
                            {
                                Debug.WriteLine("Fechei os automoveis");
                                if(cycle_flag != 1)
                                    maintence(0);
                            }
                                
                        }

                        if(i == 3)
                        {

                        }
                    }


                }
            }
            else
            {
                if(cycle_flag != 1)
                {

                    bool[] false_arr = new bool[] { false, false, false, false, false, false };
                    int arr1TrueCount = false_arr.Count(b => b);
                    int arr2TrueCount = sensores_status.Count(b => b);

                    // Verificar o meu array
                    if (arr1TrueCount == arr2TrueCount)
                    {
                        Console.WriteLine("OLHA PEDRO CHUPA TU");
                        maintence(0);
                    }
                }
            }




                valores_sensores = K8055.ReadAllDigital();
            }





            /*
            //Verificar se houve alteração nos sensores
            if (valores_sensores != K8055.ReadAllDigital())
            {
                switch (K8055.ReadAllDigital())
                {

                    //Meter os peoões com prioridade ( Nenhum carro )
                    case 0:
                        if (this.cycle_flag != 1)
                        {
                            maintence(0);
                        }
                        this.cycle_flag = 0;
                        break;


                      
                    //Meter o sinal B verde para os automoveis
                    case 1:
                    case 2:
                    case 3:

                        
                        if(valores_sensores == 0)
                        {
                            maintence(1);
                        }
                        this.cycle_flag = 1;
                        break;

                }
                valores_sensores = K8055.ReadAllDigital();
            }
            

        }*/

        
        //Verificamos as horas
        private void verifica_horas(object sender, EventArgs e)
        {

            
        }


        //Every 20 seconds this function occurs
        private void semaforo_temporario(object sender, EventArgs e)
        {
            if (mode == 1)
            {
                maintence(this.cycle_flag);
            }

        }


        /*
         * 
         */
        public async void maintence(int flag)
        {
           // tim.Interval = TimeSpan.FromSeconds(20);

            switch (flag)
            {
                case -1:                          // This is the INITIAL MOMENT!
                    this.cycle_flag = 0;          // Update flag --> 0
                    K8055.SetDigitalChannel(4);   // Semaphore B --> GREEN
                    K8055.SetDigitalChannel(3);   // Semaphore A --> RED
                    K8055.SetDigitalChannel(8);   // Semaphore D --> RED
                    
                    await Task.Delay(5000);
                    break;

                case  0:                          // In this particular case, we need to consider some situations [B IS GREEN | OTHERS ARE RED]
                    this.cycle_flag = 1;          // Update flag --> 1
                    await Task.Delay(5000);
                    Console.WriteLine("OLHA CHUPAI");
                    K8055.ClearDigitalChannel(4); // Clear the channel
                    K8055.SetDigitalChannel(5);   // Turn the semaphore B yellow, the others stay RED
                    await Task.Delay(2000);       // Delay of 1.2 seconds
                    K8055.ClearDigitalChannel(5); // Clear the channel 
                    K8055.SetDigitalChannel(6);   // Turn the semaphore B RED, the others stay RED
                    await Task.Delay(2000);       // Delay of 1.5 seconds
                    K8055.ClearDigitalChannel(3); // Clear the channel
                    K8055.SetDigitalChannel(1);   // Semaphore A --> GREEN
                    K8055.ClearDigitalChannel(8); // Clear the channel
                    K8055.SetDigitalChannel(7);   // Semaphore D --> GREEN
                    await Task.Delay(10000);
                    break;

                case  1:                          // In this particular case, we need to consider some situations [B IS RED | OTHERS ARE GREEN]
                    this.cycle_flag = 0;          // Update flag --> 0
                    await Task.Delay(5000);
                    K8055.ClearDigitalChannel(1); // Clear the channel
                    K8055.SetDigitalChannel(2);   // Turn the semaphore A yellow 
                    K8055.ClearDigitalChannel(7); // Clear the channel
                    K8055.SetDigitalChannel(8);   // Turn the semaphore D red
                    await Task.Delay(2000);       // Delay of 1.2 seconds
                    K8055.ClearDigitalChannel(2); // Clear the channel
                    K8055.SetDigitalChannel(3);   // Semaphore A --> RED
                    await Task.Delay(2000);       // Delay of 1.5 seconds
                    K8055.ClearDigitalChannel(6); // Clear the channel
                    K8055.SetDigitalChannel(4);   // Semaphore B --> GREEN
                    
                    await Task.Delay(20000);
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


        public bool check_Time()
        {
            String time = DateTime.Now.ToString("HH:mm:ss");
            if(DateTime.Parse(time) > DateTime.Parse("00:00:00") && DateTime.Parse(time) < DateTime.Parse("06:00:00"))
                return true;
            return false;
        }


        //Botão de Modo Normal
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //K8055.CloseDevice(); //Closes communication with the K8055
            mode = 1;
            tim.Start();
            timer_sensores.Stop();
        }

        
        //Botão da policia
        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {

            policia = !policia;

            //Fica intermitente
            if (policia)
            {
                tim_policia.Start();
                maintence(2);
                timer_sensores.Stop();
                tim.Stop();


            }

            //Caso o botão seja desligado
            else
            {
                
                tim_policia.Stop();
                await Task.Delay(4000);


                if (mode == 1)
                {
                    maintence(-1);
                    tim.Start();
                    
                }

                else
                {
                    maintence(-1);
                    timer_sensores.Start();
                    valores_sensores = -2;

                }
            }
    


        }


        //Botão de Peão
        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {

            

            

            
        }


        //Botõ de Modo Automático
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            mode = 2;
            timer_sensores.Start();
            //tim.Stop();
            //maintence(0);


        }
    }
}
