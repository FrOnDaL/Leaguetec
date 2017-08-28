using Aimtec;
using System.Linq;
using System.Drawing;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Util;
using Aimtec.SDK.Damage;
using FrOnDaL_AIO.Common;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Extensions;
using FrOnDaL_AIO.Common.Utils;
using Spell = Aimtec.SDK.Spell;
using Aimtec.SDK.Menu.Components;
using static FrOnDaL_AIO.Common.Misc;
using Aimtec.SDK.Prediction.Skillshots;
using static FrOnDaL_AIO.Common.Utils.XyOffset;
using static FrOnDaL_AIO.Common.Utils.Extensions;
using static FrOnDaL_AIO.Common.Utils.Invulnerable;

namespace FrOnDaL_AIO.Champions
{
    internal class Swain
    {
        public Swain()
        {
            Q = new Spell(SpellSlot.Q, 700);
            W = new Spell(SpellSlot.W, 900);
            E = new Spell(SpellSlot.E, 625);
            R = new Spell(SpellSlot.R, 625);

            Q.SetSkillshot(0.250f, 325, 1250, false, SkillshotType.Circle);
            W.SetSkillshot(0.7f, 200f, 1200, false, SkillshotType.Circle);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var swain = new Menu("swain", "Swain");
            {
                var combo = new Menu("combo", "Combo")
                {
                    new MenuBool("q", "Use combo Q"),
                    new MenuBool("w", "Use combo W"),
                    new MenuBool("wCC", "Auto W on CC"),
                    new MenuSlider("UnitsWhit", "W hit x units enemy", 1, 1, 3),
                    new MenuBool("e", "Use combo E"),
                    new MenuBool("r", "Use combo R"),
                    new MenuSliderBool("autoR", "Auto R if x enemies in range", true, 2, 1, 5),
                    new MenuBool("rClose", "Use auto close R", false),
                };
                swain.Add(combo);

                var harass = new Menu("harass", "Harass")
                {
                    new MenuKeyBind("keyHarass", "Harass key:", KeyCode.C, KeybindType.Press),
                    new MenuBool("autoHarass", "Auto harass", false),     
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 50, 0, 99),
                    new MenuSliderBool("w", "Use W / if mana >= x%", false, 50, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", true, 50, 0, 99)
                };
                swain.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 60, 0, 99),
                    new MenuSlider("UnitsQhit", "Q hit x units minions >= x%", 3, 1, 6),
                    new MenuSliderBool("w", "Use W / if mana >= x%", true, 60, 0, 99),
                    new MenuSlider("UnitsWhit", "W hit x units minions >= x%", 3, 1, 6),
                    new MenuSliderBool("e", "Use E / if mana >= x%", false, 60, 0, 99),
                    new MenuSliderBool("r", "Use R / if mana >= x%", false, 60, 0, 99)
                };
                swain.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("r", "Use R / if mana >= x%", true, 50, 0, 99)
                };
                swain.Add(jungleclear);

