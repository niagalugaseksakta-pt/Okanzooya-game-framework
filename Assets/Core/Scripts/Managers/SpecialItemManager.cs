using UnityEngine;

public class SpecialItemManager : MonoBehaviour
{
    public static SpecialItemManager Instance;

    void Awake() => Instance = this;

    public void RollForSpecial()
    {
        if (Random.value < 0.2f) // 20% chance
        {
            Debug.Log("Special Item Spawned!");
            ActivateSpecial("DoubleScore");
        }
    }

    public void ActivateSpecial(string id)
    {
        switch (id)
        {
            case "DoubleScore":
                ScoreManager.Instance.SetMultiplier(2, 5f);
                break;
            default:
                break;
        }
    }
}
