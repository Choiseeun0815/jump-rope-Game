using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    static public ScoreManager Instance;
    
    [SerializeField] TextMeshProUGUI scoreText;
    private int currentScore = 0;

    private void Awake()
    {
        Instance = this;
    }

    public void SetScoreText()
    {
        currentScore++;
        scoreText.text = $"Score: {currentScore}";
    }
    public void SetGameOverText()
    {
        scoreText.text = $"GameOver";

    }
}
