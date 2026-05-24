namespace UniNav.Core 
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using UnityEngine.XR.ARFoundation;

    [RequireComponent(typeof(ARTrackedImageManager))]
    public class SceneAlign : MonoBehaviour {

        [SerializeField] private Transform buildingScanRoot;
        [SerializeField] private Transform virtualAnchor;

        [Header("UI Control")]
        [SerializeField] private GameObject navigationUI;

        [Header("Debug Settings")]
        [SerializeField] private Transform xrCamera;

        private ARTrackedImageManager _imageManager;

        private void Start() {
            DebugForceAlign();
        }

        private void Update() {
            // Polling hardware directly per Input System guidelines
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) {
                DebugForceAlign();
            }
        }

        private void Awake() {
            _imageManager = GetComponent<ARTrackedImageManager>();
        }

        private void OnEnable() {
            _imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        }

        private void OnDisable() {
            _imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        }
        private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs) {
            foreach (var addedImage in eventArgs.added) {
                AlignBuilding(addedImage.transform);
            }

            foreach (var updatedImage in eventArgs.updated) {
                if (updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking) {
                    AlignBuilding(updatedImage.transform);
                }
            }
        }

        private void AlignBuilding(Transform detectedImage) {
            Quaternion rotationOffset = detectedImage.rotation * Quaternion.Inverse(virtualAnchor.localRotation);
            buildingScanRoot.rotation = rotationOffset;

            Vector3 positionOffset = detectedImage.position - (rotationOffset * virtualAnchor.localPosition);
            buildingScanRoot.position = positionOffset;

            buildingScanRoot.gameObject.SetActive(true);

            if (navigationUI != null) {
                navigationUI.SetActive(true);
            }
        }

        public void DebugForceAlign() {
            // 1. Force the building to absolute zero to perfectly match JSON coordinates
            buildingScanRoot.position = Vector3.zero;
            buildingScanRoot.rotation = Quaternion.identity;

            // 2. Activate the systems
            buildingScanRoot.gameObject.SetActive(true);

            if (navigationUI != null) {
                navigationUI.SetActive(true);
            }

            Debug.Log("Bulletproof Align: Building snapped to Vector3.zero and rotation zeroed out.");
        }

    }
}
    
