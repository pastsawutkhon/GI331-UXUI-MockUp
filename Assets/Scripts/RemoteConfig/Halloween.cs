using System;
using Unity.Services.RemoteConfig;
using UnityEngine;
using UnityEngine.UI;

public class Halloween : MonoBehaviour
{
    private const string HalloweenKey = "isHalloween";

    [SerializeField] private GameObject halloweenVisuals;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color halloweenBackgroundTint = new Color(0.62f, 0.48f, 0.73f, 1f);

    [SerializeField] private bool isHalloween;
    private Color normalBackgroundTint;
    private bool subscribed;

    private void Awake()
    {
        if (backgroundImage != null)
            normalBackgroundTint = backgroundImage.color;

        ApplyTheme(false);
    }

    private async void Start()
    {
        var settings = RemoteConfigSettings.Instance;
        if (settings == null)
        {
            Debug.LogError("Halloween requires RemoteConfigSettings in the scene.", this);
            return;
        }

        RemoteConfigService.Instance.FetchCompleted += ApplyHalloweenRemote;
        subscribed = true;

        try
        {
            await settings.FetchAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
            if (this != null)
                ApplyTheme(false);
        }
    }

    private void ApplyHalloweenRemote(ConfigResponse response)
    {
        if (this == null) return;

        bool enabled = RemoteConfigService.Instance.appConfig.GetBool(HalloweenKey, false);
        ApplyTheme(enabled);
        Debug.Log($"Remote Config {HalloweenKey} = {enabled} ({response.requestOrigin})", this);
    }

    public void ApplyTheme(bool enabled)
    {
        isHalloween = enabled;
        if (halloweenVisuals != null)
            halloweenVisuals.SetActive(enabled);
        if (backgroundImage != null)
            backgroundImage.color = enabled ? halloweenBackgroundTint : normalBackgroundTint;
    }

    [ContextMenu("Preview Halloween Theme")]
    private void PreviewHalloween() => ApplyTheme(true);

    [ContextMenu("Preview Normal Theme")]
    private void PreviewNormal() => ApplyTheme(false);

    private void OnDestroy()
    {
        if (subscribed)
            RemoteConfigService.Instance.FetchCompleted -= ApplyHalloweenRemote;
    }
}
