using Game.Model.Player;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarUi : MonoBehaviour
{
    [Header("Entity Reference")]
    public PlayerEntity playerEntity;

    [Header("UI Settings")]
    public GameObject segmentPrefab;
    public Transform container;
    public int maxSegments = 10;         // jumlah icon maksimal di healthbar

    private List<GameObject> segments = new List<GameObject>();
    private float hpPerSegment;
    private int lastCurrentHealth = -1;
    private int lastMaxHealth = -1;


    private void Awake()
    {
        if (playerEntity == null)
        {
            playerEntity = FinderTagHelper.FindPlayer<PlayerEntity>().GetComponent<PlayerEntity>();
        }

        maxSegments = playerEntity.Stats.MaxHealth;

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (playerEntity != null)
            BuildSegments();
    }

    void Update()
    {
        if (playerEntity == null) return;

        // Jika MaxHealth berubah, rebuild
        if (playerEntity.Stats.MaxHealth != lastMaxHealth)
            BuildSegments();

        // Jika CurrentHealth berubah, refresh
        if (playerEntity.Stats.CurrentHealth != lastCurrentHealth)
            RefreshSegments();
    }

    /// <summary>
    /// Membuat segmen berdasarkan maxSegments
    /// </summary>
    private void BuildSegments()
    {
        foreach (var s in segments)
            Destroy(s);

        segments.Clear();

        if (segmentPrefab == null || container == null)
        {
            Debug.LogWarning("HealthBarUi: prefab atau container belum di-set.");
            return;
        }

        // HP per segmen
        hpPerSegment = (float)playerEntity.Stats.MaxHealth / maxSegments;
        lastMaxHealth = playerEntity.Stats.MaxHealth;

        // Buat icon sesuai maxSegments
        for (int i = 0; i < maxSegments; i++)
        {
            GameObject seg = Instantiate(segmentPrefab, container);
            seg.SetActive(true);
            segments.Add(seg);
        }

        RefreshSegments();
    }

    /// <summary>
    /// Mengaktifkan segmen berdasarkan pembagian proporsional
    /// </summary>
    private void RefreshSegments()
    {
        int current = Mathf.Clamp(playerEntity.Stats.CurrentHealth, 0, playerEntity.Stats.MaxHealth);

        // hitung jumlah segmen aktif
        int active = Mathf.CeilToInt(current / hpPerSegment);

        // clamp
        active = Mathf.Clamp(active, 0, maxSegments);

        for (int i = 0; i < segments.Count; i++)
            segments[i].SetActive(i < active);

        lastCurrentHealth = current;
    }
}
