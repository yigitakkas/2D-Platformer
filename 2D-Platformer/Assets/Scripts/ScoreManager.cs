using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager scoreManager;
    public TextMeshProUGUI text;
    public int score=0;
    // Start is called before the first frame update
    void Start()
    {
        if(scoreManager == null)
        {
            scoreManager = this;
        }
    }

    public void ChangeScore(int coinValue)
    {
        score += coinValue;
        text.text = score.ToString();
    }
}
