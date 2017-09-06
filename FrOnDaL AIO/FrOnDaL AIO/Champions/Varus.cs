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
    internal class Varus
    {
        internal static bool IsPreAa;
        internal static bool IsAfterAa;
        public Varus()
        {
            Q = new Spell(SpellSlot.Q, 1000);
            E = new Spell(SpellSlot.E, 925);
            R = new Spell(SpellSlot.R, 1100f);

            Q.SetSkillshot(0.25f, 70, 1900, false, SkillshotType.Line);
            Q.SetCharged("VarusQ", "VarusQ", 1000, 1600, 1.3f);
            E.SetSkillshot(250, 235, 1500f, false, SkillshotType.Circle);
            R.SetSkillshot(0.25f, 120f, 1950f, false, SkillshotType.Line);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var varus = new Menu("varus", "Varus");
            {
                var combo = new Menu("combo", "Combo");
                {
                    combo.Add(new MenuBool("q", "Use combo Q"));
                    combo.Add(new MenuSliderBool("qstcW", "Minimum W stack for Q", false, 2, 1, 3));
                    var whiteListQ = new Menu("whiteListQ", "Q white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteListQ.Add(new MenuBool("qWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                        }
                    }
                    combo.Add(whiteListQ);
                    combo.Add(new MenuBool("e", "Use combo E"));
                    combo.Add(new MenuSlider("UnitsEhit", "E hit x units enemy", 1, 1, 3));
                    combo.Add(new MenuSliderBool("eStcW", "Minimum W stack for E", false, 1, 1, 3));
                    combo.Add(new MenuKeyBind("keyR", "Semi-manual cast R key", KeyCode.T, KeybindType.Press));
                    combo.Add(new MenuSlider("rHit", "Minimum enemies for R", 1, 1, 5));
                    combo.Add(new MenuSliderBool("autoR", "Auto R minimum enemies for", false, 3, 1, 5));
                    var whiteListR = new Menu("whiteListR", "R white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteListR.Add(new MenuBool("rWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                        }
                    }
                    combo.Add(whiteListR);
                }
                varus.Add(combo);

                var harass = new Menu("harass", "Harass");
                {
                    harass.Add(new MenuKeyBind("keyHarass", "Harass key:", KeyCode.C, KeybindType.Press));
                    harass.Add(new MenuBool("autoHarass", "Auto harass", false));                   
                    harass.Add(new MenuSliderBool("q", "Use Q / if mana >= x%", true, 30, 0, 99));
                    var whiteListQ = new Menu("whiteListQ", "Q white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteListQ.Add(new MenuBool("qWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                        }
                    }
                    harass.Add(whiteListQ);
                    harass.Add(new MenuSliderBool("e", "Use E / if mana >= x%", false, 30, 0, 99));
                }
                varus.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 60, 0, 99),
                    new MenuSlider("UnitsQhit", "Q Hit x units minions >= x%", 3, 1, 6),
                    new MenuSliderBool("e", "Use E / if mana >= x%", false, 60, 0, 99),
                    new MenuSlider("UnitsEhit", "E Hit x units minions >= x%", 3, 1, 4)
                };
                varus.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("jungW", "Minimum W stack for Q", false, 2, 1, 3),
                    new MenuSliderBool("e", "Use E / if mana >= x%", true, 30, 0, 99)
                };
                varus.Add(jungleclear);

                //var antiGapcloser = new Menu("antiGapcloser", "Varus anti-gapcloser spells")
                //{
                //    new MenuBool("e", "Anti-gapcloser E"),
                //    new MenuBool("r", "Anti-gapcloser R")
                //};
                //varus.Add(antiGapcloser);
                //Gapcloser.Attach(varus, "Anti-gapcloser settings");
                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q"));
                    drawings.Add(new MenuBool("q2", "Draw Q Charged Max Range"));
                    drawings.Add(new MenuBool("e", "Draw E", false));
                    drawings.Add(new MenuBool("r", "Draw R", false));
                    var drawDamage = new Menu("drawDamage", "Use draw damage");
                    {
                        drawDamage.Add(new MenuBool("enabled", "Enabled"));
                        drawDamage.Add(new MenuBool("q", "Draw Q damage"));
                        drawDamage.Add(new MenuBool("w", "Draw W damage"));
                        drawDamage.Add(new MenuBool("e", "Draw E damage", false));
                        drawDamage.Add(new MenuBool("r", "Draw R damage", false));

                    }
                    drawings.Add(drawDamage);
                }
                varus.Add(drawings);
            }
            Main.Add(varus);
            Main.Attach();
            Game.OnUpdate += Game_OnUpdate;
            Misc.Orbwalker.PreAttack += OnPreAttack;
            //Gapcloser.OnGapcloser += AntiGapcloser;
            Render.OnPresent += DamageDraw;
            Render.OnPresent += SpellDraw;
            Misc.Orbwalker.PreAttack += (a, b) => IsPreAa = true;
            Misc.Orbwalker.PostAttack += (a, b) => { IsPreAa = false; IsAfterAa = true; };
        }

        private static int GetBuffCount(Obj_AI_Base target) => target.GetBuffCount("VarusWDebuff");
        private static float QDamage(Obj_AI_Base d)
        {
            if (!Q.Ready) return 0;
            var damageQ = Player.CalculateDamage(d, DamageType.Physical, (float)new double[] { 12, 58, 104, 150, 196 }[Player.SpellBook.GetSpell(SpellSlot.Q).Level - 1] + Player.TotalAttackDamage / 100 * 132);
            return (float)damageQ;
        }

        private static float EDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.E);
        }

        private static float RDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.R);
        }

        private static void SpellDraw()
        {
            if (Main["drawings"]["q"].As<MenuBool>().Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Q.ChargedMinRange, 180, Color.Green);
            }
            if (Main["drawings"]["q2"].As<MenuBool>().Enabled && Q.Ready)
            {
                Render.Circle(Player.Position, Q.ChargedMaxRange - 100, 180, Color.Green);
            }
            if (Main["drawings"]["e"].As<MenuBool>().Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 180, Color.Green);
            }
            if (Main["drawings"]["r"].As<MenuBool>().Enabled && R.Ready)
            {
                Render.Circle(Player.Position, W.Range, 180, Color.Green);
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

            if (Main["combo"]["keyR"].As<MenuKeyBind>().Enabled)
            {
                ManualR();
            }
            if (Main["harass"]["autoHarass"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Laneclear && Misc.Orbwalker.Mode != OrbwalkingMode.Combo && Misc.Orbwalker.Mode != OrbwalkingMode.Mixed)
            {
                Harass();
            }
            if (Main["combo"]["autoR"].As<MenuSliderBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range - 100);
                if (target == null) return;
                var rHit = GameObjects.EnemyHeroes.Where(x => x.Distance(target) <= 450f).ToList();
                if (Main["combo"]["whiteListR"]["rWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && R.Ready &&
                    target.IsValidTarget(R.Range - 100) && rHit.Count >= Main["combo"]["autoR"].As<MenuSliderBool>().Value)
                {
                    var pred = R.GetPrediction(target);
                    if (pred.HitChance >= HitChance.Medium)
                    {
                        R.Cast(pred.UnitPosition);
                    }
                }
            }
        }

        private static void Combo()
        {
            var target = GetBestEnemyHeroTargetInRange(Q.ChargedMaxRange - 100);
            if (target == null) return;
            if (Main["combo"]["q"].As<MenuBool>().Enabled && Main["combo"]["whiteListQ"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && Q.Ready && !Check(target, DamageType.Magical))
            {
                if ((Main["combo"]["qstcW"].As<MenuSliderBool>().Enabled && Player.Distance(target) < 750 && GetBuffCount(target) >= Main["combo"]["qstcW"].As<MenuSliderBool>().Value) || !Main["combo"]["qstcW"].As<MenuSliderBool>().Enabled || Q.ChargePercent >= 100 || target.Health <= QDamage(target) || Player.Distance(target) > 800)
                {
                    if (!Q.IsCharging && !IsPreAa)
                    {
                        Q.StartCharging(Q.GetPrediction(target).CastPosition); return;
                    }
                    if (!Q.IsCharging) return;
                    if (Player.Distance(target) < 750 && Q.ChargePercent >= 30 || Player.Distance(target) > 750 && Q.ChargePercent >= 100)
                    {
                        var prediction = Q.GetPrediction(target);

                        if (prediction.HitChance >= HitChance.Medium)
                        {
                            Q.Cast(Q.GetPrediction(target).CastPosition);
                        }
                    }
                }
            }

            if (!Main["combo"]["e"].As<MenuBool>().Enabled || !target.IsValidTarget(E.Range) || !E.Ready || Q.IsCharging) return;

            if ((!Main["combo"]["eStcW"].As<MenuSliderBool>().Enabled || !(Player.Distance(target) < 700) ||
                 GetBuffCount(target) < Main["combo"]["eStcW"].As<MenuSliderBool>().Value) && Main["combo"]["eStcW"].As<MenuSliderBool>().Enabled && !(Player.Distance(target) > 750))
                return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(E.Range)))
            {
                if (enemy == null) return;
                if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(E.Width, false, true, E.GetPrediction(enemy).CastPosition)) >= Main["combo"]["UnitsEhit"].As<MenuSlider>().Value)
                {
                    E.Cast(target.Position);
                }
            }
        }

        private static void ManualR()
        {
            var target = GetBestEnemyHeroTargetInRange(R.Range - 100);
            if (target == null) return;
            var rHit = GameObjects.EnemyHeroes.Where(x => x.Distance(target) <= 450f).ToList();
            if (Main["combo"]["whiteListR"]["rWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && R.Ready && target.IsValidTarget(R.Range - 100) && rHit.Count >= Main["combo"]["rHit"].As<MenuSlider>().Value && !Q.IsCharging)
            {
                var pred = R.GetPrediction(target);
                if (pred.HitChance >= HitChance.Medium)
                {
                    R.Cast(pred.UnitPosition);
                }
            }
        }

        private static void Harass()
        {
            var target = GetBestEnemyHeroTargetInRange(Q.ChargedMaxRange - 100);
            if (target == null) return;
            if (Main["harass"]["q"].As<MenuSliderBool>().Enabled && Main["harass"]["whiteListQ"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && Q.Ready && !Player.IsUnderEnemyTurret() && !Check(target, DamageType.Magical))
            {
                if (!Q.IsCharging && !IsPreAa && Player.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value)
                {
                    Q.StartCharging(Q.GetPrediction(target).CastPosition); return;
                }
                if (!Q.IsCharging) return;
                if (Player.Distance(target) < 750 && Q.ChargePercent >= 30 || Player.Distance(target) > 750 && Q.ChargePercent >= 100)
                {
                    Q.Cast(Q.GetPrediction(target).CastPosition);
                }
            }

            if (!Main["harass"]["e"].As<MenuSliderBool>().Enabled || !(Player.ManaPercent() > Main["harass"]["e"].As<MenuSliderBool>().Value) || Player.IsUnderEnemyTurret() ||
                !target.IsValidTarget(E.Range) || !E.Ready)
                return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(E.Range)))
            {
                if (enemy == null) return;
                if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(E.Width, false, false, E.GetPrediction(enemy).CastPosition)) >= 1)
                {
                    E.Cast(enemy.Position);
                }
            }
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Q.Ready)
            {
                foreach (var targetL in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.ChargedMaxRange)))
                {

                    var range = Q.IsCharging ? Q.Range : Q.ChargedMaxRange;
                    var result = Polygon.GetLinearLocation(range, Q.Width);
                    if (result == null) return;
                    if (Player.ManaPercent() >= Main["laneclear"]["q"].As<MenuSliderBool>().Value &&
                        result.NumberOfMinionsHit >= Main["laneclear"]["UnitsQhit"].As<MenuSlider>().Value &&
                        !Q.IsCharging && !IsPreAa)
                    {
                        Q.StartCharging(result.CastPosition);
                        return;
                    }
                    if (!Q.IsCharging) return;
                    if (Player.Distance(targetL) > 600 && Q.ChargePercent >= 90)
                    {
                        Q.Cast(result.CastPosition);
                    }
                    else if (Player.Distance(targetL) < 600 && Q.ChargePercent >= 65)
                    {
                        Q.Cast(result.CastPosition);
                    }
                }
            }

            if (!Main["laneclear"]["e"].As<MenuSliderBool>().Enabled || !(Player.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value) || !E.Ready) return;
            {
                foreach (var targetE in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range)))
                {
                    if (targetE == null) return;
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(E.Width, false, false, E.GetPrediction(targetE).CastPosition)) >= Main["laneclear"]["UnitsEhit"].As<MenuSlider>().Value)
                    {
                        E.Cast(E.GetPrediction(targetE).CastPosition);
                    }
                }
            }
        }
        private static void JungleClear()
        {
            foreach (var targetJ in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(Q.ChargedMaxRange)))
            {
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && targetJ.IsValidTarget(1000) && Q.Ready)
                {
                    if (Main["jungleclear"]["jungW"].As<MenuSliderBool>().Enabled && GetBuffCount(targetJ) >=
                        Main["jungleclear"]["jungW"].As<MenuSliderBool>().Value || !Main["jungleclear"]["jungW"].As<MenuSliderBool>().Enabled || Q.ChargePercent >= 100)
                    {

                        if (!Q.IsCharging && Player.ManaPercent() >= Main["jungleclear"]["q"].As<MenuSliderBool>().Value)
                        {
                            if (!IsPreAa)
                                Q.StartCharging(Q.GetPrediction(targetJ).CastPosition);
                        }
                        else if (Q.IsCharging && Q.ChargePercent >= 100)
                        {
                            Q.Cast(targetJ.Position);
                        }
                        else if (Player.Distance(targetJ) < 700 && Q.ChargePercent >= 30)
                        {
                            Q.Cast(targetJ.Position);
                        }
                    }
                }

                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && targetJ.IsValidTarget(E.Range) && E.Ready)
                {
                    E.Cast(targetJ.Position);
                }
            }
        }
        //private static void AntiGapcloser(Obj_AI_Hero target, GapcloserArgs args)
        //{
        //    if (target == null || !target.IsValidTarget(E.Range)) return;

        //    switch (args.Type)
        //    {
        //        case SpellType.Melee:
        //            if (Main["antiGapcloser"]["r"].As<MenuBool>().Enabled && R.Ready && target.IsValidTarget(target.AttackRange + target.BoundingRadius + 100))
        //            {
        //                var rPred = R.GetPrediction(target);
        //                R.Cast(rPred.UnitPosition);
        //            }
        //            break;
        //        case SpellType.Dash:
        //            if (Main["antiGapcloser"]["r"].As<MenuBool>().Enabled && R.Ready && args.EndPosition.DistanceToPlayer() <= 350)
        //            {
        //                var rPred = R.GetPrediction(target);
        //                R.Cast(rPred.UnitPosition);
        //            }
        //            break;
        //        case SpellType.SkillShot:
        //        {
        //            if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && Game.TickCount > 2500)
        //            {
        //                var ePred = E.GetPrediction(target);

        //                E.Cast(ePred.UnitPosition);
        //            }
        //            }
        //            break;
        //        case SpellType.Targeted:
        //        {
        //            if (Main["antiGapcloser"]["r"].As<MenuBool>().Enabled && R.Ready && Game.TickCount > 2500)
        //            {
        //                var rPred = R.GetPrediction(target);
        //                R.Cast(rPred.UnitPosition);
        //            }     
        //            if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && Game.TickCount > 2500)
        //            {
        //                    var ePred = E.GetPrediction(target);

        //                    E.Cast(ePred.UnitPosition);
        //            }              
        //        }
        //            break;
        //    }
        //}
        private static void OnPreAttack(object sender, PreAttackEventArgs args)
        {
            switch (Misc.Orbwalker.Mode)
            {

                case OrbwalkingMode.Combo:
                case OrbwalkingMode.Mixed:
                case OrbwalkingMode.Lasthit:
                case OrbwalkingMode.Laneclear:
                    if (Player.HasBuff("VarusQ") && Q.IsCharging)
                    {
                        args.Cancel = true;
                    }
                    break;
            }
        }
        // ReSharper disable once UnusedMethodReturnValue.Local
        private static float StacksWDamage(Obj_AI_Base unit)
        {
            if (GetBuffCount(unit) == 0) return 0;
            float[] damageStackW = { 0, 0.02f, 0.0275f, 0.035f, 0.0425f, 0.05f };
            var stacksWCount = GetBuffCount(unit);
            var extraDamage = 2 * (Player.FlatMagicDamageMod / 100);
            var damageW = unit.MaxHealth * damageStackW[Player.SpellBook.GetSpell(SpellSlot.W).Level] * stacksWCount + (extraDamage - extraDamage % 2);
            var expiryDamage = Player.CalculateDamage(unit, DamageType.Magical, damageW > 360 && unit.GetType() != typeof(Obj_AI_Hero) ? 360 : damageW);
            return (float)expiryDamage;
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
                if (GetBuffCount(enemy) > 0 && Main["drawDamage"]["w"].As<MenuBool>().Enabled)
                {
                    wdmgDraw = StacksWDamage(enemy);
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
