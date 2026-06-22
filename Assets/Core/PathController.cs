using UnityEngine;
using UnityEngine.AI;

namespace UniNav.Core {
    public class PathController : MonoBehaviour {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform xrCamera;
        [SerializeField] private Transform buildingRoot;

        private NavMeshPath _path;
        private Vector3 _startPos;
        private bool _hasStart = false;
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

        public void SetStart(Vector3 localCoordinates) {
            Debug.Log($"Path Step 1: SetStart received local coords {localCoordinates}");
            //line.positionCount = 0;
            if (buildingRoot == null) {
                Debug.LogError("CRASH POINT: Building Root is missing in PathController!");
                return;
            }

            _startPos = buildingRoot.TransformPoint(localCoordinates);
            _hasStart = true;

            Debug.Log($"Path Step 2: Start converted to World Space: {_startPos}");
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

            if (_hasStart == false)
            {
                Debug.Log("START POSITION NOT SET, using camera position as start.");
                _startPos = xrCamera.position; // use the field, not a local variable
                _hasStart = true;
            }
            else
            {
                Debug.Log("Using provided start position.");
            }

            // Snap target to navmesh  
            NavMeshHit targetHit;
            if (!NavMesh.SamplePosition(_targetPos, out targetHit, 2f, NavMesh.AllAreas)) {
                Debug.LogError($"TARGET POSITION {_targetPos} is not on NavMesh! Check coordinates.");
                _hasTarget = false;
                return;
            }
            else 
            {
                Debug.Log($"Snapped target: {targetHit.position}");
            }
            
            // Snap start to navmesh
            NavMeshHit startHit;
            Debug.Log($"StartPos: {_startPos}");
            if (!NavMesh.SamplePosition(_startPos, out startHit, 5f, NavMesh.AllAreas)) {
                Debug.LogError($"START POSITION {_startPos} is not on NavMesh! Check bake.");
                Debug.Log($" DISTANCE: dist={Vector3.Distance(_startPos, startHit.position)}  at {startHit.position}");
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
                }
                else {
                    Debug.LogWarning($"Path status: {_path.status} � is the navmesh fully connected?");
                    _hasTarget = false;
                }
            }
            else {
                Debug.LogError("NavMesh.CalculatePath returned FALSE � no path found at all.");
                _hasTarget = false;
            }
        }
    }
}