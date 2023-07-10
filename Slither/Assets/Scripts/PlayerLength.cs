using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

public class PlayerLength : NetworkBehaviour
{
    public NetworkVariable<ushort> length = new(1);
    private List<GameObject> _tails;
    [SerializeField] private GameObject _tailPrefab;
    private Transform _lastTransform;

    [CanBeNull] public static event System.Action<ushort> ChangedLengthEvent;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _tails = new List<GameObject>();
        _lastTransform = transform;

        if (!IsServer) length.OnValueChanged += LengthChangedEvent;
    }

    private void LengthChanged()
    {
        InstantiateTail();

        if (!IsOwner) return;

        ChangedLengthEvent?.Invoke(length.Value);
    }

    private void LengthChangedEvent(ushort prevValue,ushort newVal)
    {
        Debug.Log("LengthChanged");
        LengthChanged();
    }

    [ContextMenu("AddLength")]
    public void AddLength()
    {
        length.Value++;
        LengthChanged();
    }

    private void InstantiateTail()
    {
        GameObject tailGameObject = Instantiate(_tailPrefab, transform.position, Quaternion.identity);

        tailGameObject.GetComponent<SpriteRenderer>().sortingOrder = -length.Value;

        if(tailGameObject.TryGetComponent(out Tail tail))
        {
            tail._networkOwner = transform;
            tail._follow = _lastTransform;
            _lastTransform = tailGameObject.transform;
            Physics2D.IgnoreCollision(tailGameObject.GetComponent<CircleCollider2D>(), GetComponent<CircleCollider2D>());
        }

        _tails.Add(tailGameObject);
    }

}
