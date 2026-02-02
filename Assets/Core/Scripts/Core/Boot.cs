using Assets.CoreFramework.Scripts.Core;
using UnityEngine;
using static Game.Config.Config;

public class Boot : BaseImportantReuseableComponent
{
    protected override void Awake()
    {
        base.Awake();
        Debug.Log("[Boot] Initializing framework...");

        Debug.Log("[Boot] All managers and service INSTANCE registered.");
        ServiceLocator.Get<SceneScriptManager>().ChangeState(SceneState.SplashScreen);
    }
}
