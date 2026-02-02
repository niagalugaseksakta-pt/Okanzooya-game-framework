using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.IAP; // Pastikan sudah install IAP dari Package Manager
using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    async void Start()
    {
        try
        {
            // 1. Inisialisasi semua layanan Unity 6
            await UnityServices.InitializeAsync();
            Debug.Log("Unity Services Initialized");

            // 2. Sign In (Bisa otomatis login ke profil Google Play di Android)
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed In as: " + AuthenticationService.Instance.PlayerId);
            }

            // 3. Hubungkan Player ID ke IAP
            InitializeIAP(AuthenticationService.Instance.PlayerId);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Initialization Failed: " + e.Message);
        }
    }

    void InitializeIAP(string playerId)
    {
        // Beritahu sistem IAP bahwa pembelinya adalah ID ini
        // Ini akan berguna saat Anda cek di Dashboard Unity
        Debug.Log("IAP Ready for Player: " + playerId);
    }
}