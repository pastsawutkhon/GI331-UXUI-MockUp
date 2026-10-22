
using UnityEngine;
using Unity.Services.LevelPlay;
using System;
using System.Collections;

public class AdsSample : MonoBehaviour
{
    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;
    private LevelPlayRewardedAd rewardedVideoAd;

    private bool isAdsEnabled = false;
    private bool hintRequested = false;
    private bool bannerShouldBeVisible = true;
    private bool fullscreenAdShowing;
    private bool interstitialLoading;
    private Action pendingInterstitialClosed;
    private Coroutine interstitialTimeout;
#if UNITY_EDITOR
    private const float EditorRewardedDuration = 5f;
    private float editorRewardedStartedAt;
    private bool editorRewardedVisible;
    private Texture2D editorRewardedBackground;
#endif

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
        FinishInterstitial();
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
        rewardedVideoAd.OnAdDisplayed += OnRewardedDisplayed;
        rewardedVideoAd.OnAdClosed += OnRewardedClosed;
        rewardedVideoAd.OnAdDisplayFailed += OnRewardedDisplayFailed;

        interstitialAd.OnAdLoaded += OnInterstitialLoaded;
        interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
        interstitialAd.OnAdClosed += OnInterstitialClosed;
        interstitialAd.OnAdDisplayFailed += OnInterstitialDisplayFailed;
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
        if (bannerShouldBeVisible && !fullscreenAdShowing)
            bannerAd?.ShowAd();
    }

    private void OnBannerLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning("Banner load failed: " + error);
    }

    public void ShowBannerAds()
    {
        bannerShouldBeVisible = true;
        if (!fullscreenAdShowing)
            bannerAd?.ShowAd();
    }

    public void HideBannerAds()
    {
        bannerShouldBeVisible = false;
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
#if UNITY_EDITOR
            editorRewardedStartedAt = Time.time;
            editorRewardedVisible = true;
#endif
            BeginFullscreenAd();
            rewardedVideoAd.ShowAd();
        }
    }

    private void OnRewardedDisplayed(LevelPlayAdInfo adInfo)
    {
        BeginFullscreenAd();
    }

    private void OnRewardedDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogWarning("Rewarded display failed: " + error);
        hintRequested = false;
#if UNITY_EDITOR
        editorRewardedVisible = false;
