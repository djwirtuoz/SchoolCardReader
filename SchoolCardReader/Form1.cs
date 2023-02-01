using System;
using System.IO.Ports;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;
using SchoolCardReader.Properties;

namespace SchoolCardReader
{
    public partial class Form1 : Form
    {
        SerialPort port = new SerialPort();
        string selectedPort;
        bool isConnected = false;
        string command_pack;
        string command;
        string all_num; //HEX all numm
        string family;  //family numm DEC
        string number;  //number numm DEC
        public Form1()
        {
            InitializeComponent();

            port.DataReceived += new SerialDataReceivedEventHandler(OnDataReceived);    //подключение события получения данных
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            bool _continue = true;
            if (port.IsOpen)
            {
                while (_continue)
                {
                    try
                    {
                        string message = port.ReadLine();
                        command_pack = message;
                    }
                    catch (TimeoutException) { }
                    finally
                    {
                        _continue = false;
                        ConvertData();
                    }
                }
            }
            port.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (port.IsOpen)
            {
                e.Cancel = true; //cancel the fom closing
                Thread CloseDown = new Thread(new ThreadStart(CloseSerialOnExit)); //close port in new thread to avoid hang
                CloseDown.Start(); //close port in new thread to avoid hang
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Size = new Size(300, 176);
            pictureBox1.Image = Resources.idle_img;
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
                }
                else
                {
                    for (int x = 0; x < portnames.Length; x++)
                    {
                        //добавляем доступные COM порты в список           
                        port_comboBox.Items.Add(portnames[x]);
                        port_comboBox.SelectedItem = portnames[x];
                        selectedPort = port_comboBox.GetItemText(port_comboBox.SelectedItem);
                        port.PortName = selectedPort;
                        port.BaudRate = 115200;

                        if (portnames[x] != null)
                        {
                            btn1.Enabled = true;
                            port_comboBox.SelectedItem = portnames[x];
                            label5.Text = "Выбран порт " + portnames[x];
                        }
                    }
                }
            }
            catch { }
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            fam_textB.Text = "";
            num_textB.Text = "";
            pictureBox1.Image = Resources.idle_img;

            try
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
            catch { label5.Text = "Порт недоступен"; }
        }

        public void ConvertData()
        {
            if (command_pack != null && command_pack.Length > 0 && command_pack != "\r")
            {
                string[] parts = command_pack.Split('-');
                command = parts[0];
                all_num = parts[1];

                if (command == "125")
                {
                    pictureBox1.Image = Resources.rfid_img;

                    all_num = all_num.Trim();
                    if (all_num.Length == 5) { all_num = "0" + all_num; }
                    if (all_num.Length == 4) { all_num = "00" + all_num; }

                    all_num = all_num.Substring(0, 6);
                    family = all_num.Substring(0, 2);
                    int family_DEC = Convert.ToInt32(family, 16);
                    fam_textB.BeginInvoke((Action)delegate () { fam_textB.Text = family_DEC.ToString(); ; });

                    number = all_num.Substring(2, 4);
                    int number_DEC = Convert.ToInt32(number, 16);
                    num_textB.BeginInvoke((Action)delegate () { num_textB.Text = number_DEC.ToString(); ; });

                    port.Close();
                    isConnected = false;
                    btn1.BeginInvoke((Action)delegate () { btn1.Text = "Старт"; ; });
                    label5.BeginInvoke((Action)delegate () { label5.Text = "Считывание остановлено"; ; });
                }

                if(command == "135")
                {
                    pictureBox1.Image = Resources.nfc_img;

                    all_num = all_num.Substring(0, 8);
                    all_num = all_num.Trim();

                    string[] hex_chanks = all_num.Split(',');
                    Array.Reverse(hex_chanks);
                    string numm = "";

                    for (int i = 0; i < hex_chanks.Length; i++)
                    {
                        numm += hex_chanks[i];
                    }

                    if (numm.Length == 5) { numm = "0" + numm; }
                    if (numm.Length == 4) { numm = "00" + numm; }

                    family = numm.Substring(0, 2);
                    int family_DEC = Convert.ToInt32(family, 16);
                    fam_textB.BeginInvoke((Action)delegate () { fam_textB.Text = family_DEC.ToString(); ; });

                    number = numm.Substring(2, 4);
                    int number_DEC = Convert.ToInt32(number, 16);
                    num_textB.BeginInvoke((Action)delegate () { num_textB.Text = number_DEC.ToString(); ; });

                    port.Close();
                    isConnected = false;
                    btn1.BeginInvoke((Action)delegate () { btn1.Text = "Старт"; ; });
                    label5.BeginInvoke((Action)delegate () { label5.Text = "Считывание остановлено"; ; });
                }
            }
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

        private void CloseSerialOnExit()
        {
            try
            {
                port.Close(); //close the serial port
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); //catch any serial port closing error messages
            }
            this.Invoke(new EventHandler(NowClose)); //now close back in the main thread
        }

        private void NowClose(object sender, EventArgs e)
        {
            this.Close(); //now close the form
        }

        private void label5_Click(object sender, EventArgs e)
        {
            this.Size = new Size(475, 176);
        }

        private void sell_comport_Click(object sender, EventArgs e)
        {
            this.Size = new Size(300, 176);
        }
    }
}