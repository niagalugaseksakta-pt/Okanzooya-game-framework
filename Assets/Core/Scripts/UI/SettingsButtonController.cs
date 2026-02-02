using UnityEngine;

public class SettingsButtonController : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    private void Awake()
    {
        ServiceLocator.ReInit();
        ServiceLocator.Get<ActionManager>().AddAction(gameObject, gameObject.name);
        DontDestroyOnLoad(gameObject);
    }
    public void OpenPanel()
    {
        panel.SetActive(true);
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
    }
}
