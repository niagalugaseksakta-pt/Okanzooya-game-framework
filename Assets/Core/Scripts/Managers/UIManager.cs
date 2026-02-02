using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("References")]
    public GameObject loadingPanel;
    public GameObject popupPanel;


    public static void Init()
    {
        if (Instance == null)
        {
            // Instance = gameObject.AddComponent<UIManager>();

            Debug.Log("[UIManager] Initialized");
        }
    }

    public void ShowLoading(bool state)
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(state);
    }

    public void ShowPopup(string message)
    {
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            popupPanel.GetComponentInChildren<UnityEngine.UI.Text>().text = message;
        }
    }
}
