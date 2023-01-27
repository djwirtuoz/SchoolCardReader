using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

namespace SchoolCardReader
{
    public partial class Form1 : Form
    {
        SerialPort port = new SerialPort();
        bool isConnected = false;
        string all_num; //HEX all numm
        string family;  //family numm DEC
        string number;  //number numm DEC
        string verify_reader;
        public Form1()
        {
            InitializeComponent();

            port.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);    //подключение события получения данных
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
            Search_dev();
        }

        private void Search_dev()
        {
            btn1.Enabled = false;
            try
            {
                port_comboBox.Items.Clear();
                // Получаем список COM портов доступных в системе
                string[] portnames = SerialPort.GetPortNames();
                // Проверяем есть ли доступные
                if (portnames.Length == 0)
                {
                    label5.Text = "Устройство не найдено";
                    btn1.Enabled = false;
                    timer1.Enabled = true;
                }
                else
                {
                    for (int x = 0; x == portnames.Length; x++)
                    {
                        //добавляем доступные COM порты в список           
                        port_comboBox.Items.Add(portnames[x]);
                        if (portnames[x] != null)
                        {
                            port.Open();
                            port.Write("reader");    //пишем в порт данные для проверки на правильность считыателя

                            fam_textB.Invoke(
                                (ThreadStart)delegate ()
                                {
                                    verify_reader = port.ReadExisting();
                                });

                            if (verify_reader == "reader")
                            {
                                port_comboBox.SelectedItem = portnames[x];
                                label5.Text = "Устройство готово";
                                btn1.Enabled = true;
                                timer1.Enabled = false;
                            }
                            port.Close();
                        }
                    }
                }

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

        private void label5_TextChanged(object sender, EventArgs e)
        {
            if(label5.Text == "Поднесите карту")
            {
                label5.ForeColor = Color.Green;
            }

            if (label5.Text == "Считывание остановлено")
            {
                label5.ForeColor = Color.Red;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Search_dev();
        }
    }
}
