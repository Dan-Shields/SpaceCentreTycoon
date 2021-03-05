using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Buildings
{
    [RequireComponent(typeof(BuildingManager))]
    public class BuildingCreator : Interactor
    {
        [Header("Prefabs")]
        public GameObject hqPrefab;
        public GameObject launchpadPrefab;

        private BuildingManager _buildingManager;
        private BuildingType? _buildingType = null;

        private GameObject _pendingBuilding;

        private State _currentState = State.Position;

        private void Awake()
        {
            _buildingManager = GetComponent<BuildingManager>();
        }

        protected override void Setup()
        {
            // Filter to just Floor layer
            _layerMask = 1 << 10;
            _interactionMode = InteractionMode.BuildingCreator;
        }

        protected override void Tick()
        {
            if (Keyboard.current[Key.Digit1].isPressed)
            {
                Destroy(_pendingBuilding);
                _pendingBuilding = Instantiate(hqPrefab, transform);
                _buildingType = BuildingType.HQ;
            } else if (Keyboard.current[Key.Digit2].isPressed)
            {
                Destroy(_pendingBuilding);
                _pendingBuilding = Instantiate(launchpadPrefab, transform);
                _buildingType = BuildingType.HQ;
            }
        }

        protected override void Interact(RaycastHit hit)
        {
            if (!hit.collider.CompareTag("Floor")) return;

            if (_buildingType == null) return;

            if (_currentState == State.Position)
            {
                _pendingBuilding.transform.position = hit.point;

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _currentState = State.Rotation;
                }
            } else if (_currentState == State.Rotation)
            {
                Vector3 del = hit.point - _pendingBuilding.transform.position;
                del.y = 0;

                float yAngle = Vector3.SignedAngle(Vector3.forward, del, Vector3.up);

                Vector3 rotation = new Vector3(0, yAngle, 0);

                _pendingBuilding.transform.rotation = Quaternion.Euler(rotation);

                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _buildingManager.CreateBuilding(_pendingBuilding);

                    _currentState = State.Position;
                }
            }
        }

        protected override void Begin()
        {
            _currentState = State.Position;
        }

        protected override void End()
        {
            if (_buildingType != null)
            {
                Destroy(_pendingBuilding);
                _buildingType = null;
            }
        }

        protected override void Pause()
        {
            if (_buildingType != null)
            {
                _pendingBuilding.SetActive(false);
            }
        }

        protected override void Resume()
        {
            if (_buildingType != null)
            {
                _pendingBuilding.SetActive(true);
            }
        }

        enum State
        {
            Position,
            Rotation
        }
    }

    

    enum BuildingType
    {
        HQ,
        Launchpad
    }
}
