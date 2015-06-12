//
//  SendSms.h
//  UnityPluginTest
//
//  Created by Jesse Hull on 4/21/15.
//  Copyright (c) 2015 Jesse Hull. All rights reserved.
//

#ifndef UnityPluginTest_SendSms_h
#define UnityPluginTest_SendSms_h

#import <Foundation/Foundation.h>

@interface Sms : NSObject

+(void) send:(NSString*)objectName
            :(NSString*)functionName :(NSString*)phoneNumber :(NSString*)message :(NSString*)imageFile;

@end

#endif
