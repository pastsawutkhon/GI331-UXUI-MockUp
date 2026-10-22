
using UnityEngine;
using Unity.Services.LevelPlay;
using System;

public class AdsSample : MonoBehaviour
{
    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedVideoAd;

    private bool isAdsEnabled = false;
    private bool hintRequested = false;

    // ให้ HintSystem เชื่อมการทำงานผ่าน Event นี้
    public event Action OnHintRewardEarned;

    private void Start()
    {
        LevelPlay.ValidateIntegration();

        LevelPlay.OnInitSuccess += SdkInitCompletedEvent;
        LevelPlay.OnInitFailed += SdkInitFailedEvent;

        LevelPlay.Init(AdConfig.AppKey);
    }

    private void SdkInitFailedEvent(LevelPlayInitError error)
    {
        Debug.LogError("Ads initialization failed: " + error);
    }

    private void SdkInitCompletedEvent(
        LevelPlayConfiguration configuration)
    {
        isAdsEnabled = true;
        EnableAds();

        // โหลดโฆษณาทั้งสามประเภทอัตโนมัติ
        LoadBannerAds();
        LoadRewardedVideoAds();
        LoadInterstitialAds();

        Debug.Log("Ads initialized successfully");
    }

    private void EnableAds()
    {
        var configBuilder = new LevelPlayBannerAd.Config.Builder();

        configBuilder
            .SetSize(LevelPlayAdSize.BANNER)
            .SetPosition(LevelPlayBannerPosition.BottomCenter);

        var bannerConfig = configBuilder.Build();

        bannerAd = new LevelPlayBannerAd(
            AdConfig.BannerAdUnitId, bannerConfig);

        interstitialAd = new LevelPlayInterstitialAd(
            AdConfig.InterstitialAdUnitId);

        rewardedVideoAd = new LevelPlayRewardedAd(
            AdConfig.RewardedVideoAdUnitId);

        // Banner
        bannerAd.OnAdLoaded += OnBannerLoaded;
        bannerAd.OnAdLoadFailed += OnBannerLoadFailed;

        // Rewarded
        rewardedVideoAd.OnAdLoaded += OnRewardedLoaded;
        rewardedVideoAd.OnAdLoadFailed += OnRewardedLoadFailed;
        rewardedVideoAd.OnAdRewarded += OnRewarded;
        rewardedVideoAd.OnAdClosed += OnRewardedClosed;
    }

    // ---------------- BANNER ----------------

    public void LoadBannerAds()
    {
        if (bannerAd != null)
            bannerAd.LoadAd();
    }

    private void OnBannerLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Banner loaded successfully");
        ShowBannerAds();
    }

    private void OnBannerLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning("Banner load failed: " + error);
    }

    public void ShowBannerAds()
    {
        bannerAd?.ShowAd();
    }

    public void HideBannerAds()
    {
        bannerAd?.HideAd();
    }

    // ---------------- REWARDED ----------------

    public void LoadRewardedVideoAds()
    {
        if (rewardedVideoAd != null)
            rewardedVideoAd.LoadAd();
    }

    private void OnRewardedLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log("Rewarded ad ready");

        if (hintRequested)
        {
            hintRequested = false;
            ShowRewardedVideoAds();
        }
    }

    private void OnRewardedLoadFailed(LevelPlayAdError error)
    {
        hintRequested = false;
        Debug.LogWarning("Rewarded load failed: " + error);
    }

    // เชื่อมปุ่ม Hint เข้ากับ Method นี้
    public void WatchAdForHint()
    {
        if (!isAdsEnabled || rewardedVideoAd == null)
        {
            Debug.Log("Ads SDK not ready");
            return;
        }

        if (rewardedVideoAd.IsAdReady())
        {
            ShowRewardedVideoAds();
        }
        else
        {
            if (hintRequested) return;

            hintRequested = true;
            LoadRewardedVideoAds();
            Debug.Log("Loading rewarded ad for hint...");
        }
    }

    public void ShowRewardedVideoAds()
    {
        if (rewardedVideoAd != null &&
            rewardedVideoAd.IsAdReady())
        {
            rewardedVideoAd.ShowAd();
        }
    }

    private void OnRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward reward)
    {
        Debug.Log("Reward earned! Unlock hint.");

        // ให้ระบบ Hint ทำงานหลังได้รับรางวัลจริง
        OnHintRewardEarned?.Invoke();
    }

    private void OnRewardedClosed(LevelPlayAdInfo adInfo)
    {
        hintRequested = false;
        LoadRewardedVideoAds();
    }

    // ---------------- INTERSTITIAL ----------------

    public void LoadInterstitialAds()
    {
        interstitialAd?.LoadAd();
    }

    public void ShowInterstitialAds()
    {
        if (interstitialAd != null &&
            interstitialAd.IsAdReady())
        {
            interstitialAd.ShowAd();
        }
        else
        {
            Debug.Log("Interstitial ad not ready");
        }
    }

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= SdkInitCompletedEvent;
        LevelPlay.OnInitFailed -= SdkInitFailedEvent;

        if (bannerAd != null)
        {
            bannerAd.OnAdLoaded -= OnBannerLoaded;
            bannerAd.OnAdLoadFailed -= OnBannerLoadFailed;
            bannerAd.DestroyAd();
        }

        if (rewardedVideoAd != null)
        {
            rewardedVideoAd.OnAdLoaded -= OnRewardedLoaded;
            rewardedVideoAd.OnAdLoadFailed -= OnRewardedLoadFailed;
            rewardedVideoAd.OnAdRewarded -= OnRewarded;
            rewardedVideoAd.OnAdClosed -= OnRewardedClosed;
        }

        interstitialAd?.DestroyAd();
    }
}
