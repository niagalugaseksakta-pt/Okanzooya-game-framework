using UnityEngine;
using UnityEngine.Serv

public class ConsentPopupUI : MonoBehaviour
{

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show()
    {
        // Only show if consent hasn't been given before
        if (AdsConsentManager.Instance.Consent == null)
            gameObject.SetActive(true);
        else
            gameObject.SetActive(false);
    }

    public void OnAccept()
    {
        AdsConsentManager.Instance.SetUserConsent(true);
        gameObject.SetActive(false);
    }

    public void OnDecline()
    {
        AdsConsentManager.Instance.SetUserConsent(false);
        gameObject.SetActive(false);
    }
}
