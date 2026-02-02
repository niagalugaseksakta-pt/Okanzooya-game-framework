using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIHoverHelper
{
    public static GameObject GetHoveredDropTarget()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<IDropHandler>() != null)
                return r.gameObject;
        }

        return null;
    }
}
