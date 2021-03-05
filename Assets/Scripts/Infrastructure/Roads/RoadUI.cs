using UnityEngine;
using System.Collections.Generic;

namespace Infrastructure.Roads
{
    using Data;

    [RequireComponent(typeof(RoadNetwork))]
    public class RoadUI : MonoBehaviour
    {
        [Header("Constants")]
        public float roadPointObjectMinSize = 0.8f;
        public float trafficRoadWidth = 0.5f;
        public float crawlerRoadWidth = 0.8f;

        [Header("References")]
        public GameObject roadSectionObjectsParent;
        public GameObject roadIntersectionObjectsParent;
        public GameObject roadPointObjectsParent;

        [Header("Prefabs")]
        public GameObject trafficRoadSectionObjectPrefab;
        public GameObject crawlerRoadSectionObjectPrefab;
        public GameObject roadIntersectionObjectPrefab;
        public GameObject roadPointObjectPrefab;

        private RoadNetwork _roadNetwork;

        private readonly List<GameObject> _sectionObjects = new List<GameObject>();
        private readonly List<GameObject> _intersectionObjects = new List<GameObject>();

        private void AddEventHandlers()
        {
            _roadNetwork.OnSectionAdded += CreateSectionObject;
            _roadNetwork.OnSectionDeleted += DeleteSectionObject;
            _roadNetwork.OnIntersectionAdded += CreateIntersectionObject;
            _roadNetwork.OnIntersectionDeleted += DeleteIntersectionObject;
        }

        private void RemoveEventHandlers()
        {
            _roadNetwork.OnSectionAdded -= CreateSectionObject;
            _roadNetwork.OnSectionDeleted -= DeleteSectionObject;
            _roadNetwork.OnIntersectionAdded -= CreateIntersectionObject;
            _roadNetwork.OnIntersectionDeleted -= DeleteIntersectionObject;
        }

        // Start is called before the first frame update
        void Awake()
        {
            _roadNetwork = GetComponent<RoadNetwork>();
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

        private void CreateSectionObject(RoadSection section)
        {
            
            GameObject sectionObjectParent = new GameObject("RoadSectionContainer");
            sectionObjectParent.transform.SetParent(roadSectionObjectsParent.transform);

            GameObject sectionObject;

            if (section.RoadType == RoadType.Crawler)
                sectionObject = Instantiate(crawlerRoadSectionObjectPrefab, sectionObjectParent.transform);
            else
                sectionObject = Instantiate(trafficRoadSectionObjectPrefab, sectionObjectParent.transform);
                
            sectionObject.GetComponent<RoadSectionComponent>().RoadSection = section;

            Vector3 roadVector = section.Target.Position - section.Source.Position;

            // POSITION section half way between its intersections
            sectionObject.transform.position = (roadVector / 2) + section.Source.Position;

            // ROTATION
            Vector2 roadVector2D = new Vector2(roadVector.x, roadVector.z);
            float roadAngle = Vector2.SignedAngle(roadVector2D, Vector2.up);
            Vector3 roadRotation = new Vector3(0, roadAngle, 0);
            sectionObject.transform.rotation = Quaternion.Euler(roadRotation);

            // SCALE in z direction by the road length and by the road width in the x direction
            Vector3 sectionScale = sectionObject.transform.localScale;
            sectionScale.z = roadVector.magnitude;

            float roadWidth = section.RoadType == RoadType.Crawler ? crawlerRoadWidth : trafficRoadWidth;
            sectionScale.x = roadWidth;

            sectionObject.transform.localScale = sectionScale;

            // Point count
            float availableLength = roadVector.magnitude - crawlerRoadWidth;
            int pointCount = Mathf.FloorToInt(availableLength / roadPointObjectMinSize);

            float stretchedPointSize = availableLength / pointCount;

            // Generate points
            for (int i = 0; i < pointCount; i++)
            {
                // POSITION
                float horizontalOffset = roadVector.magnitude - ((crawlerRoadWidth / 2) + (stretchedPointSize / 2) + (stretchedPointSize * i));
                Vector3 offsetPosition = roadVector.normalized * horizontalOffset;
                Vector3 pointPosition = offsetPosition + section.Source.Position;

                GameObject pointObject = Instantiate(roadPointObjectPrefab, sectionObjectParent.transform);
                RoadPoint point = new RoadPoint(section.ID, pointPosition);
                // Store point in pointObject
                pointObject.GetComponent<RoadPointComponent>().RoadPoint = point;

                // Bump y position of pointObject slightly above the point position to avoid z-fighting
                pointPosition.y += 0.05f;
                pointObject.transform.position = pointPosition;

                // ROTATION
                pointObject.transform.localRotation = Quaternion.Euler(roadRotation);

                // SCALE collider in z direction to match stretchedPointSize and by the road width in the x direction
                // TODO: this is not at all safe
                Transform colliderTransform = pointObject.transform.GetComponentInChildren<Collider>().gameObject.transform;
                Vector3 colliderScale = colliderTransform.localScale;
                colliderScale.z = stretchedPointSize;
                colliderScale.x = roadWidth * 1.5f;
                colliderTransform.localScale = colliderScale;
            }

            _sectionObjects.Add(sectionObjectParent);
        }

        private void DeleteSectionObject(RoadSection section)
        {
            GameObject sectionToBeDeleted = _sectionObjects.Find(sectionObject => sectionObject.GetComponentInChildren<RoadSectionComponent>().RoadSection.ID == section.ID);

            if (sectionToBeDeleted)
            {
                _sectionObjects.Remove(sectionToBeDeleted);
                Destroy(sectionToBeDeleted);
            }
        }

        private void CreateIntersectionObject(RoadIntersection intersection)
        {
            GameObject intersectionObject = Instantiate(roadIntersectionObjectPrefab, roadIntersectionObjectsParent.transform);

            intersectionObject.GetComponent<RoadIntersectionComponent>().RoadIntersection = intersection;

            intersectionObject.transform.position = intersection.Position;

            Vector3 scale = new Vector3(crawlerRoadWidth * 2, intersectionObject.transform.localScale.y, crawlerRoadWidth * 2);

            intersectionObject.transform.localScale = scale;

            _intersectionObjects.Add(intersectionObject);
        }

        private void DeleteIntersectionObject(RoadIntersection intersection)
        {
            GameObject intersectionToBeDeleted = _intersectionObjects.Find(intersectionObject => intersectionObject.GetComponent<RoadIntersectionComponent>().RoadIntersection.ID == intersection.ID);

            if (intersectionToBeDeleted)
            {
                _intersectionObjects.Remove(intersectionToBeDeleted);
                Destroy(intersectionToBeDeleted);
            }
        }
    }
}
