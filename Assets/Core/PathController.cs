namespace UniNav.core {
    using UnityEngine;
    using UnityEngine.AI;

    public class PathController : MonoBehaviour {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform target;

        private NavMeshPath _path;

        private void Start() {
            _path = new NavMeshPath();
        }
        private void Update() {
            if (target != null) { CalculateAndDrawPath(); }
        }

        private void CalculateAndDrawPath() { }
    }
}
