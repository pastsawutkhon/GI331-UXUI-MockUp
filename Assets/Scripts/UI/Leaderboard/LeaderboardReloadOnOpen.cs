using UnityEngine;

public class LeaderboardReloadOnOpen : MonoBehaviour
{
    [SerializeField] private UGSRankingManager rankingManager;

    private void OnEnable()
    {
        if (rankingManager != null)
            rankingManager.ReloadLeaderboard();
    }
}
