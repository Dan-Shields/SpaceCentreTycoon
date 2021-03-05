using UnityEngine;
using QuikGraph;
using System;
using Util;

namespace Infrastructure.Roads.Data
{
    public class RoadSection : UndirectedEdge<RoadIntersection>
    {
        public Guid ID { get; }
        public RoadType RoadType { get; }
        public float Length { get; }

        public Vector2[] Points { get; } = new Vector2[2];

        public RoadSection(RoadIntersection sourceIntersection, RoadIntersection targetIntersection, RoadType roadType) : base(sourceIntersection, targetIntersection)
        {
            ID = Guid.NewGuid();
            RoadType = roadType;
            Length = Vector3.Distance(sourceIntersection.Position, targetIntersection.Position);

            Points[0] = new Vector2(Source.Position.x, Source.Position.z);
            Points[1] = new Vector2(Target.Position.x, Target.Position.z);
        }

        public bool CheckOverlap2D(RoadSection testRoad)
        {
            return LineIntersection.DoIntersect(Points, testRoad.Points);
        }

        public bool CheckSharePoints(RoadSection testRoad)
        {
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (testRoad.Points[i] == Points[j])
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
