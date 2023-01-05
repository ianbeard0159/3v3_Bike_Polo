using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] private TMP_Text redScoreText;
    [SerializeField] private TMP_Text blueScoreText;
    private int redScore = 0;
    private int blueScore = 0;
    void Awake()
    {
        instance = this;

        Goal.Event_GoalScored += EventSub_GoalScored;
    }
    void OnDestroy() {
        Goal.Event_GoalScored -= EventSub_GoalScored;
    }
    void EventSub_GoalScored(string in_goalName) {
        if (in_goalName == "Red Goal") {
            blueScore += 1;
            blueScoreText.text = blueScore.ToString();
        }
        else if (in_goalName == "Blue Goal") {
            redScore += 1;
            redScoreText.text = redScore.ToString();
        }
    }
}
