
namespace VungleAds
{
    ﻿using System.Collections.Generic;

    #if UNITY_EDITOR || !UNITY_IOS && !UNITY_ANDROID

    public partial class VungleUnityEditor : IVungleSdk
    {
    	public void Init(string appId) { return; }
    }

    #endif
}
