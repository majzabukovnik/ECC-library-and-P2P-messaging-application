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
using Ecc;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Timers;
using System.Threading;
using System.Windows.Threading;

namespace EccGuiWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Timers.Timer timer = new System.Timers.Timer();
        string name = "Neznanec";
        string IP = "";

        Dispatcher dispUI;
        DiffieHelman diffieHelman1 = new DiffieHelman(33);
        DiffieHelman diffieHelman2 = new DiffieHelman(55);

        bool firstTime = true;
        int selectedIndex = 0;

        DiffieHelman.Listen ff;
        public MainWindow()
        {
            dispUI = Dispatcher.CurrentDispatcher;
            //MessageBox.Show(GetIP());
            ff = new DiffieHelman.Listen(diffieHelman1, GetIP());
            InitializeComponent();
            ff.StartListening();
            string last = "";
            Task.Run(() =>
            {
            while (true)
            {
                    if (last != ff.GetLastMessage())
                    {
                        if (ff.GetLastMessage().StartsWith("//?IP"))
                        {
                            string ip = ff.IP;
                            foreach (var item in welcome.saved.Items)
                            {
                                if(item.ToString().Contains(ip))
                                {
                                    dispUI.BeginInvoke(new Action(() => name = item.ToString().Split(" ")[0]));
                                }                                
                            }
                            
                        }
                        else if (ff.GetLastMessage().StartsWith("//?exit"))
                        {
                            dispUI.BeginInvoke(new Action(() => ff.StopMessaging()));
                            dispUI.BeginInvoke(new Action(() => chat.listbox.Items.Clear()));
                            dispUI.BeginInvoke(new Action(() => welcome.Visibility = Visibility.Visible));
                            dispUI.BeginInvoke(new Action(() => chat.Visibility = Visibility.Hidden));

                        }
                        else
                            dispUI.BeginInvoke(new Action(() => chat.listbox.Items.Add(name + ": " + ff.GetLastMessage())));
                        dispUI.BeginInvoke(new Action(() => last = ff.GetLastMessage()));
                    }
                    Thread.Sleep(300);
                }
            });
            timer.Interval = 200;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();

            welcome.btnAdd.Click += BtnAdd_Click;
            welcome.btnConnect.Click += BtnConnect_Click;
            welcome.btnMore.Click += BtnMore_Click;
            welcome.btnSave.Click += BtnSave_Click;
            welcome.InfoButton.Click += InfoButton_Click;

            chat.btnSend.Click += BtnSend_Click;
            chat.btnExit.Click += BtnExit_Click;
            chat.InfoButtonChat.Click += BtnExit_Click1;

            
        }

        private void BtnExit_Click1(object sender, RoutedEventArgs e)
        {
            infoChat infoChat = new infoChat();
            infoChat.Show();
        }

        private void InfoButton_Click(object sender, RoutedEventArgs e)
        {
            info info = new info();
            info.Show();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (welcome.TextBox_Name.Text != "" && welcome.TextBox_Ip.Text != "")
            {
                welcome.saved.Items[selectedIndex] = welcome.TextBox_Name.Text + " " + welcome.TextBox_Ip.Text;
                welcome.TextBox_Name.Text = "";
                welcome.TextBox_Ip.Text = "";

                string save = "";
                foreach(string i in welcome.saved.Items)
                {
                    save += i.ToString() + "\n";
                }
                File.WriteAllText("saved.txt", save);
            }

        }

        private void BtnMore_Click(object sender, RoutedEventArgs e)
        {
            if(welcome.saved.SelectedIndex != -1)
            {
                string[] info = welcome.saved.SelectedItem.ToString().Split(" ");
                welcome.TextBox_Name.Text = info[0];
                welcome.TextBox_Ip.Text = info[1];
                selectedIndex = welcome.saved.SelectedIndex;
            }
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            if (ff.ConnectionMade)
            {
                ff.SendMessage("//?exit");
                ff.StopMessaging();
                chat.listbox.Items.Clear();
                welcome.Visibility = Visibility.Visible;
                chat.Visibility = Visibility.Hidden;

            }
            else if (diffieHelman1.Connecter.ConnectionMade)
            {
                diffieHelman1.Connecter.SendMessage("//?exit");
                diffieHelman1.Connecter.StopMessaging();
                chat.listbox.Items.Clear();
                welcome.Visibility = Visibility.Visible;
                chat.Visibility = Visibility.Hidden;

            }

        }

        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {

            if (ff.ConnectionMade)
            {
                ff.SendMessage(chat.polje.Text);
                chat.listbox.Items.Add("Jaz: " + chat.polje.Text);
            }
            else if (diffieHelman1.Connecter.ConnectionMade)
            {                
                diffieHelman1.Connecter.SendMessage(chat.polje.Text);
                chat.listbox.Items.Add("Jaz: " + chat.polje.Text);
            }
            chat.polje.Clear();
        }

        private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (ff.ConnectionMade)
            {
                dispUI.BeginInvoke(new Action(() => welcome.Visibility = Visibility.Hidden));
                dispUI.BeginInvoke(new Action(() => chat.Visibility = Visibility.Visible));
                
            }
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (welcome.saved.SelectedItem.ToString() == "") return;
            string[] item = welcome.saved.SelectedItem.ToString().Split(" ");
            name = item[0];
            IP = item[1];
            welcome.Visibility = Visibility.Hidden;
            chat.Visibility = Visibility.Visible;

            string last = "";
            Random random = new Random();
            int a = random.Next(2, 60);
            ECC eccBob = new ECC(2, 2, 5, 1, 17, a);

            diffieHelman1.Connecter.ConnectTo(eccBob, IP);
            diffieHelman1.Connecter.SendMessage("//?IP");
            if (firstTime)
            {


                Task.Run(() =>
                {
                    while (true)
                    {
                        if (last != diffieHelman1.Connecter.GetLastMessage())
                        {
                            if (diffieHelman1.Connecter.GetLastMessage().StartsWith("//?exit"))
                            {
                                dispUI.BeginInvoke(new Action(() => diffieHelman1.Connecter.StopMessaging()));
                                dispUI.BeginInvoke(new Action(() => chat.listbox.Items.Clear()));
                                dispUI.BeginInvoke(new Action(() => welcome.Visibility = Visibility.Visible));
                                dispUI.BeginInvoke(new Action(() => chat.Visibility = Visibility.Hidden));

                            }

                            dispUI.BeginInvoke(new Action(() => chat.listbox.Items.Add(name + ": " + diffieHelman1.Connecter.GetLastMessage())));
                            dispUI.BeginInvoke(new Action(() => last = diffieHelman1.Connecter.GetLastMessage()));

                        }
                        Thread.Sleep(300);
                    }
                });
            }

            firstTime = false;

        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if(welcome.TextBox_Name.Text!="" && welcome.TextBox_Ip.Text != "")
            {
                File.AppendAllText("saved.txt", welcome.TextBox_Name.Text + " " + welcome.TextBox_Ip.Text + "\n");
                welcome.saved.Items.Add(welcome.TextBox_Name.Text + " " + welcome.TextBox_Ip.Text);
            }
            
        }

        private void welcome_Loaded(object sender, RoutedEventArgs e)
        {
   

            if (File.Exists("saved.txt"))
            {
                string[] lines = File.ReadAllLines("saved.txt");
                for (int i = 0; i<lines.Length; i++)
                {
                    welcome.saved.Items.Add(lines[i]);
                }
            }
            
        }

        string GetIP()
        {
            var n = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var i in n)
            {
                if (i.AddressFamily == AddressFamily.InterNetwork)
                {
                    return i.ToString();
                }
            }
            return null;
        }



    }
}
