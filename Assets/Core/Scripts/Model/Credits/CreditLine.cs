using UnityEngine;
using static Game.Config.Config;

[System.Serializable]
public class CreditLine
{
    public CreditRole role;
    [TextArea(1, 2)]
    public string content;
}
