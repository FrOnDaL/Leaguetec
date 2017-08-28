using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Aimtec;
using Aimtec.SDK.Extensions;

namespace FrOnDaL_AIO.Common.Utils
{
    public class Invulnerable
    {   
        private static readonly List<InvulnerableEntry> PEntries = new List<InvulnerableEntry>();
        static Invulnerable()
        {
            PEntries.AddRange(
                new List<InvulnerableEntry>
                {
                    new InvulnerableEntry("JudicatorIntervention")
                    {
                        IsShield = true
                    },
                    new InvulnerableEntry("BlackShield")
                    {
                        IsShield = true,
                        DamageType = DamageType.Magical
                    },
                    new InvulnerableEntry("BansheesVeil")
                    {
                        IsShield = true,
                        DamageType = DamageType.Magical
                    },
                    new InvulnerableEntry("KindredrNoDeathBuff")
                    {
                        MinHealthPercent = 10
                    },
                    new InvulnerableEntry("FerociousHowl")
                    {
                        ChampionName = "Alistar",
                        CheckFunction = (target, type) =>
                            Misc.Player.CountEnemyHeroesInRange(Misc.Player.AttackRange) > 1
                    },
                    new InvulnerableEntry("Meditate")
                    {
                        ChampionName = "MasterYi",
                        CheckFunction = (target, type) =>
                            Misc.Player.CountEnemyHeroesInRange(Misc.Player.AttackRange) > 1
                    },
                    new InvulnerableEntry("FioraW")
                    {
                        ChampionName = "Fiora",
                        IsShield = true
                    },
                    new InvulnerableEntry("JaxCounterStrike")
                    {
                        ChampionName = "Jax",
                        IsShield = true,
                        DamageType = DamageType.Physical
                    },
                    new InvulnerableEntry("malzaharpassiveshield")
                    {
                        ChampionName = "Malzahar",
                        IsShield = true
                    },
                    new InvulnerableEntry("NocturneShroudofDarkness")
                    {
                        ChampionName = "Nocturne",
                        IsShield = true
                    },
                    new InvulnerableEntry("OlafRagnarock")
                    {
                        ChampionName = "Olaf",
                        IsShield = true
                    },
                    new InvulnerableEntry("SivirE")
                    {
                        ChampionName = "Sivir",
                        IsShield = true
                    },
                    new InvulnerableEntry("UndyingRage")
                    {
                        ChampionName = "Tryndamere",
                        MinHealthPercent = 5
                    }
                });
        }
        public static ReadOnlyCollection<InvulnerableEntry> Entries => PEntries.AsReadOnly();
        public static bool Check(
            Obj_AI_Hero hero,
            DamageType damageType = DamageType.True,
            bool ignoreShields = true,
            float damage = -1f)
        {
            if (hero.Buffs.Any(b => b.Type == BuffType.Invulnerability) || hero.IsInvulnerable)
            {
                return true;
            }
            foreach (var entry in Entries)
            {
                if (entry.ChampionName == null || entry.ChampionName == hero.ChampionName)
                {
                    if (entry.DamageType == null || entry.DamageType == damageType)
                    {
                        if (hero.HasBuff(entry.BuffName))
                        {
                            if (!ignoreShields || !entry.IsShield)
                            {
                                if (entry.CheckFunction == null || ExecuteCheckFunction(entry, hero, damageType))
                                {
                                    if (damage <= 0 || entry.MinHealthPercent <= 0
                                        || (hero.Health - damage) / hero.MaxHealth * 100 < entry.MinHealthPercent)
                                    {
                                        return true;
                                    }
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }
        public static void Deregister(InvulnerableEntry entry)
        {
            if (PEntries.Any(i => i.BuffName.Equals(entry.BuffName)))
            {
                PEntries.Remove(entry);
            }
        }
        public static InvulnerableEntry GetItem(
            string buffName,
            StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
        {
            return PEntries.FirstOrDefault(w => w.BuffName.Equals(buffName, stringComparison));
        }
        public static void Register(InvulnerableEntry entry)
        {
            if (!string.IsNullOrEmpty(entry.BuffName) && !PEntries.Any(i => i.BuffName.Equals(entry.BuffName)))
            {
                PEntries.Add(entry);
            }
        }
        private static bool ExecuteCheckFunction(InvulnerableEntry entry, Obj_AI_Hero hero, DamageType damageType)
        {
            return entry != null && entry.CheckFunction(hero, damageType);
        }
    }

    public class InvulnerableEntry
    {
        public InvulnerableEntry(string buffName)
        {
            BuffName = buffName;
        }
        public string BuffName { get; set; }
        public string ChampionName { get; set; }
        public Func<Obj_AI_Base, DamageType, bool> CheckFunction { get; set; }
        public DamageType? DamageType { get; set; }
        public bool IsShield { get; set; }
        public int MinHealthPercent { get; set; }
    }
}
