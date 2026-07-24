//
//  VunglePluginReferences.m
//  UnityFramework
//

#import <Foundation/Foundation.h>
#import "VunglePluginReferences.h"

@interface VunglePluginReferences ()

@property(nonatomic) NSMutableDictionary *references;
@property(nonatomic) dispatch_queue_t referenceQueue;

@end

@implementation VunglePluginReferences {
    
}

+ (instancetype)sharedInstance {
    static VunglePluginReferences *sharedInstance;
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        sharedInstance = [[self alloc] init];
    });
    return sharedInstance;
}

- (id)init {
    self = [super init];
    if (self) {
        _references = [[NSMutableDictionary alloc] init];
        _referenceQueue = dispatch_queue_create("com.liftoff.references.queue", DISPATCH_QUEUE_SERIAL);
    }
    return self;
}

- (void)setObject:(id)object {
    dispatch_async(_referenceQueue, ^{
        NSString *key = [NSString stringWithFormat:@"%p", (void *)object];
        self->_references[key] = object;
    });
}

- (void)removeObjectForKey:(id)object {
    dispatch_async(_referenceQueue, ^{
        NSString *key = [NSString stringWithFormat:@"%p", (void *)object];
        [self->_references removeObjectForKey:key];
    });
}

@end
