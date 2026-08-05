using System.Collections.Generic;
using UnityEngine;
using VungleAds;

namespace VungleAds.Samples
{
    public class VungleTestManager : MonoBehaviour
    {
        static VungleTestManager _instance;

        public static VungleTestManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("VungleTestManager");
                    _instance = go.AddComponent<VungleTestManager>();
                }
                return _instance;
            }
        }

        public static bool HasInstance => _instance != null;

        public bool SdkInitialized { get; private set; }
        public string LogText { get; private set; } = "";
        public event System.Action OnLogChanged;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            VungleSdk.onInitializeSuccessEvent += () =>
            {
                SdkInitialized = true;
                Log("SDK initialized");
                SetFpdData();
            };
            VungleSdk.onInitializeFailedEvent += (err) => Log("SDK init failed: " + err);
        }

        public void Log(string msg)
        {
            Debug.Log("[VungleTest] " + msg);
            LogText += msg + "\n";
            OnLogChanged?.Invoke();
        }

        public void ClearLog()
        {
            LogText = "";
            OnLogChanged?.Invoke();
        }

        public void InitializeSdk()
        {
    #if UNITY_EDITOR
            SdkInitialized = true;
            Log("SDK initialized (editor mock)");
    #elif UNITY_IOS || UNITY_ANDROID
            Log("SDK initializing...");
            VungleSdk.Init(VungleConstants.AppId);
    #else
            SdkInitialized = true;
            Log("SDK initialized (unsupported platform)");
    #endif
        }

        void SetFpdData()
        {
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
            VungleFpd.SetAge(25);
            VungleFpd.SetLengthOfResidenceYears(2.5);
            VungleFpd.SetMedianHomeValueUsd(1000);
            VungleFpd.SetMonthlyHousingPaymentUsd(50);
            VungleFpd.SetCountry("us");
            VungleFpd.SetDma(94112);
            VungleFpd.SetRegionState("ca");
            VungleFpd.SetEarningsByPlacementUsd(8.5);
            VungleFpd.SetIsUserAPurchaser(true);
            VungleFpd.SetIsUserASubscriber(false);
            VungleFpd.SetLast30DaysMeanSpendUsd(30.1);
            VungleFpd.SetLast30DaysMedianSpendUsd(30.2);
            VungleFpd.SetLast30DaysPlacementFillRate(30.3);
            VungleFpd.SetLast30DaysTotalSpendUsd(30.4);
            VungleFpd.SetLast30DaysUserLtvUsd(30.5);
            VungleFpd.SetLast30DaysUserPltvUsd(30.6);
            VungleFpd.SetLast7DaysMeanSpendUsd(7.1);
            VungleFpd.SetLast7DaysMedianSpendUsd(7.2);
            VungleFpd.SetLast7DaysPlacementFillRate(7.3);
            VungleFpd.SetLast7DaysTotalSpendUsd(7.4);
            VungleFpd.SetLast7DaysUserLtvUsd(7.5);
            VungleFpd.SetLast7DaysUserPltvUsd(7.6);
            VungleFpd.SetTopNAdomain(new List<string> { "domain1.com", "domain2.com" });
            VungleFpd.SetTotalEarningsUsd(100);
            VungleFpd.SetFriends(new List<string> { "friend1", "friend2" });
            VungleFpd.SetHealthPercentile(90);
            VungleFpd.SetInGamePurchases(800.80);
            VungleFpd.SetLevelPercentile(10.1);
            VungleFpd.SetPage("page1");
            VungleFpd.SetSessionStartTime(new System.DateTime(2024, 9, 5));
            VungleFpd.SetSessionDuration(500);
            VungleFpd.SetSignupDate(new System.DateTime(2024, 8, 5));
            VungleFpd.SetTimeSpent(2000);
            VungleFpd.SetUserId("userId");
            VungleFpd.SetUserLevelPercentile(50);
            VungleFpd.SetUserScorePercentile(60.7);
            VungleFpd.SetCustomData(new Dictionary<string, string> { { "domain3", "uidsomething" } });
            VungleFpd.AddCustomData("domain4", "uid2");
    #endif
        }
    }
}
