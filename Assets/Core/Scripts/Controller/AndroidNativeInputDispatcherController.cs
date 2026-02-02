using System;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using static Game.Config.Config;
#endif

public class AndroidNativeInputDispatcher : MonoBehaviour
{
    private static readonly Stack<Action> stack = new();

    public static void Push(Action handler)
        => stack.Push(handler);

    public static void Pop(Action handler)
    {
        if (stack.Count > 0 && stack.Peek() == handler)
            stack.Pop();
    }

    public static bool Handle()
    {
        if (stack.Count == 0) return false;
        stack.Pop()?.Invoke();
        return true;
    }

    private void Awake()
    {
        ServiceLocator.ReInit();
    }
    private void LateUpdate()
    {
        bool escapePressed = false;

#if ENABLE_INPUT_SYSTEM
        // Use new Input System if available
        escapePressed = Keyboard.current?.escapeKey.wasPressedThisFrame == true;
#else
        // Fallback to legacy input
        escapePressed = Input.GetKeyDown(KeyCode.Escape);
#endif

        if (!escapePressed) return;

        if (!Handle())
        {
            // default action: change to last current scene using SceneScriptManager
            // Intent: pressing escape should transition back to the previous scene state
            // Caller assumes existence of SceneScriptManager and its _lastCurrentScene member.
            try
            {
                if (ServiceLocator.Get<SceneScriptManager>()._lastCurrentState == SceneState.Menu)
                {
                    ServiceLocator.Get<SceneScriptManager>().ChangeState(ServiceLocator.Get<SceneScriptManager>()._lastCurrentState);
                }
                else
                {
                    ServiceLocator.Get<ActionManager>().DoEscapeLastAction();
                }
            }
            catch (Exception)
            {
                // If SceneScriptManager is not present or call fails, fall back to quitting the application.
                Application.Quit();
            }
        }
    }
}
