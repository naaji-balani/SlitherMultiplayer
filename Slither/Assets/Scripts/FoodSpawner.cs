using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Unity.Netcode;

public class FoodSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _foodPrefab;
    [SerializeField] private const int maxPrefabCount = 50;

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnServerStarted += SpawnFoodStart;
    }

    private void SpawnFoodStart()
    {
        NetworkManager.Singleton.OnServerStarted -= SpawnFoodStart;
        NetworkObjectPool.Singleton.InitializePool();

        for (int i = 0; i < 30; i++)
        {
            SpawnFood();
        }

        StartCoroutine(SpawnOverTime());
    }

    private void SpawnFood()
    {
        NetworkObject obj = NetworkObjectPool.Singleton.GetNetworkObject(_foodPrefab, GetRandomPosOnMap(), Quaternion.identity);
        obj.GetComponent<Food>().prefab = _foodPrefab;
        if(!obj.IsSpawned) obj.Spawn(true);
    }

    private Vector3 GetRandomPosOnMap()
    {
        return new Vector3(Random.Range(-9f, 9f), Random.Range(-5f, 5f), 0);
    }

    IEnumerator SpawnOverTime()
    {
        while(NetworkManager.Singleton.ConnectedClients.Count > 0)
        {
            yield return new WaitForSeconds(2);
            if(NetworkObjectPool.Singleton.GetCurrentPrefabCount(_foodPrefab) < maxPrefabCount) SpawnFood();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
