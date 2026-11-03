using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public static class UGSManager
{
    private static readonly object initializationLock = new object();
    private static Task initializationTask;

    public static Task InitializeAsync()
    {
        lock (initializationLock)
        {
            if (initializationTask == null || initializationTask.IsFaulted || initializationTask.IsCanceled ||
                (initializationTask.IsCompleted && !AuthenticationService.Instance.IsSignedIn))
                initializationTask = InitializeServicesAsync();

            return initializationTask;
        }
    }

    private static async Task InitializeServicesAsync()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            await UnityServices.InitializeAsync();
        }

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        Debug.Log($"Player ID: {AuthenticationService.Instance.PlayerId}");
    }

    public static async Task SetPlayerNameAsync(string playerName)
    {
        await AuthenticationService.Instance.UpdatePlayerNameAsync(playerName);
        Debug.Log($"Player Name: {AuthenticationService.Instance.PlayerName}");
    }

    public static string PlayerId => AuthenticationService.Instance.PlayerId;

    public static string PlayerName => AuthenticationService.Instance.PlayerName;
}
