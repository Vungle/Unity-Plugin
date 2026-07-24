//
//  VunglePluginReferences.h
//  Unity-iPhone
//

@interface VunglePluginReferences : NSObject

+ (nonnull instancetype)sharedInstance;
- (void)setObject:(nonnull id)object;
- (void)removeObjectForKey:(nonnull id)object;

@end
