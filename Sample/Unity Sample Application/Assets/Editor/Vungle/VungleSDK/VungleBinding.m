//
//  VungleBinding.m
//  VungleTest
//
//
#import <VungleSDK/VungleSDK.h>
#import "VungleManager.h"
// import <UIViewController.h>
// import <UIView.h>

// Converts C style string to NSString
#define GetStringParam( _x_ ) ( _x_ != NULL ) ? [NSString stringWithUTF8String:_x_] : [NSString stringWithUTF8String:""]

// Converts C style string to NSString as long as it isnt empty
#define GetStringParamOrNil( _x_ ) ( _x_ != NULL && strlen( _x_ ) ) ? [NSString stringWithUTF8String:_x_] : nil

#define VUNGLE_API_KEY   @"vungle.api_endpoint"
#define VUNGLE_FILE_SYSTEM_SIZE_FOR_INIT_KEY  @"vungleMinimumFileSystemSizeForInit"
#define VUNGLE_MINIMUM_FILE_SYSTEM_SIZE_FOR_AD_REQUEST_KEY  @"vungleMinimumFileSystemSizeForAdRequest"
#define VUNGLE_MINIMUM_FILE_SYSTEM_SIZE_FOR_ASSET_DOWNLOAD_KEY  @"vungleMinimumFileSystemSizeForAssetDownload"

void UnitySendMessage( const char * className, const char * methodName, const char * param );
UIViewController *UnityGetGLViewController();

static BOOL bInit = false;
void _vungleStartWithAppId( const char * appId, const char * pluginVersion, BOOL initHeaderBiddingDelegate)
{
	if (bInit)
		return;
	if( [[VungleSDK sharedSDK] respondsToSelector:@selector(setPluginName:version:)] )
		[[VungleSDK sharedSDK] performSelector:@selector(setPluginName:version:) withObject:@"unity" withObject:GetStringParam(pluginVersion)];

    NSError * error;
    [VungleSDK sharedSDK].delegate = [VungleManager sharedManager];
    [VungleSDK sharedSDK].creativeTrackingDelegate = [VungleManager sharedManager];
    if (initHeaderBiddingDelegate)
	[VungleSDK sharedSDK].headerBiddingDelegate = [VungleManager sharedManager];
    [[VungleSDK sharedSDK] startWithAppId:GetStringParam(appId) error:&error];
    bInit = true;
    
    [[VungleSDK sharedSDK] setLoggingEnabled:true];
    [[VungleSDK sharedSDK] attachLogger:[VungleManager sharedManager]];
}


void _vungleSetSoundEnabled( BOOL enabled )
{
	[VungleSDK sharedSDK].muted = !enabled;
}


void _vungleEnableLogging( BOOL shouldEnable )
{
	[[VungleSDK sharedSDK] setLoggingEnabled:shouldEnable];
}


BOOL _vungleIsAdAvailable(const char* placementID)
{
	return [[VungleSDK sharedSDK] isAdCachedForPlacementID:GetStringParam(placementID)];
}

UIInterfaceOrientationMask makeOrientation(NSNumber* code) {
    UIInterfaceOrientationMask orientationMask;
    int i = [code intValue];
    switch( i )
    {
        case 1:
            orientationMask = UIInterfaceOrientationMaskPortrait;
            break;
        case 2:
            orientationMask = UIInterfaceOrientationMaskLandscapeLeft;
            break;
        case 3:
            orientationMask = UIInterfaceOrientationMaskLandscapeRight;
            break;
        case 4:
            orientationMask = UIInterfaceOrientationMaskPortraitUpsideDown;
            break;
        case 5:
            orientationMask = UIInterfaceOrientationMaskLandscape;
            break;
        case 6:
            orientationMask = UIInterfaceOrientationMaskAll;
            break;
        case 7:
            orientationMask = UIInterfaceOrientationMaskAllButUpsideDown;
            break;
        default:
            orientationMask = UIInterfaceOrientationMaskAllButUpsideDown;
    }
    return orientationMask;
}

BOOL _vungleLoadAd(const char* placementID)
{
    NSError * error;
    return [[VungleSDK sharedSDK] loadPlacementWithID:GetStringParam(placementID) error:&error];
}

BOOL _vungleCloseAd(const char* placementID)
{
    [[VungleSDK sharedSDK] finishedDisplayingAd];
    return TRUE;
}

