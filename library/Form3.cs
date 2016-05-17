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
    public partial class Form3 : Form
    {
        public static String serial_com, string_connection;
        Thread thread;
        public static int result = 3; 
        public static bool running = true;
        SerialPort serial_port;
        public static String receive = "";
        public static int return_id = 0;
        public Form3()
        {
            InitializeComponent();
        }

        public void run()
        {
            int cnt = 0;
            while(running)
            {
                try
                {
                    cnt += 1;
                    if(cnt == 1)
                    {
                        txt_log.Text = "카드 인식을 위한 준비중 입니다\n잠시만 기달려 주세요";
                        serial_port = new SerialPort();
                        try
                        {
                            serial_port.PortName = serial_com;
                            serial_port.BaudRate = 9600;
                            serial_port.Open();
                        }
                        catch(Exception e)
                        {
                            running = false;
                            MessageBox.Show("문제가 생겼습니다\n관리자에게 문의하세요:(\n" + e);
                            save_log(e);
                        }
                    }
                    else if(cnt == 2)
                    {
                        txt_log.Text = "곧 카드 인식을 시작할 예정이니 태그할 카드를 올려놓아 주세요"; 
                        serial_port.Write("1");
                        receive = serial_port.ReadLine().ToString();
                    }
                    else if(cnt == 6)
                    {
                        txt_log.Text = "카드 인식을 시작합니다";
                        serial_port.Write("2");
                        receive = serial_port.ReadLine().ToString();
                    }
                    else if(cnt == 7)
                    {
                        txt_log.Text = "카드 정보를 분석중입니다";
                        serial_port.Write("3");
                        if(serial_port!=null)
                        {
                            serial_port.Close();
                        }
                        db_check();
                        if (serial_port!=null)
                        {
                            serial_port.Close();
                        }
                    }
                    Thread.Sleep(1000);
                }
                catch(Exception e)
                {
                    running = false;
                    MessageBox.Show("문제가 생겼습니다\n관리자에게 문의하세요:(\n" + e);
                    save_log(e);
                }
            }
            this.Close();
        }
        public void save_log(Exception e)
        {

        }

        public void db_check()
        {
            try
            {
                int temp = 0;
                MySqlConnection mysql_connection = new MySqlConnection(string_connection);
                mysql_connection.Open();
                String sql = "select * from student where card='" + receive + "';";
                MySqlCommand command = new MySqlCommand(sql, mysql_connection);
                MySqlDataReader data = command.ExecuteReader();
                while (data.Read())
                {
                    temp = int.Parse("" + data["student_id"]);
                }
                if (temp == 0)
                {
                    result = 2;
                }
                else
                {
                    result = 1;
                }
                mysql_connection.Close();
                return_id = temp;
            }
            catch(Exception e)
            {
                running = false;
                MessageBox.Show("문제가 생겼습니다\n관리자에게 문의하세요:(\n" + e);
                save_log(e);
            }
            running = false;
        }

        public int check_student()
        {
            running = true;

            thread = new Thread(run);
            thread.Start();

            txt_log.Text = serial_com;
            /*while(running)
            {

            }*/
            return result;
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            running = true;

            thread = new Thread(run);
            thread.Start();

            txt_log.Text = serial_com;
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
