using UnityEngine;
using System.Collections;
using Infrastructure.Buildings.Data;
using Infrastructure.Roads.Data;

public class BuildingRoadConnectionComponent : MonoBehaviour
{
    public RoadType fixedRoadType = RoadType.Crawler;
    public int maxConnections = 1;

    public BuildingRoadConnection RoadConnection { get; set; }

    public void Awake()
    {
        RoadConnection = new BuildingRoadConnection(transform.position, fixedRoadType, maxConnections);
    }
}
