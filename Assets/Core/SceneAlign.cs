namespace UniNav.Core 
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Debug = UnityEngine.Debug;
    //using UnityEngine.XR.ARFoundation;
    using TMPro;
    //using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Diagnostics;

    //[RequireComponent(typeof(ARTrackedImageManager))]
    public class SceneAlign : MonoBehaviour {

        [SerializeField] private Transform buildingScanRoot;
        //[SerializeField] private Transform virtualAnchor;

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
            // Polling hardware directly per Input System guidelines
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

        // Re-base the XR rig so the selected start anchor maps onto the user's real pose.
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

            // Yaw that turns the camera's facing onto the anchor's facing
            float yaw = Vector3.SignedAngle(camFwd, anchorFwd, Vector3.up);

            // Pivot on the camera so the user's body stays put while the world turns
            xrOrigin.RotateAround(xrCamera.position, Vector3.up, yaw);

            // Slide the rig so the camera lands on the anchor; keep AR-driven height
            Vector3 delta = anchor.position - xrCamera.position;
            delta.y = 0f;
            xrOrigin.position += delta;

            if (scanUI != null) scanUI.SetActive(false);
            if (navigationUI != null) navigationUI.SetActive(true);
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

        //public void ConfirmStartLocation() {
        //    // get the selected dropdown option index
        //    int selectedIndex = startLocationDropdown.value;

        //    // safety check to ensure we don't pick an anchor that doesn't exist
        //    if (selectedIndex < 0 || selectedIndex >= startAnchors.Length) {
        //        Debug.LogError("Selected dropdown index has no matching Start Anchor!");
        //        return;
        //    }

        //    // get the transform of the chosen virtual anchor
        //    Transform selectedAnchor = startAnchors[selectedIndex];

        //    Vector3 camForward = xrCamera.forward;
        //    camForward.y = 0;
        //    camForward.Normalize();

        //    Vector3 anchorForward = selectedAnchor.forward;
        //    anchorForward.y = 0;
        //    anchorForward.Normalize();

        //    // find the difference between where the camera is looking and where the anchor is looking
        //    float angleOffset = Vector3.SignedAngle(camForward, anchorForward, Vector3.up);

        //    xrCamera.Rotate(xrCamera.position, Vector3.up, angleOffset, Space.World);

        //    Vector3 positionOffset = selectedAnchor.position - xrCamera.position;
        //    positionOffset.y = 0;
        //    buildingScanRoot.position += positionOffset;

        //    // swap
        //    if (scanUI != null) scanUI.SetActive(false);
        //    if (navigationUI != null) navigationUI.SetActive(true);
        //}

        public void DebugForceAlign() {
            // force the building to zero match JSON coordinates
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
    
