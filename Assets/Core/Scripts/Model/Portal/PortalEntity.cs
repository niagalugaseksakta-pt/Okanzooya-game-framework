using UnityEngine;
using static Game.Config.Config;

public class PortalEntity : EntityPortalBase
{

    [SerializeField] public PortalState nextPortalId;
    // Register each portal setted in the global registry upon awakening or active scene load
    protected override void Awake()
    {
        base.Awake();
        PortalRegistry.Register(this);
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        PortalRegistry.Unregister(this);
    }

}
