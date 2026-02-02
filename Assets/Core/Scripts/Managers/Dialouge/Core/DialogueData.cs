using System;

namespace Assets.CoreFramework.Scripts.Managers.Dialouge.Core
{
    [Serializable]
    public class DialogueData
    {
        public string title;
        public string scene;         // 🌄 Default or global scene background
        public float autoTimer;      // ⏲️ Default timer for lines without their own
        public DialogueLine[] lines;
    }
}
