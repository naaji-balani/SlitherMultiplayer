using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using JetBrains.Annotations;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;
    [CanBeNull]  public static event System.Action GameOverEvent;

    private PlayerLength _playerLength;
    private Camera _mainCamera;
    private Vector3 _mouseInput;

    private readonly ulong[] _targetClientsArray = new ulong[1];

    private void Initialize()
    {
        _mainCamera = Camera.main;
        _playerLength = GetComponent<PlayerLength>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void Update()
    {
        if (!Application.isFocused || !IsOwner) return;

        _mouseInput.x = Input.mousePosition.x;
        _mouseInput.y = Input.mousePosition.y;
        _mouseInput.z = _mainCamera.nearClipPlane;
        Vector3 mouseWorldCordinates = _mainCamera.ScreenToWorldPoint(_mouseInput);

        mouseWorldCordinates.z = 0;

        transform.position = Vector3.MoveTowards(transform.position, mouseWorldCordinates, Time.deltaTime * speed);

        //Rotate
        if(mouseWorldCordinates != transform.position)
        {
            Vector3 targetDirection = mouseWorldCordinates - transform.position;
            targetDirection.z = 0;
            transform.up = targetDirection;
        }
    }

    [ServerRpc]
    private void DetermineWinnerRpc(PlayerData player1,PlayerData player2)
    {
        if(player1.length > player2.length)
        {
            WinInformationRpc(player1.id, player2.id);
        }
        else
        {
            WinInformationRpc(player2.id, player1.id);
        }
    }

    [ServerRpc]
    private void WinInformationRpc(ulong winner,ulong loser) 
    {
        _targetClientsArray[0] = winner;

        ClientRpcParams newParams = new ClientRpcParams {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = _targetClientsArray
            }
        };

        AtePlayerClientRpc();

        _targetClientsArray[0] = loser;
        newParams.Send.TargetClientIds = _targetClientsArray;
        GameOverClientRpc(newParams);
    }

    [ClientRpc]
    private void AtePlayerClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        Debug.Log("You ate a Player");
    }

    [ClientRpc]
    private void GameOverClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!IsOwner) return;

        Debug.Log("You Loose...!");
        GameOverEvent?.Invoke();
        NetworkManager.Singleton.Shutdown();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Player Collision");
        if (!collision.gameObject.CompareTag("Player")) return;

        if (!IsOwner) return;

        if(collision.gameObject.TryGetComponent(out PlayerLength playerLen))
        {
            var player1 = new PlayerData()
            {
                id = OwnerClientId,
                length = _playerLength.length.Value
            };

            var player2 = new PlayerData()
            {
                id = _playerLength.OwnerClientId,
                length = playerLen.length.Value
            };

            DetermineWinnerRpc(player1,player2);
        }
        else if(collision.gameObject.TryGetComponent(out Tail tail))
        {
            Debug.Log("You Hit Tail");
            WinInformationRpc(tail._networkOwner.GetComponent<PlayerController>().OwnerClientId,OwnerClientId);
        }
    }

    struct PlayerData : INetworkSerializable
    {
        public ulong id;
        public ushort length;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref id);
            serializer.SerializeValue(ref length);

        }
    }
}
