using Aimtec;
using System.Linq;
using Aimtec.SDK.Extensions;
using System.Collections.Generic;
using Aimtec.SDK.Util.ThirdParty;

namespace FrOnDaL_AIO.Common.Utils
{
    internal class PolygonEnemy
    {
        public abstract class EnemyPolygon
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

        public class Rectangle : EnemyPolygon
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

        public class EnemyResult
        {
            public EnemyResult(int hit, Vector3 cp)
            {
                EnemyHit = hit;
                CastPosition = cp;
            }
            public int EnemyHit;
            public Vector3 CastPosition;
        }
        public static EnemyResult GetLinearLocation(float range, float width)
        {
            var enemyHero = GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(range));
            var objAiHeroes = enemyHero as Obj_AI_Hero[] ?? enemyHero.ToArray();
            var positions = objAiHeroes.Select(x => x.ServerPosition).ToList();
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
            var results = new HashSet<EnemyResult>();
            foreach (var p in locations)
            {
                var rect = new Rectangle(Misc.Player.Position, p, width);

                var count = objAiHeroes.Count(m => rect.Contains(m.Position));

                results.Add(new EnemyResult(count, p));
            }
            var maxhit = results.MaxBy(x => x.EnemyHit);
            return maxhit;
        }
    }
}
