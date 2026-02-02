using UnityEngine;
using UnityEngine.Advertisements;

public class BannerAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] private string androidAdUnitId = "Banner_Android";
    [SerializeField] private string iosAdUnitId = "Banner_iOS";

    private string adUnitId;

    private void Awake()
    {
        //#if UNITY_IOS
        //        adUnitId = iosAdUnitId;
        //#elif UNITY_ANDROID
        //        adUnitId = androidAdUnitId;
        //#elif UNITY_EDITOR
        adUnitId = androidAdUnitId;
        //#endif

        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
    }

    public void LoadBannerAd()
    {
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = BannerLoaded,
            errorCallback = BannerLoadedError
        };

        Advertisement.Banner.Load(adUnitId, options);
    }

    public void ShowBannerAd()
    {
        BannerOptions options = new BannerOptions
        {
            showCallback = BannerShown,
            clickCallback = BannerClicked,
            hideCallback = BannerHidden
        };
        Advertisement.Banner.Show(adUnitId, options);

    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
    }


    #region Show Callbacks
    private void BannerHidden()
    {
        Debug.Log("Banner By Options Hidden");
    }

    private void BannerClicked()
    {
        Debug.Log("Banner By Options Clicked");
    }

    private void BannerShown()
    {
        Debug.Log("Banner By Options Show");
    }
    #endregion

    #region Load Callbacks
    private void BannerLoadedError(string message)
    {
        Debug.Log($"Banner Ad Load error {message}");
    }

    private void BannerLoaded()
    {
        Debug.Log("Banner Ad Loaded");
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log($"Banner Loaded ");
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Banner AdsFailedToLoad {error} ");
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Banner AdsShowFailure {error} ");
    }

    public void OnUnityAdsShowStart(string placementId)
    {

    }

    public void OnUnityAdsShowClick(string placementId)
    {

    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {

    }
    #endregion


}
