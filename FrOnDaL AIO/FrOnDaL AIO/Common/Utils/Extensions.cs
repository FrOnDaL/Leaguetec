using Aimtec;
using System.Linq;
using Aimtec.SDK.Extensions;
using Spell = Aimtec.SDK.Spell;
using System.Collections.Generic;

namespace FrOnDaL_AIO.Common.Utils
{
    internal static class Extensions
    {     
        public static List<Obj_AI_Minion> GetAllGenericMinionsTargets()
        {
            return GetAllGenericMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetAllGenericMinionsTargetsInRange(float range)
        {
            return GetEnemyLaneMinionsTargetsInRange(range).Concat(GetGenericJungleMinionsTargetsInRange(range)).ToList();
        }

        public static List<Obj_AI_Base> GetAllGenericUnitTargets()
        {
            return GetAllGenericUnitTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Base> GetAllGenericUnitTargetsInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(range)).Concat<Obj_AI_Base>(GetAllGenericMinionsTargetsInRange(range)).ToList();
        }

        public static List<Obj_AI_Hero> GetAllyHeroesTargets()
        {
            return GetAllyHeroesTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Hero> GetAllyHeroesTargetsInRange(float range)
        {
            return GameObjects.AllyHeroes.Where(h => h.IsValidTarget(range, true)).ToList();
        }

        public static List<Obj_AI_Minion> GetAllyLaneMinionsTargets()
        {
            return GetAllyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetAllyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.AllyMinions.Where(m => m.IsValidTarget(range, true)).ToList();
        }

        public static List<Obj_AI_Hero> GetBestEnemyHeroesTargets()
        {
            return GetBestEnemyHeroesTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Hero> GetBestEnemyHeroesTargetsInRange(float range)
        {
            return Misc.TargetSelector.GetOrderedTargets(range);
        }

        public static Obj_AI_Hero GetBestEnemyHeroTarget()
        {
            return GetBestEnemyHeroTargetInRange(float.MaxValue);
        }

        public static Obj_AI_Hero GetBestEnemyHeroTargetInRange(float range)
        {
            var heroTarget = Misc.Orbwalker.GetOrbwalkingTarget() as Obj_AI_Hero;
            var target = heroTarget != null
                ? heroTarget
                : Misc.TargetSelector.GetTarget(range);

            if (target != null &&
                target.IsValidTarget() &&
                !Invulnerable.Check(target))
            {
                return target;
            }

            return null;
        }

        public static Obj_AI_Hero GetBestKillableHero(this Spell spell, DamageType damageType = DamageType.True, bool ignoreShields = false)
        {
            return Misc.TargetSelector.GetOrderedTargets(spell.Range - 100f).FirstOrDefault(t => !Invulnerable.Check(t, damageType, ignoreShields));
        }

        public static List<Obj_AI_Hero> GetEnemyHeroesTargets()
        {
            return GetEnemyHeroesTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Hero> GetEnemyHeroesTargetsInRange(float range)
        {
            return GameObjects.EnemyHeroes.Where(h => h.IsValidTarget(range)).ToList();
        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargets()
        {
            return GetEnemyLaneMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetEnemyLaneMinionsTargetsInRange(float range)
        {
            return GameObjects.EnemyMinions.Where(m => m.IsValidTarget(range) && m.UnitSkinName.Contains("Minion") && !m.UnitSkinName.Contains("Odin")).ToList();
        }

        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargets()
        {
            return GetGenericJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetGenericJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.Jungle.Where(m => !GameObjects.JungleSmall.Contains(m) && m.IsValidTarget(range)).ToList();
        }

        public static List<Obj_AI_Minion> GetLargeJungleMinionsTargets()
        {
            return GetLargeJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetLargeJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.JungleLarge.Where(m => m.IsValidTarget(range)).ToList();
        }

        public static List<Obj_AI_Minion> GetLegendaryJungleMinionsTargets()
        {
            return GetLegendaryJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetLegendaryJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.JungleLegendary.Where(m => m.IsValidTarget(range)).ToList();
        }

        public static List<Obj_AI_Minion> GetSmallJungleMinionsTargets()
        {
            return GetSmallJungleMinionsTargetsInRange(float.MaxValue);
        }

        public static List<Obj_AI_Minion> GetSmallJungleMinionsTargetsInRange(float range)
        {
            return GameObjects.JungleSmall.Where(m => m.IsValidTarget(range)).ToList();
        }
    }
}
