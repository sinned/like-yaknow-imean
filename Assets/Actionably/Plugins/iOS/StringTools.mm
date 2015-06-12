#import "StringTools.h"

@implementation StringTools

+(NSString*) createNSString:(const char*)string
{
    if(string != NULL) {
        return [[NSString alloc] initWithUTF8String:string];
    }
    else
        return NULL;
}

+(char*) createCString:(NSString*)nsstring
{
    const char* string = [nsstring UTF8String];
    if (string == NULL)
        return NULL;
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    
    return res;
}

@end