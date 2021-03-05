using UnityEngine;

namespace Infrastructure.Roads
{
    using Data;
    using Buildings.Data;
    using UnityEngine.InputSystem;

    [RequireComponent(typeof(RoadNetwork))]
    public class RoadCreator : Interactor
    {
        [Header("Constants")]
        public float lengthSnapping;

        [Header("References")]
        public GameObject startNodeMarker;
        public GameObject endNodeMarker;

        private RoadType _roadType = RoadType.Traffic;

        private NewNodeType _startNodeType;
        private Vector3 _startNodePosition;
        private IRoadIntersectionPrototype _startNodeData;

        private NewNodeType _endNodeType;
        private Vector3 _endNodePosition;
        private IRoadIntersectionPrototype _endNodeData;

        private RoadNetwork _roadNetwork;

        private State _currentState;

        public void Awake()
        {
            _roadNetwork = GetComponent<RoadNetwork>();
        }

        protected override void Setup()
        {
            // Filter to just RoadUI and Floor layers
            int layerMask = 1 << 12;
            layerMask |= 1 << 10;

            _interactionMode = InteractionMode.RoadCreator;
            _layerMask = layerMask;
        }

        protected override void Tick()
        {
            if (Keyboard.current[Key.Digit1].wasPressedThisFrame)
                _roadType = RoadType.Traffic;
            else if (Keyboard.current[Key.Digit2].wasPressedThisFrame)
                _roadType = RoadType.Crawler;
        }

        protected override void Interact(RaycastHit hit)
        {
            if (!(hit.collider.CompareTag("Floor") || hit.collider.CompareTag("RoadIntersectionCollider") || hit.collider.CompareTag("RoadPointCollider") || hit.collider.CompareTag("BuildingConnectionCollider")))
                return;

            switch (_currentState)
            {
                case State.DrawingStart:
                    UpdateStartPoint(hit);
                    break;

                case State.DrawingEnd:
                    UpdateEndPoint(hit);
                    break;
            }
        }

        protected override void Begin()
        {
            _currentState = State.DrawingStart;
        }

        protected override void End()
        {
            HideMarkers();
        }

        protected override void Pause()
        {
            HideMarkers();
        }

        protected override void Resume()
        {
            if (_currentState == State.DrawingStart)
                startNodeMarker.SetActive(true);

            if (_currentState == State.DrawingEnd)
            {
                startNodeMarker.SetActive(true);
                endNodeMarker.SetActive(true);
            }
        }

        private void UpdateStartPoint(RaycastHit hit)
        {
            switch (hit.collider.tag)
            {
                case "BuildingConnectionCollider":
                    _startNodeType = NewNodeType.BuildingConnection;

                    // Snap to building connector
                    _startNodePosition = hit.collider.transform.parent.position;

                    // Store the snapped connector
                    BuildingRoadConnectionComponent buildingConnectionComponent = hit.collider.transform.parent.gameObject.GetComponent<BuildingRoadConnectionComponent>();
                    _startNodeData = buildingConnectionComponent.RoadConnection;

                    break;

                case "RoadIntersectionCollider":
                    _startNodeType = NewNodeType.Intersection;

                    // Snap to existing intersection
                    _startNodePosition = hit.collider.transform.parent.position;

                    // Store the snapped intersection
                    RoadIntersectionComponent roadIntersectionComponent = hit.collider.transform.parent.gameObject.GetComponent<RoadIntersectionComponent>();
                    _startNodeData = roadIntersectionComponent.RoadIntersection;

                    break;

                case "RoadPointCollider":
                    _startNodeType = NewNodeType.Point;

                    // Snap to road point
                    _startNodePosition = hit.collider.transform.parent.position;

                    // Store the snapped point
                    RoadPointComponent roadPointComponent = hit.collider.transform.parent.gameObject.GetComponent<RoadPointComponent>();
                    _startNodeData = roadPointComponent.RoadPoint;

                    break;

                case "Floor":
                    _startNodeType = NewNodeType.Position;

                    // Start node will be a new intersection
                    _startNodePosition = hit.point;

                    _startNodeData = new RoadPosition(_startNodePosition);

                    break;
            }

            Vector3 newStartMarkerPosition = _startNodePosition;
            newStartMarkerPosition.y += 0.05f;

            startNodeMarker.transform.position = newStartMarkerPosition;

            // Enable start game marker if it isn't already
            if (!startNodeMarker.activeSelf) startNodeMarker.SetActive(true);

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Don't connect if max connections to this intersection has already been reached
                if (_startNodeType == NewNodeType.Intersection && _roadNetwork.GetConnectionCount(_startNodeData as RoadIntersection) >= (_startNodeData as RoadIntersection).MaxConnections)
                    return;

                // Don't connect if existing intersection requires a certain type of road and we're not drawing that
                if (_startNodeType == NewNodeType.Intersection && (_startNodeData as RoadIntersection).FixedRoadType != RoadType.Any && (_startNodeData as RoadIntersection).FixedRoadType != _roadType)
                    return;

                // Don't connect if building connection requires a certain type of road and we're not drawing that
                if (_startNodeType == NewNodeType.BuildingConnection && (_startNodeData as BuildingRoadConnection).FixedRoadType != RoadType.Any && (_startNodeData as BuildingRoadConnection).FixedRoadType != _roadType)
                    return;

                // Fix startMarker position, begin moving endMarker
                endNodeMarker.SetActive(true);
                _currentState = State.DrawingEnd;
            }
        }

