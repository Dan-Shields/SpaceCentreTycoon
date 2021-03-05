using UnityEngine;
using UnityEngine.InputSystem;

namespace Infrastructure.Buildings
{
    using Menus;
    using UnityEngine.EventSystems;

    public class Selection : Interactor
    {
        public Canvas canvas;
        public BuildingManager buildingManager;

        private GameObject _activeMenu = null;

        protected override void Setup()
        {
            // Filter to UI and Buildings layers
            _layerMask = 1 << 5;
            _layerMask |= 1 << 13;

            _interactionMode = InteractionMode.Free;
        }

        protected override void Tick()
        {
        }

        protected override void Interact(RaycastHit hit)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame && hit.collider.CompareTag("BuildingSelectionCollider") && !EventSystem.current.IsPointerOverGameObject())
            {
                if (_activeMenu != null)
                {
                    // Close existing menu
                    _activeMenu.gameObject.SetActive(false);
                    _activeMenu = null;
                }

                Building building = hit.rigidbody.gameObject.GetComponent<Building>();

                // This shouldn't happen because of the tag check for "BuildingSelectionCollider"
                // TODO: do something if this happens
                if (!building) return;

                GameObject menuCardPrefab = building.GetMenuCardPrefab();

                _activeMenu = Instantiate(menuCardPrefab, canvas.transform);

                BuildingMenuBase buildingMenu = _activeMenu.GetComponent<BuildingMenuBase>();
                buildingMenu.closeButton.onClick.AddListener(End);
            }
        }

        protected override void Begin()
        {
        }

        protected override void End()
        {
            if (_activeMenu != null)
            {
                _activeMenu.gameObject.SetActive(false);
                Destroy(_activeMenu);
                _activeMenu = null;
            }
        }

        protected override void Pause()
        {
        }

        protected override void Resume()
        {
        }
    }
}
