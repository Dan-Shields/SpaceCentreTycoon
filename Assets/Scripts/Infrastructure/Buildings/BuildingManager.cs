using UnityEngine;
using System.Collections.Generic;

namespace Infrastructure.Buildings
{
    public class BuildingManager : MonoBehaviour
    {
        [Header("References")]
        public InteractionManager interactionManager;
        public Transform buildingObjectParent;

        public List<Building> Buildings { get; } = new List<Building>();

        private void AddEventHandlers()
        {
            interactionManager.OnInteractionModeChange += CheckShouldSwapConnectors;
        }

        private void RemoveEventHandlers()
        {
            interactionManager.OnInteractionModeChange -= CheckShouldSwapConnectors;
        }

        // Use this for initialization
        void Start()
        {
            AddEventHandlers();
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

        /// <summary>
        /// If infrastructure interaction mode is now RoadCreator, enable all road connectors. If infrastructure interaction mode was RoadCreator, disable all road connectors.
        /// </summary>
        /// <param name="newInteractionMode"></param>
        /// <param name="oldInteractionMode"></param>
        private void CheckShouldSwapConnectors(InteractionMode newInteractionMode, InteractionMode oldInteractionMode)
        {
            if (newInteractionMode == InteractionMode.RoadCreator)
            {
                foreach (Building building in Buildings)
                    building.EnableRoadConnectors();

            } else if (oldInteractionMode == InteractionMode.RoadCreator)
            {
                foreach (Building building in Buildings)
                    building.DisableRoadConnectors();
            }
        }

        public bool CreateBuilding(GameObject pendingBuildingObject)
        {
            Building pendingBuilding = pendingBuildingObject.GetComponent<Building>();

            if (pendingBuilding.IsColliding) return false;

            GameObject concreteBuildingObject = Instantiate(pendingBuildingObject, buildingObjectParent);

            Buildings.Add(concreteBuildingObject.GetComponent<Building>());

            return true;
        }

        public bool DemolishBuilding(Building building)
        {
            if (Buildings.Remove(building))
            {
                Destroy(building.gameObject);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
