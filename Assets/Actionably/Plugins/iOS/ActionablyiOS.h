#ifndef AddressBook_ActionablyContacts_h
#define AddressBook_ActionablyContacts_h

#import <Foundation/Foundation.h>

void listContacts(const char* objectName, const char* functionName, const char* errFunctionName);
int getContactStatus();
void sendSms(const char* objectName, const char* functionName,
                  const char* phoneNumber, const char* message, const char* imageFile);
const char* getWebViewUserAgent();
#endif
