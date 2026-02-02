using UnityEngine;
using UnityEngine.Advertisements;

public class InterstitialAdManager : MonoBehaviour,
    IUnityAdsLoadListener,
    IUnityAdsShowListener
{
    [Header("Placement IDs")]
    [SerializeField] private string androidAdUnitId = "Interstitial_Android";
    [SerializeField] private string iosAdUnitId = "Interstitial_iOS";

    private string adUnitId;
    private bool isLoaded;

    private void Start()
    {
//#if UNITY_IOS
//        adUnitId = iosAdUnitId;
//#elif UNITY_ANDROID
//        adUnitId = androidAdUnitId;
//#elif UNITY_EDITOR
      adUnitId = androidAdUnitId;  
//#endif

        if (string.IsNullOrEmpty(adUnitId))
        {
            Debug.LogError("❌ Interstitial Ad Unit ID is missing.");
            return;
        }

        InvokeRepeating(nameof(TryLoad), 0f, 1f);
    }

    public void TryLoad()
    {
        if (!Advertisement.isInitialized || isLoaded)
            return;

        Advertisement.Load(adUnitId, this);
    }

    public void ShowInterstitialAd()
    {
        if (!isLoaded)
        {
            Debug.LogWarning("⚠ Interstitial ad not ready.");
            return;
        }

        Advertisement.Show(adUnitId, this);
    }

    #region Load Callbacks
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != adUnitId) return;

        isLoaded = true;
        Debug.Log("✅ Interstitial Ad Loaded");
        CancelInvoke(nameof(TryLoad));
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"❌ Interstitial Load Failed: {error} - {message}");
    }
    #endregion

    #region Show Callbacks
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"❌ Interstitial Show Failed: {error} - {message}");
        isLoaded = false;
        TryLoad();
    }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState state)
    {
        isLoaded = false;
        TryLoad();
        Debug.Log("ℹ Interstitial finished");
    }
    #endregion
}
