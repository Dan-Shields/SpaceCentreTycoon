using UnityEngine;
using System.Collections.Generic;

namespace Infrastructure.Buildings
{
    public class Building : MonoBehaviour
    {
        public GameObject menuCardPrefab;

        public bool IsColliding { get => _collisionCount > 0; }

        private readonly List<GameObject> roadConnectorObjects = new List<GameObject>();

        private int _collisionCount = 0;

        void Start()
        {
            BuildingRoadConnectionComponent[] roadConnectors = GetComponentsInChildren<BuildingRoadConnectionComponent>(true);

            foreach (BuildingRoadConnectionComponent roadConnector in roadConnectors)
            {
                roadConnectorObjects.Add(roadConnector.gameObject);
            }
        }

        public void EnableRoadConnectors()
        {
            foreach (GameObject roadConnectorObject in roadConnectorObjects)
            {
                roadConnectorObject.SetActive(true);
            }
        }

        public void DisableRoadConnectors()
        {
            foreach (GameObject roadConnectorObject in roadConnectorObjects)
            {
                roadConnectorObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            _collisionCount = 0;
        }

        private void OnCollisionEnter(Collision collision)
        {
            _collisionCount++;
        }

        private void OnCollisionExit(Collision collision)
        {
            _collisionCount--;
        }

        public GameObject GetMenuCardPrefab()
        {
            return menuCardPrefab;
        }
    }
}
