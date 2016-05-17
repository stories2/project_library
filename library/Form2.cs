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
    public partial class Form2 : Form,IMessageFilter
    {
        Graphics graphic;
        public static String serial_com, string_connection;
        int width, height, frame_counter, frame_cycle, view_x, view_y, limit, seat_num, draw_counter, mouse_x, mouse_y, block_size;
        bool running,r_flag,click ;
        Thread thread;
        int[,] box_database,seat_db,seat_student;
        Font font;
        float metrics_x, metrics_y;
        MySqlConnection mysql_connection;
        MySqlCommand mysql_command;
        MySqlDataReader mysql_data;
        String card_tag;

        public Form2()
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
        }

        public bool PreFilterMessage(ref Message msg)
        {
            if(msg.Msg == 514)
            {
                click = true;
                return true;
            }
            return false;
        }


        public void run()
        {
            while(running)
            {
                try
                {
                    if(r_flag == true)
                    {
                        r_flag = false;
                        click = false;
                        /*
                         1 : 상단바
                         
                         
                         
                         */
                        init();

                        font = new System.Drawing.Font(new FontFamily("돋움"), ((float)height/900)*10, FontStyle.Bold);
                    }
                    if(click == true)
                    {
                        click = false;
                        mouse_x = Control.MousePosition.X;
                        mouse_y = Control.MousePosition.Y;
                        int i,t = 0;
                        for(i=1;i<=draw_counter;i+=1)
                        {
                            if(box_database[i,5] == 1 && box_database[i,6] == 1)
                            {
                                t += 1;
                                if (box_database[i, 1] <= mouse_x && mouse_x <= box_database[i, 1] + block_size && box_database[i, 2] <= mouse_y && mouse_y <= box_database[i, 2] + block_size)
                                {
                                    //MessageBox.Show("" + t + "번째 자리");
                                    break;
                                }
                            }
                        }

                        if(box_database[i,7] == 0)
                        {
                            if(MessageBox.Show("자리 선택을 취소하시겠습니까?","",MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {
                                int temp,id;
                                Form3 check_student = new Form3();
                                Form3.serial_com = serial_com;
                                Form3.string_connection = string_connection;
                                check_student.ShowDialog();
                                //temp = check_student.check_student();
                                while(true)
                                {
                                    if(Form3.running == false)
                                    {
                                        temp = Form3.result;
                                        id = Form3.return_id;
                                        card_tag = Form3.receive;
                                        break;
                                    }
                                }
                                //check_student.Close();
                                if(temp == 1)//ok
                                {
                                    //MessageBox.Show("1"+id);
                                    if(box_database[i,8] == id)
                                    {
                                        MessageBox.Show("자리 사용을 취소합니다");
                                        box_database[i, 9] = 0;
                                        box_database[i, 8] = 0;
                                        box_database[i, 7] = 1;
                                        frame_counter = 0;
                                    }
                                    else
                                    {
                                        MessageBox.Show("본인이 아닌경우에는 자리사용을 취소할 수 없습니다");
                                    }
                                }
                                else if(temp == 2)//newbee
                                {
                                    MessageBox.Show("등록하지 않은 학생은 자리를 취소할 권한이 없습니다");
                                }
                                else if(temp == 3)//error
                                {
                                    MessageBox.Show("문제가 생겼습니다\n관리자에게 문의하세요:(");
                                }
                            }
                        }
                        else
                        {
                            if (MessageBox.Show(""+t+"번째 자리를 선택하시겠습니까?", "", MessageBoxButtons.YesNo) == DialogResult.Yes)
                            {

                                int temp, id;
                                Form3 check_student = new Form3();
                                Form3.serial_com = serial_com;
                                Form3.string_connection = string_connection;
                                check_student.ShowDialog();
                                //temp = check_student.check_student();
                                while (true)
                                {
                                    if (Form3.running == false)
                                    {
                                        temp = Form3.result;
                                        id = Form3.return_id;
                                        card_tag = Form3.receive;
                                        break;
                                    }
                                }
                                //check_student.Close();
                                if (temp == 1)//ok
                                {
                                    MessageBox.Show("자리 사용을 시작합니다");
                                    box_database[i, 9] = 7200;
                                    box_database[i, 8] = id;
                                    box_database[i, 7] = 0;
                                    frame_counter = 0;
                                }
                                else if (temp == 2)//newbee
                                {
                                    MessageBox.Show("등록하지 않은 학생이므로 등록절차로 넘어갑니다");
                                    Form4 update_student = new Form4();
                                    //Form4.serial_com = serial_com;
                                    Form4.string_connection = string_connection;
                                    Form4.card_tag = card_tag;
                                    update_student.ShowDialog();
                                }
                                else if (temp == 3)//error
                                {
                                    MessageBox.Show("문제가 생겼습니다\n관리자에게 문의하세요:(");
                                }
                            }
                        }
                    }
                    if(frame_counter % frame_cycle == 0)
                    {
                        this.Invalidate();
                        on_draw();
                    }
                    frame_counter += 1;
                    Thread.Sleep(1);
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

        public void db_init()
        {
            try
            {
                int cnt = 0;

                mysql_connection = new MySqlConnection(string_connection);
                mysql_connection.Open();
                String sql = "select * from seat;";
                mysql_command = new MySqlCommand(sql, mysql_connection);
                mysql_data = mysql_command.ExecuteReader();
                while(mysql_data.Read())
                {
                    cnt+=1;
                    try
                    {
                        seat_db[cnt, 1] = int.Parse("" + mysql_data["empty"]);
                        seat_db[cnt, 2] = int.Parse("" + mysql_data["who_use"]);
                        seat_db[cnt, 3] = int.Parse("" + mysql_data["sec"]);
                    }
                    catch(Exception e)
                    {
                        running = false;
                        MessageBox.Show("문제가 생겼습니다\n관리자에게 문의하세요:(\n" + e);
                        save_log(e);
                    }
                }
                mysql_connection.Close();
            }
            catch(Exception e)
            {
                running = false;
                MessageBox.Show("문제가 생겼습니다\n관리자에게 문의하세요:(\n" + e);
                save_log(e);
            }
        }
        public void init()
        {
            db_init();
            int i, t;
            block_size = 100;
            for(i=1;i<=16*8;i+=1)
            {
                box_database[i, 1] = ((i - 1) % 16) * block_size;//x
                box_database[i, 2] = ((i - 1) / 16) * block_size + 20;//y
                box_database[i, 3] = block_size;//width
                box_database[i, 4] = block_size;//height
                box_database[i, 5] = 0;//is it seat? 0 : not 1 : seat
                box_database[i, 6] = 1;// 0 : fill , 1 : stroke
                box_database[i, 7] = 0;//empty? 0: not , 1 : yes
                box_database[i, 8] = 0;//who use?
            }
            box_database[i + 1, 1] = 0;
            box_database[i + 1, 2] = 0;
            box_database[i + 1, 3] = width;
            box_database[i + 1, 4] = 20;
            box_database[i + 1, 5] = 0;
            box_database[i + 1, 6] = 0;

            box_database[i + 2, 1] = 0;
            box_database[i + 2, 2] = height - block_size+20;
            box_database[i + 2, 3] = width;
            box_database[i + 2, 4] = block_size - 20;
            box_database[i + 2, 5] = 0;
            box_database[i + 2, 6] = 0;
            draw_counter = i + 2;

            box_database[18, 5] = 1;
            box_database[19, 5] = 1;
            box_database[22, 5] = 1;
            box_database[23, 5] = 1;
            box_database[26, 5] = 1;
            box_database[27, 5] = 1;
            box_database[30, 5] = 1;
            box_database[31, 5] = 1;
            box_database[34, 5] = 1;
            box_database[35, 5] = 1;
            box_database[38, 5] = 1;
            box_database[39, 5] = 1;
            box_database[42, 5] = 1;
            box_database[43, 5] = 1;
            box_database[46, 5] = 1;
            box_database[47, 5] = 1;
            box_database[50, 5] = 1;
            box_database[51, 5] = 1;
            box_database[54, 5] = 1;
            box_database[55, 5] = 1;
            box_database[58, 5] = 1;
            box_database[59, 5] = 1;
            box_database[62, 5] = 1;
            box_database[63, 5] = 1;
            box_database[82, 5] = 1;
            box_database[83, 5] = 1;
            box_database[86, 5] = 1;
            box_database[87, 5] = 1;
            box_database[90, 5] = 1;
            box_database[91, 5] = 1;
            box_database[94, 5] = 1;
            box_database[95, 5] = 1;
            box_database[98, 5] = 1;
            box_database[99, 5] = 1;
            box_database[102, 5] = 1;
            box_database[103, 5] = 1;
            box_database[106, 5] = 1;
            box_database[107, 5] = 1;
            box_database[110, 5] = 1;
            box_database[111, 5] = 1;

            t = 0;
            for(i=1;i<=draw_counter;i+=1)
            {
                if(box_database[i,6] == 1 && box_database[i,5] == 1)
                {
                    t += 1;
                    box_database[i, 7] = seat_db[t, 1];
                    box_database[i, 8] = seat_db[t, 2];
                    box_database[i, 9] = seat_db[t, 3];
                }
            }
        }

        public void on_draw()
        {
            
            int i,seat_counter = 0;
            for(i=1;i<=draw_counter;i+=1)
            {
                //graphic.FillRectangle(Brushes.Black,box_database[i, 1], box_database[i, 2], box_database[i, 3], box_database[i, 4]);
                if(box_database[i,6] == 1)//just draw fill? stroke?
                {
                    if(box_database[i,5] == 1)//seat?
                    {
                        seat_counter += 1;

                        if(box_database[i,7] == 1)//empty?
                        {
                            graphic.FillRectangle(Brushes.Green, box_database[i, 1], box_database[i, 2], box_database[i, 3], box_database[i, 4]);
                        }
                        else
                        {
                            graphic.FillRectangle(Brushes.Yellow, box_database[i, 1], box_database[i, 2], box_database[i, 3], box_database[i, 4]);
                            graphic.DrawString("ID : " + box_database[i, 8] , font, Brushes.Black, new PointF(box_database[i, 1], box_database[i, 2] + 10));
                            graphic.DrawString("" + (box_database[i, 9] / 3600) + ":" + ((box_database[i, 9] % 3600) / 60) + ":" + (((box_database[i, 9] % 3600) / 60)%60), font, Brushes.Black, new PointF(box_database[i, 1], box_database[i, 2] + 20));
                        }
                        graphic.DrawString("" + seat_counter, font, Brushes.Black, new PointF(box_database[i, 1], box_database[i, 2]));
                        //graphic.DrawRectangle(new Pen(Brushes.Blue), new Rectangle(box_database[i, 1], box_database[i, 2], box_database[i, 3], box_database[i, 4]));
                    }
                    graphic.DrawRectangle(new Pen(Brushes.Black), new Rectangle(box_database[i, 1], box_database[i, 2], box_database[i, 3], box_database[i, 4]));

                    
                    
                }
                else
                {
                    graphic.FillRectangle(Brushes.Blue, box_database[i, 1], box_database[i, 2], box_database[i, 3], box_database[i, 4]);
                }
            }
            graphic.DrawString("본 프로그램은 해상도 1600*900에 최적화 되어있습니다", font, Brushes.White, new PointF(0, 0));
            graphic.DrawString("원하시는 자리를 선택해 주세요", font, Brushes.White, new PointF(box_database[draw_counter,1], box_database[draw_counter,2]));
        }

        public void save_log(Exception e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            int i, t;

            graphic = this.CreateGraphics();
            width = Screen.PrimaryScreen.Bounds.Width;
            height = Screen.PrimaryScreen.Bounds.Height;
            running = true;
            frame_counter = 0;
            frame_cycle = 960;
            r_flag = true;
            view_x = 0;
            view_y = 0;
            limit = 1000;
            metrics_x = (float)width / 1600;
            metrics_y = (float)height / 900;
            seat_num = 40;
            click = false;
            mouse_x = 0;
            mouse_y = 0;

            box_database = new int[limit,limit];
            seat_db = new int[limit, limit];
            seat_student = new int[limit, limit];
            for (i = 0; i < limit;i+=1)
            {
                for(t=0;t<limit;t+=1)
                {
                    box_database[i, t] = 0;
                    seat_db[i, t] = 0;
                    seat_student[i, t] = 0;
                }
            }

            thread = new Thread(run);
            thread.Start();
        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(running == true)
            {
                MessageBox.Show("도서관 자리 서비스를 종료합니다");
                running = false;
            }
            graphic.Dispose();
        }
    }
}