                var antiGapcloser = new Menu("antiGapcloser", "Swain anti-gapcloser spells")
                {
                    new MenuBool("q", "Anti-gapcloser Q"),
                    new MenuBool("w", "Anti-gapcloser W")
                };
                swain.Add(antiGapcloser);
                Gapcloser.Attach(swain, "Anti-gapcloser settings");

                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q", false));
                    drawings.Add(new MenuBool("w", "Draw W"));
                    drawings.Add(new MenuBool("e", "Draw E"));
                    drawings.Add(new MenuBool("r", "Draw R"));
                    var drawDamage = new Menu("drawDamage", "Use draw damage");
                    {
                        drawDamage.Add(new MenuBool("enabled", "Enabled"));
                        drawDamage.Add(new MenuBool("q", "Draw Q damage"));
                        drawDamage.Add(new MenuBool("w", "Draw W damage"));
                        drawDamage.Add(new MenuBool("e", "Draw E damage"));
                        drawDamage.Add(new MenuBool("r", "Draw R damage"));

                    }
                    drawings.Add(drawDamage);
                }
                swain.Add(drawings);
            }
            Main.Add(swain);
            Main.Attach();
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += SpellDraw;
            Render.OnPresent += DamageDraw;
            Gapcloser.OnGapcloser += AntiGapcloser;
        }

        private static void SpellDraw()
        {
            if (Main["drawings"]["q"].As<MenuBool>().Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Q.Range, 180, Color.Green);
            }
            if (Main["drawings"]["w"].As<MenuBool>().Enabled && W.Ready)
            {
                Render.Circle(Player.Position, W.Range, 180, Color.Green);
            }
            if (Main["drawings"]["e"].As<MenuBool>().Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 180, Color.Green);
            }
            if (Main["drawings"]["r"].As<MenuBool>().Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 180, Color.Green);
            }
        }

        private static void Game_OnUpdate()
        {
            if (Player.IsDead || MenuGUI.IsChatOpen()) return;

            switch (Misc.Orbwalker.Mode)
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

            if (W.Ready && Main["combo"]["wCC"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Combo)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target != null)
                {
                    if (target.IsValidTarget(W.Range) && !Check(target, DamageType.Magical, false))
                    {
                        if (target.IsImmobile())
                        {
                            W.Cast(target.Position);
                        }
                    }
                }               
            }

            if (Main["harass"]["autoHarass"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Mixed)
            {
                Harass();
            }

            if (Main["combo"]["autoR"].As<MenuSliderBool>().Enabled && R.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target == null) return;
                if (Player.HasBuff("SwainMetamorphism") && Player.CountEnemyHeroesInRange(800) == 0 && Main["combo"]["rClose"].As<MenuBool>().Enabled)
                {
                    R.Cast();
                }
                if (!Player.HasBuff("SwainMetamorphism") && Player.CountEnemyHeroesInRange(550) >= Main["combo"]["autoR"].As<MenuSliderBool>().Value)
                {
                    R.Cast();
                }
            }
        }

        private static void Combo()
        {
            if (Q.Ready && Main["combo"]["q"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target == null) return;
                if (target.IsValidTarget(Q.Range) && !Check(target, DamageType.Magical, false))
                {
                    if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(Q.Width, false, true, Q.GetPrediction(target).CastPosition)) >= 1 || target.IsImmobile())
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (W.Ready && Main["combo"]["w"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target == null) return;
                if (target.IsValidTarget(W.Range) && !Check(target, DamageType.Magical, false))
                {                  
                    if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(W.Width, false, true, W.GetPrediction(target).CastPosition)) >= Main["combo"]["UnitsWhit"].As<MenuSlider>().Value || target.IsImmobile())
                    {
                        W.Cast(W.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (E.Ready && Main["combo"]["e"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target == null) return;
                if (target.IsValidTarget(E.Range) && !Check(target, DamageType.Magical, false))
                {
                    E.CastOnUnit(target);
                }
            }

            if (Main["combo"]["r"].As<MenuBool>().Enabled && R.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target == null) return;
                if (Player.HasBuff("SwainMetamorphism") && Player.CountEnemyHeroesInRange(800) == 0 && Main["combo"]["rClose"].As<MenuBool>().Enabled)
                {
                    R.Cast();
                }
                if (!Player.HasBuff("SwainMetamorphism") && target.IsValidTarget(R.Range))
                {
                    R.Cast();
                }
            }
        }

        private static void Harass()
        {
            if (Q.Ready && Main["harass"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target == null) return;
                if (target.IsValidTarget(Q.Range) && !Check(target, DamageType.Magical, false))
                {
                    if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(Q.Width, false, true, Q.GetPrediction(target).CastPosition)) >= 1 || target.IsImmobile())
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (W.Ready && Main["harass"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["w"].As<MenuSliderBool>().Value)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target == null) return;
                if (target.IsValidTarget(W.Range) && !Check(target, DamageType.Magical, false))
                {
                    if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(W.Width, false, true, W.GetPrediction(target).CastPosition)) >= 1 || target.IsImmobile())
                    {
                        W.Cast(W.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (E.Ready && Main["harass"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["e"].As<MenuSliderBool>().Value)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target == null) return;
                if (target.IsValidTarget(E.Range) && !Check(target, DamageType.Magical, false))
                {
                    E.CastOnUnit(target);
                }
            }
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["laneclear"]["w"].As<MenuSliderBool>().Value && W.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(W.Range)))
                {
                    if (target == null) continue;
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(W.Width, false, false, W.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsWhit"].As<MenuSlider>().Value)
                    {
                        W.Cast(W.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value && Q.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)))
                {
                    if (target == null) continue;
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(Q.Width, false, false, Q.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsQhit"].As<MenuSlider>().Value)
                    {
                        Q.Cast(Q.GetPrediction(target).CastPosition);
                    }
                }
            }

            if (Main["laneclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value && E.Ready)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range)).ToList())
                {
                    if (!minion.IsValidTarget(E.Range) || minion == null) continue;

                    E.CastOnUnit(minion);
                }
            }

            if (Main["laneclear"]["r"].As<MenuSliderBool>().Enabled && R.Ready)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(R.Range)).ToList())
                {
                    if (Player.HasBuff("SwainMetamorphism") && Player.ManaPercent() < Main["laneclear"]["r"].As<MenuSliderBool>().Value)
                    {
                        R.Cast();
                    }
                    if (!minion.IsValidTarget(R.Range) || minion == null) continue;
                    var countt = GameObjects.EnemyMinions.Count(x => x.IsValidTarget(R.Range + 25));
                    if (countt >= 3 && !Player.HasBuff("SwainMetamorphism") && Player.ManaPercent() > Main["laneclear"]["r"].As<MenuSliderBool>().Value)
                    {
                        R.Cast();
                    }
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var targetJ in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready && targetJ.IsValidTarget(W.Range))
                {

                    W.Cast(targetJ.Position);
                }

                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready && targetJ.IsValidTarget(Q.Range))
                {
                    Q.Cast(targetJ.Position);
                }

                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready && targetJ.IsValidTarget(E.Range))
                {
                    E.CastOnUnit(targetJ);
                }

                if (Main["jungleclear"]["r"].As<MenuSliderBool>().Enabled && R.Ready)
                {
                    if (targetJ.IsValidTarget(R.Range) && !Player.HasBuff("SwainMetamorphism") && Player.ManaPercent() > Main["jungleclear"]["r"].As<MenuSliderBool>().Value)
                    {
                        R.Cast(targetJ);
                    }
                    else if (Player.HasBuff("SwainMetamorphism") && Player.ManaPercent() < Main["jungleclear"]["r"].As<MenuSliderBool>().Value)
                    {
                        R.Cast();
                    }
                }
            }
        }

        private static void AntiGapcloser(Obj_AI_Hero target, GapcloserArgs args)
        {
            if (target == null || !target.IsValidTarget(E.Range) || args.HaveShield) return;

            switch (args.Type)
            {
                case SpellType.SkillShot:
                {
                    if (target.IsValidTarget(300))
                    {
                        if (Main["antiGapcloser"]["q"].As<MenuBool>().Enabled && Q.Ready && Game.TickCount > 2500)
                        {
                            var ePred = Q.GetPrediction(target);

                            Q.Cast(ePred.UnitPosition);
                        }
                        if (Main["antiGapcloser"]["w"].As<MenuBool>().Enabled && W.Ready && Game.TickCount > 2500)
                        {
                            var wPred = W.GetPrediction(target);

                            W.Cast(wPred.UnitPosition);
                        }
                    }
                }
                    break;
                case SpellType.Targeted:
                {
                    if (target.IsValidTarget(400))
                    {
                        if (Main["antiGapcloser"]["q"].As<MenuBool>().Enabled && Q.Ready && Game.TickCount > 2500)
                        {
                                var ePred = Q.GetPrediction(target);

                                Q.Cast(ePred.UnitPosition);
                        }
                        if (Main["antiGapcloser"]["w"].As<MenuBool>().Enabled && W.Ready && Game.TickCount > 2500)
                        {
                            var wPred = W.GetPrediction(target);

                            W.Cast(wPred.UnitPosition);
                        }
                     }
                }
                    break;
            }
        }
        private static float QDamage(Obj_AI_Base d)
        {
                return (float)Player.GetSpellDamage(d, SpellSlot.Q);
        }

        private static float WDamage(Obj_AI_Base d)
        {
                return (float)Player.GetSpellDamage(d, SpellSlot.W);
        }

        private static float EDamage(Obj_AI_Base d)
        {
                return (float)Player.GetSpellDamage(d, SpellSlot.E);
        }
        private static float RDamage(Obj_AI_Base d)
        {
                return (float)Player.GetSpellDamage(d, SpellSlot.R);
        }

        private static void DamageDraw()
        {
            if (!Main["drawDamage"]["enabled"].Enabled) return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && Player.Distance(x) < 1700))
            {
                float qdmgDraw = 0, wdmgDraw = 0, edmgDraw = 0, rdmgDraw = 0;
                if (Q.Ready && Main["drawDamage"]["q"].As<MenuBool>().Enabled)
                {
                    qdmgDraw = QDamage(enemy) * 3;
                }
                if (W.Ready && Main["drawDamage"]["w"].As<MenuBool>().Enabled)
                {
                    wdmgDraw = WDamage(enemy);
                }
                if (E.Ready && Main["drawDamage"]["e"].As<MenuBool>().Enabled)
                {
                    edmgDraw = EDamage(enemy);
                }
                if (R.Ready && Main["drawDamage"]["r"].As<MenuBool>().Enabled)
                {
                    rdmgDraw = RDamage(enemy);
                }
                var damage = qdmgDraw + wdmgDraw + edmgDraw + rdmgDraw;

                var xOffset = X(enemy);
                var yOffset = Y(enemy);
                var barPos = enemy.FloatingHealthBarPosition;
                barPos.X += xOffset;
                barPos.Y += yOffset;
                var drawEndXPos = barPos.X + 103 * (enemy.HealthPercent() / 100);
                var drawStartXPos = (barPos.X + (enemy.Health > damage ? 103 * ((enemy.Health - damage) / enemy.MaxHealth * 100 / 100) : 0));
                Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 9, true, enemy.Health < damage ? Color.GreenYellow : Color.ForestGreen);
            }
        }
    }
}
