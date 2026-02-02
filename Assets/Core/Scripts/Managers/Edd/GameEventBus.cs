using System;
using System.Collections.Generic;
using UnityEngine;

public class GameEventBus : MonoBehaviour
{
    private Dictionary<Type, Delegate> _events; // no `readonly` if you want to reinit

    public static GameEventBus _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("[GameEventBus] Duplicate instance detected. Destroying this one.");
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // ✅ This is the "registration" of your event dictionary
        _events = new Dictionary<Type, Delegate>();

        DontDestroyOnLoad(gameObject);
        Debug.Log("[GameEventBus] Initialized event dictionary.");
    }

    public static GameEventBus Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("GameEventBus");
                _instance = go.AddComponent<GameEventBus>();
                DontDestroyOnLoad(go);
                Debug.Log("[GameEventBus] Created persistent instance.");
            }
            return _instance;
        }
    }

    public void Subscribe<T>(Action<T> listener)
    {
        if (listener == null) return;
        if (_events.TryGetValue(typeof(T), out var existing))
            _events[typeof(T)] = Delegate.Combine(existing, listener);
        else
            _events[typeof(T)] = listener;
    }

    public void Unsubscribe<T>(Action<T> listener)
    {
        if (listener == null) return;
        if (_events.TryGetValue(typeof(T), out var existing))
        {
            var updated = Delegate.Remove(existing, listener);
            if (updated == null)
                _events.Remove(typeof(T));
            else
                _events[typeof(T)] = updated;
        }
    }

    public void Publish<T>(T evt)
    {
        if (evt == null)
        {
            Debug.LogWarning($"[GameEventBus] Tried to publish null event of type {typeof(T)}");
            return;
        }

        if (_events.TryGetValue(typeof(T), out var del))
        {
            try { ((Action<T>)del)?.Invoke(evt); }
            catch (Exception ex)
            {
                Debug.LogError($"[GameEventBus] Exception while invoking {typeof(T)}: {ex}");
            }
        }
    }

    public void Clear() => _events.Clear();
}
