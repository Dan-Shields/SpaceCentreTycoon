using UnityEngine;
using System.Collections.Generic;
using QuikGraph;
using System.Linq;

namespace Infrastructure.Roads
{
    using Data;
    using QuikGraph.Algorithms;

    /// <summary>Stores roads in an undirected graph structure.</summary>
    public class RoadNetwork : MonoBehaviour
    {
        public float yLevel = 0.995f;

        public IEnumerable<RoadSection> Sections
        {
            get => _roadGraph.Edges;
        }

        public IEnumerable<RoadIntersection> Intersections
        {
            get => _roadGraph.Vertices;
        }

        // Events
        public delegate void SectionAdded(RoadSection section);
        public event SectionAdded OnSectionAdded;

        public delegate void SectionDeleted(RoadSection section);
        public event SectionDeleted OnSectionDeleted;
        
        public delegate void IntersectionAdded(RoadIntersection intersection);
        public event IntersectionAdded OnIntersectionAdded;

        public delegate void IntersectionDeleted(RoadIntersection intersection);
        public event IntersectionDeleted OnIntersectionDeleted;

        /// <summary>
        /// _roadGraph is an undirected graph storing verticies as RoadIntersections and edges as RoadSections
        /// </summary>
        private readonly UndirectedGraph<RoadIntersection, RoadSection> _roadGraph = new UndirectedGraph<RoadIntersection, RoadSection>(false);

        public void Awake()
        {
            // Propogate graph events upwards
            _roadGraph.EdgeAdded += edge => OnSectionAdded(edge);
            _roadGraph.EdgeRemoved += edge => OnSectionDeleted(edge);
            _roadGraph.VertexAdded += vertex => OnIntersectionAdded(vertex);
            _roadGraph.VertexRemoved += vertex => OnIntersectionDeleted(vertex);
        }

        /// <summary>
        /// Create a new road section from any two RoadIntersection prototypes.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="roadType"></param>
        /// <returns>The end RoadIntersection.</returns>
        public RoadIntersection CreateRoad(IRoadIntersectionPrototype startPrototype, IRoadIntersectionPrototype endPrototype, RoadType roadType)
        {
            RoadIntersection startIntersection = startPrototype.GetOrCreateIntersection(this);
            RoadIntersection endIntersection = endPrototype.GetOrCreateIntersection(this);

            startIntersection.SetYLevel(yLevel);
            endIntersection.SetYLevel(yLevel);

            RoadSection newRoad;

            if (startIntersection.CompareTo(endIntersection) < 0)
                newRoad = new RoadSection(startIntersection, endIntersection, roadType);
            else
                newRoad = new RoadSection(endIntersection, startIntersection, roadType);

            return AddRoad(newRoad) ? endIntersection : null;
        }

        /// <summary>Trys to add a road to the network if it isn't overlapping and doesn't already exist.</summary>
        /// <returns>Success or fail.</returns>
        private bool AddRoad(RoadSection newRoad)
        {
            List<RoadSection> overlappingRoads = GetOverlappingRoads(newRoad);

            return (overlappingRoads.Count == 0 && _roadGraph.AddVerticesAndEdge(newRoad));
        }

        public bool DemolishRoad(RoadSection section)
        {
            if (_roadGraph.RemoveEdge(section))
            {
                RemoveUnconnectedVerticies();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Deletes RoadIntersections from graph if they aren't connected to anything.
        /// </summary>
        /// <returns></returns>
        private int RemoveUnconnectedVerticies() => _roadGraph.RemoveVertexIf(vertex => _roadGraph.IsAdjacentEdgesEmpty(vertex));

        /// <summary>
        /// Creates a division on an exising RoadSection at the specified RoadPoint.
        /// </summary>
        /// <param name="endPoint"></param>
        /// <returns>The newly created RoadIntersection</returns>
        public RoadIntersection SubdivideRoadAtPoint(RoadPoint endPoint)
        {
            RoadSection existingSection = Sections.First(road => road.ID == endPoint.ParentRoadSectionID);

            if (existingSection == null || !_roadGraph.RemoveEdge(existingSection))
            {
                Debug.LogError("Could not subdivide Road");
                return null;
            }

            RoadIntersection newIntersection = new RoadIntersection(endPoint.Position);

            // Create two new roads, each connecting to one of the old edge's verticies and the new vertex
            List<RoadSection> newRoadSections = new List<RoadSection>
            {
                new RoadSection(existingSection.Source, newIntersection, existingSection.RoadType),
                new RoadSection(existingSection.Target, newIntersection, existingSection.RoadType)
            };

            _roadGraph.AddVertex(newIntersection);

            _roadGraph.AddEdgeRange(newRoadSections);

            return newIntersection;
        }

        /// <returns>All roads in the network that overlap with testRoad when viewed from above.</returns>
        public List<RoadSection> GetOverlappingRoads(RoadSection testRoad)
        {
            List<RoadSection> overlappingEdges = new List<RoadSection>();

            foreach(RoadSection section in Sections)
            {
                if (!testRoad.CheckSharePoints(section) && testRoad.CheckOverlap2D(section))
                {
                    overlappingEdges.Add(section);
                }
            }
            
            return overlappingEdges;
        }

        /// <returns>The RoadIntersection at position if found, otherwise null.</returns>
        public RoadIntersection GetRoadByPosition(Vector3 position)
        {
            foreach (RoadIntersection intersection in _roadGraph.Vertices)
            {
                if (intersection.Position == position) return intersection;
            }

            return null;
        }

        /// <param name="intersection"></param>
        /// <returns>Number of RoadSections connected to the specified RoadIntersection</returns>
        public int GetConnectionCount(RoadIntersection intersection)
        {
            // TODO: this might get expensive wrt GCing
            IEnumerable<RoadSection> adjacentEdges = _roadGraph.AdjacentEdges(intersection);

            return adjacentEdges.Count();
        }

        public IEnumerable<RoadSection> GetCrawlerPath(RoadIntersection start, RoadIntersection destination)
        {
            TryFunc<RoadIntersection, IEnumerable<RoadSection>> tryGetPaths = _roadGraph.ShortestPathsDijkstra(edge => edge.RoadType == RoadType.Crawler ? edge.Length : Mathf.Infinity, start);

            return tryGetPaths(destination, out IEnumerable<RoadSection> path) ? path : Enumerable.Empty<RoadSection>();
        }
    }
}
