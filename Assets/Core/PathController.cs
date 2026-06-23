using UnityEngine;
using UnityEngine.AI;

namespace UniNav.Core {
    public class PathController : MonoBehaviour {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform xrCamera;
        [SerializeField] private Transform buildingRoot;

        private NavMeshPath _path;
        private Vector3 _cameraOffset = Vector3.zero;
        //private Vector3 _startPos;
        //private bool _hasStart = false;

        private Vector3 _targetPos;
        private bool _hasTarget = false;

        private float _lastRecalcTime = 0f;
        private const float RecalcInterval = 0.5f;


        private void Start() {
            _path = new NavMeshPath();
            if (xrCamera == null) Debug.LogError("XR CAMERA NOT ASSIGNED");
            if (buildingRoot == null) Debug.LogError("BUILDING ROOT NOT ASSIGNED");
            if (line == null) Debug.LogError("LINE RENDERER NOT ASSIGNED");

            NavMeshTriangulation tri = NavMesh.CalculateTriangulation();
            Debug.Log($"NavMesh check: {tri.vertices.Length} vertices found on device.");
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

            Vector3 worldStart = buildingRoot.TransformPoint(localCoordinates);
            Vector3 cameraPos = xrCamera.position;
            _cameraOffset = new Vector3(
                worldStart.x - cameraPos.x,
                0f,
                worldStart.z - cameraPos.z
                );

            //_startPos = buildingRoot.TransformPoint(localCoordinates);
            //_hasStart = false;

            Debug.Log($"Camera offset set: {_cameraOffset}");
        }

        public void SetTarget(Vector3 localCoordinates) {
            if (buildingRoot == null) {
                Debug.LogError("Building Root missing!");
                return;
            }
            line.positionCount = 0;
            _targetPos = buildingRoot.TransformPoint(localCoordinates);
            _hasTarget = true;
            //_hasStart = false;
            _cameraOffset = Vector3.zero;
            Debug.Log($"Target set in world space: {_targetPos}");
        }

        private void CalculateAndDrawPath() {
            if (Time.time - _lastRecalcTime < RecalcInterval) return;
            _lastRecalcTime = Time.time;

            if (xrCamera == null || line == null) return;
            Vector3 cameraPos = xrCamera.position;
            Vector3 startPos = new Vector3(
                cameraPos.x + _cameraOffset.x,
                1.89f,
                cameraPos.z + _cameraOffset.z
            );

            //Vector3 startPos;
            //if (_hasStart) {
            //    startPos = _startPos; // from dropdown selection
            //    Debug.Log($"Using manual start: {startPos}");
            //}
            //else {
            //    Vector3 cameraPos = xrCamera.position;
            //    startPos = new Vector3(cameraPos.x + _cameraOffset.x, buildingRoot.position.y, cameraPos.z + _cameraOffset.z);
            //    Debug.Log($"Using camera start: {startPos}");
            //}

            //if (Physics.Raycast(cameraPos, Vector3.down, out RaycastHit hit, 10f)) {
            //    startPos = hit.point;
            //}
            //else {
            //    startPos = new Vector3(cameraPos.x, buildingRoot.position.y, cameraPos.z);
            //}

            NavMeshHit startHit;
            if (!NavMesh.SamplePosition(startPos, out startHit, 5f, NavMesh.AllAreas)) {
                Debug.LogWarning($"Start {startPos} not on NavMesh.");
                return;
            }

            NavMeshHit targetHit;
            if (!NavMesh.SamplePosition(_targetPos, out targetHit, 2f, NavMesh.AllAreas)) {
                Debug.LogError($"Target {_targetPos} not on NavMesh.");
                _hasTarget = false;
                return;
            }

            if (NavMesh.CalculatePath(startHit.position, targetHit.position, NavMesh.AllAreas, _path)) {
                if (_path.status == NavMeshPathStatus.PathComplete) {
                    Vector3[] corners = _path.corners;
                    for (int i = 0; i < corners.Length; i++)
                        corners[i].y += 0.10f;
                    line.positionCount = corners.Length;
                    line.SetPositions(corners);
                    Debug.Log($"Path drawn: {corners.Length} corners.");

                    //_hasStart = false; // comment out to disable camera tracking
                }
                else {
                    Debug.LogWarning($"Path status: {_path.status}");
                    _hasTarget = false;
                }
            }
            else {
                Debug.LogError("CalculatePath returned false.");
                _hasTarget = false;
            }
        }
    }
}