using System;
using System.Drawing;
using Aimtec;
using System.Linq;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Events;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Extensions;
using Spell = Aimtec.SDK.Spell;
using FrOnDaL_AIO.Common.Utils;
using Aimtec.SDK.TargetSelector;
using Aimtec.SDK.Prediction.Health;
using Aimtec.SDK.Prediction.Skillshots;

namespace FrOnDaL_AIO.Common
{
    internal static class Misc
    {
        public static Spell Q { get; set; }
        public static Spell Q2 { get; set; }
        public static Spell W { get; set; }
        public static Spell W2 { get; set; }
        public static Spell E { get; set; }
        public static Spell E2 { get; set; }
        public static Spell R { get; set; }
        public static Spell R2 { get; set; }
        public static Spell Flash { get; set; }
        public static Menu Main { get; set; }
        public static IOrbwalker Orbwalker => Aimtec.SDK.Orbwalking.Orbwalker.Implementation;
        public static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();
        public static IHealthPrediction HealthPrediction => Aimtec.SDK.Prediction.Health.HealthPrediction.Implementation;

        public static readonly string[] NoAttacks =
        {
            "asheqattacknoonhit", "volleyattackwithsound", "volleyattack",
            "annietibbersbasicattack", "annietibbersbasicattack2",
            "azirsoldierbasicattack", "azirsundiscbasicattack",
            "elisespiderlingbasicattack", "gravesbasicattackspread",
            "gravesautoattackrecoil", "heimertyellowbasicattack",
            "heimertyellowbasicattack2", "heimertbluebasicattack",
            "jarvanivcataclysmattack", "kindredwolfbasicattack",
            "malzaharvoidlingbasicattack", "malzaharvoidlingbasicattack2",
            "malzaharvoidlingbasicattack3", "shyvanadoubleattack",
            "shyvanadoubleattackdragon", "sivirwattackbounce",
            "monkeykingdoubleattack", "yorickspectralghoulbasicattack",
            "yorickdecayedghoulbasicattack", "yorickravenousghoulbasicattack",
            "zyragraspingplantattack", "zyragraspingplantattack2",
            "zyragraspingplantattackfire", "zyragraspingplantattack2fire"
        };
        public static Prediction Prediction => Prediction.Instance;
        public static readonly string[] Attacks =
        {
            "caitlynheadshotmissile", "kennenmegaproc", "masteryidoublestrike",
            "quinnwenhanced", "renektonexecute", "renektonsuperexecute",
            "trundleq", "viktorqbuff", "xenzhaothrust", "xenzhaothrust2",
            "xenzhaothrust3"
        };

        public static ITargetSelector TargetSelector => Aimtec.SDK.TargetSelector.TargetSelector.Implementation;
        public static bool IsChampionSupported = true;



        public static bool IsAutoAttack(string name)
        {
            name = name.ToLower();
            return (name.Contains("attack") && !NoAttacks.Contains(name)) || Attacks.Contains(name);
        }

        public static bool IsWall(this Vector3 pos, bool includeBuildings = false)
        {
            var point = NavMesh.WorldToCell(pos).Flags;
            return point.HasFlag(NavCellFlags.Wall) || includeBuildings && point.HasFlag(NavCellFlags.Building);
        }

        public static bool HasSheenLikeBuff(this Obj_AI_Hero unit)
        {
            var sheenLikeBuffNames = new[] { "sheen", "LichBane", "dianaarcready", "ItemFrozenFist", "sonapassiveattack", "AkaliTwinDisciplines" };
            return sheenLikeBuffNames.Any(b => Player.HasBuff(b));
        }

        public static bool HasTearLikeItem(this Obj_AI_Hero unit)
        {
            return
                unit.HasItem(ItemId.Manamune) ||
                unit.HasItem(ItemId.ArchangelsStaff) ||
                unit.HasItem(ItemId.TearoftheGoddess) ||
                unit.HasItem(ItemId.ManamuneQuickCharge) ||
                unit.HasItem(ItemId.ArchangelsStaffQuickCharge) ||
                unit.HasItem(ItemId.TearoftheGoddessQuickCharge);
        }

        public static bool AnyWallInBetween(Vector2 startPos, Vector2 endPos)
        {
            for (var i = 0; i < startPos.Distance(endPos); i += 5)
            {
                var point = NavMesh.WorldToCell((Vector3)startPos.Extend(endPos, i));
                if (point.Flags.HasFlag(NavCellFlags.Wall) ||
                    point.Flags.HasFlag(NavCellFlags.Building))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsHardCc(this Buff buff)
        {
            return
                buff.Type == BuffType.Stun ||
                buff.Type == BuffType.Fear ||
                buff.Type == BuffType.Flee ||
                buff.Type == BuffType.Snare ||
                buff.Type == BuffType.Taunt ||
                buff.Type == BuffType.Charm ||
                buff.Type == BuffType.Knockup ||
                buff.Type == BuffType.Suppression;
        }

        public static bool IsImmobile(this Obj_AI_Base target)
        {
            if (target.IsDead ||
                target.IsDashing() ||
                target.Name.Equals("Target Dummy") ||
                target.HasBuffOfType(BuffType.Knockback))
            {
                return false;
            }

            return target.ValidActiveBuffs().Any(buff => buff.IsHardCc());
        }

        public static bool ShouldPreserveSheen(this Obj_AI_Hero source)
        {
            return source.ActionState.HasFlag(ActionState.CanAttack);
        }

        public static bool ShouldShieldAgainstSender(Obj_AI_Base sender)
        {
            return
                GameObjects.EnemyHeroes.Contains(sender) ||
                GameObjects.EnemyTurrets.Contains(sender) ||
                Extensions.GetGenericJungleMinionsTargets().Contains(sender);
        }
        public static bool IsSpellHeroCollision(Obj_AI_Base t, Spell ulti, int extraWith = 50)
        {
            foreach (var hero in GameObjects.EnemyHeroes.Where( hero => hero.IsValidTarget(ulti.Range + ulti.Width, false, false, ulti.From) && t.NetworkId != hero.NetworkId))
            {
                var prediction = ulti.GetPrediction(hero);
                var powCalc = Math.Pow(ulti.Width + extraWith + hero.BoundingRadius, 2);

                if (prediction.UnitPosition.To2D().Distance(Player.ServerPosition.To2D(), ulti.GetPrediction(t).CastPosition.To2D(), true, true) <= powCalc)
                {
                    return true;
                }

                if (prediction.UnitPosition.To2D().Distance(Player.ServerPosition.To2D(), t.ServerPosition.To2D(), true, true) <= powCalc)
                {
                    return true;
                }
            }
            return false;
        }
        internal static float DistanceToPlayer(this Vector3 position)
        {
            return position.To2D().DistanceToPlayer();
        }
        internal static float DistanceToPlayer(this Vector2 position)
        {
            return ObjectManager.GetLocalPlayer().ServerPosition.Distance(position);
        }

        public static void DrawCircle(Vector2 centre, float radius, Color color)
        {
            for (var i = 0; i < 20; i++)
            {
                var x1 = (float) (centre.X + radius * Math.Cos(i / 20.0 * 2 * Math.PI));
                var y1 = (float) (centre.Y + radius * Math.Sin(i / 20.0 * 2 * Math.PI));

                var x2 = (float) (centre.X + radius * Math.Cos((i + 1) / 20.0 * 2 * Math.PI));
                var y2 = (float) (centre.Y + radius * Math.Sin((i + 1) / 20.0 * 2 * Math.PI));

                Render.Line(x1, y1, x2, y2, color);
            }
        }
    }
}
