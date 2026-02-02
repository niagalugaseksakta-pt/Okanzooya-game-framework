using System.Collections;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    private int score = 0;
    private int multiplier = 1;

    void Awake() => Instance = this;

    public void AddScore(int basePoints)
    {
        score += basePoints * multiplier;
        Debug.Log("Score: " + score);
    }

    public void SetMultiplier(int m, float duration)
    {
        StartCoroutine(MultiplierRoutine(m, duration));
    }

    private IEnumerator MultiplierRoutine(int m, float duration)
    {
        multiplier = m;
        yield return new WaitForSeconds(duration);
        multiplier = 1;
    }
}
