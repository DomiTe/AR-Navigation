using UnityEngine;
using UnityEngine.AI;

namespace UniNav.Core {
    public class PathController : MonoBehaviour {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform xrCamera;
        [SerializeField] private Transform buildingRoot;

        private NavMeshPath _path;
        private Vector3 _targetPos;
        private bool _hasTarget = false;

        private void Start() {
            _path = new NavMeshPath();
        }

        private void Update() {
            if (_hasTarget) {
                CalculateAndDrawPath();
            }
        }

        public void SetTarget(Vector3 localCoordinates) {
            Debug.Log($"Path Step 1: SetTarget received local coords {localCoordinates}");
            line.positionCount = 0;
            if (buildingRoot == null) {
                Debug.LogError("CRASH POINT: Building Root is missing in PathController!");
                return;
            }

            _targetPos = buildingRoot.TransformPoint(localCoordinates);
            _hasTarget = true;

            Debug.Log($"Path Step 2: Target converted to World Space: {_targetPos}");
        }

        private void CalculateAndDrawPath() {
            if (xrCamera == null || line == null) {
                Debug.LogError("LINE OR CAMERA IS NULL");
                return;
            }

            Vector3 startPos = Vector3.zero; // Demo: building root

            // Snap start to navmesh
            NavMeshHit startHit;
            if (!NavMesh.SamplePosition(startPos, out startHit, 2f, NavMesh.AllAreas)) {
                Debug.LogError($"START POSITION {startPos} is not on NavMesh! Check bake.");
                return;
            }

            // Snap target to navmesh  
            NavMeshHit targetHit;
            if (!NavMesh.SamplePosition(_targetPos, out targetHit, 2f, NavMesh.AllAreas)) {
                Debug.LogError($"TARGET POSITION {_targetPos} is not on NavMesh! Check coordinates.");
                return;
            }

            Debug.Log($"Snapped start: {startHit.position}, Snapped target: {targetHit.position}");

            if (NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, _path)) {
                if (_path.status == NavMeshPathStatus.PathComplete) {
                    Vector3[] corners = _path.corners;
                    for (int i = 0; i < corners.Length; i++)
                        corners[i].y += 0.15f;

                    line.positionCount = corners.Length;
                    line.SetPositions(corners);
                    Debug.Log($"Path drawn with {corners.Length} corners.");
                    _hasTarget = false;
                }
                else {
                    Debug.LogWarning($"Path status: {_path.status} — is the navmesh fully connected?");
                    _hasTarget = false;
                }
            }
            else {
                Debug.LogError("NavMesh.CalculatePath returned FALSE — no path found at all.");
            }
        }
    }
}