using Unity.Netcode;
using UnityEngine;

public class Food : NetworkBehaviour
{
    public GameObject prefab;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (!NetworkManager.Singleton.IsServer) return;

        if(collision.TryGetComponent(out PlayerLength playerLen))
        {
            playerLen.AddLength();
        }
        else if(collision.TryGetComponent(out Tail tail))
        {
            tail._networkOwner.GetComponent<PlayerLength>().AddLength();
        }

        NetworkObjectPool.Singleton.ReturnNetworkObject(NetworkObject,prefab);
        NetworkObject.Despawn();
    }
}
