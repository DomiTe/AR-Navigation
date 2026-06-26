namespace UniNav.Core {
    using UnityEngine;
    using UnityEngine.InputSystem;
    //using UnityEngine.XR.ARFoundation;
    using TMPro;

    //[RequireComponent(typeof(ARTrackedImageManager))]
    public class SceneAlign : MonoBehaviour {

        [SerializeField] private Transform buildingScanRoot;
        [SerializeField] private Transform virtualAnchor;

        [Header("UI Control")]
        [SerializeField] private GameObject navigationUI;
        [SerializeField] private GameObject scanUI;

        [SerializeField] private TMP_Dropdown startLocationDropdown;
        [SerializeField] private Transform[] startAnchors;

        [Header("Debug Settings")]
        [SerializeField] private Transform xrCamera;

        //private ARTrackedImageManager _imageManager;

        private void Start() {
#if UNITY_EDITOR
            DebugForceAlign();
#else
            DebugForceAlign();
            if (scanUI != null) scanUI.SetActive(true);
            if (navigationUI != null) navigationUI.SetActive(false);
#endif
        }

        private void Update() {
            // Polling hardware directly per Input System guidelines
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) {
                DebugForceAlign();
            }
        }

        //private void Awake() {
        //    _imageManager = GetComponent<ARTrackedImageManager>();
        //}

        //private void OnEnable() {
        //    _imageManager.trackablesChanged.AddListener(OnTrackablesChanged);
        //}

        //private void OnDisable() {
        //    _imageManager.trackablesChanged.RemoveListener(OnTrackablesChanged);
        //}
        //private void OnTrackablesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs) {
        //    foreach (var addedImage in eventArgs.added) {
        //        AlignBuilding(addedImage.transform);
        //    }

        //    foreach (var updatedImage in eventArgs.updated) {
        //        if (updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking) {
        //            AlignBuilding(updatedImage.transform);
        //        }
        //    }
        //}

        private void AlignBuilding(Transform detectedImage) {
            Quaternion rotationOffset = detectedImage.rotation * Quaternion.Inverse(virtualAnchor.localRotation);
            buildingScanRoot.rotation = rotationOffset;

            Vector3 positionOffset = detectedImage.position - (rotationOffset * virtualAnchor.localPosition);
            buildingScanRoot.position = positionOffset;

            buildingScanRoot.gameObject.SetActive(true);

            if (scanUI != null) {
                scanUI.SetActive(false);
            }
            if (navigationUI != null) {
                navigationUI.SetActive(true);
            }
        }

        public void ConfirmStartLocation() {
            // get the selected dropdown option index
            int selectedIndex = startLocationDropdown.value;

            // safety check to ensure we don't pick an anchor that doesn't exist
            if (selectedIndex < 0 || selectedIndex >= startAnchors.Length) {
                Debug.LogError("Selected dropdown index has no matching Start Anchor!");
                return;
            }

            // get the transform of the chosen virtual anchor
            Transform selectedAnchor = startAnchors[selectedIndex];

            Vector3 camForward = xrCamera.forward;
            camForward.y = 0;
            camForward.Normalize();

            Vector3 anchorForward = selectedAnchor.forward;
            anchorForward.y = 0;
            anchorForward.Normalize();

            // find the difference between where the camera is looking and where the anchor is looking
            float angleOffset = Vector3.SignedAngle(anchorForward, camForward, Vector3.up);

            buildingScanRoot.Rotate(0, angleOffset, 0, Space.World);

            Vector3 positionOffset = xrCamera.position - selectedAnchor.position;
            positionOffset.y = 0;
            buildingScanRoot.position += positionOffset;

            // swap
            if (scanUI != null) scanUI.SetActive(false);
            if (navigationUI != null) navigationUI.SetActive(true);
        }

        public void DebugForceAlign() {
            // force the building to zero match JSON coordinates
            buildingScanRoot.position = new Vector3(0f, -1.89f, 0f);
            buildingScanRoot.rotation = Quaternion.Euler(0f, 0f, 0f);

            buildingScanRoot.gameObject.SetActive(true);

            if (navigationUI != null) {
                navigationUI.SetActive(true);
            }

            Debug.Log("Align: Building snapped to Vector3.zero and rotation zeroed out.");
        }

    }
}
