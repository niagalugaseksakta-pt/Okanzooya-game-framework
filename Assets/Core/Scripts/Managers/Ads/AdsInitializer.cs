using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    [SerializeField] protected string _androidGameId = "5966844";
    [SerializeField] protected string _iOSGameId = "5966845";
    [SerializeField] bool _testMode = true;
    private string _gameId;

    void Awake()
    {
        InitializeAds();
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeAds()
    {
#if UNITY_IOS
    _gameId = _iOSGameId;
#elif UNITY_ANDROID
        _gameId = _androidGameId;
#else
    _gameId = _androidGameId;
#endif

        MetaData userMetaData = new MetaData("user");
        userMetaData.Set("nonbehavioral", "true");
        Advertisement.SetMetaData(userMetaData);
        if (!Advertisement.isInitialized)
        {
            if (!Advertisement.isInitialized && Advertisement.isSupported)
            {
                Advertisement.Initialize(_gameId, _testMode, this);
            }
        }
    }


    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}
