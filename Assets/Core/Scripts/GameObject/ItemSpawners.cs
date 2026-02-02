using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawners : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform spawnPoint;
    //public Color[] possibleColors;
    public float spawnInterval = 0.5f;
    private Queue<GameObject> itemQueue = new Queue<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnBalls());
    }

    IEnumerator SpawnBalls()
    {
        while (true)
        {
            GameObject ball = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity);
            //Color randomColor = possibleColors[Random.Range(0, possibleColors.Length)];

            itemQueue.Enqueue(ball);
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}
