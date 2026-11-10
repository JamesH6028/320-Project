using System;
using UnityEngine;

public class KillObject : MonoBehaviour
{
    [SerializeField]
    private float _deathPlaneHeight = -100;
    [SerializeField]
    private bool _enableDeathDistance = true;
    [SerializeField]
    private float _deathDistanceFromPlayer = 5;

    public enum OnKillAction
    {
        none,
        destroy,
        disable,
        respawn
    }
    [SerializeField]
    private OnKillAction _onKillAction = OnKillAction.respawn;

    [SerializeField]
    private RespawnFields _respawn;
    [Serializable]
    private class RespawnFields
    {
        public bool _respawnPositionIsLocal = true;
        public Vector3 _respawnPosition = Vector3.zero;
        public Quaternion _respawnRotation = Quaternion.identity;
#if UNITY_EDITOR
        public EditorOnly _editorOnly;
        [Serializable]
        public class EditorOnly
        {
            public MeshFilter[] _objectMeshes;
        }
#endif
    }

    private Rigidbody _rb;
    private PlayerController _playerController;

    public delegate void OnKillHandler();
    public event OnKillHandler OnKill;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerController = FindAnyObjectByType<PlayerController>();

        if (_respawn._respawnPositionIsLocal)
        {
            _respawn._respawnPosition = transform.TransformPoint(_respawn._respawnPosition);
        }
    }

    void Update()
    {
        if (
            transform.position.y < _deathPlaneHeight && (
                !_enableDeathDistance ||
                Vector3.Distance(_playerController.PlayerCollider.transform.position, transform.position) > _deathDistanceFromPlayer))
        {
            OnKill?.Invoke();
            switch (_onKillAction)
            {
                case OnKillAction.destroy:
                    Destroy(gameObject);
                    break;
                case OnKillAction.disable:
                    gameObject.SetActive(false);
                    break;
                case OnKillAction.respawn:
                    Respawn();
                    break;
            }
        }
    }

    private void Respawn()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(_respawn._respawnPosition, _respawn._respawnRotation);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.darkRed;
        if (_enableDeathDistance)
        {
            Gizmos.DrawWireSphere(transform.position, _deathDistanceFromPlayer);
        }
        Gizmos.DrawWireCube(
            new Vector3(
                transform.position.x,
                _deathPlaneHeight,
                transform.position.z),
            new Vector3(10, 0, 10));
#if UNITY_EDITOR
        if (_onKillAction == OnKillAction.respawn)
        {
            Gizmos.color = Color.cyan;
            foreach (MeshFilter mesh in _respawn._editorOnly._objectMeshes)
            {
                if (mesh == null)
                {
                    continue;
                }
                Gizmos.DrawWireMesh(
                    mesh.sharedMesh,
                    _respawn._respawnPositionIsLocal && !Application.isPlaying
                        ? transform.TransformPoint(_respawn._respawnPosition)
                        : _respawn._respawnPosition,
                    _respawn._respawnRotation,
                    mesh.transform.lossyScale
                    );
            }
        }
#endif
    }
}
