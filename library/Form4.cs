using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace library
{
    public partial class Form4 : Form
    {

        public static String string_connection, card_tag;
        public Form4()
        {
            InitializeComponent();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            bool pass = true;
            int student_id = 0;
            try
            {
                student_id = int.Parse(txt_studentid.Text.ToString());
            }
            catch (Exception err)
            {
                pass = false;
                MessageBox.Show("학번은 숫자로 이루어져야 합니다");
                save_log(err);
            }
            int i,count = 0;
            String temp = txt_mail.Text.ToString();
            char check;
            for(i=0;i<temp.Length;i+=1)
            {
                check = temp[i];
                if(check == '@')
                {
                    count += 1;
                }
            }
            if(count!=1)
            {
                pass = false;
                MessageBox.Show("간단한 수법이였지만 당신은 올바른 이메일을 입력하지 않았습니다");
            }
            try
            {
                if(pass == true)
                {
                    //MessageBox.Show(string_connection);
                    MySqlConnection mysql_connection = new MySqlConnection(string_connection);
                    mysql_connection.Open();
                    MySqlCommand command = new MySqlCommand("INSERT INTO student VALUES(" + student_id + ",'" + card_tag + "','" + temp + "');",mysql_connection);
                    command.ExecuteNonQuery();
                    mysql_connection.Close();
                }
            }
            catch (Exception err)
            {

                pass = false;
                MessageBox.Show("등록하는데 문제가 생겼습니다\n"+err);
                save_log(err);
            }
            if(pass == true)
            {
                
                MessageBox.Show("등록되었습니다");
            }
            else
            {
                MessageBox.Show("등록실패");
            }
            
            this.Close();
        }
        public void save_log(Exception err)
        {

        }
    }
}
