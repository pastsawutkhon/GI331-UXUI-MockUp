using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

// โครงสร้างข้อมูลของผู้เล่นแต่ละคน
[System.Serializable]
public class PlayerData
{
    public string username;
    public float timeInSeconds;
}

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    public Transform contentContainer;
    public GameObject rowPrefab;

    [Header("Mock Data")]
    public List<PlayerData> playerList;

    void Start()
    {
        UpdateLeaderboardUI();
    }

    [ContextMenu("Update Leaderboard UI")]
    public void UpdateLeaderboardUI()
    {
        var sortedList = playerList.OrderBy(player => player.timeInSeconds).ToList();

        for (int i = contentContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = contentContainer.GetChild(i).gameObject;
            
            if (Application.isPlaying)
            {
                Destroy(child); 
            }
            else
            {
                DestroyImmediate(child); 
            }
        }

        for (int i = 0; i < sortedList.Count; i++)
        {
            GameObject newRow = Instantiate(rowPrefab, contentContainer);
            LeaderboardRow rowScript = newRow.GetComponent<LeaderboardRow>();

            int currentRank = i + 1; 

            TimeSpan t = TimeSpan.FromSeconds(sortedList[i].timeInSeconds);
            string formattedTime = string.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);

            rowScript.Setup(currentRank, sortedList[i].username, formattedTime);
        }
    }
}