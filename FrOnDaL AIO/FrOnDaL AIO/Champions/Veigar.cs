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
    internal class Veigar
    {
        public Veigar()
        {
            Q = new Spell(SpellSlot.Q, 900f);
            W = new Spell(SpellSlot.W, 900f);
            E = new Spell(SpellSlot.E, 725f);
            E2 = new Spell(SpellSlot.E, 725f);
            R = new Spell(SpellSlot.R, 650f);

            Q.SetSkillshot(0.25f, 70f, 2000f, true, SkillshotType.Line);
            W.SetSkillshot(1.35f, 225f, float.MaxValue, false, SkillshotType.Circle);
            E.SetSkillshot(.8f, 350f, float.MaxValue, false, SkillshotType.Circle);
            E2.SetSkillshot(.8f, 350f, float.MaxValue, false, SkillshotType.Circle);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var veigar = new Menu("veigar", "Veigar");
            {
                var combo = new Menu("combo", "Combo")
                {
                    new MenuBool("q", "Use combo Q"),
                    new MenuBool("w", "Use combo W"),
                    new MenuList("w2", "Use combo W", new []{ "Normal", "Stun" }, 0),
                    new MenuBool("e", "Use combo E"),
                    new MenuList("e2", "Use combo E", new []{ "Normal-1", "Normal-2", "Stun" }, 0),
                    new MenuSlider("UnitsEhit", "Normal mod minimum enemies for E", 1, 1, 3),
                    new MenuSliderBool("r", "Use combo R / Enemies health", false, 30, 1, 99),
                    new MenuBool("rKillSteal", "Use auto R killsteal"),
                    new MenuBool("disableAA", "Disable AutoAttacks", false)
                };
                veigar.Add(combo);

                var harass = new Menu("harass", "Harass")
                {
                    new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                    new MenuBool("autoHarass", "Auto Harass", false),
                    new MenuSliderBool("q", "Use harass Q", true, 50, 0, 99),
                    new MenuSliderBool("w", "Use harass W", true, 50, 0, 99),
                    new MenuList("w2", "Use harass W", new []{ "Normal", "Stun" }, 1),
                    new MenuSliderBool("e", "Use harass E", true, 50, 0, 99),
                    new MenuList("e2", "Use harass E", new []{ "Normal-1", "Normal-2", "Stun" }, 0),
                    new MenuSlider("UnitsEhit", "Normal Mod Minimum enemies for E", 1, 1, 4)
                };
                veigar.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q (stack) / if Mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if Mana >= x%", true, 60, 0, 99),
                    new MenuSlider("UnitsWhit", "W Hit x Units minions >= x%", 3, 1, 4)
                };
                veigar.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if Mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if Mana >= x%", true, 30, 0, 99)
                };
                veigar.Add(jungleclear);

                var antiGapcloser = new Menu("antiGapcloser", "Veigar anti-gapcloser spells")
                {
                    new MenuBool("e", "Anti-gapcloser E")
                };
                veigar.Add(antiGapcloser);
                Gapcloser.Attach(veigar, "Anti-gapcloser settings");

                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q", false));
                    drawings.Add(new MenuBool("w", "Draw W", false));
                    drawings.Add(new MenuBool("e", "Draw E", false));
                    drawings.Add(new MenuBool("r", "Draw R"));
                    var drawDamage = new Menu("drawDamage", "Use draw damage");
                    {
                        drawDamage.Add(new MenuBool("enabled", "Enabled"));
                        drawDamage.Add(new MenuBool("q", "Draw Q damage", false));
                        drawDamage.Add(new MenuBool("w", "Draw W damage", false));
                        drawDamage.Add(new MenuBool("e", "Draw E damage", false));
                        drawDamage.Add(new MenuBool("r", "Draw R damage"));

                    }
                    drawings.Add(drawDamage);
                }
                veigar.Add(drawings);
            }
            Main.Add(veigar);
            Main.Attach();
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Render.OnPresent += SpellDraw;
            Gapcloser.OnGapcloser += AntiGapcloser;
            Misc.Orbwalker.PreAttack += OnPreAttack;
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
        private static float QDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.Q);
        }
        private static float WDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.W);
        }
        private static float EDamage()
        {
            return 0;
        }
        private static float RDamage(Obj_AI_Base d)
        {
            if (!R.Ready) return 0;
            var damage = new float[] { 0, 175, 250, 325 }[Player.SpellBook.GetSpell(SpellSlot.R).Level] + (100 - d.HealthPercent()) * 1.5 / 100 * new float[] { 0, 175, 250, 325 }[Player.SpellBook.GetSpell(SpellSlot.R).Level] + 0.75 * Player.FlatMagicDamageMod;
            return (float)Player.CalculateDamage(d, DamageType.Magical, (float)damage);
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

            if (Main["harass"]["autoHarass"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Mixed)
            {
                Harass();
            }

            if (Main["combo"]["rKillSteal"].As<MenuBool>().Enabled && R.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if(target == null) return;
                if (target.IsValidTarget(R.Range) && RDamage(target) > target.Health)
                {
                    R.CastOnUnit(target);
                }
            }
        }
        public static void CastE(Obj_AI_Base target)
        {
            var pred = E.GetPrediction(target);
            var castVec = pred.UnitPosition.To2D() - Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
            if (pred.HitChance >= HitChance.VeryHigh && E.Ready)
            {
                E.Cast(castVec);
            }
        }

        public static void CastE(Obj_AI_Hero target)
        {
            var pred = E.GetPrediction(target);
            var castVec = pred.UnitPosition.To2D() - Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width;
            if (pred.HitChance >= HitChance.VeryHigh && E.Ready)
            {
                E.Cast(castVec);
            }
        }

        public static void CastE(Vector3 pos)
        {
            var castVec = pos.To2D() - Vector2.Normalize(pos.To2D() - Player.Position.To2D()) * E.Width;
            if (E.Ready)
            {
                E.Cast(castVec);
            }
        }

        public static void CastE(Vector2 pos)
        {
            var castVec = pos;
            if (E.Ready)
            {
                E.Cast(castVec);
            }
        }
        private static void Combo()
        {            
            if (E.Ready && Main["combo"]["e"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(1250);
                if(target == null) return;
                if (target.IsValidTarget(1250) && !Check(target, DamageType.Magical))
                {
                    switch (Main["combo"]["e2"].As<MenuList>().Value)
                    {
                        case 0:                           
                                if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(E2.Width, false, true,
                                        E2.GetPrediction(target).CastPosition)) >= Main["combo"]["UnitsEhit"].As<MenuSlider>().Value && target.IsValidTarget(E.Range))
                                {
                                    E2.Cast(target.Position);
                                }                           
                            break;
                        case 1:
                            if (Player.Distance(target.Position) <= 1050f + 200)
                            {
                                var pred = E.GetPrediction(target);
                                if (pred.HitChance == HitChance.VeryHigh)
                                {
                                    CastE2(target);
                                }
                            }
                            break;
                        case 2:
                            if (Player.Distance(target.Position) <= 1050f + 200)
                            {
                                var pred = E.GetPrediction(target);
                                if (pred.HitChance == HitChance.VeryHigh)
                                {
                                    CastE(target);
                                }
                            }
                            break;
                    }
                }
            }

            if (Q.Ready && Main["combo"]["q"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target == null) return;
                if (target.IsValidTarget(Q.Range) && !Check(target, DamageType.Magical, false))
                {                  
                        var pred = Q.GetPrediction(target);
                        if (pred.HitChance == HitChance.VeryHigh && pred.CollisionObjects.Count == 0 || pred.HitChance == HitChance.Medium && target.IsImmobile())
                        {
                            Q.Cast(pred.CastPosition);
                        }                 
                }
            }

            if (W.Ready && Main["combo"]["w"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target == null) return;
                if (target.IsValidTarget(W.Range) && !Check(target, DamageType.Magical, false))
                {
                    switch (Main["combo"]["w2"].As<MenuList>().Value)
                    {
                        case 0:
                            if (Player.Distance(target.Position) <= W.Range - 80)
                            {
                                var pred = W.GetPrediction(target);
                                if (pred.HitChance == HitChance.VeryHigh || pred.HitChance == HitChance.Medium && target.IsImmobile())
                                {
                                    W.Cast(pred.CastPosition);
                                }
                            }
                            break;
                        case 1:
                            if (Player.Distance(target.Position) <= W.Range)
                            {
                                if (target.IsImmobile())
                                {
                                    W.Cast(target);
                                }
                            }
                            break;
                    }                  
                }
            }

            if (R.Ready && Main["combo"]["r"].As<MenuSliderBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target == null) return;
                if (target.IsValidTarget(R.Range) && !Check(target, DamageType.Magical))
                {
                    if (target.HealthPercent() < Main["combo"]["r"].As<MenuSliderBool>().Value)
                    {
                        R.CastOnUnit(target);
                    }
                }
            }
        }

        private static void Harass()
        {
            if (E.Ready && Main["harass"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["e"].As<MenuSliderBool>().Value)
            {
                var target = GetBestEnemyHeroTargetInRange(1250);
                if (target == null) return;
                if (target.IsValidTarget(1250) && !Check(target, DamageType.Magical))
                {
                    switch (Main["harass"]["e2"].As<MenuList>().Value)
                    {
                        case 0:
                            if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(E2.Width, false, true,
                                    E2.GetPrediction(target).CastPosition)) >= Main["harass"]["UnitsEhit"].As<MenuSlider>().Value && target.IsValidTarget(E.Range))
                            {
                                E2.Cast(target.Position);
                            }
                            break;
                        case 1:
                            if (Player.Distance(target.Position) <= 1050f + 200)
                            {
                                var pred = E.GetPrediction(target);
                                if (pred.HitChance == HitChance.VeryHigh)
                                {
                                    CastE2(target);
                                }
                            }
                            break;
                        case 2:
                            if (Player.Distance(target.Position) <= 1050f + 200)
                            {
                                var pred = E.GetPrediction(target);
                                if (pred.HitChance == HitChance.VeryHigh)
                                {
                                    CastE(target);
                                }
                            }
                            break;
                    }
                }
            }

            if (Q.Ready && Main["harass"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target == null) return;
                if (target.IsValidTarget(Q.Range) && !Check(target, DamageType.Magical, false))
                {
                    var pred = Q.GetPrediction(target);
                    if (pred.HitChance == HitChance.VeryHigh && pred.CollisionObjects.Count == 0 || pred.HitChance == HitChance.Medium && target.IsImmobile())
                    {
                        Q.Cast(pred.CastPosition);
                    }
                }
            }

            if (W.Ready && Main["harass"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["w"].As<MenuSliderBool>().Value)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target == null) return;
                if (target.IsValidTarget(W.Range) && !Check(target, DamageType.Magical, false))
                {
                    switch (Main["harass"]["w2"].As<MenuList>().Value)
                    {
                        case 0:
                            if (Player.Distance(target.Position) <= W.Range - 80)
                            {
                                var pred = W.GetPrediction(target);
                                if (pred.HitChance == HitChance.VeryHigh || pred.HitChance == HitChance.Medium && target.IsImmobile())
                                {
                                    W.Cast(pred.CastPosition);
                                }
                            }
                            break;
                        case 1:
                            if (Player.Distance(target.Position) <= W.Range)
                            {
                                if (target.IsImmobile())
                                {
                                    W.Cast(target);
                                }
                            }
                            break;
                    }
                }
            }
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Q.Ready && Player.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value)
            {                 
                    foreach (var minion in GameObjects.EnemyMinions.Where(m => Player.Distance(m.Position) <= Q.Range && m.Health < Player.GetSpellDamage(m, SpellSlot.Q)))
                    {
                        if (!Player.SpellBook.IsAutoAttacking)
                        {
                          Q.Cast(minion);
                        }            
                    }              
            }

            if (Main["laneclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["laneclear"]["w"].As<MenuSliderBool>().Value && W.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(W.Range)))
                {
                    if (target == null) return;
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(W.Width - 80, false, false, W.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsWhit"].As<MenuSlider>().Value)
                    {
                        W.Cast(W.GetPrediction(target).CastPosition);
                    }
                }
                
            }
        }

        private static void JungleClear()
        {
            foreach (var targetJ in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready)
                {
                    CastE(targetJ.Position);
                }
                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready)
                {
                    W.Cast(targetJ.Position);
                }
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready && (targetJ.HasBuffOfType(BuffType.Stun) || !E.Ready))
                {
                    Q.Cast(targetJ.Position);
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
                        if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && Game.TickCount > 2500)
                        {
                            E.Cast(target.Position.Extend(Player.ServerPosition, Vector3.Distance(target.Position, Player.Position) - 450));
                            }
                    }
                }
                    break;
                case SpellType.Targeted:
                {
                    if (target.IsValidTarget(400))
                    {
                        if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && Game.TickCount > 2500)
                        {
                            E.Cast(target.Position.Extend(Player.ServerPosition, Vector3.Distance(target.Position, Player.Position) - 450));
                            }
                    }
                }
                    break;
            }
        }

        public static void OnPreAttack(object sender, PreAttackEventArgs args)
        {
            switch (Misc.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    if (Main["combo"]["disableAA"].As<MenuBool>().Enabled)
                    {
                        args.Cancel = true;
                    }
                    break;
            }
        }

        public static void CastE2(Obj_AI_Base target)
        {
            var pred = E.GetPrediction(target);
            var castVec = pred.UnitPosition.To2D() - Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width + 60;
            if (pred.HitChance >= HitChance.VeryHigh && E.Ready)
            {
                E.Cast(castVec);
            }
        }

        public static void CastE2(Obj_AI_Hero target)
        {
            var pred = E.GetPrediction(target);
            var castVec = pred.UnitPosition.To2D() - Vector2.Normalize(pred.UnitPosition.To2D() - Player.Position.To2D()) * E.Width + 60;
            if (pred.HitChance >= HitChance.VeryHigh && E.Ready)
            {
                E.Cast(castVec);
            }
        }

        public static void CastE2(Vector3 pos)
        {
            var castVec = pos.To2D() - Vector2.Normalize(pos.To2D() - Player.Position.To2D()) * E.Width + 60;
            if (E.Ready)
            {
                E.Cast(castVec);
            }
        }

        public static void CastE2(Vector2 pos)
        {
            var castVec = pos;
            if (E.Ready)
            {
                E.Cast(castVec);
            }
        }

        private static void DamageDraw()
        {
            if (!Main["drawDamage"]["enabled"].Enabled) return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.IsVisible && Player.Distance(x) < 1700))
            {
                float qdmgDraw = 0, wdmgDraw = 0, edmgDraw = 0, rdmgDraw = 0;
                if (Q.Ready && Main["drawDamage"]["q"].As<MenuBool>().Enabled)
                {
                    qdmgDraw = QDamage(enemy);
                }
                if (W.Ready && Main["drawDamage"]["w"].As<MenuBool>().Enabled)
                {
                    wdmgDraw = WDamage(enemy);
                }
                if (E.Ready && Main["drawDamage"]["e"].As<MenuBool>().Enabled)
                {
                    edmgDraw = EDamage();
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
