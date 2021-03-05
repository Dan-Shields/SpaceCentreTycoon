using UnityEngine;
using System;

namespace Infrastructure.Roads.Data
{
    /// <summary>Representation of an vertex in a RoadNetwork.</summary>
    public class RoadIntersection : IComparable, IRoadIntersectionPrototype
    {
        public static int NextId = 0;

        public int ID { get; }

        public Vector3 Position { get; private set; }

        public int MaxConnections { get; }

        public RoadType FixedRoadType { get; }

        public RoadIntersection(Vector3 position, RoadType fixedRoadType = RoadType.Any, int maxConnections = 6)
        {
            ID = NextId++;
            Position = position;
            MaxConnections = maxConnections;
            FixedRoadType = fixedRoadType;
        }

        public void SetYLevel(float yLevel)
        {
            Vector3 newPosition = Position;
            newPosition.y = yLevel;
            Position = newPosition;
        }

        // As per: https://github.com/KeRNeLith/QuikGraph/wiki/Edges#undirected-edges
        // "edges that implement the IUndirectedEdge<TVertex> interface must sort the vertices so that Source <= Target"
        // Hence why an Auto-Incrementing id was used over a GUID
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (obj is RoadIntersection otherRoadIntersection)
            {
                return ID.CompareTo(otherRoadIntersection.ID);
            } else
            {
                throw new ArgumentException("Object is not a RoadIntersection");
            }
        }

        public RoadIntersection GetOrCreateIntersection(RoadNetwork roadNetwork) => this;
    }
}
