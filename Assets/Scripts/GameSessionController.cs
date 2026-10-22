using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSessionController : MonoBehaviour
{
    [SerializeField] private AdsSample ads;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private TMP_Text liveTimerText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text resultTimerText;

    private float elapsedSeconds;
    private bool gameEnded;

    private void Awake()
    {
        Time.timeScale = 1f;
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        UpdateLiveTimer();
    }

    private void Update()
    {
        if (gameEnded) return;
        elapsedSeconds += Time.deltaTime;
        UpdateLiveTimer();
    }

    public void EndGame()
    {
        if (gameEnded) return;
        gameEnded = true;
        Time.timeScale = 0f;

        int score = Mathf.FloorToInt(elapsedSeconds);
        if (scoreText != null) scoreText.text = score.ToString();
        if (resultTimerText != null) resultTimerText.text = FormatTime(score);

        if (ads != null)
            ads.ShowInterstitialBefore(ShowLevelComplete);
        else
            ShowLevelComplete();
    }

    private void ShowLevelComplete()
    {
        if (this == null) return;
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);
    }

    public void OpenMainMenu()
    {
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void UpdateLiveTimer()
    {
        if (liveTimerText != null)
            liveTimerText.text = FormatTime(Mathf.FloorToInt(elapsedSeconds));
    }

    private static string FormatTime(int totalSeconds)
    {
        return string.Format("{0:00}:{1:00}", totalSeconds / 60, totalSeconds % 60);
    }
}
