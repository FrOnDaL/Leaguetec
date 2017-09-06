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
    internal class JarvanIv
    {
        public JarvanIv()
        {
            Q = new Spell(SpellSlot.Q, 770);
            W = new Spell(SpellSlot.W, 625);
            E = new Spell(SpellSlot.E, 860);
            R = new Spell(SpellSlot.R, 650);
            R2 = new Spell(SpellSlot.R, 650);

            Q.SetSkillshot(0.6f, 70, float.MaxValue, false, SkillshotType.Line);
            E.SetSkillshot(0.5f, 175, float.MaxValue, false, SkillshotType.Circle);
            R2.SetSkillshot(0.25f, 325f, float.MaxValue, false, SkillshotType.Circle);
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner1).SpellData.Name == "SummonerFlash")
            {
                Flash = new Spell(SpellSlot.Summoner1, 425);
            }
            if (Player.SpellBook.GetSpell(SpellSlot.Summoner2).SpellData.Name == "SummonerFlash")
            {
                Flash = new Spell(SpellSlot.Summoner2, 425);
            }

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var jarvan = new Menu("jarvan", "Jarvan IV");
            {
                var combo = new Menu("combo", "Combo")
                {
                    new MenuBool("q", "Use combo Q"),       
                    new MenuBool("w", "Use combo W"),
                    new MenuSlider("wEnemy", "Minimum enemies for use W", 1, 1, 5),
                    new MenuBool("e", "Use combo E"),
                    new MenuBool("eQ", "Save E For E-Q"),
                    new MenuKeyBind("r", "Semi-manual R key", KeyCode.S, KeybindType.Press),
                    new MenuBool("rKS", "Auto R kill-steal"),
                    new MenuSliderBool("autoR", "Auto R minimum enemies for >= x", true, 3, 1, 5),
                    new MenuKeyBind("eqrKey", "Use cast E-Q-R key", KeyCode.T, KeybindType.Press),
                    new MenuKeyBind("eqfKey", "Use cast Flash-E-Q key", KeyCode.G, KeybindType.Press),
                    new MenuKeyBind("fleeKey", "Flee (cursor position) E-Q key", KeyCode.Z, KeybindType.Press)
                };
                jarvan.Add(combo);

                var harass = new Menu("harass", "Harass")
                {
                    new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                    new MenuBool("autoH", "Auto Harass", false),
                    new MenuSliderBool("q", "Harass use Q / if mana >= x%", true, 50, 0, 99),
                };
                jarvan.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 60, 0, 99),
                    new MenuSlider("UnitsQhit", "Q Hit x units minions >= x%", 3, 1, 6),
                    new MenuSliderBool("eQ", "Use E-Q / if mana >= x% (Beta)", false, 60, 0, 99),
                };
                jarvan.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", true, 30, 0, 99)
                };
                jarvan.Add(jungleclear);

                //var antiGapcloser = new Menu("antiGapcloser", "Jarvan IV anti-gapcloser spells")
                //{
                //    new MenuBool("w", "Anti-gapcloser W")
                //};
                //jarvan.Add(antiGapcloser);
                //Gapcloser.Attach(jarvan, "Anti-gapcloser settings");
                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q"));
                    drawings.Add(new MenuBool("w", "Draw W", false));
                    drawings.Add(new MenuBool("e", "Draw E", false));
                    drawings.Add(new MenuBool("r", "Draw R", false));
                    drawings.Add(new MenuBool("eqr", "Draw E-Q-R"));
                    drawings.Add(new MenuBool("eqf", "Draw Flash-E-Q"));
                    var drawDamage = new Menu("drawDamage", "Use draw damage");
                    {
                        drawDamage.Add(new MenuBool("enabled", "Enabled"));
                        drawDamage.Add(new MenuBool("q", "Draw Q damage", false));
                        drawDamage.Add(new MenuBool("e", "Draw E damage", false));
                        drawDamage.Add(new MenuBool("r", "Draw R damage"));
                    }
                    drawings.Add(drawDamage);
                }
                jarvan.Add(drawings);
            }
            Main.Add(jarvan);
            Main.Attach();
            Render.OnPresent += SpellDraw;
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            //Gapcloser.OnGapcloser += AntiGapcloser;
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
            if (Main["drawings"]["eqr"].As<MenuBool>().Enabled && E.Ready && Q.Ready && R.Ready)
            {
                Render.Circle(Player.Position, Q.Range + R.Range - 50, 180, Color.GreenYellow);
            }
            if (Main["drawings"]["eqf"].As<MenuBool>().Enabled && E.Ready && Q.Ready && Flash.Ready)
            {
                Render.Circle(Player.Position, Q.Range + Flash.Range - 100, 180, Color.Yellow);
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
                case OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case OrbwalkingMode.Laneclear:
                    LaneClear();
                    JungleClear();
                    break;
            }

            if (R.Ready && Main["combo"]["rKS"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target != null && !Player.HasBuff("JarvanIVCataclysm"))
                {
                    if (target.Health < RDamage(target) && !Check(target))
                    {
                        if (Player.Distance(target.ServerPosition) < 650 && (Player.Distance(target.ServerPosition) > 450 && !E.Ready && !Q.Ready || Player.HealthPercent() < 15))
                        {
                            R.Cast(target);
                        }
                        if (Player.Distance(target.ServerPosition) > 870 && Player.Health > target.Health && target.CountEnemyHeroesInRange(700) <= 2)
                        {
                            FullCombo();
                        }           
                    }             
                }
            }
            if (Main["combo"]["autoR"].As<MenuSliderBool>().Enabled && R.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target != null && !Player.HasBuff("JarvanIVCataclysm"))
                {
                    if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(R2.Width, false, true, R2.GetPrediction(target).CastPosition)) >= Main["combo"]["autoR"].As<MenuSliderBool>().Value)
                    {      
                        R.CastOnUnit(target);
                    }
                }               
            }
            if (R.Ready && Main["combo"]["r"].As<MenuKeyBind>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target != null && !Player.HasBuff("JarvanIVCataclysm"))                    
                {
                    R.CastOnUnit(target);
                }
            }
            if (Main["combo"]["eqrKey"].As<MenuKeyBind>().Enabled)
            {
                FullCombo();
            }
            if (Main["combo"]["eqfKey"].As<MenuKeyBind>().Enabled)
            {
                EqFlash();
            }
            if (Main["combo"]["fleeKey"].As<MenuKeyBind>().Enabled)
            {
                if (Q.Ready && E.Ready && Player.Mana > Player.SpellBook.GetSpell(SpellSlot.E).Cost + Player.SpellBook.GetSpell(SpellSlot.Q).Cost)
                {
                    E.Cast(Game.CursorPos);
                    Q.Cast(Game.CursorPos);
                }     
            }
            if (Main["harass"]["autoH"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Combo && Misc.Orbwalker.Mode != OrbwalkingMode.Mixed)
            {
                Harass();
            }
        }
        private static float QDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.Q);
        }

        private static float EDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.E);
        }
        private static float RDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.R);
        }

        private static void Combo()
        {
            if (Main["combo"]["e"].As<MenuBool>().Enabled && E.Ready)
            {
                var eKillSteal = GetBestEnemyHeroTargetInRange(E.Range);
                if (eKillSteal != null)
                {
                    if (eKillSteal.Health < EDamage(eKillSteal))
                    {
                        E.Cast(eKillSteal);
                    }
                }             
                if (Main["combo"]["eQ"].As<MenuBool>().Enabled && Main["combo"]["q"].As<MenuBool>().Enabled &&
                    (Player.Mana < Player.SpellBook.GetSpell(SpellSlot.E).Cost + Player.SpellBook.GetSpell(SpellSlot.Q).Cost ||!Q.Ready))
                {
                    return;
                }
                var target = GetBestEnemyHeroTargetInRange(E.Range + (Main["combo"]["q"].As<MenuBool>().Enabled && Q.Ready ? 0 : E.Width / 2));
                if (target != null)
                {
                    var pred = E.GetPrediction(target);
                    if (pred.HitChance >= HitChance.VeryHigh && E.Cast(pred.UnitPosition.Extend(Player.ServerPosition, -E.Width / (target.IsFacing(Player) ? 2 : 1))))
                    {
                        if (Main["combo"]["q"].As<MenuBool>().Enabled && Q.Ready)
                        {
                            Q.Cast(pred.UnitPosition);
                        }
                        return;
                    }
                }
            }
            if (Main["combo"]["q"].As<MenuBool>().Enabled && Q.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target != null)
                {
                    if (!Main["combo"]["eQ"].As<MenuBool>().Enabled || !E.Ready)
                    {
                        var pred = Q.GetPrediction(target);
                        if (pred.HitChance >= HitChance.High)
                        {
                            Q.Cast(target, true);
                        }                     
                    }
                }      
            }
            if (Main["combo"]["w"].As<MenuBool>().Enabled && W.Ready && Player.CountEnemyHeroesInRange(W.Range - 60) >= Main["combo"]["wEnemy"].As<MenuSlider>().Value)
            {
                W.Cast();
            }
        }

        private static void FullCombo()
        {
            var manacheck = Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.E).Cost +
                            Player.SpellBook.GetSpell(SpellSlot.R).Cost;
            var target = GetBestEnemyHeroTargetInRange(Q.Range + R.Range - 100);
            if (target != null)
            {
                if (Player.Distance(target.ServerPosition) > 860 && E.Ready && Q.Ready && R.Ready && manacheck && target.IsValidTarget(Q.Range + R.Range - 100))
                {
                    E.Cast(target.ServerPosition);
                    Q.Cast(target.ServerPosition);
                }
                if (R.Ready && !Player.HasBuff("JarvanIVCataclysm") && target.IsValidTarget(R.Range))
                {       
                    R.CastOnUnit(target);
                }
                if (W.Ready)
                {
                    if (target.IsValidTarget(W.Range) && Player.CountEnemyHeroesInRange(W.Range - 100) >= 1)
                    {
                        W.Cast();
                    }
                }
            }     
        }

        private static void EqFlash()
        {
            var manacheck = Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.E).Cost;
            var target = GetBestEnemyHeroTargetInRange(Q.Range + Flash.Range - 100);
            if (target != null && manacheck && Q.Ready && E.Ready)
            {
                if (target.IsValidTarget(Q.Range + Flash.Range) && Player.Distance(target.ServerPosition) < 1180 && Player.Distance(target.ServerPosition) > 600)
                {
                        var xpos = target.Position.Extend(target.ServerPosition, E.Range);
                        Flash.Cast(xpos);
                    
                        E.Cast(target.ServerPosition.Extend(Player.ServerPosition, -E.Width-100));
                        Q.Cast(target.ServerPosition);
                    if (W.Ready)
                    {
                        if (target.IsValidTarget(W.Range) && Player.CountEnemyHeroesInRange(W.Range - 100) >= 1)
                        {
                            W.Cast();
                        }
                    }
                }                      
            }
        }

        private static void Harass()
        {
            var target = GetBestEnemyHeroTargetInRange(Q.Range);
            if (target == null) return;
            if (Q.Ready && Main["harass"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value)
            {
                var pred = Q.GetPrediction(target);
                if (pred.HitChance >= HitChance.High)
                {
                    Q.Cast(pred.CastPosition);
                }
            }
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["eQ"].As<MenuSliderBool>().Enabled &&
                Player.ManaPercent() > Main["laneclear"]["eQ"].As<MenuSliderBool>().Value && E.Ready && Q.Ready && Player.Mana > Player.SpellBook.GetSpell(SpellSlot.E).Cost + Player.SpellBook.GetSpell(SpellSlot.Q).Cost)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range)))
                {
                    if (target != null)
                    {
                        if (GameObjects.EnemyMinions.Count(t => Player.Distance(t) > 500 && t.IsValidTarget(E.Width + 50, false, false,
                            E.GetPrediction(target).CastPosition)) >= 3 && Player.CountEnemyHeroesInRange(1000) == 0 && GameObjects.EnemyMinions.Count(x => x.IsValidTarget(800)) >= 6)
                        {
                            E.Cast(E.GetPrediction(target).CastPosition);
                            Q.Cast(E.GetPrediction(target).CastPosition);
                        }
                    }
                }
            }
            if (Main["laneclear"]["eQ"].As<MenuSliderBool>().Enabled && E.Ready)
            {
                return;
            }
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Q.Ready &&
                Player.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value)
            {
                    var result = Polygon.GetLinearLocation(Q.Range, Q.Width);
                    if (result == null) return;
                    if (result.NumberOfMinionsHit >= Main["laneclear"]["UnitsQhit"].As<MenuSlider>().Value)
                    {
                        Q.Cast(result.CastPosition);
                    }   
            }          
        }
        private static void JungleClear()
        {
            foreach (var target in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready && target.IsValidTarget(E.Range))
                {
                    E.Cast(target.Position);
                    if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Q.Ready)
                    {
                        Q.Cast(target.Position);
                    }                 
                }
                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready && Player.Distance(target) < 350)
                {
                    W.Cast(Player.Position);
                }               
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready && !E.Ready)
                {
                    Q.Cast(target.Position);
                }
            }
        }
        //private static void AntiGapcloser(Obj_AI_Hero target, GapcloserArgs args)
        //{
        //    if (target == null || !target.IsValidTarget(E.Range) || args.HaveShield) return;

        //    switch (args.Type)
        //    {
        //        case SpellType.SkillShot:
        //        {
        //            if (Main["antiGapcloser"]["w"].As<MenuBool>().Enabled && W.Ready && target.IsValidTarget(W.Range))
        //            {
        //                W.Cast();
        //            }
        //            }
        //            break;
        //        case SpellType.Targeted:
        //        {
        //            if (Main["antiGapcloser"]["w"].As<MenuBool>().Enabled && W.Ready && target.IsValidTarget(W.Range))
        //            {
        //                W.Cast();
        //            }
        //            }
        //            break;
        //    }
            
        //}
        private static void DamageDraw()
        {
            if (!Main["drawDamage"]["enabled"].Enabled) return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.IsVisible && Player.Distance(x) < 1700))
            {
                float qdmgDraw = 0, edmgDraw = 0, rdmgDraw = 0;
                if (Q.Ready && Main["drawDamage"]["q"].As<MenuBool>().Enabled)
                {
                    qdmgDraw = QDamage(enemy);
                }
                if (E.Ready && Main["drawDamage"]["e"].As<MenuBool>().Enabled)
                {
                    edmgDraw = EDamage(enemy);
                }
                if (R.Ready && Main["drawDamage"]["r"].As<MenuBool>().Enabled)
                {
                    rdmgDraw = RDamage(enemy);
                }
                var damage = qdmgDraw + edmgDraw + rdmgDraw;

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
