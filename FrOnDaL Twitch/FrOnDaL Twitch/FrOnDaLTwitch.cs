using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Aimtec;

using Aimtec.SDK;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Damage.JSON;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Prediction;
using Aimtec.SDK.Prediction.Skillshots;

using Aimtec.SDK.Menu;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.TargetSelector;
using Aimtec.SDK.Util;
using Aimtec.SDK.Prediction.Health;
using Aimtec.SDK.Util.Cache;
using Spell = Aimtec.SDK.Spell;

namespace FrOnDaL_Twitch
{
   
    internal class FrOnDaLTwitch
    {
        public static Menu Main = new Menu("Index", "FrOnDaL Twitch", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Twitch => ObjectManager.GetLocalPlayer();
        public static HealthPrediction HealthPrediction = new HealthPrediction();
        private static Spell _q, _w, _e;
        public static double TotalDamage(Obj_AI_Base unit)
        {
            return Twitch.GetSpellDamage(unit, SpellSlot.E) + Twitch.GetSpellDamage(unit, SpellSlot.E, DamageStage.Buff);
        }
        public static bool Target(Obj_AI_Base unit)
        {
            if (!unit.HasBuff("twitchdeadlyvenom")) return false;
            return unit.Type == GameObjectType.obj_AI_Minion;
        }
        public FrOnDaLTwitch()
        {
            /*Spells*/
            _q = new Spell(SpellSlot.Q);
            _w = new Spell(SpellSlot.W, 950f);
            _e = new Spell(SpellSlot.E, 1200f);
            _w.SetSkillshot(0.25f, 200f, 1400f, false, SkillshotType.Circle);          

            Orbwalker.Attach(Main);

            /*Combo Menu*/
            var combo = new Menu("combo", "Combo")
            {
                //new MenuBool("qAfterKill", "Use Q After Kill"),
                new MenuBool("w", "Use Combo W"),
                new MenuSlider("UnitsWhit", "W Hit x Units Enemy", 1, 1, 3),
                new MenuBool("e", "Use Auto E Kill Steal")
            };
            Main.Add(combo);

            /*LaneClear Menu*/
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuSliderBool("w", "Use W / if Mana >= x%", true, 60, 0, 99),
                new MenuSlider("UnitsWhit", "W Hit x Units Minions", 3, 1, 4),
                new MenuSliderBool("e", "Use E / if Mana >= x%", true, 50, 0, 99),
                new MenuSlider("UnitsEhit", "E if killable minions >= x%", 3, 1, 7)
            };
            Main.Add(laneclear);

            /*JungleClear Menu*/
            var jungleclear = new Menu("jungleclear", "Jungle Clear")
            {
                new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("w", "Use W / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("e", "Use E / if Mana >= x%", true, 30, 0, 99),
                new MenuBool("eJungleSteal", "E JungleSteal", false)
            };
            Main.Add(jungleclear);

            /*Auto Smite Menu*/
            //var autoSmite = new Menu("autoSmite", "Auto Smite")
            //{
            //    new MenuKeyBind("autoSmite", "Use Auto Smite Key:", KeyCode.M, KeybindType.Toggle),
            //    new MenuBool("hero", "Auto Smite Champions"),
            //    new MenuBool("reblue", "Auto Smite Blue and Red"),
            //    new MenuBool("wolf", "Auto Smite MurkWolf", false),
            //    new MenuBool("gromp", "Auto Smite Gromp", false),
            //    new MenuBool("krug", "Auto Smite Krugs", false),
            //    new MenuBool("razor", "Auto Smite Razorbeak", false)
            //};
            //Main.Add(autoSmite);

            /*Miscellaneous Menu*/
            var misc = new Menu("misc", "Misc")
            {
                new MenuBool("autob", "Auto Base Q"),
                //new MenuBool("ghostBladeR", "Youmuu Ghost Blade --> Use R"),
                //new MenuBool("ghostBladeQ", "Youmuu Ghost Blade --> Use Q ends"),
                //new MenuBool("autoBotrk", "Use Blade Of The Ruined King"),
                //new MenuBool("autoCutlass", "Use Bilgewater Cutlass")
            };
            Main.Add(misc);

            /*Drawings Menu*/
            var drawings = new Menu("drawings", "Drawings")
            {
                new MenuBool("er", "Draw E and R"),
                new MenuBool("w", "Draw W"),
                //new MenuBool("smiteDrw", "Smite Draw"),
                //new MenuBool("EKillStealD", "Damage Indicator [E Damage]"),
                //new MenuBool("smiteDamage", "Damage Indicator [Smite Damage]")
            };
            Main.Add(drawings);
            Main.Attach();

            Game.OnUpdate += Game_OnUpdate;
            SpellBook.OnCastSpell += auto_baseQ;
            Render.OnPresent += SpellDraw;
        }

        private static void SpellDraw()
        {
            if (Main["drawings"]["w"].As<MenuBool>().Enabled)
            {
                Render.Circle(Twitch.Position, _w.Range, 180, Color.Green);
            }
            if (Main["drawings"]["er"].As<MenuBool>().Enabled)
            {
                Render.Circle(Twitch.Position, _e.Range, 180, Color.Green);
            }
        }
        private static void Game_OnUpdate()
        {
            if (Twitch.IsDead || MenuGUI.IsChatOpen()) return;
            
            switch (Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    break;

                case OrbwalkingMode.Laneclear:
                    LaneClear();
                    JungleClear();
                    break;
            }
        }

        /*LaneClear*/
        private static void LaneClear()
        {     
            if (Main["laneclear"]["w"].As<MenuSliderBool>().Enabled && Twitch.ManaPercent() > Main["laneclear"]["w"].As<MenuSliderBool>().Value && _w.Ready)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_w.Range)).ToList())
                {
                    if (!minion.IsValidTarget(_w.Range) || minion == null) continue;
                    var countt = GameObjects.EnemyMinions.Count(x => x.IsValidTarget(_e.Range));
                    if (countt >= Main["laneclear"]["UnitsWhit"].As<MenuSlider>().Value)
                    {
                        _w.Cast(minion);
                    }
                }
            }

            if (!Main["laneclear"]["e"].As<MenuSliderBool>().Enabled || !(Twitch.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value) || !_e.Ready) return;
            {
                var killableMinions = GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_e.Range)).Count(x => Target(x) && TotalDamage(x) > x.Health);
                if (killableMinions >= Main["laneclear"]["UnitsEhit"].As<MenuSlider>().Value)
                {
                    _e.Cast();
                }
            }
        }
     
        /*JungleClear*/
        private static void JungleClear()
        {
            for (var index = 0; index < GameObjects.Jungle.Where(m => m.IsValidTarget(_w.Range)).ToList().Count; index++)
            {
                var jungleTarget = GameObjects.Jungle.Where(m => m.IsValidTarget(_w.Range)).ToList()[index];
                if (!jungleTarget.IsValidTarget() || !GameObjects.Jungle.Concat(GameObjects.JungleSmall).Where(m => m.IsValidTarget(_w.Range)).ToList().Contains(jungleTarget))
                {
                    return;
                }

                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Twitch.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && _q.Ready &&
                    jungleTarget.IsValidTarget(_w.Range))
                {
                    _q.Cast(jungleTarget);
                }

                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Twitch.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && _w.Ready &&
                    jungleTarget.IsValidTarget(_w.Range))
                {
                    _w.Cast(jungleTarget.Position);
                }

                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && !Main["jungleclear"]["eJungleSteal"].As<MenuBool>().Enabled && Twitch.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && _e.Ready)
                {                    
                    if (jungleTarget.GetBuffCount("TwitchDeadlyVenom") > 5)
                    {
                        _e.Cast();
                    }
                }

                if (!Main["jungleclear"]["eJungleSteal"].As<MenuBool>().Enabled || !(Twitch.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value) || !_e.Ready) continue;
                var killableMonster = GameObjects.Jungle.Where(x => x.IsValidTarget(_e.Range)).Count(x => Target(x) && TotalDamage(x) > x.Health);
                if (killableMonster >= 1)
                {
                    _e.Cast();                      
                }
            }
        }

        private static void Combo()
        {            
            if (Main["combo"]["w"].As<MenuBool>().Enabled && _w.Ready)
            {
                var target = TargetSelector.GetTarget(_w.Range);
                if (target == null) return;
                if (target.CountEnemyHeroesInRange(_w.Width) >= Main["combo"]["UnitsWhit"].As<MenuSlider>().Value)
                {
                    _w.Cast(target.Position);
                }              
            }

            if (Main["combo"]["e"].As<MenuBool>().Enabled && _e.Ready)
            {
                if (GameObjects.EnemyHeroes.Any(x => x.IsValidTarget(_e.Range) && ESpellDamage(x) > x.Health))
                {
                    _e.Cast();            
                }
            }          
        }
      
        public static float ESpellDamage(Obj_AI_Base obj)
        {
            var temel = Twitch.SpellBook.GetSpell(SpellSlot.E).Level - 1;
            float temelDamage = 0;
            if (!_e.Ready) return temelDamage;
            var temelDeger = new float[] { 20, 35, 50, 65, 80 }[temel];
            var temelHit = new float[] { 15, 20, 25, 30, 35 }[temel];
            var ekstraHasarD = 0.25f * Twitch.FlatPhysicalDamageMod;
            var ekstraHasarP = 0.20f * Twitch.TotalAbilityDamage;
            temelDamage = temelDeger + (temelHit + ekstraHasarD + ekstraHasarP) * obj.BuffManager.GetBuffCount("TwitchDeadlyVenom");
            return temelDamage;
        }

        private static void auto_baseQ(Obj_AI_Base sender, SpellBookCastSpellEventArgs eventArgs)
        {
            if (eventArgs.Slot != SpellSlot.Recall || !_q.Ready || !Main["misc"]["autob"].As<MenuBool>().Enabled) return;
            _q.Cast();           
            DelayAction.Queue((int)Twitch.SpellBook.GetSpell(SpellSlot.Q).SpellData.SpellCastTime + 300,
                () => Twitch.SpellBook.CastSpell(SpellSlot.Recall));
        }
    }

}
