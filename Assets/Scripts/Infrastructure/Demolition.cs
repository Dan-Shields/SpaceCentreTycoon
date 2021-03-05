using UnityEngine;
using UnityEngine.InputSystem;
using Infrastructure.Roads;
using Infrastructure.Buildings;

namespace Infrastructure
{
    public class Demolition : Interactor
    {
        public RoadNetwork roadNetwork;
        public BuildingManager buildingManager;

        protected override void Setup()
        {
            // Filter to Roads and Buildings layers
            _layerMask = 1 << 11;
            _layerMask |= 1 << 13;

            _interactionMode = InteractionMode.Demolition;
        }

        protected override void Tick()
        {
        }

        protected override void Interact(RaycastHit hit)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                if (hit.collider.CompareTag("RoadSectionCollider"))
                {
                    RoadSectionComponent roadSectionComponent = hit.transform.parent.gameObject.GetComponent<RoadSectionComponent>();

                    roadNetwork.DemolishRoad(roadSectionComponent.RoadSection);
                } else if (hit.collider.CompareTag("BuildingSelectionCollider"))
                {
                    Building building = hit.rigidbody.gameObject.GetComponent<Building>();

                    buildingManager.DemolishBuilding(building);
                }
            }
        }

        protected override void Begin()
        {
        }

        protected override void End()
        {
        }

        protected override void Pause()
        {
        }

        protected override void Resume()
        {
        }
    }
}

