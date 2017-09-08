using System;
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
    internal class Ziggs
    {
        public Ziggs()
        {
            Q = new Spell(SpellSlot.Q, 850f);
            Q2 = new Spell(SpellSlot.Q, 1125f);
            Q3 = new Spell(SpellSlot.Q, 1400f);
            W = new Spell(SpellSlot.W, 970);
            E = new Spell(SpellSlot.E, 900f);
            R = new Spell(SpellSlot.R, 5300f);

            Q.SetSkillshot(0.3f, 130f, 1700f, false, SkillshotType.Circle);
            Q2.SetSkillshot(0.25f + Q.Delay, 130f, 1700f, false, SkillshotType.Circle);
            Q3.SetSkillshot(0.3f + Q2.Delay, 130f, 1700f, false, SkillshotType.Circle);
            W.SetSkillshot(0.250f, 275, 1800, false, SkillshotType.Circle);
            E.SetSkillshot(1.000f, 180f, 2700, false, SkillshotType.Circle);
            R.SetSkillshot(0.7f, 340f, 1500f, false, SkillshotType.Circle);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var ziggs = new Menu("ziggs", "Ziggs");
            {
                var combo = new Menu("combo", "Combo")
                {
                    new MenuBool("q", "Use combo Q"),
                    new MenuBool("w", "Use combo W"),
                    new MenuBool("turW", "Auto W destroy turrets"),
                    new MenuBool("defW", "Auto W defensively"),
                    new MenuKeyBind("fleeW", "W to mouse key (Flee)", KeyCode.S, KeybindType.Press),
                    new MenuBool("e", "Use combo E"),
                    new MenuBool("eCC", "Auto E on Stun"),
                    new MenuBool("eT", "Auto E teleport"),
                    new MenuSliderBool("r", "Use combo R / Enemies health", true, 25, 1, 99),
                    new MenuSlider("rHit", "R enemy Hit Count", 1, 1, 5),
                    new MenuBool("rCC", "Use combo R on CC"),
                    new MenuKeyBind("rKey", "Semi-manual cast R key (2500 Range)", KeyCode.T, KeybindType.Press),
                    new MenuBool("rKS", "Auto R kill-steal"),
                    new MenuSlider("rMin", "R minimum range", 900, 0, 5000),
                    new MenuSlider("rMax", "R maximum range", 3000, 0, 5000),

                };
                ziggs.Add(combo);

                var harass = new Menu("harass", "Harass")
                {
                    new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                    new MenuSliderBool("q", "Harass use Q / if mana >= x%", true, 50, 0, 99),
                    new MenuSliderBool("e", "Harass use E / if mana >= x%", false, 50, 0, 99)
                };
                ziggs.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 60, 0, 99),
                    new MenuSlider("UnitsQhit", "Q Hit x units minions >= x%", 2, 1, 3),
                    new MenuSliderBool("e", "Use E / if mana >= x%", false, 60, 0, 99),
                    new MenuSlider("UnitsEhit", "E Hit x units minions >= x%", 3, 1, 4)
                };
                ziggs.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if mana >= x%", false, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", true, 30, 0, 99)
                };
                ziggs.Add(jungleclear);

                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q"));
                    drawings.Add(new MenuBool("w", "Draw W", false));
                    drawings.Add(new MenuBool("e", "Draw E", false));
                    drawings.Add(new MenuBool("r", "Draw R", false));
                    drawings.Add(new MenuBool("rMini", "Draw R minimap"));
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
                ziggs.Add(drawings);
            }
            Main.Add(ziggs);
            Main.Attach();
            Render.OnPresent += SpellDraw;
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
        }
        public static int LastWToMouseT;
        public static int UseSecondWt;
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
            if (Main["drawings"]["rMini"].As<MenuBool>().Enabled && R.Ready)
            {
                Vector2 centre;
                Render.WorldToMinimap(ObjectManager.GetLocalPlayer().Position, out centre);
                var rangePosition = ObjectManager.GetLocalPlayer().Position;
                rangePosition.X += R.Range;
                Vector2 end;
                Render.WorldToMinimap(rangePosition, out end);
                var radius = Math.Abs(end.X - centre.X);
                DrawCircle(centre, radius, Color.Aqua);
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
            if (W.Ready && Main["combo"]["defW"].As<MenuBool>().Enabled)
            {
                foreach (var pos in from enemy in ObjectManager.Get<Obj_AI_Hero>()
                    where
                    enemy.IsValidTarget() &&
                    enemy.Distance(Player) <=
                    enemy.BoundingRadius + enemy.AttackRange + Player.BoundingRadius &&
                    enemy.IsMelee
                    let direction =
                    (enemy.ServerPosition.To2D() - Player.ServerPosition.To2D()).Normalized()
                    let pos = Player.ServerPosition.To2D()
                    select pos + Math.Min(200, Math.Max(50, enemy.Distance(Player) / 2)) * direction)
                {
                    W.Cast(pos.To3D());
                    UseSecondWt = Game.TickCount;
                }
            }

            if (W.Ready && Main["combo"]["turW"].As<MenuBool>().Enabled)
            {
                var turret = ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(x => x.IsEnemy && x.IsValidTarget(W.Range) && x.HealthPercent() < 22.5 + Player.SpellBook.GetSpell(SpellSlot.W).Level * 2.5);
                if (turret != null)
                {
                    W.Cast(turret);
                }
            }

            if (W.Ready)
            {
                if (Main["combo"]["fleeW"].As<MenuKeyBind>().Enabled || Game.TickCount - LastWToMouseT < 400)
                {
                    var pos = Player.ServerPosition.To2D().Extend(Game.CursorPos.To2D(), -150).To3D();
                    W.Cast(pos);
                    if (Main["combo"]["fleeW"].As<MenuKeyBind>().Enabled)
                    {
                        LastWToMouseT = Game.TickCount;
                    }
                }
            }
            if (E.Ready && Main["combo"]["eCC"].As<MenuBool>().Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.HasBuffOfType(BuffType.Stun) && x.Distance(Player) < E.Range))
                {
                    E.Cast(target);
                }
            }
            if (E.Ready && Main["combo"]["eT"].As<MenuBool>().Enabled)
            {
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsEnemy && x.Distance(Player) <= W.Range && x.ValidActiveBuffs().Any(y => y.Name.Equals("teleport_target"))))
                {
                    E.Cast(minion.ServerPosition);
                }
            }
            if (R.Ready && Main["combo"]["rKS"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Main["combo"]["rMax"].As<MenuSlider>().Value);
                if (target != null && target.IsValidTarget())
                {
                    if (target.Health < RDamage(target) && target.CountAllyHeroesInRange(500) == 0 && Player.Distance(target) > Main["combo"]["rMin"].As<MenuSlider>().Value)
                    {
                        R.Cast(target);
                    }
                }
            }
            if (R.Ready && Main["combo"]["rKey"].As<MenuKeyBind>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(2500);
                if (target != null && target.IsValidTarget())
                {
                    R.Cast(target, true);
                }
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
        private static void Combo()
        {
            if (W.Ready && Main["combo"]["w"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(W.Range);
                if (target != null && target.IsValidTarget())
                {
                    var prediction = W.GetPrediction(target);
                    var pos = V3E(Player.Position, target.Position, Vector3.Distance(Player.Position, prediction.CastPosition) - 10);

                    W.Cast(pos);
                }
            }
            
            if (Q.Ready && Main["combo"]["q"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Q3.Range);
                if (target != null && target.IsValidTarget())
                {
                    if (Player.Distance(target) > 700)
                    {
                        if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.E).Cost + Player.SpellBook.GetSpell(SpellSlot.R).Cost)
                            CastQ(target);
                        else if (QDamage(target) > target.Health && !Check(target, DamageType.Magical, false))
                            CastQ(target);
                        else if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.R).Cost)
                        {
                            foreach (var unused in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q3.Range) && x.IsImmobile()))
                            {
                                CastQ(target);
                            }
                        }
                    }else if (Player.Distance(target) < 700)
                    {
                        var pred = Q.GetPrediction(target);
                        if (pred.HitChance >= HitChance.High)
                        {
                            if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost +
                                Player.SpellBook.GetSpell(SpellSlot.E).Cost +
                                Player.SpellBook.GetSpell(SpellSlot.R).Cost)
                                Q.Cast(pred.UnitPosition);
                            else if (QDamage(target) > target.Health && !Check(target, DamageType.Magical, false))
                                Q.Cast(pred.UnitPosition);
                            else if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.R).Cost)
                            {
                                foreach (var unused in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q3.Range) && x.IsImmobile()))
                                {
                                    Q.Cast(pred.UnitPosition);
                                }
                            }
                        }
                        
                    }                   
                }
            }

            if (E.Ready && Main["combo"]["e"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target != null && target.IsValidTarget())
                {
                    var prediction = E.GetPrediction(target);
                    var pos = V3E(Player.Position, prediction.CastPosition,
                        Vector3.Distance(Player.Position, prediction.CastPosition) + 30);
                    if (prediction.HitChance >= HitChance.Medium)
                    {
                        E.Cast(pos);
                    }
                }
            }
            if (R.Ready && Main["combo"]["r"].As<MenuSliderBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Main["combo"]["rMax"].As<MenuSlider>().Value);
                if (target != null && target.IsValidTarget())
                {
                    if (target.HealthPercent() < Main["combo"]["r"].As<MenuSliderBool>().Value && Player.Distance(target) > Main["combo"]["rMin"].As<MenuSlider>().Value)
                    {
                        if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(R.Width, false, true, R.GetPrediction(target).CastPosition)) >= Main["combo"]["rHit"].As<MenuSlider>().Value)
                        {
                            R.Cast(target, true);
                        }
                        if (Main["combo"]["rCC"].As<MenuBool>().Enabled && target.IsImmobile())
                        {
                            R.Cast(target, true);
                        }
                    }
                }
            }
        }

        private static void Harass()
        {
            if (Q.Ready && Main["harass"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value)
            {
                var target = GetBestEnemyHeroTargetInRange(Q3.Range);
                if (target != null && target.IsValidTarget())
                {
                    if (Player.Distance(target) > 700)
                    {
                        if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.E).Cost + Player.SpellBook.GetSpell(SpellSlot.R).Cost)
                            CastQ(target);
                        else if (QDamage(target) > target.Health && !Check(target, DamageType.Magical, false))
                            CastQ(target);
                        else if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.R).Cost)
                        {
                            foreach (var unused in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q3.Range) && x.IsImmobile()))
                            {
                                CastQ(target);
                            }
                        }
                    }
                    else if (Player.Distance(target) < 700)
                    {
                        var pred = Q.GetPrediction(target);
                        if (pred.HitChance >= HitChance.High)
                        {
                            if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost +
                                Player.SpellBook.GetSpell(SpellSlot.E).Cost +
                                Player.SpellBook.GetSpell(SpellSlot.R).Cost)
                                Q.Cast(pred.UnitPosition);
                            else if (QDamage(target) > target.Health && !Check(target, DamageType.Magical, false))
                                Q.Cast(pred.UnitPosition);
                            else if (Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.R).Cost)
                            {
                                foreach (var unused in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(Q3.Range) && x.IsImmobile()))
                                {
                                    Q.Cast(pred.UnitPosition);
                                }
                            }
                        }

                    }
                }
            }

            if (E.Ready && Main["harass"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["e"].As<MenuSliderBool>().Value)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target != null && target.IsValidTarget())
                {
                    var prediction = E.GetPrediction(target);
                    var pos = V3E(Player.Position, prediction.CastPosition,
                        Vector3.Distance(Player.Position, prediction.CastPosition) + 30);
                    if (prediction.HitChance >= HitChance.Medium)
                    {
                        E.Cast(pos);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)))
            {
                if (target != null)
                {
                    if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Q.Ready &&
                        Player.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value)
                    {
                        if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(220, false, false, E.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsQhit"].As<MenuSlider>().Value)
                        {
                            Q.Cast(Q.GetPrediction(target).CastPosition);
                        }
                    }
                    if (Main["laneclear"]["e"].As<MenuSliderBool>().Enabled && Q.Ready &&
                        Player.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value)
                    {
                        if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(220, false, false, E.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsEhit"].As<MenuSlider>().Value)
                        {
                            E.Cast(E.GetPrediction(target).CastPosition);
                        }
                    }
                }
            }
            
        }
        private static void JungleClear()
        {
            foreach (var target in GameObjects.Jungle.Where( x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready)
                {
                    Q.Cast(target.ServerPosition);
                }

                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready)
                {
                    W.Cast(target.ServerPosition);
                }
                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready)
                {
                    E.Cast(target.ServerPosition);
                }
            }
        }

        private static Vector3 V3E(Vector3 from, Vector3 direction, float distance)
        {
            return (from.To2D() + distance * Vector2.Normalize(direction.To2D() - from.To2D())).To3D();
        }

        private static void CastQ(Obj_AI_Base target)
        {
            PredictionOutput prediction;
            if (Player.Distance(target) < Q.Range)
            {
                var oldrange = Q.Range;
                Q.Range = Q2.Range;
                prediction = Q.GetPrediction(target);
                Q.Range = oldrange;
            }
            else if (Player.Distance(target) < Q2.Range)
            {
                var oldrange = Q2.Range;
                Q2.Range = Q3.Range;
                prediction = Q2.GetPrediction(target);
                Q2.Range = oldrange;
            }
            else if (Player.Distance(target) < Q3.Range)
            {
                prediction = Q3.GetPrediction(target);
            }
            else
            {
                return;
            }

            if (prediction.HitChance >= HitChance.High)
            {
                if (Player.ServerPosition.Distance(prediction.CastPosition) <= Q.Range + Q.Width)
                {
                    Vector3 p;
                    if (Player.ServerPosition.Distance(prediction.CastPosition) > 300)
                    {
                        p = prediction.CastPosition - 100 * (prediction.CastPosition.To2D() - Player.ServerPosition.To2D()).Normalized()
                            .To3D();
                    }
                    else
                    {
                        p = prediction.CastPosition;
                    }
                    Q.Cast(p);
                }
                else if (Player.ServerPosition.Distance(prediction.CastPosition) <= (Q.Range + Q2.Range) / 2)
                {
                    var p = Player.ServerPosition.To2D().Extend(prediction.CastPosition.To2D(), Q.Range - 100);
                    if (!QCollision(target, prediction.UnitPosition, p.To3D()))
                    {
                        Q.Cast(p.To3D());
                    }
                }
                else
                {
                    var p = Player.ServerPosition.To2D() + Q.Range * (prediction.CastPosition.To2D() - Player.ServerPosition.To2D()).Normalized();
                    if (!QCollision(target, prediction.UnitPosition, p.To3D()))
                    {
                        Q.Cast(p.To3D());
                    }
                }
            }
        }
        private static bool QCollision(GameObject target, Vector3 targetPosition, Vector3 castPosition)
        {
            var direction = (castPosition.To2D() - Player.ServerPosition.To2D()).Normalized();
            var firstBouncePosition = castPosition.To2D();
            var secondBouncePosition = firstBouncePosition + direction * 0.4f * Player.ServerPosition.To2D().Distance(firstBouncePosition);
            var thirdBouncePosition = secondBouncePosition + direction * 0.6f * firstBouncePosition.Distance(secondBouncePosition);


            if (thirdBouncePosition.Distance(targetPosition.To2D()) < Q.Width + target.BoundingRadius)
            {
                if ((from minion in ObjectManager.Get<Obj_AI_Minion>()
                    where minion.IsValidTarget(3000)
                    let predictedPos = Q2.GetPrediction(minion)
                    where predictedPos.UnitPosition.To2D().Distance(secondBouncePosition) <
                          Q2.Width + minion.BoundingRadius
                    select minion).Any())
                {
                    return true;
                }
            }

            if (secondBouncePosition.Distance(targetPosition.To2D()) < Q.Width + target.BoundingRadius ||
                thirdBouncePosition.Distance(targetPosition.To2D()) < Q.Width + target.BoundingRadius)
            {
                return (from minion in ObjectManager.Get<Obj_AI_Minion>() where minion.IsValidTarget(3000) let predictedPos = Q.GetPrediction(minion) where predictedPos.UnitPosition.To2D().Distance(firstBouncePosition) < Q.Width + minion.BoundingRadius select minion).Any();
            }
            return true;
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