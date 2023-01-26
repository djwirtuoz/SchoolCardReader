using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SchoolCardReader
{
    public partial class Form1 : Form
    {
        SerialPort port = new SerialPort();
        bool isConnected = false;
        string all_num;
        string family;
        string number;
        public Form1()
        {
            InitializeComponent();

            port.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            fam_textB.Invoke(
                (ThreadStart)delegate ()
                {
                    all_num = port.ReadExisting();
                    ConvertData();
                });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            port.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if(port_comboBox.Text == "")
            {
                btn1.Enabled= false;
            }

            try
            {
                port_comboBox.Items.Clear();
                // Получаем список COM портов доступных в системе
                string[] portnames = SerialPort.GetPortNames();
                // Проверяем есть ли доступные
                if (portnames.Length == 0)
                {
                    MessageBox.Show("COM PORT not found");
                }
                foreach (string portName in portnames)
                {
                    //добавляем доступные COM порты в список           
                    port_comboBox.Items.Add(portName);
                    Console.WriteLine(portnames.Length);
                    if (portnames[0] != null)
                    {
                        port_comboBox.SelectedItem = portnames[0];
                    }
                }
                btn1.Enabled = true;

                string selectedPort = port_comboBox.GetItemText(port_comboBox.SelectedItem);
                port.PortName = selectedPort;
                port.BaudRate = 115200;
            }
            catch { }
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                isConnected = true;
                port.Open();
                btn1.Text = "Стоп";
                label5.Text = "Поднесите карту";
            }
            else
            {
                isConnected = false;
                port.Close();
                btn1.Text = "Старт";
                label5.Text = "Считывание остановлено";
            }
        }

        public void ConvertData()
        {
            if (all_num != null && all_num != "")
            {
                all_num = all_num.Trim();
                family = ReverseString(all_num);
                family = family.Substring(4);
                family = ReverseString(family); //итоговое семейство, обрезано четыре символа с конца
                int family_DEC = Convert.ToInt32(family, 16);
                fam_textB.Text = family_DEC.ToString();

                number = ReverseString(all_num);
                number = number.Substring(0, number.Length - family.Length);
                number = ReverseString(number); //итоговое семейство, обрезано четыре символа с конца
                int number_DEC = Convert.ToInt32(number, 16);
                num_textB.Text = number_DEC.ToString();
            }
        }

        public static string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
    }
}
