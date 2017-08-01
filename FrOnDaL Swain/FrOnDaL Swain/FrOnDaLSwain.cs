using Aimtec;
using System.Linq;
using System.Drawing;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Util;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Extensions;
using Spell = Aimtec.SDK.Spell;
using Aimtec.SDK.TargetSelector;
using System.Collections.Generic;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Prediction.Skillshots;

namespace FrOnDaL_Swain
{
    internal class FrOnDaLSwain
    {
        public static Menu Main = new Menu("Index", "FrOnDaL Swain", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Swain => ObjectManager.GetLocalPlayer();
        private static Spell _q, _w, _e, _r;
        public static readonly List<string> SpecialChampions = new List<string> { "Annie", "Jhin" };
        public static int SxOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 1 : 10;
        }
        public static int SyOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 7 : 20;
        }
        public FrOnDaLSwain()
        {
            /*Spells*/
            _q = new Spell(SpellSlot.Q, 700);
            _w = new Spell(SpellSlot.W, 900);
            _e = new Spell(SpellSlot.E, 625);
            _r = new Spell(SpellSlot.R, 625);

            _q.SetSkillshot(0.250f, 325, 1250, false, SkillshotType.Circle);
            _w.SetSkillshot(0.7f, 200f, 1200, false, SkillshotType.Circle);

            Orbwalker.Attach(Main);

            /*Combo Menu*/
            var combo = new Menu("combo", "Combo")
            {
                new MenuBool("q", "Use Combo Q"),
                new MenuList("qHit", "Q Hitchances", new []{ "Low", "Medium", "High", "VeryHigh", "Dashing", "Immobile" }, 1),
                new MenuBool("w", "Use Combo W"),
                new MenuSlider("UnitsWhit", "W Hit x Units Enemy", 1, 1, 3),
                new MenuList("wHit", "w Hitchances", new []{ "Low", "Medium", "High", "VeryHigh", "Dashing", "Immobile" }, 1),
                new MenuBool("e", "Use Combo E"),
                new MenuBool("r", "Use Combo R"),
                new MenuBool("rClose", "Use Auto Close R", false),
            };
            combo.OnValueChanged += HcMenu_ValueChanged;
            Main.Add(combo);

            /*LaneClear Menu*/
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 60, 0, 99),
                new MenuSlider("UnitsQhit", "Q Hit x Units minions >= x%", 3, 1, 6),
                new MenuSliderBool("w", "Use W / if Mana >= x%", true, 60, 0, 99),
                new MenuSlider("UnitsWhit", "W Hit x Units minions >= x%", 3, 1, 6),
                new MenuSliderBool("e", "Use E / if Mana >= x%", false, 60, 0, 99),
                new MenuSliderBool("r", "Use R / if Mana >= x%", false, 60, 0, 99),
            };
            Main.Add(laneclear);

            /*JungleClear Menu*/
            var jungleclear = new Menu("jungleclear", "Jungle Clear")
            {
                new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("w", "Use W / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("e", "Use E / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("r", "Use R / if Mana >= x%", true, 50, 0, 99),
            };
            Main.Add(jungleclear);

            /*Harass Menu*/
            var harass = new Menu("harass", "Harass")
            {
                new MenuBool("autoHarass", "Auto Harass", false),
                new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 70, 0, 99),
                new MenuSliderBool("w", "Use W / if Mana >= x%", false, 70, 0, 99),
                new MenuSliderBool("e", "Use E / if Mana >= x%", true, 70, 0, 99)
            };
            Main.Add(harass);

            /*Drawings Menu*/
            var drawings = new Menu("drawings", "Drawings")
            {
                new MenuBool("q", "Draw Q", false),
                new MenuBool("w", "Draw W"),
                new MenuBool("er", "Draw E and R"),
                new MenuBool("drawDamage", "Use Draw Ulti(R) Damage")
            };
            Main.Add(drawings);
            Main.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Render.OnPresent += SpellDraw;
        }

        /*Drawings*/
        private static void SpellDraw()
        {
            if (Main["drawings"]["q"].As<MenuBool>().Enabled)
            {
                Render.Circle(Swain.Position, _q.Range, 180, Color.Green);
            }
            if (Main["drawings"]["w"].As<MenuBool>().Enabled)
            {
                Render.Circle(Swain.Position, _w.Range, 180, Color.Green);
            }
            if (Main["drawings"]["er"].As<MenuBool>().Enabled)
            {
                Render.Circle(Swain.Position, _e.Range, 180, Color.Green);
            }          
        }

        private static void Game_OnUpdate()
        {
            if (Swain.IsDead || MenuGUI.IsChatOpen()) return;

            switch (Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                   Combo();
                    break;

                case OrbwalkingMode.Laneclear:
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }

            if (Main["harass"]["autoHarass"].As<MenuBool>().Enabled)
            {
                Harass();
            }
        }
        /*Combo*/
        private static void Combo()
        {
            var target = TargetSelector.GetTarget(1500);
            if (target != null)
                if (Main["combo"]["w"].As<MenuBool>().Enabled && _w.Ready && target.IsValidTarget(_w.Range))
                {
                    if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(_w.Width, false, true, _w.GetPrediction(target).CastPosition)) >= Main["combo"]["UnitsWhit"].As<MenuSlider>().Value)
                    {
                        _w.Cast(_w.GetPrediction(target).CastPosition);
                    }
                }


            if (Main["combo"]["q"].As<MenuBool>().Enabled && _q.Ready && target.IsValidTarget(_q.Range))
            {
                if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(_q.Width, false, true, _q.GetPrediction(target).CastPosition)) >= 1)
                {
                    _q.Cast(_q.GetPrediction(target).CastPosition);
                }
                
            }

            if (Main["combo"]["e"].As<MenuBool>().Enabled && _e.Ready && target.IsValidTarget(_e.Range))
            {
               
                _e.CastOnUnit(target);
            }

            if (Main["combo"]["r"].As<MenuBool>().Enabled && _r.Ready )
            {
                if (Swain.HasBuff("SwainMetamorphism") && Swain.CountEnemyHeroesInRange(800) == 0 && Main["combo"]["rClose"].As<MenuBool>().Enabled)
                {
                    _r.Cast();
                }             
                if (!Swain.HasBuff("SwainMetamorphism") && target.IsValidTarget(_r.Range))
                {
                    _r.Cast();
                }           
            }
        }
        /*Harass*/
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(1500);
            if (target == null) return;
            if (Main["harass"]["q"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value && _q.Ready && target.IsValidTarget(_q.Range))
            {
                if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(_q.Width, false, true, _q.GetPrediction(target).CastPosition)) >= 1)
                {
                    _q.Cast(_q.GetPrediction(target).CastPosition);
                }
            }

            if (Main["harass"]["w"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["harass"]["w"].As<MenuSliderBool>().Value && _w.Ready && target.IsValidTarget(_w.Range))
            {
                if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(_w.Width, false, true, _w.GetPrediction(target).CastPosition)) >= 1)
                {
                    _w.Cast(_w.GetPrediction(target).CastPosition);
                }
            }
            if (Main["harass"]["e"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["harass"]["e"].As<MenuSliderBool>().Value && _e.Ready && target.IsValidTarget(_e.Range))
            {
               
                _e.CastOnUnit(target);
            }
        }

        /*LaneClear*/
        private static void LaneClear()
        {
            if (Main["laneclear"]["w"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["laneclear"]["w"].As<MenuSliderBool>().Value && _w.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_w.Range)))
                {
                    if (target == null) continue;
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(_w.Width, false, false, _w.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsWhit"].As<MenuSlider>().Value && !Swain.IsUnderEnemyTurret())
                    {
                        _w.Cast(_w.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value && _q.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_q.Range)))
                {
                    if (target == null) continue;
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(_q.Width, false, false, _q.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsQhit"].As<MenuSlider>().Value && !Swain.IsUnderEnemyTurret())
                    {
                        _q.Cast(_q.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (Main["laneclear"]["e"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value && _e.Ready)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_e.Range)).ToList())
                {
                    if (!minion.IsValidTarget(_e.Range) || minion == null) continue;
                    
                    _e.CastOnUnit(minion);
                }
            }

            if (Main["laneclear"]["r"].As<MenuSliderBool>().Enabled && _r.Ready)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_r.Range)).ToList())
                {
                    if (Swain.HasBuff("SwainMetamorphism") && Swain.ManaPercent() < Main["laneclear"]["r"].As<MenuSliderBool>().Value)
                    {
                        _r.Cast();
                    }
                    if (!minion.IsValidTarget(_r.Range) || minion == null) continue;
                    var countt = GameObjects.EnemyMinions.Count(x => x.IsValidTarget(_r.Range + 25));
                    if (countt >= 3 && !Swain.HasBuff("SwainMetamorphism") && Swain.ManaPercent() > Main["laneclear"]["r"].As<MenuSliderBool>().Value)
                    {
                        _r.Cast();
                    }                    
                }
            }
        }

        /*JungleClear*/
        private static void JungleClear()
        {
            foreach (var targetJ in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && _w.Ready && targetJ.IsValidTarget(_w.Range))
                {                 
                       
                            _w.Cast(targetJ.Position);                                        
                }

                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && _q.Ready && targetJ.IsValidTarget(_q.Range))
                {                                      
                            _q.Cast(targetJ.Position);                                       
                }

                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Swain.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && _e.Ready && targetJ.IsValidTarget(_e.Range))
                {                      
                            _e.CastOnUnit(targetJ);             
                }

                if (Main["jungleclear"]["r"].As<MenuSliderBool>().Enabled && _r.Ready)
                {                   
                        if (targetJ.IsValidTarget(_r.Range) && !Swain.HasBuff("SwainMetamorphism") && Swain.ManaPercent() > Main["jungleclear"]["r"].As<MenuSliderBool>().Value)
                        {
                            _r.Cast(targetJ);
                        }
                        else if (Swain.HasBuff("SwainMetamorphism") && Swain.ManaPercent() < Main["jungleclear"]["r"].As<MenuSliderBool>().Value)
                        {
                            _r.Cast();
                        }                  
                }
            }                   
        }
        /*Draw Damage Ulti */
        private static void DamageDraw()
        {
            ObjectManager.Get<Obj_AI_Base>()
                .Where(h => h is Obj_AI_Hero && h.IsValidTarget() && h.IsValidTarget(1700))
                .ToList()
                .ForEach(
                    unit =>
                    {
                        var heroUnit = unit as Obj_AI_Hero;
                        const int width = 103;
                        var xOffset = SxOffset(heroUnit);
                        var yOffset = SyOffset(heroUnit);
                        var barPos = unit.FloatingHealthBarPosition;
                        barPos.X += xOffset;
                        barPos.Y += yOffset;
                        var drawEndXPos = barPos.X + width * (unit.HealthPercent() / 100);
                        var drawStartXPos =
                            (float)(barPos.X + (unit.Health >
                                                Swain.GetSpellDamage(unit, SpellSlot.Q) +
                                                Swain.GetSpellDamage(unit, SpellSlot.W) + 
                                                Swain.GetSpellDamage(unit, SpellSlot.E) +
                                                Swain.GetSpellDamage(unit, SpellSlot.R)
                                        ? width * ((unit.Health - (Swain.GetSpellDamage(unit, SpellSlot.Q) +
                                                                   Swain.GetSpellDamage(unit, SpellSlot.W) + 
                                                                   Swain.GetSpellDamage(unit, SpellSlot.E) +
                                                                   Swain.GetSpellDamage(unit, SpellSlot.R))) /
                                                   unit.MaxHealth * 100 / 100)
                                        : 0));

                        Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 8, true,
                            unit.Health < Swain.GetSpellDamage(unit, SpellSlot.Q) +
                            Swain.GetSpellDamage(unit, SpellSlot.W) +
                            Swain.GetSpellDamage(unit, SpellSlot.E) +
                            Swain.GetSpellDamage(unit, SpellSlot.R)
                                ? Color.GreenYellow
                                : Color.ForestGreen);
                    });       
        }

        /*Credit Eox*/
        private static void HcMenu_ValueChanged(MenuComponent sender, ValueChangedArgs args)
        {
            if (args.InternalName == "qHit")
            {
                _q.HitChance = (HitChance)args.GetNewValue<MenuList>().Value + 3;
            }

            if (args.InternalName == "wHit")
            {
                _w.HitChance = (HitChance)args.GetNewValue<MenuList>().Value + 3;
            }
        }
    }
}
