using UnityEngine;

namespace Assets.CoreFramework.Scripts.Controller
{
    public class DialogueStarter : MonoBehaviour
    {
        public DialogueController controller;

        private void Start()
        {
            controller.StartDialogueFromFile("Dialogues/IntroScene.json");
        }
    }

}
