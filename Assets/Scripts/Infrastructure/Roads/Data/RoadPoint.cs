using UnityEngine;
using System;

namespace Infrastructure.Roads.Data
{
    public class RoadPoint : IRoadIntersectionPrototype
    {
        // ID of road section which this point lies on
        public Guid ParentRoadSectionID { get; set; }
        public Vector3 Position { get; set; }

        public RoadPoint(Guid guid, Vector3 position)
        {
            ParentRoadSectionID = guid;
            Position = position;
        }

        public RoadIntersection GetOrCreateIntersection(RoadNetwork roadNetwork) => roadNetwork.SubdivideRoadAtPoint(this);
    }
}

