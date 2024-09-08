using TMPro;
using UnityEngine;

public class PlayerScoreUIBlock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI nameText;

    public void Show(string name, Color color, int score)
    {
        nameText.color = color;
        nameText.text = name;
        scoreText.text = score.ToString();
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
