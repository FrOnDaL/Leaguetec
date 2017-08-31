using Aimtec;
using System.Linq;
using System.Drawing;
using Aimtec.SDK.Util;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Damage;
using FrOnDaL_AIO.Common;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Extensions;
using FrOnDaL_AIO.Common.Utils;
using Spell = Aimtec.SDK.Spell;
using Aimtec.SDK.Menu.Components;
using System.Collections.Generic;
using static FrOnDaL_AIO.Common.Misc;
using Aimtec.SDK.Prediction.Collision;
using Aimtec.SDK.Prediction.Skillshots;
using static FrOnDaL_AIO.Common.Utils.XyOffset;
using static FrOnDaL_AIO.Common.Utils.Extensions;
using static FrOnDaL_AIO.Common.Utils.Invulnerable;

namespace FrOnDaL_AIO.Champions
{
    internal class DrMundo
    {
        public DrMundo()
        {
            Q = new Spell(SpellSlot.Q, 975);
            W = new Spell(SpellSlot.W, 325);
            E = new Spell(SpellSlot.E, 300);
            R = new Spell(SpellSlot.R);
            Q.SetSkillshot(0.275f, 60, 2000, true, SkillshotType.Line);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var drMundo = new Menu("drMundo", "Dr.Mundo");
            {
                var combo = new Menu("combo", "Combo");
                {
                    combo.Add(new MenuBool("q", "Use combo Q"));
                    combo.Add(new MenuBool("qKS", "Auto Q kill-steal"));
                    combo.Add(new MenuSlider("qHealth", "My health >= x% / use Q", 10, 1, 99));
                    var whiteList = new Menu("whiteList", "Q white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteList.Add(new MenuBool("qWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                        }
                    }
                    combo.Add(whiteList);
                    combo.Add(new MenuBool("w", "Use combo W"));
                    combo.Add(new MenuSlider("wHealth", "My health >= x% / use W", 20, 1, 99));
                    combo.Add(new MenuBool("wClose", "Use auto close W"));
                    combo.Add(new MenuBool("e", "Use combo E"));
                    combo.Add(new MenuBool("r", "Use Auto R"));
                    combo.Add(new MenuSlider("rHealth", "My health <= x% / use R", 20, 1, 99));
                    combo.Add(new MenuSliderBool("autR", "Use Auto R / if Count Enemy Heroes In Minimum Range >= 1", true, 700, 1, 900));
                }
                drMundo.Add(combo);

                var harass = new Menu("harass", "Harass");
                {
                    harass.Add(new MenuKeyBind("qkeyHarass", "Q Harass and Flee key :", KeyCode.C, KeybindType.Press));
                    harass.Add(new MenuBool("q", "Auto harass Q", false));
                    harass.Add(new MenuSlider("qHealth", "My health >= x% / use Q", 40, 1, 99));
                    var whiteList = new Menu("whiteList", "Q white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteList.Add(new MenuBool("qWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                        }
                    }
                    harass.Add(whiteList);
                }
                drMundo.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / My health >= x%", true, 50, 0, 99),
                    new MenuBool("qlast", "Use Q to last hit minions"),
                    new MenuSliderBool("w", "Use W / My health >= x%", false, 50, 0, 99),
                    new MenuSlider("UnitsWhit", "W hit x units minions >= x%", 3, 1, 4),
                    new MenuBool("e", "Use E lane clear", false)
                };
                drMundo.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / My health >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / My health >= x%", true, 30, 0, 99),
                    new MenuSliderBool("e", "Use E jungle clear", true, 30, 0, 99)
                };
                drMundo.Add(jungleclear);

                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q"));
                    drawings.Add(new MenuBool("w", "Draw W"));

                    var drawDamage = new Menu("drawDamage", "Use draw damage");
                    {
                        drawDamage.Add(new MenuBool("enabled", "Enabled"));
                        drawDamage.Add(new MenuBool("q", "Draw Q damage"));
                    }
                    drawings.Add(drawDamage);
                }
                drMundo.Add(drawings);
            }
            Main.Add(drMundo);
            Main.Attach();
            Render.OnPresent += SpellDraw;
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Obj_AI_Base.OnProcessAutoAttack += AfterAttackE;
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
        }
        private static float QDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.Q);
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
            if (Q.Ready && Main["combo"]["qKS"].As<MenuBool>().Enabled)
            {
                var target = TargetSelector.GetTarget(Q.Range);
                if (target != null && target.Health <= QDamage(target) && target.IsValidTarget(Q.Range) && !Check(target, DamageType.Magical))
                {
                    var pred = Q.GetPrediction(target);
                    var collision = Collision.GetCollision(new List<Vector3> { target.ServerPosition }, Q.GetPredictionInput(target));
                    var col = collision.Count(x => x.IsEnemy && x.IsMinion);
                    if (col == 0 && pred.HitChance >= HitChance.High)
                    {
                        Q.Cast(target);
                    }
                }
            }
            if (R.Ready && Main["combo"]["r"].As<MenuBool>().Enabled)
            {
                if (Player.HealthPercent() <= Main["combo"]["rHealth"].As<MenuSlider>().Value)
                {
                    if (Player.CountEnemyHeroesInRange(Main["combo"]["autR"].As<MenuSliderBool>().Value) >= 1 && Main["combo"]["autR"].As<MenuSliderBool>().Enabled)
                    {
                        R.Cast();
                    }
                    else if (!Main["combo"]["autR"].As<MenuSliderBool>().Enabled)
                    {
                        R.Cast();
                    }
                }
            }
        
            if (Main["harass"]["q"].As<MenuBool>().Enabled)
            {
                Harass();
            }
        }

        private static void Combo()
        {
            if (Main["combo"]["w"].As<MenuBool>().Enabled && W.Ready)
            {
                if (IsBurning() && (Player.CountEnemyHeroesInRange(500) == 0 && Main["combo"]["wClose"].As<MenuBool>().Enabled || Player.HealthPercent() <= 10 && Player.CountEnemyHeroesInRange(500) >= 1))
                {
                    W.Cast();
                }
            }
            var target = GetBestEnemyHeroTargetInRange(Q.Range);
            if (target == null) return;
            if (Main["combo"]["q"].As<MenuBool>().Enabled && Q.Ready && !Check(target, DamageType.Magical))
            {
                if (Player.HealthPercent() >= Main["combo"]["qHealth"].As<MenuSlider>().Value && target.IsValidTarget(Q.Range) && Main["combo"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled)
                {
                    var pred = Q.GetPrediction(target);
                    var collision = Collision.GetCollision(new List<Vector3> { target.ServerPosition }, Q.GetPredictionInput(target));
                    var col = collision.Count(x => x.IsEnemy && x.IsMinion);
                    if (col == 0 && pred.HitChance >= HitChance.VeryHigh)
                    {
                        Q.Cast(target);
                    }
                }
            }
            if (Main["combo"]["w"].As<MenuBool>().Enabled && W.Ready)
            {
                if (Player.HealthPercent() >= Main["combo"]["wHealth"].As<MenuSlider>().Value && !IsBurning() && target.IsValidTarget(325))
                {
                    W.Cast();
                }
            }            
        }

        private static void AfterAttackE(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            var attack = sender as Obj_AI_Hero;
            if (Misc.Orbwalker.Mode == OrbwalkingMode.Combo && attack != null && attack.IsMe && attack.IsHero)
            {
                if (Main["combo"]["e"].As<MenuBool>().Enabled && E.Ready)
                {
                    DelayAction.Queue(200, () => E.Cast());
                }
            }
        }

        private static void LaneClear()
        {   
            foreach (var target in GameObjects.EnemyMinions.Where(m => m.IsValidTarget(Q.Range)))
            {
                if (target != null)
                {
                    if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Q.Ready && Player.HealthPercent() >= Main["laneclear"]["q"].As<MenuSliderBool>().Value)
                    {
                        if (target.Health < QDamage(target) && Main["laneclear"]["qlast"].As<MenuBool>().Enabled || target.IsValidTarget(Q.Range) && !Main["laneclear"]["qlast"].As<MenuBool>().Enabled)
                        {
                            Q.Cast(target);
                        }
                    }

                    if (Main["laneclear"]["w"].As<MenuSliderBool>().Enabled && W.Ready)
                    {
                        if (GameObjects.EnemyMinions.Count(x => x.Distance(Player) < 300 && x.IsValidTarget(300)) >= Main["laneclear"]["UnitsWhit"].As<MenuSlider>().Value && Player.HealthPercent() >= Main["laneclear"]["w"].As<MenuSliderBool>().Value && !IsBurning())
                        {
                            W.Cast();
                        }
                        else if (IsBurning() && GameObjects.EnemyMinions.Count(x => x.Distance(Player) < 400 && x.IsValidTarget(400)) <= 1)
                        {
                            W.Cast();
                        }
                    }
                    if (Main["laneclear"]["e"].As<MenuBool>().Enabled && E.Ready)
                    {
                        if (GameObjects.EnemyMinions.Count(x => x.Distance(Player) < 300 && x.IsValidTarget(300)) >= 3)
                        {
                            DelayAction.Queue(300, () => E.Cast());
                        }
                    }
                }           
            }
        }

        private static void JungleClear()
        {
            if (IsBurning() && GameObjects.Jungle.Count(x => x.Distance(Player) < 600 && x.IsValidTarget(600) && x.IsValidTarget(600)) == 0 && Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && GameObjects.EnemyMinions.Count(x => x.Distance(Player) < 700 && x.IsValidTarget(700)) < 1)
             {
                    W.Cast();
             }
            
            foreach (var target in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (target != null)
                {
                    if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.HealthPercent() >= Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready)
                    {
                        Q.Cast(target);
                    }

                    if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.HealthPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready && Player.Distance(target.Position) < 300 && !IsBurning())
                    {
                        W.Cast();
                    }

                    if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && E.Ready && Player.Distance(target.Position) < 250)
                    {
                        E.Cast();
                    }
                }          
            }
        }

        private static void Harass()
        {
            var target = GetBestEnemyHeroTargetInRange(Q.Range);
            if (target == null) return;
            if (Player.HealthPercent() >= Main["harass"]["qHealth"].As<MenuSlider>().Value && target.IsValidTarget(Q.Range) && Main["harass"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && !Check(target, DamageType.Magical) && Q.Ready)
            {
                var pred = Q.GetPrediction(target);
                var collision = Collision.GetCollision(new List<Vector3> { target.ServerPosition }, Q.GetPredictionInput(target));
                var col = collision.Count(x => x.IsEnemy && x.IsMinion);
                if (col == 0 && pred.HitChance >= HitChance.VeryHigh)
                {
                    Q.Cast(target);
                }
            }
        }
        private static bool IsBurning()
        {
            return Player.HasBuff("BurningAgony");
        }

        private static void DamageDraw()
        {
            if (!Main["drawDamage"]["enabled"].Enabled) return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.IsVisible && Player.Distance(x) < 1700))
            {
                float qdmgDraw = 0;
                if (Q.Ready && Main["drawDamage"]["q"].As<MenuBool>().Enabled)
                {
                    qdmgDraw = QDamage(enemy);
                }
                var damage = qdmgDraw;
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
