#import <Foundation/Foundation.h>

@interface StringTools : NSObject

+(NSString*) createNSString:(const char*)string;
+(char*) createCString:(NSString*)nsstring;

@end
