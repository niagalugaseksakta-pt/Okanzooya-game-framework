using UnityEngine;

public class AdManagers : MonoBehaviour
{
    public static AdManagers Instance { get; private set; }

    [Header("Managers")]
    public BannerAds bannerAds;
    public RewardedAdManager rewardedAds;
    public InterstitialAdManager interstitialAds;


    private float timer = 120f;
    // Prepare Ads on Awake
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        bannerAds.LoadBannerAd();
        interstitialAds.TryLoad();
        rewardedAds.TryLoad();

        DontDestroyOnLoad(gameObject);
    }

    // Testing purpose
    private void Update()
    {
        var adTimer = Time.deltaTime;
        timer -= adTimer;
        if (timer <= 0f)
        {
            interstitialAds.ShowInterstitialAd();
            bannerAds.ShowBannerAd();
            timer = 120f;
        }
    }

}
