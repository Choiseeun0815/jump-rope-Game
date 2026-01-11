using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    static public ScoreManager Instance;
    
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] TextMeshProUGUI jumpTimingText;
    [SerializeField] TextMeshProUGUI perpectComboText;

    private int currentScore = 0;
    private int perpectCombo = 0;

    private void Awake()
    {
        Instance = this;
        if (jumpTimingText != null) jumpTimingText.text = "";
    }

    public void SetScoreText()
    {
        currentScore++;
        scoreText.text = $"Score: {currentScore}";
    }
    public void SetGameOverText()
    {
        scoreText.text = $"GameOver";
        if (jumpTimingText != null) jumpTimingText.text = "";
    }
    public void ShowJumpTimingText(string text, Color color)
    {
        if (jumpTimingText == null) return;

        jumpTimingText.text = text;
        jumpTimingText.color = color;

        StopAllCoroutines();
        StartCoroutine(HidePerpectText());
    }
    IEnumerator HidePerpectText()
    {
        yield return new WaitForSeconds(.5f);
        jumpTimingText.text = "";
    }
}