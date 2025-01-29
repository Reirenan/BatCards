using GoogleMobileAds.Api;
using SimpleSolitaire.Model.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleSolitaire.Controller
{
    [Serializable]
    public class AdsIds
    {
        public string InterId;
        public string RewardId;
        public string BannerId;
    }

    public class AdsData
    {
        public AdsIds Ids;

        public bool IsTestADS;
        public bool IsBanner;
        public bool IsIntersitial;
        public bool IsReward;
        public bool IsHandeldAction;
    }

    public class InterVideoAds : MonoBehaviour
    {
        public static Action<RewardAdsState, RewardAdsType> RewardAction { get; set; }

        [SerializeField] private GameManager _gameManagerComponent;
        [SerializeField] private UndoPerformer _undoPerformerComponent;

        [SerializeField] private AdsIds _androidIds;
        [SerializeField] private AdsIds _iosIds;

        private string _interId;
        private string _rewardId;
        private string _bannerId;

        private readonly string _testBannerId = "ca-app-pub-3940256099942544/6300978111";
        private readonly string _testIntersitialId = "ca-app-pub-3940256099942544/1033173712";
        private readonly string _testRewardId = "ca-app-pub-3940256099942544/5224354917";

        [Space(5f)] [SerializeField] private bool _intersitialRepeatCall;
        [SerializeField] private int _intersitialCallsBorder = 3;
        [SerializeField] private int _firstCallIntersitialTime;
        [SerializeField] private int _repeatIntersitialTime;

        private int _intersitialCallsCounter = 0;

        private AdRequest _requestAdmob;
        private BannerView _bannerView;
        private InterstitialAd _interstitial;

        private RewardedAd _rewardVideo;

        [SerializeField] private bool _isTestADS;
        [SerializeField] private bool _isBanner;
        [SerializeField] private bool _isIntersitial;
        [SerializeField] private bool _isReward;
        [SerializeField] private bool _isHandeldAction;

        private AdSize _currentBannerSize = AdSize.Banner;

        public readonly string NoAdsKey = "NoAds";

        private RewardAdsType _lastShowingType = RewardAdsType.None;
        private RewardVideoStatus _lastRewardVideoStatus = RewardVideoStatus.None;

        private bool _isRewarded = false;

        private void Start()
        {
            InitializeADS();
        }

        private void OnDestroy()
        {
            HideBanner();
        }

        /// <summary>
        /// Initialize admob requests variable.
        /// </summary>
        private void AdMobRequest()
        {
            if (_isTestADS)
            {
                List<string> deviceIds = new List<string>()
                {
                    SystemInfo.deviceUniqueIdentifier
                };

                RequestConfiguration requestConfiguration = new RequestConfiguration();
                requestConfiguration.TestDeviceIds.AddRange(deviceIds);

                MobileAds.SetRequestConfiguration(requestConfiguration);
            }

            _requestAdmob = new AdRequest();
        }

        #region Requests ADS

        /// <summary>
        /// Banned ad request.
        /// </summary>
        private void ShowBanner()
        {
            if (IsHasKeyNoAds())
                return;

            if (_bannerView != null)
            {
                Debug.Log("Destroying banner ad.");
                _bannerView.Destroy();
                _bannerView = null;
            }

            _bannerView = new BannerView((_isTestADS) ? _testBannerId : _bannerId, _currentBannerSize, AdPosition.Bottom);

            if (_bannerView != null)
            {
                AdMobRequest();
                _bannerView.LoadAd(_requestAdmob);
            }
        }

        /// <summary>
        /// Intersitial video request.
        /// </summary>
        public void RequestInterstitial()
        {
            if (IsHasKeyNoAds())
                return;

            if (_interstitial != null)
            {
                _interstitial.Destroy();
                _interstitial = null;
            }

            AdMobRequest();
            var adUnitId = _isTestADS ? _testIntersitialId : _interId;
            InterstitialAd.Load(adUnitId, _requestAdmob,
                (InterstitialAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("interstitial ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Interstitial ad loaded with response : "
                              + ad.GetResponseInfo());

                    _interstitial = ad;
                });
        }

        /// <summary>
        /// Reward video request.
        /// </summary>
        private void RequestRewardBasedVideo(bool isRequiredRequest = false)
        {
            if (IsHasKeyNoAds() && !isRequiredRequest)
                return;

            if (_rewardVideo != null)
            {
                _rewardVideo.Destroy();
                _rewardVideo = null;
            }

            var adUnitId = _isTestADS ? _testRewardId : _rewardId;
            AdMobRequest();
            // send the request to load the ad.
            RewardedAd.Load(adUnitId, _requestAdmob,
                (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        Debug.LogError("Rewarded ad failed to load an ad " +
                                       "with error : " + error);
                        return;
                    }

                    Debug.Log("Rewarded ad loaded with response : "
                              + ad.GetResponseInfo());

                    _rewardVideo = ad;
                });
        }

        #endregion

        #region Handlers

        private void RegisterReloadHandler(InterstitialAd ad)
        {
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Interstitial Ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                RequestInterstitial();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Interstitial ad failed to open full screen content " +
                               "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                RequestInterstitial();
            };
        }

        private void RegisterReloadHandler(RewardedAd ad)
        {
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded Ad full screen content closed.");

                // Reload the ad so that we can show another as soon as possible.
                RequestRewardBasedVideo();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                RequestRewardBasedVideo();
            };
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                Debug.Log(String.Format("Rewarded ad paid {0} {1}.",
                    adValue.Value,
                    adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () => { Debug.Log("Rewarded ad recorded an impression."); };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () => { Debug.Log("Rewarded ad was clicked."); };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () => { Debug.Log("Rewarded ad full screen content opened."); };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                Debug.Log("Rewarded ad full screen content closed.");
                HandleClosedBasedVideoRewarded();
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                Debug.LogError("Rewarded ad failed to open full screen content " +
                               "with error : " + error);
            };
        }

        #endregion

        #region Show/Hide ADS

        public void TryShowIntersitialByCounter()
        {
            if (++_intersitialCallsCounter >= _intersitialCallsBorder)
            {
                _intersitialCallsCounter = 0;
                ShowInterstitial();
            }
        }

        /// <summary>
        /// Show intersitial ads <see cref="_interstitial"/> if ads available for watch.
        /// </summary>
        public void ShowInterstitial()
        {
            if (IsHasKeyNoAds())
                return;

            if (_interstitial != null && _interstitial.CanShowAd())
            {
                Debug.Log("Showing interstitial ad.");
                _interstitial.Show();
                RegisterReloadHandler(_interstitial);
            }
            else
            {
                Debug.LogError("Interstitial ad is not ready yet.");
            }
        }

        /// <summary>
        /// Show reward video <see cref="_rewardVideo"/> if ads available for watch.
        /// </summary>
        public void ShowRewardBasedVideo()
        {
            if (IsHasKeyNoAds())
                return;

            const string rewardMsg =
                "Rewarded ad rewarded the user. Type: {0}, amount: {1}.";

            if (_rewardVideo != null && _rewardVideo.CanShowAd())
            {
                RegisterReloadHandler(_rewardVideo);
                RegisterEventHandlers(_rewardVideo);
                _rewardVideo.Show((Reward reward) =>
                {
                    // TODO: Reward the user.
                    Debug.Log(String.Format(rewardMsg, reward.Type, reward.Amount));
                });
            }
        }

        /// <summary>
        /// This method hide Smart banner from bottom of screen.
        /// </summary>
        public void HideBanner()
        {
            if (_bannerView != null)
                _bannerView.Hide();
        }

        /// <summary>
        /// Show reward video. If user watch it the ads will disappear for current game session.
        /// </summary>
        public void NoAdsAction()
        {
            _lastShowingType = RewardAdsType.NoAds;
            StartCoroutine(LoadRewardedVideo(_rewardVideo, _lastShowingType));
        }

        /// <summary>
        /// Show reward video. If user watch it the free undo tries will be add for user.
        /// </summary>
        public void ShowGetUndoAction()
        {
            _lastShowingType = RewardAdsType.GetUndo;
            StartCoroutine(LoadRewardedVideo(_rewardVideo, _lastShowingType));
        }

        private IEnumerator LoadRewardedVideo(RewardedAd ads, RewardAdsType type)
        {
            _lastRewardVideoStatus = RewardVideoStatus.None;

            if (_isHandeldAction)
            {
#if UNITY_IPHONE
                Handheld.SetActivityIndicatorStyle(UnityEngine.iOS.ActivityIndicatorStyle.Gray);
#elif UNITY_ANDROID
                Handheld.SetActivityIndicatorStyle(AndroidActivityIndicatorStyle.Small);
#elif !UNITY_STANDALONE
                Handheld.StartActivityIndicator();
#endif
            }

            yield return new WaitUntil(() => _isTestADS
                                             || _rewardVideo != null && _rewardVideo.CanShowAd()
                //Used for old Admob sdk Before version 6.1.2
                /* || Application.isEditor*/);
            if (_isHandeldAction)
            {
#if !UNITY_STANDALONE                  
                Handheld.StopActivityIndicator();
#endif
            }

            _lastRewardVideoStatus = _rewardVideo != null && _rewardVideo.CanShowAd() || _isTestADS ? _lastRewardVideoStatus = RewardVideoStatus.Loaded : RewardVideoStatus.FailedToLoad;

            //Used for old Admob sdk Before version 6.1.2
            // if (Application.isEditor)
            // {
            //     OnRewardedUser();
            //     yield break;
            // }

            _isRewarded = false;

            switch (_lastRewardVideoStatus)
            {
                case RewardVideoStatus.None:
                case RewardVideoStatus.FailedToLoad:
                    RewardAction?.Invoke(RewardAdsState.DID_NOT_LOADED, type);
                    break;
                case RewardVideoStatus.Loaded:
                    RegisterReloadHandler(ads);
                    RegisterEventHandlers(ads);
                    ads.Show(HandleRewardBasedVideoRewarded);
                    break;
            }
        }

        #endregion

        #region EventsHandlers

        /// <summary>
        /// On close reward video event.
        /// </summary>
        private void HandleClosedBasedVideoRewarded()
        {
            Debug.LogError($"HandleRewardBasedVideoRewarded {_isRewarded}");

            if (_isRewarded)
            {
                return;
            }

            switch (_lastShowingType)
            {
                case RewardAdsType.NoAds:
                    RequestRewardBasedVideo();
                    break;
                case RewardAdsType.GetUndo:
                    RequestRewardBasedVideo(true);
                    break;
            }

            RewardAction?.Invoke(RewardAdsState.TOO_EARLY_CLOSE, _lastShowingType);
        }

        /// <summary>
        /// On full watch reward video event.
        /// </summary>
        public void HandleRewardBasedVideoRewarded(Reward args)
        {
            _isRewarded = true;

            Debug.LogError($"HandleRewardBasedVideoRewarded {_isRewarded}");

            OnRewardedUser();
        }

        /// <summary>
        /// Reward actions by type of reward ads.
        /// </summary>
        public void OnRewardedUser()
        {
            switch (_lastShowingType)
            {
                case RewardAdsType.NoAds:
                    PlayerPrefs.SetInt(NoAdsKey, 1);
                    HideBanner();
                    _gameManagerComponent.OnNoAdsRewardedUser();
                    RequestRewardBasedVideo(true);
                    break;
                case RewardAdsType.GetUndo:
                    _gameManagerComponent.OnClickAdsCloseBtn();
                    _undoPerformerComponent.UpdateUndoCounts();
                    RequestRewardBasedVideo(true);
                    break;
            }
        }

        #endregion

        /// <summary>
        /// Initialize all active advertisment.
        /// </summary>
        public void InitializeADS()
        {
            if (IsHasKeyNoAds())
                PlayerPrefs.DeleteKey(NoAdsKey);

            MobileAds.Initialize(initStatus => { Debug.Log("Sdk init status:" + initStatus); });

            var ids = Application.platform == RuntimePlatform.Android ? _androidIds : Application.platform == RuntimePlatform.IPhonePlayer ? _iosIds : null;

            if (ids != null)
            {
                _interId = ids.InterId;
                _rewardId = ids.RewardId;
                _bannerId = ids.BannerId;
            }

            if (_isBanner)
            {
                ShowBanner();
                _gameManagerComponent.InitializeBottomPanel(_currentBannerSize.Height * GetAdmobBannerScaleBasedOnDPI());
            }

            if (_isReward)
            {
                RequestRewardBasedVideo(true);
            }

            if (_isIntersitial)
            {
                RequestInterstitial();
                if (_intersitialRepeatCall)
                {
                    // First call after _firstCallIntersitialTime seconds. Repeating intersitial video every _repeatIntersitialTime seconds.
                    InvokeRepeating("ShowInterstitial", _firstCallIntersitialTime, _repeatIntersitialTime);
                }
            }
        }

        /// <summary>
        /// Check for exist in player prefs key <see cref="NoAdsKey"/>
        /// </summary>
        /// <returns></returns>
        private bool IsHasKeyNoAds()
        {
            return PlayerPrefs.HasKey(NoAdsKey);
        }

        private float GetAdmobBannerScaleBasedOnDPI()
        {
            //By default banner has no scaling.
            float scale = 1f;

            //All information about scaling has provided on Google Admob API
            //Low Density Screens, around 120 DPI, scaling factor 0.75, e.g. 320×50 becomes 240×37.
            //Medium Density Screens, around 160 DPI, no scaling, e.g. 320×50 stays at 320×50.
            //High Density Screens, around 240 DPI, scaling factor 1.5, e.g. 320×50 becomes 480×75.
            //Extra High Density Screens, around 320 DPI, scaling factor 2, e.g. 320×50 becomes 640×100.
            //Extra Extra High Density Screens, around 480 DPI, scaling factor 3, e.g. 320×50 becomes 960×150.

            if (Screen.dpi > 480)
            {
                scale = 3f;
            }
            else if (Screen.dpi > 320)
            {
                scale = 2f;
            }
            else if (Screen.dpi > 240)
            {
                scale = 1.5f;
            }
            else if (Screen.dpi > 160)
            {
                scale = 1f;
            }
            else if (Screen.dpi > 120)
            {
                scale = 0.75f;
            }

            return scale;
        }
    }
}