using UnityEngine;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI timeText;

    public void Setup(int rank, string username, string timeFormatted)
    {
        rankText.text = rank.ToString();
        nameText.text = username;
        timeText.text = timeFormatted;
    }
}