void _vunglePlayAd(char* opt, const char* placementID) {
    NSObject* obj = [VungleManager objectFromJson:GetStringParam( opt )];
    if([obj isKindOfClass:[NSDictionary class]])
    {
        NSError * error;
        NSDictionary *from = obj;
        NSMutableDictionary *options = [NSMutableDictionary dictionary];
        options[VunglePlayAdOptionKeyOrientations] = @(makeOrientation(from[@"orientation"]));
        if (from[@"userTag"])
            options[VunglePlayAdOptionKeyUser]  = from[@"userTag"];
        if (from[@"alertTitle"])
            options[VunglePlayAdOptionKeyIncentivizedAlertTitleText] = from[@"alertTitle"];
        if (from[@"alertText"])
            options[VunglePlayAdOptionKeyIncentivizedAlertBodyText] = from[@"alertText"];
        if (from[@"closeText"])
            options[VunglePlayAdOptionKeyIncentivizedAlertCloseButtonText] = from[@"closeText"];
        if (from[@"continueText"])
            options[VunglePlayAdOptionKeyIncentivizedAlertContinueButtonText] = from[@"continueText"];
        if (from[@"flexCloseSec"])
            options[VunglePlayAdOptionKeyFlexViewAutoDismissSeconds] = from[@"flexCloseSec"];
        if (from[@"ordinal"])
            options[VunglePlayAdOptionKeyOrdinal] = [NSNumber numberWithUnsignedInteger:[from[@"ordinal"] integerValue]];
        [[VungleSDK sharedSDK] playAd:UnityGetGLViewController() options:options placementID:GetStringParam(placementID) error:&error];
    }
}

void _updateConsentStatus(int status, const char * version) {
    if (status == 1)
        [[VungleSDK sharedSDK] updateConsentStatus:VungleConsentAccepted consentMessageVersion:GetStringParam(version)];
    else if (status == 2)
        [[VungleSDK sharedSDK] updateConsentStatus:VungleConsentDenied consentMessageVersion:GetStringParam(version)];
}

int _getConsentStatus() {
    VungleConsentStatus consent = [[VungleSDK sharedSDK] getCurrentConsentStatus];
    if (consent == NULL) return 0;
    return (consent == VungleConsentAccepted)?1:2;
}

void _vungleClearSleep( )
{
    [[VungleSDK sharedSDK] clearSleep];
}

void _vungleSetEndPoint(const char * endPoint) {
    NSString *endPointString = GetStringParamOrNil( endPoint );
    if (endPointString) {
        NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
        [defaults setObject:endPointString forKey:VUNGLE_API_KEY];
    }
}

static char* MakeStringCopy (const char* string) {
    if (string == NULL) {
        return NULL;
    }
    
    char* res = (char*)malloc(strlen(string) + 1);
    strcpy(res, string);
    return res;
}

char * _vungleGetEndPoint() {
    NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
    return MakeStringCopy([[defaults objectForKey:VUNGLE_API_KEY] UTF8String]);
}
/*
void alertMessage(NSString *error) {
	UIApplication *application = [UIApplication sharedApplication];
   UIWindow *window = application.keyWindow;
   UIViewController *viewController = window.rootViewController;
   UIViewController *topViewController;
   if ([viewController isMemberOfClass:[UINavigationController class]]) {
       UINavigationController *nav = (UINavigationController* )viewController;
       topViewController = nav.topViewController;
   } else {
       UIViewController *vc;
       vc = window.rootViewController;
       while (vc) {
           topViewController = vc;
           vc = vc.presentedViewController;
       }
   }
   UIAlertController *alert = [UIAlertController alertControllerWithTitle:@"Vungle Info"
                                                                  message:error
                                                           preferredStyle:UIAlertControllerStyleAlert];
   [alert addAction:[UIAlertAction actionWithTitle:@"OK" style:UIAlertActionStyleDefault handler:nil]];
   [topViewController:alert completion:nil];
}
*/

void _vungleSetPublishIDFV( BOOL shouldEnable )
{
	//alertMessage(@"Setting should Enable");
	[VungleSDK setPublishIDFV:shouldEnable];
}

void _vungleSetMinimumSpaceForInit(int minimumSize)
{
	//alertMessage(@"Setting Min Space for Init");
	NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];
	[defaults setInteger:minimumSize forKey:VUNGLE_FILE_SYSTEM_SIZE_FOR_INIT_KEY];
}

void _vungleSetMinimumSpaceForAd(int minimumSize)
{
	//alertMessage(@"Setting Min Space for Ads");
	NSUserDefaults* defaults = [NSUserDefaults standardUserDefaults];

	[defaults setInteger:minimumSize forKey:VUNGLE_MINIMUM_FILE_SYSTEM_SIZE_FOR_AD_REQUEST_KEY];
	[defaults setInteger:minimumSize forKey:VUNGLE_MINIMUM_FILE_SYSTEM_SIZE_FOR_ASSET_DOWNLOAD_KEY];	
}


