using System;

namespace Assets.CoreFramework.Scripts.Managers.Dialouge.Core
{
    [Serializable]
    public class DialogueChoice
    {
        public string choiceText;
        public string nextFile;
        public string actionType;
    }
}