        private void UpdateEndPoint(RaycastHit hit)
        {
            switch (hit.collider.tag)
            {
                case "BuildingConnectionCollider":
                    _endNodeType = NewNodeType.BuildingConnection;

                    // Snap to building connector
                    _endNodePosition = hit.collider.transform.parent.position;

                    // Store the snapped connector
                    BuildingRoadConnectionComponent buildingConnectionComponent = hit.collider.transform.parent.gameObject.GetComponent<BuildingRoadConnectionComponent>();
                    _endNodeData = buildingConnectionComponent.RoadConnection;

                    break;

                case "RoadIntersectionCollider":
                    _endNodeType = NewNodeType.Intersection;

                    // Snap to existing intersection
                    _endNodePosition = hit.collider.transform.parent.position;

                    // Store the snapped node
                    RoadIntersectionComponent roadIntersectionComponent = hit.collider.transform.parent.gameObject.GetComponent<RoadIntersectionComponent>();
                    _endNodeData = roadIntersectionComponent.RoadIntersection;

                    break;

                case "RoadPointCollider":
                    _endNodeType = NewNodeType.Point;

                    // Snap to road point
                    _endNodePosition = hit.collider.transform.parent.position;

                    // Store the snapped point
                    RoadPointComponent roadPointComponent = hit.collider.transform.parent.gameObject.GetComponent<RoadPointComponent>();
                    _endNodeData = roadPointComponent.RoadPoint;

                    break;

                case "Floor":
                    _endNodeType = NewNodeType.Position;

                    // End node will be a new node
                    // Do length snapping

                    if (lengthSnapping != 0)
                    {
                        Vector3 offset = hit.point - _startNodePosition;

                        float newMagnitude = Mathf.Round(offset.magnitude / lengthSnapping) * lengthSnapping;

                        Vector3 newOffset = offset.normalized * newMagnitude;
                        _endNodePosition = _startNodePosition + newOffset;
                    } else
                    {
                        _endNodePosition = hit.point;
                    }

                    _endNodeData = new RoadPosition(_endNodePosition);

                    break;
            }

            Vector3 newRoadEndMarkerPosition = _endNodePosition;
            newRoadEndMarkerPosition.y += 0.05f;

            endNodeMarker.transform.position = newRoadEndMarkerPosition;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Don't connect if max connections to this intersection has already been reached
                if (_endNodeType == NewNodeType.Intersection && _roadNetwork.GetConnectionCount(_endNodeData as RoadIntersection) >= (_endNodeData as RoadIntersection).MaxConnections)
                    return;

                // Don't connect if existing intersection requires a certain type of road and we're not drawing that
                if (_endNodeType == NewNodeType.Intersection && (_endNodeData as RoadIntersection).FixedRoadType != RoadType.Any && (_endNodeData as RoadIntersection).FixedRoadType != _roadType)
                    return;

                // Don't connect if building connection requires a certain type of road and we're not drawing that
                if (_endNodeType == NewNodeType.BuildingConnection && (_endNodeData as BuildingRoadConnection).FixedRoadType != RoadType.Any && (_endNodeData as BuildingRoadConnection).FixedRoadType != _roadType)
                    return;

                RoadIntersection lastIntersection = _roadNetwork.CreateRoad(_startNodeData, _endNodeData, _roadType);

                if (lastIntersection != null)
                {
                    // Set start node to newly created 
                    _startNodeType = NewNodeType.Intersection;
                    _startNodeData = lastIntersection;
                    _startNodePosition = _endNodePosition;

                    startNodeMarker.transform.position = endNodeMarker.transform.position;
                } else
                {
                    // TODO: handle road creation failure
                }
            }
        }

        private void HideMarkers()
        {
            startNodeMarker.SetActive(false);
            endNodeMarker.SetActive(false);
        }

        private enum State
        {
            DrawingStart,
            DrawingEnd
        }

        private enum NewNodeType
        {
            BuildingConnection,
            Intersection,
            Point,
            Position
        }
    }
}
