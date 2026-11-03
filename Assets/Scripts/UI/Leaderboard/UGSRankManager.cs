using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards.Exceptions;

public class UGSRankingManager : MonoBehaviour
{
    private const string LeaderboardId = "High_Score";

    [Header("UI")]
    [SerializeField] private RankUIManager rankUIManager;

    [Header("Test")]
    [SerializeField] private int testScore = 1000;
    private readonly TaskCompletionSource<bool> initialization = new();
    private readonly System.Threading.SemaphoreSlim reloadLock = new(1, 1);


    private async void Start()
    {
        try
        {
            await UGSManager.InitializeAsync();
            initialization.TrySetResult(true);
        }
        catch (System.Exception exception)
        {
            initialization.TrySetException(exception);
            Debug.LogException(exception, this);
            return;
        }
        AuthenticationService.Instance.SignedIn += OnSignedIn;
        AuthenticationService.Instance.SignedOut += OnSignedOut;
        await ReloadLeaderboardAsync();
    }

    private async void OnSignedIn()
    {
        try { await ReloadLeaderboardAsync(); }
        catch (System.Exception exception) { Debug.LogException(exception, this); }
    }

    private void OnSignedOut()
    {
        rankUIManager.ShowSignedOutUser();
    }

    private void OnDestroy()
    {
        if (Unity.Services.Core.UnityServices.State != Unity.Services.Core.ServicesInitializationState.Initialized) return;
        AuthenticationService.Instance.SignedIn -= OnSignedIn;
        AuthenticationService.Instance.SignedOut -= OnSignedOut;
    }

    private async Task ReloadCurrentUserAsync()
    {
        var auth = AuthenticationService.Instance;
        if (!auth.IsSignedIn) { rankUIManager.ShowSignedOutUser(); return; }
        string playerId = auth.PlayerId;
        LeaderboardEntry entry = null;
        try
        {
            entry = await LeaderboardsService.Instance.GetPlayerScoreAsync(LeaderboardId);
        }
        catch (LeaderboardsException exception) when (exception.Reason == LeaderboardsExceptionReason.EntryNotFound)
        {
            // New players can have a profile before submitting their first score.
        }
        if (!auth.IsSignedIn || auth.PlayerId != playerId) return;
        // Names are not necessarily cached after sign-in, especially before a first score.
        string playerName = await auth.GetPlayerNameAsync(false);
        if (string.IsNullOrEmpty(playerName)) playerName = entry?.PlayerName;
        if (string.IsNullOrEmpty(playerName)) playerName = "Guest";
        Texture2D avatar = await PlayerProfileService.LoadAvatarAsync(playerId);
        if (this == null || !auth.IsSignedIn || auth.PlayerId != playerId)
        {
            if (avatar != null) Destroy(avatar);
            return;
        }
        rankUIManager.ShowCurrentUser(new PlayerData(playerId, entry == null ? 0 : entry.Rank + 1,
            playerName, entry == null ? 0 : Mathf.RoundToInt((float)entry.Score), avatar));
    }

    public async Task SubmitScoreAsync(int score)
    {
        await LeaderboardsService.Instance.AddPlayerScoreAsync(LeaderboardId, score);
        Debug.Log($"Score submitted: {score}");
    }

    public async Task ReloadLeaderboardAsync()
    {
        await initialization.Task;
        await reloadLock.WaitAsync();
        try
        {
            if (this == null) return;
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                rankUIManager.ShowSignedOutUser();
                return;
            }
            await ReloadCurrentUserAsync();
            if (!AuthenticationService.Instance.IsSignedIn) return;
            await LoadLeaderboardAsync();
        }
        finally
        {
            reloadLock.Release();
        }
    }

    private async Task LoadLeaderboardAsync()
    {
        var scores = await LeaderboardsService.Instance.GetScoresAsync(LeaderboardId,
            new GetScoresOptions
            {
                Offset = 0,
                Limit = 10
            }
            );

        List<PlayerData> playerDatas = new List<PlayerData>();

        foreach(var entry in scores.Results)
        {
            Texture2D avatar = await PlayerProfileService.LoadAvatarAsync(entry.PlayerId);
            PlayerData playerData =
                new PlayerData(
                    entry.PlayerId,

                    entry.Rank + 1,
                    entry.PlayerName,
                    Mathf.RoundToInt((float)entry.Score), avatar
                    );

            playerDatas.Add(playerData);
        }

        rankUIManager.ShowRanking(playerDatas);
    }

    [ContextMenu("Submit Test Score")]
    private async void SubmitTestScore()
    {
        await SubmitScoreAsync(testScore);
        await ReloadLeaderboardAsync();
    }

    [ContextMenu("Reload Leaderboard")]
    public async void ReloadLeaderboard()
    {
        try { await ReloadLeaderboardAsync(); }
        catch (System.Exception exception) { Debug.LogException(exception, this); }
    }
}
 
