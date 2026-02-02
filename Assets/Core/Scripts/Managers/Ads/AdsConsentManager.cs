using UnityEngine;
using GoogleMobileAds.Ump.Api; // Untuk Google UMP
using UnityEngine.Advertisements; // Untuk Unity Ads Legacy Metadata


public class AdsConsentManager : MonoBehaviour
{
    public static AdsConsentManager Instance { get; private set; }

    private const string ConsentKey = "user_consent";

    // Properti untuk mengecek status consent yang tersimpan
    public bool? Consent => PlayerPrefs.HasKey(ConsentKey)
        ? PlayerPrefs.GetInt(ConsentKey) == 1
        : (bool?)null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // STEP 1: Panggil ini saat Game Start (Splash Screen)
    public void StartConsentProcess()
    {
        Debug.Log("[AdsConsentManager] Checking for UMP/GDPR...");

        // Konfigurasi UMP Google
        var requestParameters = new ConsentRequestParameters { TagForChildDirectedTreatment = false };

        // Update informasi consent dari server Google
        ConsentInformation.Update(requestParameters, (FormError error) =>
        {
            if (error != null)
            {
                Debug.LogError($"[UMP Error] {error.Message}");
                ShowManualConsentIfRequired(); // Fallback ke UI manual jika UMP gagal
                return;
            }

            // Jika UMP tersedia (User di wilayah GDPR/Eropa)
            if (ConsentInformation.IsConsentFormAvailable())
            {
                LoadAndShowUMP();
            }
            else
            {
                // User di wilayah Non-GDPR (seperti Indonesia)
                ShowManualConsentIfRequired();
            }
        });
    }

    private void LoadAndShowUMP()
    {
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError error) =>
        {
            if (error != null) Debug.LogError($"[UMP Show Error] {error.Message}");

            // Setelah UMP selesai (User klik Agree/Disagree), init iklan.
            // IronSource otomatis membaca sinyal UMP Google di background.
            InitializeAllAdsSystems(true);
        });
    }

    private void ShowManualConsentIfRequired()
    {
        if (!PlayerPrefs.HasKey(ConsentKey))
        {
            // MUNCULKAN UI MANUAL ANDA DI SINI
            // Contoh: UIManager.Instance.ShowConsentPanel();
            Debug.Log("[AdsConsentManager] Waiting for manual user consent...");
        }
        else
        {
            SetUserConsent(PlayerPrefs.GetInt(ConsentKey) == 1);
        }
    }

    // STEP 2: Fungsi yang dipanggil dari Button UI Manual Anda
    public void SetUserConsent(bool consent)
    {
        PlayerPrefs.SetInt(ConsentKey, consent ? 1 : 0);
        PlayerPrefs.Save();

        // A. Beritahu IronSource (Wajib)
        // Ini akan meneruskan status consent ke Google AdMob & network lainnya
        IronSource.Agent.setConsent(consent);

        // B. Beritahu Unity Ads secara spesifik
        MetaData gdpr = new MetaData("gdpr");
        gdpr.Set("consent", consent ? "true" : "false");
        Advertisement.SetMetaData(gdpr);

        Debug.Log($"[AdsConsentManager] Consent set to: {consent}");

        InitializeAllAdsSystems(consent);
    }

    private void InitializeAllAdsSystems(bool consent)
    {
        // Panggil script initializer IronSource Anda
        // Pastikan AdsInitializer menggunakan IronSource.Agent.init()
        var initializer = GetComponent<AdsInitializer>();
        if (initializer != null)
        {
            initializer.InitializeAds();
        }
    }
}