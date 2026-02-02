using Game.Config;
using Game.Model.Player;
using UnityEngine;
using UnityEngine.Advertisements;

public class RewardedAdManager : MonoBehaviour,
    IUnityAdsLoadListener,
    IUnityAdsShowListener
{
    [Header("Placement IDs")]
    [SerializeField] private string androidAdUnitId = "Rewarded_Android";
    [SerializeField] private string iosAdUnitId = "Rewarded_iOS";

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
            Debug.LogError("❌ Rewarded Ad Unit ID is missing.");
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

    public void ShowRewardedAd()
    {
        if (!isLoaded)
        {
            Debug.LogWarning("⚠ Rewarded ad not loaded yet.");
            return;
        }

        Advertisement.Show(adUnitId, this);
    }

    #region Load Callbacks
    public void OnUnityAdsAdLoaded(string placementId)
    {
        if (placementId != adUnitId) return;

        isLoaded = true;
        Debug.Log("✅ Rewarded Ad Loaded");
        CancelInvoke(nameof(TryLoad));
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogError($"❌ Rewarded Load Failed: {error} - {message}");
    }
    #endregion

    #region Show Callbacks
    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogError($"❌ Rewarded Show Failed: {error} - {message}");
        isLoaded = false;
        TryLoad();
    }

    public void OnUnityAdsShowStart(string placementId) { }

    public void OnUnityAdsShowClick(string placementId) { }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState state)
    {
        isLoaded = false;

        if (state == UnityAdsShowCompletionState.COMPLETED)
        {
            Debug.Log("🎉 Reward granted");
            // Grant reward here

            //sample
            PlayerEntity playerEntity = null;
            if (playerEntity == null)
            {
                playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
                playerEntity.Stats.CurrentHealth += 1;
            }
        }

        TryLoad();
    }
    #endregion
}
