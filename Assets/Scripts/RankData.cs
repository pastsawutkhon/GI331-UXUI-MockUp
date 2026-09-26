using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct PlayerData
{
    public string playerId;
    public string playerName;
    public int rankNumber;
    public int playerScore;
    public Texture2D profileTexture;

    public PlayerData(string playerId, int rankNumber, string playerName
        , int playerScore, Texture2D profileTexture = null)
    {
        this.playerId = playerId;
        this.rankNumber = rankNumber;
        this.playerName = playerName;
        this.playerScore = playerScore;
        this.profileTexture = profileTexture;
    }
}

public class RankData : MonoBehaviour
{
    [SerializeField] private RawImage profileImg;
    [SerializeField] private Texture defaultProfileTexture;
    private Texture2D loadedProfileTexture;
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;

    public void SetData(PlayerData playerData)
    {
        if (loadedProfileTexture != null && loadedProfileTexture != playerData.profileTexture)
            Destroy(loadedProfileTexture);
        loadedProfileTexture = playerData.profileTexture;
        if (profileImg != null)
            profileImg.texture = loadedProfileTexture != null ? loadedProfileTexture : defaultProfileTexture;
        rankText.text = playerData.rankNumber > 0 ? playerData.rankNumber.ToString() : "-";
        playerNameText.text = playerData.playerName;
        int totalSeconds = Mathf.Max(0, playerData.playerScore);
        scoreText.text = playerData.rankNumber > 0 ? $"{totalSeconds / 60:00}:{totalSeconds % 60:00}" : "--:--";
    }

    public void ShowSignedOut()
    {
        SetData(new PlayerData(null, 0, "-----", 0));
        rankText.text = "-";
    }

    private void OnDestroy()
    {
        if (loadedProfileTexture != null)
            Destroy(loadedProfileTexture);
    }
}
