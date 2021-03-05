using UnityEngine;
using Util.ObserverSubject;
using QuikGraph;
using System.Linq;
using System.Collections.Generic;

namespace Infrastructure.Roads
{
    using Data;

    [RequireComponent(typeof(RoadNetwork))]
    public class RoadRenderer : MonoBehaviour
    {
        public GameObject roadLinesParent;
        public GameObject roadLinePrefab;

        private RoadNetwork _roadNetwork;
        private List<GameObject> _roadLines = new List<GameObject>();

        // Start is called before the first frame update
        void Start()
        {
            _roadNetwork = GetComponent<RoadNetwork>();
            _roadNetwork.OnSectionAdded += GenerateRoadLine;
        }

        // TODO: make this one mesh, not a bunch of line renderers
        private void GenerateRoadLine(RoadSection newSection)
        {
            
            IEnumerable<RoadSection> sections = _roadNetwork.Sections;

            while (_roadLines.Count != sections.Count())
            {
                if (sections.Count() > _roadLines.Count)
                {
                    _roadLines.Add(Instantiate(roadLinePrefab, roadLinesParent.transform));
                }
                else if (sections.Count() < _roadLines.Count)
                {
                    _roadLines.Remove(_roadLines.Last());
                }
            }

            int i = 0;

            foreach (RoadSection section in sections)
            {
                LineRenderer lineRenderer = _roadLines[i].GetComponent<LineRenderer>();

                Vector3[] linePositions = { section.Source.Position, section.Target.Position };

                linePositions[0].y += 0.2f;
                linePositions[1].y += 0.2f;

                lineRenderer.SetPositions(linePositions);

                i++;
            }
        }
    }
}

