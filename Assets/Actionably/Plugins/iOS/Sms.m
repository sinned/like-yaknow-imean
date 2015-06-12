//
//  SendSms.m
//  UnityPluginTest
//
//  Created by Jesse Hull on 4/21/15.
//  Copyright (c) 2015 Jesse Hull. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <MessageUI/MessageUI.h>
#import <MobileCoreServices/UTCoreTypes.h>
#import "Unity.h"
#import "StringTools.h"
#import "Sms.h"

@interface Sms () <MFMessageComposeViewControllerDelegate>

@end
@implementation Sms : NSObject 

static Sms* smsDelegate = NULL;
static NSString* theObjectName = NULL;
static NSString* theFunctionName = NULL;

- (void)messageComposeViewController:(MFMessageComposeViewController *)controller
                 didFinishWithResult:(MessageComposeResult)result
{
    [controller dismissViewControllerAnimated:YES completion:nil];
    NSString* status = @"unknown";
    switch (result) {
        case MessageComposeResultCancelled:
            status = @"cancelled";
            break;
            
        case MessageComposeResultFailed:
            status = @"failed";
            break;
            
        case MessageComposeResultSent:
            status = @"sent";
            break;
        default:
            break;
    }
    
    UnitySendMessage([StringTools createCString:theObjectName],
                     [StringTools createCString:theFunctionName],
                     [StringTools createCString:status]);
}


+(void) send:(NSString*)objectName :(NSString*)functionName :(NSString*)phoneNumber :(NSString*)message :(NSString*)imageFile
{
    if(smsDelegate == NULL) smsDelegate = [[Sms alloc] init];
    theObjectName = objectName;
    theFunctionName = functionName;
    NSLog(@"object and function %@ %@", theObjectName, theFunctionName);
 
    if(![MFMessageComposeViewController canSendText]) {
        UIAlertView *warningAlert = [[UIAlertView alloc] initWithTitle:@"Error" message:@"Your device doesn't support SMS!" delegate:nil cancelButtonTitle:@"OK" otherButtonTitles:nil];
        [warningAlert show];
        return;
    }
    
    NSArray *recipents = @[phoneNumber];
    
    MFMessageComposeViewController *messageController = [[MFMessageComposeViewController alloc] init];
    messageController.messageComposeDelegate = smsDelegate;
    if (imageFile != NULL) {
        NSData *imgData = [NSData dataWithContentsOfFile:imageFile];
        if (imgData == NULL) {
            printf("imgData null");
        }
        BOOL didAttachImage = [messageController addAttachmentData:imgData typeIdentifier:(NSString*)kUTTypeImage filename:@"image.jpg"];
        printf("attached %d", didAttachImage);
    }
    
    [messageController setRecipients:recipents];
    [messageController setBody:message];
    
    // Present message view controller on screen
    [UnityGetGLViewController() presentViewController:messageController animated:YES completion:nil];
}

@end
