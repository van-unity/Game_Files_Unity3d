using TMPro;
using UnityEngine;

public class GameplayScreen : MonoBehaviour {
    private const string SCORE_FORMAT = "{0}";
    [SerializeField] private TextMeshProUGUI _scoreText;

    public void UpdateScore(int score) {
        _scoreText.text = string.Format(SCORE_FORMAT, score);
    }
}