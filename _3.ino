#include "Arduino.h"

void card_init();
void card_receive(int );

char wake[24]={
  0x55, 0x55, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 
0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x03, 0xfd, 0xd4, 0x14, 0x01, 0x17, 0x00};
char wake_test[15] = {0x00,0x00,0xff,0x00,0xff,0x00,0x00,0x00,0xff,0x02,0xfe,0xd5,0x15,0x16,0x00
};
char tag[11] = {0x00, 0x00, 0xFF, 0x04, 0xFC, 0xD4, 0x4A, 0x01, 0x00, 0xE1, 0x00};
char tag_fail[7] = {0x00,0x00,0xff,0x00,0xff,0x00,0xff};
char return_msg[60] = {'\0'};

int order,reset_pin = 22;




void setup() {
  // put your setup code here, to run once:
  digitalWrite(reset_pin,HIGH); //아두이노킷 재부팅을 위한 기능
  pinMode(13,OUTPUT); //LED점멸
  pinMode(reset_pin,OUTPUT); 
  Serial.begin(9600); //보드레이트 9600
  Serial1.begin(115200); //보드레이트 115200
  Serial.println("booting..."); //사용 준비가 완료됬음을 사용자에게 알림
  digitalWrite(13,LOW); //LED비활성화
}

void loop() {
  // put your main code here, to run repeatedly:

  if(Serial.available())
  {
    order = Serial.read();//사용자가 입력한 명령어를 받아들임
    if(order == '1')//NFC모듈을 사용하기 위한 준비과정
    {
      card_init();
      delay(100);
      card_receive(15);
    }
    else if(order == '2')//NFC모듈을 이용하여 카드의 RFID태그를 받아들임
    {
      card_tag();
      delay(100);
      card_receive(50);
    }
    else if(order == '3')//아두이노킷 재부팅
    {
      Serial.println("rebooting...");
      digitalWrite(13,HIGH);
      delay(125);
      digitalWrite(reset_pin,LOW);
    }
  }
}

void card_tag()//태그를 읽겠다는 명령을 NFC모듈에 전송
{
  
  int i,temp ;
  temp = 11;
  for(i=0;i<temp;i+=1)
  {
     Serial1.write(tag[i]);
  }
  Serial1.flush();
}

void card_receive(int temp)//NFC모듈에서 오는 모든 정보를 받아들임
{
  if(Serial1.available())
  {
    int i;
    bool flag = true;
    for(i=0;i<temp;i+=1)
    {
      return_msg[i] = Serial1.read();
      //Serial.print(return_msg[i],HEX);
      if(return_msg[i] != wake_test[i])
      {
        flag = false;
      }
    }
    if(order=='1')//초기화 과정일 시 NFC모듈에서 오는 값이 정상인지 체크
    {
      if(flag == true)
      {
        Serial.print("NFC Initialization successfully\n");  
      }
      else
      {
        Serial.print("NFC Initialization fail\n");
      }
    }
    else if(order=='2')//태그 읽기 모드일 시 받아들인 값들을 출력
    {
      flag = false;
      for(i=0;i<7;i+=1)
      {
          if(return_msg[i] != tag_fail[i])
          {
             flag = true;
          }
      }
      if(flag == true)
      {
        
      for(i=0;i<temp;i+=1)
      {
        Serial.print("0x");
        Serial.print(return_msg[i],HEX);
        if(i<7)
        {
           if(return_msg[i] != tag_fail[i])
           {
             flag = true;
           }
        }
        Serial.print(" ");
      }
      Serial.println("");
      }
      else
      {
        Serial.print("tag fail\n");
      }
    }
  }
}

void card_init()//NFC모듈을 깨우기 위한 과정
{
  int i,temp = strlen(wake);
  temp = 24;
  for(i=0;i<temp;i+=1)
  {
     Serial1.write(wake[i]);
  }
  Serial1.flush();
  
}

