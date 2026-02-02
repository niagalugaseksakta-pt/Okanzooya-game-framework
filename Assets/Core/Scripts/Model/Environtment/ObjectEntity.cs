using UnityEngine;

namespace Game.Model.Environtment
{
    public class ObjectEntity : EntityBase
    {
        public bool isInteractable;

        public void Interact()
        {
            if (isInteractable)
                Debug.Log($"Interacted with {DisplayName}");
        }
    }
}
