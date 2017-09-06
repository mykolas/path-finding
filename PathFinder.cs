using System;
using System.Collections.Generic;
using System.Linq;

namespace PathFinder
{
    public class PathFinder
    {
        private readonly int _destination;
        private readonly Func<int, ISet<int>> _getNeighborLocations;
        private Dictionary<int, Point> _distancesToDestination;
        private Dictionary<int, Point> DistancesToDestination
        {
            get
            {
                if (_distancesToDestination == null)
                {
                    _distancesToDestination = GetDistancesToLocation(_destination, _getNeighborLocations);
                }

                return _distancesToDestination.ToDictionary(s => s.Key, s => s.Value);
            }
        }

        public PathFinder(int destination, Func<int, ISet<int>> getNeighborLocations)
        {
            _destination = destination;
            _getNeighborLocations = getNeighborLocations;
        }

        public int GetNextLocation(int from, int enemy, int maxDepth)
        {
            var r = new Random();
            var enemyDistanceSearchDepth = r.Next(0, maxDepth);
            var byDestination = DistancesToDestination;
            var byEnemy = GetDistancesToLocation(enemy, _getNeighborLocations, enemyDistanceSearchDepth);

            var maxDistanceToDestination = byDestination.First(s => s.Value.Value == byDestination.Values.Max(v => v.Value)).Value.Value;

            /* Increase location values by how far it is from enemy*/
            foreach (var location in byDestination.ToList())
            {
                var point = location.Value;
                if (byEnemy.ContainsKey(location.Key))
                {

                    point.Value = maxDistanceToDestination + enemyDistanceSearchDepth - byEnemy[location.Key].Value;
                }
                byDestination[location.Key] = point;
            }

            var neighbours = _getNeighborLocations(from);
            var availablePoints = byDestination.Where(s => neighbours.Contains(s.Key)).ToList();
            var sortedByValueAsc = availablePoints.OrderBy(s => s.Value.Value);

            var bestValue = sortedByValueAsc.First().Value.Value;
            var allWithBestValue = availablePoints.Where(s => s.Value.Value == bestValue).ToList();
            if (allWithBestValue.Count > 1)
            {
                /*Posible dead-end, so choose random move*/
                var randomIndex = r.Next(availablePoints.Count);
                return availablePoints[randomIndex].Value.Location;
            }
            return allWithBestValue.First().Value.Location;
        }

        private static Dictionary<int, Point> GetDistancesToLocation(int location, Func<int, ISet<int>> getNeighborLocations, int depth = int.MaxValue)
        {
            var queue = new Queue<Point>();
            var nodes = new Dictionary<int, Point>();
            var destinationPoint = new Point(location, 0);
            queue.Enqueue(destinationPoint);
            nodes[location] = destinationPoint;
            while (queue.Any())
            {
                var p = queue.Dequeue();
                if (p.Value > depth)
                {
                    return nodes;
                }
                var neighbours = getNeighborLocations(p.Location).Except(nodes.Keys);

                foreach (var neighbor in neighbours)
                {
                    var neighborPoint = new Point(neighbor, p.Value + 1);
                    nodes[neighbor] = neighborPoint;
                    queue.Enqueue(neighborPoint);
                }
            }
            return nodes;
        }

        public struct Point
        {
            public int Location { get; set; }
            public int Value { get; set; }

            public Point(int location, int value)
            {
                Location = location;
                Value = value;
            }
        }
    }
}