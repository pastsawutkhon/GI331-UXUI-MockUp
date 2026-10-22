using UnityEngine;


public static class AdConfig
{
    public static string AppKey => GetAppKey();
    public static string BannerAdUnitId => GetBannerAdUnitId();
    public static string InterstitialAdUnitId => GetInterstitialAdUnitId();
    public static string RewardedVideoAdUnitId => GetRewardedVideoAdUnitId();

    private static string GetAppKey()
    {
        return "28ed49fb5";
    }

    private static string GetBannerAdUnitId()
    {
        return "j5sigj09yfhtg9v3";
    }

    private static string GetInterstitialAdUnitId()
    {
        return "v6g3aqidodm7mik2";
    }

    private static string GetRewardedVideoAdUnitId()
    {
        return "gltifr2jwolu4r9f";
    }
}
