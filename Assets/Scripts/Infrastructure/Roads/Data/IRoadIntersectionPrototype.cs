namespace Infrastructure.Roads.Data
{
    public interface IRoadIntersectionPrototype
    {
        RoadIntersection GetOrCreateIntersection(RoadNetwork roadNetwork);
    }
}
