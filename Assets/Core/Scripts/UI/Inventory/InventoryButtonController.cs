using Inventory.UI;
using UnityEngine;

public class InventoryButtonController : MonoBehaviour
{
    [SerializeField] private GameObject panelInHirarchy;
    [SerializeField] private UIInventoryPage panel_child;

    private void Awake()
    {
        ServiceLocator.ReInit();
        ServiceLocator.Get<ActionManager>().AddAction(gameObject, gameObject.name);
        DontDestroyOnLoad(gameObject);
    }
    public void OpenPanel()
    {
        panelInHirarchy.SetActive(true);
        panel_child.ResetSelectionInPanel();
    }

    public void ClosePanel()
    {
        panelInHirarchy.SetActive(false);
        panel_child.ResetToHideActionPanel();
    }
}
