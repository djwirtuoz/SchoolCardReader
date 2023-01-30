  #include <Arduino.h>
  #include <Wire.h>
  #include <PN532_I2C.h>
  #include <PN532.h>
  #include <NfcAdapter.h>
  #include <rdm6300.h>

  PN532_I2C pn532i2c(Wire);
  PN532 nfc(pn532i2c);

  #define RDM6300_RX_PIN 4 // read the SoftwareSerial doc above! may need to change this pin to 10...
  Rdm6300 rdm6300;

  const int buttonPin = 2;

  bool mode; //выбирается считыватель, если true, то RDM 
  int buttonState = 0;
  
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

  pinMode(11, OUTPUT);
  pinMode(12, OUTPUT);
  pinMode(3, INPUT);
}

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
        digitalWrite(11, HIGH);
        digitalWrite(12, LOW);

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
    digitalWrite(12, HIGH);
    digitalWrite(11, LOW);

    bool success;
    uint8_t uid[] = { 0, 0, 0, 0, 0, 0, 0 };  // Buffer to store the returned UID
    uint8_t uidLength;                        // Length of the UID (4 or 7 bytes depending on ISO14443A card type)
    
    success = nfc.readPassiveTargetID(PN532_MIFARE_ISO14443A, &uid[0], &uidLength);
    
    if (success && uidLength > 1) {
      Serial.print("135-");
      for (uint8_t i = 0; i < uidLength; i++) 
      {
        Serial.print(uid[i], HEX); Serial.print(","); 
      }
      Serial.println();
    }
    delay(300);
  }
}