namespace UniNav.Core 
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Debug = UnityEngine.Debug;
    using TMPro;
    using System.Collections.Generic;
    using System.Diagnostics;

    //[RequireComponent(typeof(ARTrackedImageManager))]
    public class SceneAlign : MonoBehaviour {

        [SerializeField] private Transform buildingScanRoot;

        [Header("Rig")]
        [SerializeField] private Transform xrOrigin;

        [Header("UI Control")]
        [SerializeField] private GameObject navigationUI;
        [SerializeField] private GameObject scanUI;

        [SerializeField] private TMP_Dropdown startLocationDropdown;
        [SerializeField] private Transform[] startAnchors;

        [Header("Debug Settings")]
        [SerializeField] private Transform xrCamera;

        private Dictionary<string, Transform> _anchorByName = new Dictionary<string, Transform>();
        /* QR-CODE SCANNING ATTEMPT */
        //private ARTrackedImageManager _imageManager;

        private void Start() {
            BuildAnchorLookup();
        #if UNITY_EDITOR
            DebugForceAlign();
        #else
            DebugForceAlign();
            if (scanUI != null) scanUI.SetActive(true);
            if (navigationUI != null) navigationUI.SetActive(false);
        #endif
        }

        private void Update() {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) {
                DebugForceAlign();
            }
        }

        // Map anchor GameObject name to its transform for order-independent lookup
        private void BuildAnchorLookup() {
            _anchorByName.Clear();
            foreach (Transform anchor in startAnchors) {
                if (anchor == null) continue;
                if (!_anchorByName.ContainsKey(anchor.name)) _anchorByName.Add(anchor.name, anchor);
                else Debug.LogWarning($"Duplicate start anchor name '{anchor.name}'.");
            }
        }

        // Re-base XR rig so the selected start anchor maps onto the real pose.
        // Building and NavMesh stay fixed; only the rig moves.
        public void AlignRigToStart(string startName) {
            Debug.Log($"AlignRigToStart: {startName}");
            if (xrOrigin == null || xrCamera == null) { Debug.LogError("xrOrigin or xrCamera not assigned!"); return; }
            if (!_anchorByName.TryGetValue(startName, out Transform anchor)) {
                Debug.LogError($"No start anchor named '{startName}'."); return;
            }

            Vector3 camFwd = xrCamera.forward; camFwd.y = 0f;
            Vector3 anchorFwd = anchor.forward; anchorFwd.y = 0f;
            if (camFwd.sqrMagnitude < 1e-4f || anchorFwd.sqrMagnitude < 1e-4f) return;
            camFwd.Normalize();
            anchorFwd.Normalize();
            float yaw = Vector3.SignedAngle(camFwd, anchorFwd, Vector3.up);

            xrOrigin.RotateAround(xrCamera.position, Vector3.up, yaw);

            Vector3 delta = anchor.position - xrCamera.position;
            delta.y = 0f;
            xrOrigin.position += delta;

            if (scanUI != null) scanUI.SetActive(false);
            if (navigationUI != null) navigationUI.SetActive(true);
        }

        /* QR-CODE SCANNING ATTEMPT */

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

        /* QR-CODE SCANNING ATTEMPT */

        //private void AlignBuilding(Transform detectedImage) {
        //    Quaternion rotationOffset = detectedImage.rotation * Quaternion.Inverse(virtualAnchor.localRotation);
        //    buildingScanRoot.rotation = rotationOffset;

        //    Vector3 positionOffset = detectedImage.position - (rotationOffset * virtualAnchor.localPosition);
        //    buildingScanRoot.position = positionOffset;

        //    buildingScanRoot.gameObject.SetActive(true);

        //    if (scanUI != null) {
        //        scanUI.SetActive(false);
        //    }
        //    if (navigationUI != null) {
        //        navigationUI.SetActive(true);
        //    }
        //}


        public void DebugForceAlign() {
            buildingScanRoot.position = Vector3.zero;
            buildingScanRoot.rotation = Quaternion.Euler(0f, 0f, 0f);

            buildingScanRoot.gameObject.SetActive(true);

            if (navigationUI != null) {
                navigationUI.SetActive(true);
            }

            Debug.Log("Align: Building snapped to Vector3.zero and rotation zeroed out.");
        }

    }
}
    
