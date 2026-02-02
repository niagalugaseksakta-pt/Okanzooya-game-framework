using UnityEngine;

namespace Assets.CoreFramework.Scripts.Managers.Dialouge.Core
{
    [CreateAssetMenu(menuName = "Dialogue/Sequence")]
    public class DialogueSequence : ScriptableObject
    {
        public DialogueLine[] lines;
    }
}
