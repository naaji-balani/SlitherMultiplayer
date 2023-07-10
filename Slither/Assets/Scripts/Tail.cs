using UnityEngine;

public class Tail : MonoBehaviour
{
    public Transform _follow;
    public Transform _networkOwner;

    [SerializeField] private float _delayTime = .1f;
    [SerializeField] private float distance = .3f;

    private Vector3 _targetPos;

    private void Update()
    {
        _targetPos = _follow.position - _follow.forward * distance;
        _targetPos += (transform.position - _targetPos) * _delayTime;

        _targetPos.z = 0;

        transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * 10);
    }
}
