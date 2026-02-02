using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();

    // =========================================================
    // INIT
    // =========================================================
    public static void Init()
    {
        Register(GameEventBus.Instance);
        Register(NetworkManager.Instance);
        Register(SceneScriptManager.Instance);
        Register(EncryptionManager.Instance);
        Register(LoadingManager.Instance);
        Register(ResourceManager.Instance);
        Register(LocalizationManager.Instance);
        Register(InventoryManager.Instance);
        Register(SaveManager.Instance);
        Register(AdsConsentManager.Instance);
        Register(ActionManager.Instance);

        Debug.Log("[ServiceLocator] Core services initialized.");
    }

    public static void ReInit()
    {
        GetOrReinit(() => GameEventBus.Instance);
        GetOrReinit(() => NetworkManager.Instance);
        GetOrReinit(() => SceneScriptManager.Instance);
        GetOrReinit(() => EncryptionManager.Instance);
        GetOrReinit(() => LoadingManager.Instance);
        GetOrReinit(() => ResourceManager.Instance);
        GetOrReinit(() => LocalizationManager.Instance);
        GetOrReinit(() => InventoryManager.Instance);
        GetOrReinit(() => SaveManager.Instance);
        GetOrReinit(() => AdsConsentManager.Instance);
        GetOrReinit(() => ActionManager.Instance);


        // If the registry is empty or contains any null-like entries, perform a full Init().
        bool needsInit = false;

        if (services == null || services.Count == 0)
        {
            needsInit = true;
        }
        else
        {
            foreach (var val in services.Values)
            {
                // plain null check
                if (val == null || val.Equals("null"))
                {
                    needsInit = true;
                    break;
                }

                // Unity objects can appear non-null in managed memory but compare equal to null
                // after they've been destroyed. Check that explicitly.
                if (val is UnityEngine.Object uo && uo == null)
                {
                    needsInit = true;
                    break;
                }
            }
        }

        if (needsInit)
        {
            Init();
        }

        Debug.Log("[ServiceLocator] Core services initialized.");
    }

    // =========================================================
    // BASIC
    // =========================================================
    public static void Register<T>(T service)
    {
        if (service == null)
            throw new Exception($"[ServiceLocator] Register failed: {typeof(T).Name} is null.");

        services[typeof(T)] = service;
    }

    public static T Get<T>()
    {
        if (services.TryGetValue(typeof(T), out var s) && s != null)
            return (T)s;

        throw new Exception($"[ServiceLocator] Service {typeof(T).Name} not found or null.");
    }

    public static bool TryGet<T>(out T service)
    {
        if (services.TryGetValue(typeof(T), out var s) && s != null)
        {
            service = (T)s;
            return true;
        }

        service = default;
        return false;
    }

    public static void Clear() => services.Clear();

    // =========================================================
    // CORE RESOLUTION LOGIC
    // =========================================================

    /// <summary>
    /// Resolve service with:
    /// 1) existing registry
    /// 2) factory
    /// 3) forceFactory (strong fallback)
    /// 4) fail loud
    /// </summary>
    public static T GetOrReinit<T>(
        Func<T> factory,
        Func<T> forceFactory = null
    )
    {
        var type = typeof(T);

        // 1) already registered?
        if (services.TryGetValue(type, out var s) && s != null)
            return (T)s;

        if (factory == null)
            throw new Exception($"[ServiceLocator] No factory provided for {type.Name}");

        T instance = default;

        // 2) try normal factory
        try
        {
            instance = factory.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[ServiceLocator] Factory for {type.Name} threw:\n{ex}");
        }

        if (instance != null)
        {
            services[type] = instance;
            Debug.Log($"[ServiceLocator] Registered: {type.Name}");
            return instance;
        }

        // 3) try force factory
        if (forceFactory != null)
        {
            try
            {
                instance = forceFactory.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ServiceLocator] ForceFactory for {type.Name} threw:\n{ex}");
            }

            if (instance != null)
            {
                services[type] = instance;
                Debug.LogWarning($"[ServiceLocator] Force-registered: {type.Name}");
                return instance;
            }
        }

        // 4) total failure
        Debug.LogError($"[ServiceLocator] FAILED to resolve service: {type.Name}");
        return default;
    }

    // =========================================================
    // FORCE API
    // =========================================================

    /// <summary>
    /// Replace any existing instance with a new one.
    /// Fail fast if null.
    /// </summary>
    public static void ForceReRegister<T>(T newInstance)
    {
        if (newInstance == null)
            throw new Exception($"[ServiceLocator] ForceReRegister failed: {typeof(T).Name} is null.");

        services[typeof(T)] = newInstance;
        Debug.Log($"[ServiceLocator] Force re-registered: {typeof(T).Name}");
    }

    // =========================================================
    // OPTIONAL: EXPLICIT AUTO-CREATE FOR MONOBEHAVIOURS
    // (opt-in only — not used by default)
    // =========================================================
    public static T GetOrReinitWithAutoCreate<T>(Func<T> factory) where T : MonoBehaviour
    {
        return GetOrReinit(
            factory,
            () =>
            {
                var go = new GameObject(typeof(T).Name + "_Auto");
                var comp = go.AddComponent<T>();
                UnityEngine.Object.DontDestroyOnLoad(go);
                Debug.LogWarning($"[ServiceLocator] Auto-created MonoBehaviour: {typeof(T).Name}");
                return comp;
            });
    }
}
