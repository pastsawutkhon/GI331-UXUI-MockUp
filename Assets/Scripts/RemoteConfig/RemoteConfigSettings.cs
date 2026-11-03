using System;
using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class RemoteConfigSettings : MonoBehaviour
{
    public static RemoteConfigSettings Instance { get; private set; }

    [Tooltip("Leave empty to use the production environment.")]
    public string environmentId;

    public struct userAttributes { }
    public struct appAttributes { }

    public Task InitializationTask { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (InitializationTask == null)
            InitializationTask = InitializeRemoteConfigAsync();
    }

    public async Task InitializeRemoteConfigAsync()
    {
        await UGSManager.InitializeAsync();

        if (!string.IsNullOrWhiteSpace(environmentId))
            RemoteConfigService.Instance.SetEnvironmentID(environmentId.Trim());
    }

    public async Task FetchAsync()
    {
        if (InitializationTask == null)
            InitializationTask = InitializeRemoteConfigAsync();

        await InitializationTask;

        await RemoteConfigService.Instance.FetchConfigsAsync<userAttributes, appAttributes>(
            new userAttributes(), new appAttributes());
    }

    [ContextMenu("Refresh Remote Config")]
    private async void RefreshFromInspector()
    {
        try
        {
            await FetchAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
