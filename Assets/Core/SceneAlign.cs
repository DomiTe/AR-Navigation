namespace UniNav.Core 
{
    using UnityEngine;
    using UnityEngine.XR.ARFoundation;

    [RequireComponent(typeof(ARTrackedImageManager))]
    public class SceneAlign : MonoBehaviour {

        [SerializeField] private Transform buildingScanRoot;
        [SerializeField] private Transform virtualAnchor;
        private ARTrackedImageManager _imageManager;

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
        }
    }
}
    
