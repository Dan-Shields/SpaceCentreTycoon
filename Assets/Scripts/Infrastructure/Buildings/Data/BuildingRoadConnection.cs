using UnityEngine;

namespace Infrastructure.Buildings.Data
{
    using Roads;
    using Roads.Data;

    public class BuildingRoadConnection : IRoadIntersectionPrototype
    {
        public Vector3 Position { get; private set; }
        public RoadType FixedRoadType { get; private set; }
        public int MaxConnections { get; private set; }

        // TODO: track connections and disallow creation after full
        public int ConnectionCount { get; private set; }

        public BuildingRoadConnection(Vector3 position, RoadType fixedRoadType, int maxConnections)
        {
            Position = position;
            FixedRoadType = fixedRoadType;
            MaxConnections = maxConnections;

            ConnectionCount = 0;
        }

        public RoadIntersection GetOrCreateIntersection(RoadNetwork roadNetwork)
        {
            return new RoadIntersection(Position, FixedRoadType, MaxConnections);
        }
    }
}
