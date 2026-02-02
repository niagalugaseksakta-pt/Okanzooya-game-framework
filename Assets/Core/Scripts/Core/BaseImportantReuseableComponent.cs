using UnityEngine;

namespace Assets.CoreFramework.Scripts.Core
{
    public abstract class BaseImportantReuseableComponent : MonoBehaviour
    {
        protected virtual void Awake()
        {
            // Initialize game systems create instance

            ServiceLocator.ReInit();
        }
    }
}
