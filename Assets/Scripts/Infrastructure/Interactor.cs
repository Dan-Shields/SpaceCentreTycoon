using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure
{
    /// <summary>
    /// When something needs to create, destroy or otherwise interact with objects in the SpaceCentre and it needs to use mouse raycasts to do it, extend this class.
    /// </summary>
    public abstract class Interactor : MonoBehaviour
    {
        [Header("References")]
        public InteractionManager interactionManager;
        public CameraController cameraController;
        public Camera mainCamera;

        protected InteractionMode _interactionMode;
        protected int _layerMask = 0;
        protected bool _paused = false;
        protected bool _enabled = false;

        private void AddEventHandlers()
        {
            cameraController.OnMouseCaptureChange += MouseStateChanged;

            interactionManager.OnInteractionModeChange += CheckEnabled;
        }

        private void RemoveEventHandlers()
        {
            cameraController.OnMouseCaptureChange -= MouseStateChanged;

            interactionManager.OnInteractionModeChange -= CheckEnabled;
        }

        // MonoBehaviour start
        public void Start()
        {
            AddEventHandlers();

            Setup();
        }

        private void OnDisable()
        {
            RemoveEventHandlers();
        }

        private void OnEnable()
        {
            AddEventHandlers();
        }

        private void OnDestroy()
        {
            RemoveEventHandlers();
        }

        private void CheckEnabled(InteractionMode newMode, InteractionMode oldMode)
        {
            if (newMode == oldMode) return;

            if (newMode == _interactionMode)
            {
                _enabled = true;
                Begin();
            } else
            {
                _enabled = false;
                End();
            }
        }

        private void MouseStateChanged(bool newState)
        {
            if (!_enabled) return;

            switch (newState)
            {
                case false:
                    _paused = true;
                    Pause();
                    break;

                case true:
                    _paused = false;
                    Resume();
                    break;
            }
        }

        public void Update()
        {
            if (!_enabled || _paused) return;

            if (Mouse.current.rightButton.wasPressedThisFrame) Reset();

            Tick();

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _layerMask))
            {
                Interact(hit);
            }
        }

        private void Reset()
        {
            End();
            Begin();
        }

        /// <summary>
        /// Called at the end of the MonoBehaviour's Start
        /// </summary>
        protected abstract void Setup();

        /// <summary>
        /// Called just before the first time Interact is called.
        /// </summary>
        protected abstract void Begin();

        /// <summary>
        /// Called just after the last time Interact is called.
        /// </summary>
        protected abstract void End();

        /// <summary>
        /// Called at the start of a pause when the user starts orbiting the camera.
        /// </summary>
        protected abstract void Pause();
        
        /// <summary>
        /// Called at the end of a pause when the user finishes orbiting the camera.
        /// </summary>
        protected abstract void Resume();

        /// <summary>
        /// Called every frame which the interactor is active.
        /// </summary>
        protected abstract void Tick();

        /// <summary>
        /// Called when the interactor is active and the mouse has hit something within the pre-defined layermask.
        /// </summary>
        protected abstract void Interact(RaycastHit hit);
    }
}