#endif
        EndFullscreenAd();
        LoadRewardedVideoAds();
    }

    private void OnRewarded(
        LevelPlayAdInfo adInfo,
        LevelPlayReward reward)
    {
#if UNITY_EDITOR
        // The LevelPlay Editor mock reports a reward even when its Close button is used early.
        if (Time.time - editorRewardedStartedAt < EditorRewardedDuration)
        {
            Debug.Log("Rewarded mock closed before completion; no hint granted.");
            return;
        }
#endif
        Debug.Log("Reward earned! Unlock hint.");

        // ให้ระบบ Hint ทำงานหลังได้รับรางวัลจริง
        OnHintRewardEarned?.Invoke();
    }

    private void OnRewardedClosed(LevelPlayAdInfo adInfo)
    {
        hintRequested = false;
#if UNITY_EDITOR
        editorRewardedVisible = false;
#endif
        EndFullscreenAd();
        LoadRewardedVideoAds();
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!editorRewardedVisible) return;

        // Cover the Editor mock's oversized Close label with a readable button.
        // Closing early is allowed, but OnRewarded grants nothing until the video completes.
        if (editorRewardedBackground == null)
            editorRewardedBackground = GetEditorRewardedBackground();

        var closeRect = new Rect(Screen.width * 0.75f, Screen.height * 0.05f,
            Screen.width * 0.2f, Screen.height * 0.05f);
        GUI.depth = -1;
        if (editorRewardedBackground != null)
        {
            var coverRect = new Rect(Screen.width * 0.71f, Screen.height * 0.01f,
                Screen.width * 0.28f, Screen.height * 0.12f);
            GUI.DrawTextureWithTexCoords(coverRect, editorRewardedBackground,
                new Rect(0.71f, 0.87f, 0.28f, 0.12f));
        }

        var style = new GUIStyle(GUI.skin.button)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 16,
            wordWrap = false
        };
        var previousColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.04f, 0.43f, 0.93f);
        if (GUI.Button(closeRect, "Close", style))
            CloseEditorMockRewarded();
        GUI.backgroundColor = previousColor;
    }

    private static Texture2D GetEditorRewardedBackground()
    {
        var mock = GameObject.Find("MockRewardedEditorAd(Clone)");
        if (mock == null) return null;

        foreach (var component in mock.GetComponents<MonoBehaviour>())
        {
            if (component == null || component.GetType().Name != "RewardedPrefab") continue;
            var background = component.GetType().BaseType.GetField("m_BackgroundTexture",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return background?.GetValue(component) as Texture2D;
        }

        return null;
    }

    private static void CloseEditorMockRewarded()
    {
        var mock = GameObject.Find("MockRewardedEditorAd(Clone)");
        if (mock == null) return;

        foreach (var component in mock.GetComponents<MonoBehaviour>())
        {
            if (component == null || component.GetType().Name != "RewardedPrefab") continue;
            var hideAd = component.GetType().GetMethod("HideAd",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            hideAd?.Invoke(component, null);
            return;
        }
    }
#endif

    // ---------------- INTERSTITIAL ----------------

    public void LoadInterstitialAds()
    {
        if (interstitialAd == null || interstitialLoading) return;
        interstitialLoading = true;
        interstitialAd.LoadAd();
    }

    public void ShowInterstitialAds()
    {
        if (interstitialAd != null &&
            interstitialAd.IsAdReady())
        {
            BeginFullscreenAd();
            interstitialAd.ShowAd();
        }
        else
        {
            Debug.Log("Interstitial ad not ready");
        }
    }

    public void ShowInterstitialBefore(Action onClosed)
    {
        if (pendingInterstitialClosed != null) return;
        pendingInterstitialClosed = onClosed;
        HideBannerAds();

        if (!isAdsEnabled || interstitialAd == null)
        {
            FinishInterstitial();
            return;
        }

        if (interstitialAd.IsAdReady())
        {
            ShowInterstitialAds();
            return;
        }

        LoadInterstitialAds();
        interstitialTimeout = StartCoroutine(InterstitialWaitTimeout());
    }

    private IEnumerator InterstitialWaitTimeout()
    {
        yield return new WaitForSecondsRealtime(8f);
        Debug.LogWarning("Interstitial timed out; continuing to result screen.");
        FinishInterstitial();
    }

    private void OnInterstitialLoaded(LevelPlayAdInfo adInfo)
    {
        interstitialLoading = false;
        if (pendingInterstitialClosed != null)
        {
            if (interstitialTimeout != null)
            {
                StopCoroutine(interstitialTimeout);
                interstitialTimeout = null;
            }
            ShowInterstitialAds();
        }
    }

    private void OnInterstitialLoadFailed(LevelPlayAdError error)
    {
        interstitialLoading = false;
        Debug.LogWarning("Interstitial load failed: " + error);
        FinishInterstitial();
    }

    private void OnInterstitialClosed(LevelPlayAdInfo adInfo)
    {
        EndFullscreenAd();
        FinishInterstitial();
        LoadInterstitialAds();
    }

    private void OnInterstitialDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogWarning("Interstitial display failed: " + error);
        EndFullscreenAd();
        FinishInterstitial();
        LoadInterstitialAds();
    }

    private void BeginFullscreenAd()
    {
        fullscreenAdShowing = true;
        bannerAd?.HideAd();
    }

    private void EndFullscreenAd()
    {
        fullscreenAdShowing = false;
        if (bannerShouldBeVisible)
            bannerAd?.ShowAd();
    }

    private void FinishInterstitial()
    {
        if (interstitialTimeout != null)
        {
            StopCoroutine(interstitialTimeout);
            interstitialTimeout = null;
        }
        var callback = pendingInterstitialClosed;
        pendingInterstitialClosed = null;
        callback?.Invoke();
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
            rewardedVideoAd.OnAdDisplayed -= OnRewardedDisplayed;
            rewardedVideoAd.OnAdClosed -= OnRewardedClosed;
            rewardedVideoAd.OnAdDisplayFailed -= OnRewardedDisplayFailed;
        }

        if (interstitialAd != null)
        {
            interstitialAd.OnAdLoaded -= OnInterstitialLoaded;
            interstitialAd.OnAdLoadFailed -= OnInterstitialLoadFailed;
            interstitialAd.OnAdClosed -= OnInterstitialClosed;
            interstitialAd.OnAdDisplayFailed -= OnInterstitialDisplayFailed;
            interstitialAd.DestroyAd();
        }
    }
}
