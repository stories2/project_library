using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using System.Threading;
using MySql.Data.MySqlClient;

namespace library
{
    public partial class Form1 : Form
    {
        Thread thread;
        bool running;
        SerialPort serial_port;
        MySqlConnection connection;
        Form2 main_program;

        public Form1()
        {
            InitializeComponent();
        }

        public void run()
        {
            String port_name = "COM";
            int cnt = 0;
            String string_connection = "";
            bool device_found = false;
            Thread.Sleep(1000);
            while(running)
            {
                try
                {
                    if(device_found == false && cnt<=1)
                    {
                        try
                        {
                            cnt += 1;
                            txt_log.Text = "Try to open COM" + cnt + "...";
                            Thread.Sleep(500);
                            serial_port.PortName = port_name + cnt;
                            serial_port.BaudRate = 9600;
                            serial_port.Open();
                            txt_log.Text = "Try to open COM" + cnt + "...OK";
                            Thread.Sleep(500);
                            device_found = true;
                        }
                        catch(Exception e)
                        {
                            txt_log.Text = "Try to open COM" + cnt + "...Fail";
                            Thread.Sleep(500);
                        }
                    }
                    else
                    {
                        if(serial_port != null)
                        {
                            serial_port.Close();
                        }
                        try
                        {
                            txt_log.Text = "Connecting to DB...";
                            Thread.Sleep(500);
                            string_connection = "Server=118.32.81.186;Database=stories2;Uid=stories2;Pwd=toortoor%^%;";
                            connection = new MySqlConnection(string_connection);
                            txt_log.Text = "Connecting to DB...OK";
                            Thread.Sleep(500);
                        }
                        catch(Exception e)
                        {
                            txt_log.Text = "Connecting to DB...Fail";
                            Thread.Sleep(500);
                        } 
                        if (connection != null)
                        {
                            connection.Close();
                        }
                        running = false;
                        break;
                    }
                    Thread.Sleep(1);
                }
                catch(Exception e)
                {
                    MessageBox.Show("error : " + e);
                }
            }
            main_program = new Form2();
            Form2.serial_com = port_name + cnt;
            Form2.string_connection = string_connection;
            main_program.ShowDialog();
            this.Close();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            running = true;

            serial_port = new SerialPort();

            thread = new Thread(run);
            thread.Start();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }
    }
}
