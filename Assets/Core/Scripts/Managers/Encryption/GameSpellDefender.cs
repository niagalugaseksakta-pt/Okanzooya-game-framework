using System;
using UnityEngine;

public static class GameSpellDefender<T> where T : IEncryptionManager, new()
{
    private static readonly T _encryption = new();

    public static TResult Secure<TResult>(Func<TResult> action)
    {
        if (!RuntimeCheck())
        {
            Debug.LogError("[GameGuard] Runtime integrity check failed!");
            return default;
        }

        try
        {
            _encryption.BeginSession();
            var result = action.Invoke();
            _encryption.EndSession();
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameGuard] Ignore This Exception in secured logic: File not created");
            return default;
        }
    }

    public static void Secure(Action action)
    {
        Secure(() =>
        {
            action();
            return true;
        });
    }

    private static bool RuntimeCheck()
    {
        // Example: detect tampering or illegal modifications
        if (Debug.isDebugBuild)
        {
            // allow in dev mode
            return true;
        }

        // add checks here: signature, environment, key, memory integrity
        return _encryption.VerifyKey();
    }
}
