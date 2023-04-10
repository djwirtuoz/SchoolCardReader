#include <Arduino.h>
#include <Wire.h>
#include <SPI.h>
#include <PN532_SPI.h>
#include <PN532.h>
#include <rdm6300.h>

PN532_SPI pn532spi(SPI, 10);
PN532 nfc(pn532spi);

#define RDM6300_RX_PIN 4 // read the SoftwareSerial doc above! may need to change this pin to 10...
Rdm6300 rdm6300;

const int buttonPin = 2;

bool mode; //выбирается считыватель, если true, то RDM 
int buttonState = 0;
String hex_uuid = "";
  
void setup() {
  Serial.begin(115200);
  rdm6300.begin(RDM6300_RX_PIN);

  nfc.begin();

  uint32_t versiondata = nfc.getFirmwareVersion();
  if (! versiondata) {
    Serial.print("Didn't find PN53x board");
    while (1); // halt
  }
  nfc.setPassiveActivationRetries(0xFF);
  
  // configure board to read RFID tags
  nfc.SAMConfig();

  mode = false;

  pinMode(7, OUTPUT);
  pinMode(6, OUTPUT);
  pinMode(3, INPUT);
}


void(* resetFunc) (void) = 0;//объявляем функцию reset с адресом 0

void loop() {

  buttonState = digitalRead(buttonPin);

  if (!mode) {  // обработчик нажатия
    mode = true;
    rdm_void();
  }

    if (mode) {  // обработчик нажатия
    mode = false;
    pn_532();
  }
}

void rdm_void() {
  while(true){
    if(mode){
      digitalWrite(7, HIGH);
      digitalWrite(6, LOW);

    if (rdm6300.get_new_tag_id()){
      Serial.print("125-");
      Serial.println(rdm6300.get_tag_id(), HEX);
    }
    }
    buttonState = digitalRead(buttonPin);
    if(buttonState == HIGH){ break; mode = false; }
  }  
}

void pn_532(){
  if(!mode){
    while(true){
    digitalWrite(6, HIGH);
    digitalWrite(7, LOW);

    bool success;
    uint8_t responseLength = 32;
    // set shield to inListPassiveTarget
    success = nfc.inListPassiveTarget();

      if(success) {
                        
        uint8_t selectApdu[] = { 0x00, /* CLA */
                                  0xA4, /* INS */
                                  0x04, /* P1  */
                                  0x00, /* P2  */
                                  0x05, /* Length of AID  */
                                  0xF2, 0x22, 0x22, 0x22, 0x22 /* AID defined on Android App */
                                };
                                  
        uint8_t response[3];  
        
        success = nfc.inDataExchange(selectApdu, sizeof(selectApdu), response, &responseLength);
        
        if(success) {
          Serial.print("135-");
          nfc.PrintHexCharNoProb(response, responseLength - 7);
          Serial.println();
          delay(1500);
          resetFunc();
        }
      }
      break;
    }
  }
}
