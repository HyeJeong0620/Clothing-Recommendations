using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//네임스페이스추가(설치해야함)
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
//JSON
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;

namespace project
{
    public partial class Form1 : Form
    {
        string Conn = "Server=localhost;Database=new_schema;Uid=root;Pwd=060620;";

        //전역으로 클래스선언
        MqttClient client;
        string clientId;
        string humi;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            //프로그램이 실행되면 자동으로 브로커와 연결하겠다
            //MQTT 브로커와 연결하는 부분
            string BrokerAddress = "broker.mqtt-dashboard.com";
            client = new MqttClient(BrokerAddress);

            //구독신청을 해서 MQTT
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            // use a unique id as client id, each time we start the application
            clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            //서버와 클라이언트가 접속이 완료되는 지점
            //IoT보드가 publish하는 topic을 subscribe해야한다
            //Subscribe Topic 추가

            string[] mytopic =
            {
                "bssm_iot/outdoor/weather",
            };

            byte[] myqos =
            {
                0,
            };

            //구독신청완료
            client.Subscribe(mytopic, myqos);
        }


        //MQTT이벤트 핸들러(메세지 수신부)
        void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string ReceivedMessage = Encoding.UTF8.GetString(e.Message);

            //DO SOMETHING..!
            richTextBox1.Text += $"[{e.Topic}]" + ReceivedMessage + "\n";

            //e.Topic
            if (e.Topic == "bssm_iot/outdoor/weather")
            {
                //JSON테이터 parse
                try
                {
                    //만약 json데이터가 불완전하다면 예외가 발생한다
                    JObject myjson = JObject.Parse(ReceivedMessage);
                    humi = myjson["humi"].ToString();
                    textBox1.Text =  myjson["wind_direct"].ToString();
                    textBox2.Text =  myjson["wind_speed"].ToString();
                    textBox3.Text =  myjson["rain_amout"].ToString();
                    textBox4.Text =  humi;
                    textBox5.Text =  myjson["temp"].ToString();

                    //삽입구문
                    using (MySqlConnection conn = new MySqlConnection(Conn))
                    {
                        conn.Open();
                        MySqlCommand msc = new MySqlCommand($"insert into weather(wind_direct,wind_speed,rain_amout,humi,temp) values({myjson["wind_direct"].ToString()},{myjson["wind_speed"].ToString()},{myjson["rain_amout"].ToString()},{myjson["humi"].ToString()},{myjson["temp"].ToString()});", conn);
                        //INSERT into new_schema.weather(wind_direct,wind_speed,rain_amout,humi,temp) value(1,2,3,4,5);
                        //MySqlCommand msc = new MySqlCommand("INSERT into new_schema.weather(wind_direct,wind_speed,rain_amout,humi,temp) value(1,2,3,4,5);");
                        msc.ExecuteNonQuery();
                    }

                    double temp = double.Parse(myjson["temp"].ToString());


                    if (chart1.Series[0].Points.Count > 10)
                    {
                        //제일 처음 데이터를 삭제한다
                        chart1.Series[0].Points.RemoveAt(0);
                    }

                    chart1.Series[0].Points.Add(temp);
                }
                catch
                {

                }
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //유저가 프로그램을 종료했다
            //그러면 MQTT도 종료하겠다
            client.Disconnect();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // humi 값이 null이 아니거나 비어 있지 않은지 확인
            if (!string.IsNullOrEmpty(humi))
            {
                // humi를 double로 파싱 (숫자 값을 나타낸다고 가정)
                if (double.TryParse(humi, out double humiValue))
                {
                    // 임계값 설정 (이 값을 조정할 수 있음)
                    double data1 = 28.0;
                    double data2 = 23.0;
                    double data3 = 20.0;
                    double data4 = 17.0;
                    double data5 = 12.0;
                    double data6 = 9.0;
                    double data7 = 5.0;
                    double data8 = 1.0;

                    // humiValue를 임계값과 비교
                    if (humiValue >= data1) // 온도 28℃ 이상
                    {
                        label6.Text = "민소매";
                        pictureBox1.Image = Properties.Resources.person21;
                    }
                    else if (humiValue >= data2) // 온도 23 ~ 27℃ 
                    {
                        label6.Text = "반팔";
                        pictureBox1.Image = Properties.Resources.pic3;
                    }
                    else if (humiValue >= data3) // 온도 20 ~ 22℃ 
                    {
                        label6.Text = "긴팔";
                        pictureBox1.Image = Properties.Resources.pic4;
                    }
                    else if (humiValue >= data4) // 온도 17 ~ 19℃ 
                    {
                        label6.Text = "가디건";
                        pictureBox1.Image = Properties.Resources.pic5;
                    }
                    else if (humiValue >= data5) // 온도 12 ~ 16℃ 
                    {
                        label6.Text = "자켓";
                        pictureBox1.Image = Properties.Resources.pic6;
                    }
                    else if (humiValue >= data6)
                    {
                        label6.Text = "목도리";
                        pictureBox1.Image = Properties.Resources.pic7;
                    }
                    else if (humiValue >= data7)
                    {
                        label6.Text = "니트";
                        pictureBox1.Image = Properties.Resources.pic8;
                    }
                    else if (humiValue >= data8)
                    {
                        label6.Text = "롱패딩";
                        pictureBox1.Image = Properties.Resources.pic9;
                    }
                    else
                    {
                        // humi를 double로 파싱할 수 없는 경우 처리
                        MessageBox.Show("잘못된 humi 값");
                        pictureBox1.Image = Properties.Resources.person;
                    }
                }
                else
                {
                    // humi가 null이거나 비어 있는 경우 처리
                    MessageBox.Show("humi 값이 사용할 수 없습니다");
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label6.Text = "옷";
            pictureBox1.Image = Properties.Resources.person;
        }
    }
}
