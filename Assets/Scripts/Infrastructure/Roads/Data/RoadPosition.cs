using UnityEngine;
using System.Collections;

namespace Infrastructure.Roads.Data
{
    public class RoadPosition : IRoadIntersectionPrototype
    {
        public Vector3 Position { get; private set; }

        public RoadPosition(Vector3 position)
        {
            Position = position;
        }

        public RoadIntersection GetOrCreateIntersection(RoadNetwork roadNetwork) => new RoadIntersection(Position);
    }
}

