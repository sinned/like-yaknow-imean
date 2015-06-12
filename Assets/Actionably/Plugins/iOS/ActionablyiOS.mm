#import <Foundation/Foundation.h>
#import <AddressBook/ABAddressBook.h>
#import <AddressBookUI/AddressBookUI.h>

#import "Unity.h"
#import "Sms.h"
#import "StringTools.h"

extern "C" {
    const char* getWebViewUserAgent() {
        UIWebView* webView = [[UIWebView alloc] initWithFrame:CGRectZero];
        NSString* secretAgent = [webView stringByEvaluatingJavaScriptFromString:@"navigator.userAgent"];
        return [StringTools createCString:secretAgent];
    }
    
    void sendSms(const char* objectName, const char* functionName,
                         const char* phoneNumber, const char* message, const char* imageFile) {
        NSString *objectNameNS = [StringTools createNSString:objectName];
        NSString *functionNameNS = [StringTools createNSString:functionName];
        NSString *phoneNumberNS = [StringTools createNSString:phoneNumber];
        NSString *messageNS = [StringTools createNSString:message];
        NSString *imageFileNS = [StringTools createNSString:imageFile];
        [Sms send:objectNameNS : functionNameNS : phoneNumberNS : messageNS : imageFileNS];
    }
    
    // 0 - not determined/asked yet
    // 1 - not allowed
    // 2 - allowed
    int getContactStatus() {
        ABAuthorizationStatus status = ABAddressBookGetAuthorizationStatus();
        if (status == kABAuthorizationStatusNotDetermined) {
            return 0;
        } else if (status == kABAuthorizationStatusAuthorized) {
            return 2;
        } else {
            return 1;
        }
    }
    
    void getContactsWithAddressBook(const char* objectName, const char* functionName,
                                    const char* errFunctionName, ABAddressBookRef addressBook) {
        NSMutableArray* contactList = [[NSMutableArray alloc] init];
        CFArrayRef allPeople = ABAddressBookCopyArrayOfAllPeople(addressBook);
        CFIndex nPeople = ABAddressBookGetPersonCount(addressBook);
        for (int i=0;i < nPeople;i++) {
            NSMutableDictionary *dOfPerson=[NSMutableDictionary dictionary];
            
            ABRecordRef ref = CFArrayGetValueAtIndex(allPeople,i);
            
            //For username and surname
            ABMultiValueRef phones =(__bridge ABMultiValueRef)((__bridge NSString*)ABRecordCopyValue(ref, kABPersonPhoneProperty));
            
            CFStringRef firstName, lastName;
            firstName = (CFStringRef)ABRecordCopyValue(ref, kABPersonFirstNameProperty);
            if (firstName != NULL) {
                [dOfPerson setObject:(__bridge NSString*)firstName forKey:@"firstName"];
            }
            lastName  = (CFStringRef)ABRecordCopyValue(ref, kABPersonLastNameProperty);
            if (lastName != NULL) {
                [dOfPerson setObject:(__bridge NSString*)lastName forKey:@"lastName"];
            }
            
            //For Email ids
            ABMutableMultiValueRef eMail  = ABRecordCopyValue(ref, kABPersonEmailProperty);
            if(ABMultiValueGetCount(eMail) > 0) {
                [dOfPerson setObject:(__bridge NSString *)ABMultiValueCopyValueAtIndex(eMail, 0) forKey:@"email"];
                
            }
            
            int count = ABMultiValueGetCount(phones);
            if (count > 0) {
                NSMutableArray* phoneNumbers = [[NSMutableArray alloc] init];
                for(CFIndex i = 0; i < count; i++) {
                    NSString* number = (__bridge NSString*)ABMultiValueCopyValueAtIndex(phones, i);
                    if (number != NULL) {
                        NSMutableDictionary *phoneNumber=[NSMutableDictionary dictionary];
                        CFStringRef mobileTypeKey = ABMultiValueCopyLabelAtIndex(phones, i);
                        NSString *mobileType =(__bridge NSString*) ABAddressBookCopyLocalizedLabel(mobileTypeKey);
                        [phoneNumber setObject:mobileType forKey:@"type"];
                        [phoneNumber setObject:number forKey:@"number"];
                        [phoneNumbers addObject:phoneNumber];
                    }
                }
                [dOfPerson setObject:phoneNumbers forKey:@"phoneNumbers"];
            }
            [contactList addObject:dOfPerson];
        }
        NSError *error = nil;
        
        NSMutableDictionary* result=[NSMutableDictionary dictionary];
        [result setObject:contactList forKey:@"contacts"];
        [result setObject:@"true" forKey:@"success"];
        
        NSData* json = [NSJSONSerialization dataWithJSONObject:result options:NSJSONWritingPrettyPrinted error:&error];
        if (json != nil && error == nil)
        {
            NSURL* docsDir = [[[NSFileManager defaultManager] URLsForDirectory:NSDocumentDirectory inDomains:NSUserDomainMask] lastObject];
            NSString *path = [docsDir.path stringByAppendingPathComponent:@"contacts.json"];
            [json writeToFile:path atomically:true];
            //NSLog([[NSString alloc] initWithData:json encoding:NSUTF8StringEncoding]);
            UnitySendMessage(objectName, functionName, [StringTools createCString:path]);
        } else {
            NSString* message = @"UNABLE_TO_WRITE_FILE";
            UnitySendMessage(objectName, errFunctionName, [StringTools createCString:message]);
        }
    }

    void listContacts(const char* objectName, const char* functionName, const char* errFunctionName) {
        ABAddressBookRef addressBook = ABAddressBookCreate();
        
        __block BOOL accessGranted = NO;
        
        if (ABAddressBookRequestAccessWithCompletion != NULL) { // We are on iOS 6
            dispatch_semaphore_t semaphore = dispatch_semaphore_create(0);
            
            ABAddressBookRequestAccessWithCompletion(addressBook, ^(bool granted, CFErrorRef error) {
                accessGranted = granted;
                dispatch_semaphore_signal(semaphore);
            });
            
            dispatch_semaphore_wait(semaphore, DISPATCH_TIME_FOREVER);
        }
        
        else { // We are on iOS 5 or Older
            accessGranted = YES;
            getContactsWithAddressBook(objectName, functionName, errFunctionName, addressBook);
        }
        
        if (accessGranted) {
            getContactsWithAddressBook(objectName, functionName, errFunctionName, addressBook);
        } else {
            NSString* message = @"NO_PERMISSION";
            UnitySendMessage(objectName, errFunctionName, [StringTools createCString:message]);
        }
    }
    
}
