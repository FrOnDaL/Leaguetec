using Aimtec;
using System.Linq;
using Aimtec.SDK.Extensions;
using System.Collections.Generic;
using Aimtec.SDK.Util.ThirdParty;

namespace FrOnDaL_AIO.Common.Utils
{
    internal static class Polygon
    {
        public abstract class LanePolygon
        {
            public List<Vector3> Points = new List<Vector3>();

            public List<IntPoint> ClipperPoints
            {
                get
                {
                    return Points.Select(p => new IntPoint(p.X, p.Z)).ToList();
                }
            }

            public bool Contains(Vector3 point)
            {
                var p = new IntPoint(point.X, point.Z);
                var inpolygon = Clipper.PointInPolygon(p, ClipperPoints);
                return inpolygon == 1;
            }
        }
        public class Rectangle : LanePolygon
        {
            public Rectangle(Vector3 startPosition, Vector3 endPosition, float width)
            {
                var direction = (startPosition - endPosition).Normalized();
                var perpendicular = Perpendicular(direction);

                var leftBottom = startPosition + width * perpendicular;
                var leftTop = startPosition - width * perpendicular;

                var rightBottom = endPosition - width * perpendicular;
                var rightLeft = endPosition + width * perpendicular;

                Points.Add(leftBottom);
                Points.Add(leftTop);
                Points.Add(rightBottom);
                Points.Add(rightLeft);
            }

            public Vector3 Perpendicular(Vector3 v)
            {
                return new Vector3(-v.Z, v.Y, v.X);
            }
        }
        public class LaneclearResult
        {
            public LaneclearResult(int hit, Vector3 cp)
            {
                NumberOfMinionsHit = hit;
                CastPosition = cp;
            }

            public int NumberOfMinionsHit;
            public Vector3 CastPosition;
        }

        public static LaneclearResult GetLinearLocation(float range, float width)
        {
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidSpellTarget(range));

            var objAiBases = minions as Obj_AI_Base[] ?? minions.ToArray();
            var positions = objAiBases.Select(x => x.ServerPosition).ToList();

            var locations = new List<Vector3>();

            locations.AddRange(positions);

            var max = positions.Count;

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (positions[j] != positions[i])
                    {
                        locations.Add((positions[j] + positions[i]) / 2);
                    }
                }
            }

            var results = new HashSet<LaneclearResult>();

            foreach (var p in locations)
            {
                var rect = new Rectangle(Misc.Player.Position, p, width);

                var count = objAiBases.Count(m => rect.Contains(m.Position));

                results.Add(new LaneclearResult(count, p));
            }

            var maxhit = results.MaxBy(x => x.NumberOfMinionsHit);

            return maxhit;
        }
        public static float Distance(this Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, bool onlyIfOnSegment = false, bool squared = false)
        {
            var objects = point.ProjectOn(segmentStart, segmentEnd);
            if (objects.IsOnSegment || onlyIfOnSegment == false)
            {
                return squared ? Vector2.DistanceSquared(objects.SegmentPoint, point) : Vector2.Distance(objects.SegmentPoint, point);
            }
            return float.MaxValue;
        }
    }

}
