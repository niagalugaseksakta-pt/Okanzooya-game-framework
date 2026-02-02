using UnityEngine;
using static Game.Config.Config;

public abstract class EntityPortalBase : MonoBehaviour
{
    [Header("Entity Identity")]
    [SerializeField, Tooltip("Unique runtime identifier")]
    public PortalState portalId = PortalState.State0;
    [SerializeField, Tooltip("Display name for debugging/UI")]
    public string portalNamebyScene = "";

    protected virtual void Awake()
    {
        portalNamebyScene = $"{gameObject.scene.name}_{gameObject.name}";
    }


}
