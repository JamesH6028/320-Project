using System;
using UnityEngine;

public class GrabObject : MonoBehaviour
{
    [SerializeField]
    private bool _hasKillPlane;
    [SerializeField]
    private KillPlane _killPlane;

    [Serializable]
    private class KillPlane
    {
        public float _killPlaneHeight = -100;
        public float _killRadiusFromPlayer = 5;
        public Vector3 _respawnPosition;
        public Quaternion _respawnRotation;
#if UNITY_EDITOR
        public MeshFilter[] _objectMeshes;
#endif
    }

    private Rigidbody _rb;
    private PlayerController _playerController;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _playerController = FindAnyObjectByType<PlayerController>();
    }

    void Update()
    {
        if(
            _hasKillPlane &&
            transform.position.y < _killPlane._killPlaneHeight &&
            Vector3.Distance(_playerController.PlayerCollider.transform.position, transform.position) > _killPlane._killRadiusFromPlayer)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(_killPlane._respawnPosition, _killPlane._respawnRotation);
    }

    void OnDrawGizmosSelected()
    {
        if (_hasKillPlane)
        {
            Gizmos.color = Color.darkRed;
            Gizmos.DrawWireSphere(transform.position, _killPlane._killRadiusFromPlayer);
            Gizmos.DrawWireCube(
                new Vector3(
                    transform.position.x,
                    _killPlane._killPlaneHeight,
                    transform.position.z),
                new Vector3(10, 0, 10));
#if UNITY_EDITOR
            Gizmos.color = Color.cyan;
            foreach (MeshFilter mesh in _killPlane._objectMeshes)
            {
                Gizmos.DrawWireMesh(
                    mesh.sharedMesh,
                    _killPlane._respawnPosition,
                    _killPlane._respawnRotation,
                    mesh.transform.lossyScale
                    );
            }
#endif
        }
    }
}
