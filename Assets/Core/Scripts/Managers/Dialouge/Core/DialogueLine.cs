using Assets.CoreFramework.Scripts.Managers.Dialouge.Core;

[System.Serializable]
public class DialogueLine
{
    public string speakerName;
    public string dialogueText;

    // 🖼️ Portrait and positioning
    public string portraitPath;
    public string portraitSide;  // "left" or "right"

    // 🌄 Optional per-line background override
    public string scene;

    // ⏲️ Timer for auto-advance (overrides global autoTimer)
    public float timer;

    public string emotion;

    // 🎯 Optional branching choices
    public DialogueChoice[] choices;
    public string voiceKey;
}