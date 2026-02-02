using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ActionManager : LoggerBehaviourManager
{
    public static ActionManager _instance { get; private set; }
    private string _lastAction = "";
    private string _currentAction = "";
    private GameObject _lastActionGO;
    private GameObject _currentActionGO = null;

    // Store actions as tuples of (GameObject, actionName)
    private List<(GameObject Go, string ActionName)> _listAction = new();

    private void Awake()
    {
        _instance = this;
        var scene = SceneManager.GetActiveScene();

        DontDestroyOnLoad(gameObject);
    }

    public static ActionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("ActionManager");
                _instance = go.AddComponent<ActionManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[ActionManager] Created persistent instance.");
            }
            return _instance;
        }
    }

    /*
    PSEUDOCODE (detailed plan)
    1) Remove null/destroyed entries from _listAction.
    2) If list empty, return.
    3) Iterate the list from most-recent (end) to oldest (start):
       a) For each tuple (candidateGo, actionName):
          - Skip if candidateGo is null.
          - Determine name, lowercase, and whether it is "close-like" based on keywords or starts with "btn".
          - Check for Unity UI Button on candidate:
              * If Button exists and (isCloseLike OR it has persistent listeners), try invoking button.onClick and return on success.
          - Check for EventTrigger on candidate:
              * If EventTrigger exists and has triggers, prepare BaseEventData if EventSystem available.
              * First pass: look for PointerClick or PointerDown entries and invoke their callbacks (safely) and return on success.
              * Second pass (fallback for close-like names): invoke the first callback entry and return on success.
          - If any invocation occurs, break and return.
    4) If none of the list entries invoked any handler, exit silently.
    5) Catch and log exceptions per-invocation but keep iterating to next candidate if an error occurred.
    6) Do not modify order of _listAction except cleaning null entries.
    */

    public void DoEscapeLastAction()
    {
        // Clean up null/destroyed entries from the list
        _listAction.RemoveAll(t => t.Go == null);

        if (_listAction.Count == 0)
        {
            return;
        }

        // Define close-like keywords once
        var closeKeywords = new[]
        {
            "close", "closebutton", "cancel", "back", "exit", "dismiss", "ok", "okay", "submit", "x",
            "btnclose", "btn_close", "button_close", "buttoncloseinventory", "buttonclosesetting"
        };

        // Iterate from most recent to oldest
        for (int i = _listAction.Count - 1; i >= 0; i--)
        {
            var entry = _listAction[i];
            var target = entry.Go;
            if (target == null)
            {
                continue;
            }

            var name = (target.name ?? string.Empty).Trim();
            var nameLower = name.ToLowerInvariant();
            bool isCloseLike = closeKeywords.Any(k => nameLower.Contains(k));

            // Quick heuristic: if not close-like and doesn't start with "btn", still allow
            // if the GameObject actually contains clickable handlers (we will check below).
            bool candidateHasButton = target.GetComponent<Button>() != null && target.gameObject.activeInHierarchy;
            var eventTrigger = target.GetComponent<EventTrigger>();
            bool candidateHasEventTrigger = eventTrigger != null && eventTrigger.triggers != null && eventTrigger.triggers.Count > 0 && target.gameObject.activeInHierarchy;

            if (!isCloseLike && !nameLower.StartsWith("btn") && !candidateHasButton && !candidateHasEventTrigger)
            {
                // Not a candidate: skip
                continue;
            }

            // 1) Try Unity UI Button first (prefer this)
            var button = target.GetComponent<Button>();
            if (button != null)
            {
                try
                {
                    if (isCloseLike && (button.onClick != null && button.onClick.GetPersistentEventCount() > 0) && target.gameObject.activeInHierarchy)
                    {
                        button.onClick.Invoke();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[ActionManager] Error invoking Button.onClick on '{name}': {ex}");
                    // continue to attempt EventTrigger fallback on same target
                }
            }

            // 2) Try EventTrigger entries
            if (eventTrigger != null && eventTrigger.triggers != null && eventTrigger.triggers.Count > 0)
            {
                BaseEventData eventData = null;
                var es = EventSystem.current != null ? EventSystem.current : UnityEngine.Object.FindAnyObjectByType<EventSystem>();
                if (es != null)
                {
                    eventData = new BaseEventData(es);
                }

                bool invoked = false;

                // First pass: pointer-specific events
                foreach (var etEntry in eventTrigger.triggers)
                {
                    if (etEntry == null) continue;

                    if (etEntry.eventID == EventTriggerType.PointerClick || etEntry.eventID == EventTriggerType.PointerDown)
                    {
                        try
                        {
                            etEntry.callback?.Invoke(eventData);
                            invoked = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[ActionManager] Error invoking EventTrigger entry on '{name}': {ex}");
                        }

                        if (invoked)
                        {
                            return;
                        }
                    }
                }

                // Second pass: fallback to first available callback if close-like
                if (isCloseLike && target.gameObject.activeInHierarchy)
                {
                    foreach (var etEntry in eventTrigger.triggers)
                    {
                        if (etEntry == null || etEntry.callback == null) continue;

                        try
                        {
                            etEntry.callback.Invoke(eventData);
                            invoked = true;
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[ActionManager] Error invoking EventTrigger fallback on '{name}': {ex}");
                        }

                        if (invoked)
                        {
                            return;
                        }
                    }
                }
            }

            // If we reached here, this candidate didn't result in a successful invocation.
            // Continue to previous entries in the list.
        }

        // No clickable handlers found on any candidate GameObjects in the list.
    }

    public void AddAction(GameObject go, string actionName)
    {
        if (go == null)
        {
            return;
        }

        // Remove duplicates by GameObject or action name
        _listAction.RemoveAll(t => t.Go == go || (!string.IsNullOrEmpty(actionName) && t.ActionName == actionName));

        // Add to the list (most recent at the end)
        _listAction.Add((go, actionName));

        // Update current/last bookkeeping
        var temp = _currentAction;
        _lastAction = temp;
        _currentAction = actionName;

        var tempGo = _currentActionGO;
        _lastActionGO = tempGo;
        _currentActionGO = go;
    }
}

