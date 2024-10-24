using Cinemachine;
using Player;
using Player.ActionHandlers;
using UnityEngine;
using Utils.Singleton;

namespace Camera
{
    public class CameraHolder : DontDestroyMonoBehaviourSingleton<CameraHolder>
    {
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private CinemachineVirtualCamera vCam;
        [SerializeField] private Transform vCamFollow;
        [SerializeField] private float vCamBoundsExtension;
        
        public UnityEngine.Camera MainCamera => mainCamera;

        private ClickHandler _clickHandler;
        private Bounds _vCamBounds;

        private void Start()
        {
            _clickHandler = ClickHandler.Instance;
            _clickHandler.DragEvent += OnDrag;
        }

        private void OnDestroy()
        {
            _clickHandler.DragEvent -= OnDrag;
        }

        public void SetBounds(Bounds newBounds)
        {
            // Add extra space to the camera bounds to make panning more convenient
            newBounds.Expand(new Vector3(vCamBoundsExtension, vCamBoundsExtension, 0.0f));
            
            // Set the camera bounds to zero if they fit within the screen bounds, since we don't want to pan content that fits
            Bounds screenBounds = ScreenBounds();
            _vCamBounds = new Bounds();
            _vCamBounds.min = Vector3.Min(newBounds.min - screenBounds.min, Vector3.zero);
            _vCamBounds.max = Vector3.Max(newBounds.max - screenBounds.max, Vector3.zero);
            
            // Set the camera position to the center of the screen immediately
            FocusAtCenter();
        }

        private void OnDrag(Vector3 dragPositionDelta)
        {
            // Don't pan content if we are already connecting nodes
            if (PlayerController.PlayerState != PlayerState.None)
                return;
            
            // Pan content within camera bounds
            vCamFollow.position = _vCamBounds.ClosestPoint(vCamFollow.position + dragPositionDelta);
        }

        private void FocusAtCenter()
        {
            // To immediately set the camera position to the center of the screen,
            // we need to first disable vCam and then manually set the position
            vCam.enabled = false;
            Vector3 delta = -vCamFollow.position;
            vCamFollow.position = Vector3.zero;
            vCam.OnTargetObjectWarped(vCamFollow, delta);
            vCam.enabled = true;
        }
        
        private Bounds ScreenBounds()
        {
            float cameraHeight = mainCamera.orthographicSize * 2;
            Vector3 cameraSize = new Vector3(cameraHeight * mainCamera.aspect, cameraHeight, 0.0f);
            return new Bounds(Vector3.zero, cameraSize);
        }
    }
}