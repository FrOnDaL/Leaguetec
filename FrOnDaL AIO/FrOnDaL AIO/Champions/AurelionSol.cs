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
    internal class AurelionSol
    {
        public AurelionSol()
        {
            Q = new Spell(SpellSlot.Q, 810f);
            W = new Spell(SpellSlot.W, 600f);
            E = new Spell(SpellSlot.E, 1400);
            R = new Spell(SpellSlot.R, 1500f);

            Q.SetSkillshot(0.25f, 180f, 850, false, SkillshotType.Circle);
            R.SetSkillshot(0.25f, 180, 1750, false, SkillshotType.Line);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var aurelionsol = new Menu("aurelionsol", "Aurelion Sol");
            {
                var combo = new Menu("combo", "Combo")
                {
                    new MenuBool("q", "Use combo Q"),
                    new MenuBool("w", "Use combo W"),
                    new MenuBool("wClose", "Use auto close W"),
                    new MenuBool("WActive", "W Active --> Disable AutoAttacks", false),
                    new MenuKeyBind("qEKey", "Use cast E-Q key", KeyCode.T, KeybindType.Press),
                    new MenuBool("qReles", "E-Q / Auto Q release --> target distance is 700"),
                    new MenuSlider("qEMax", "E-Q maximum range to cast", 3000, 1000, 3000),
                    new MenuBool("r", "Use combo R"),
                    new MenuBool("rKS", "Auto R kill-steal"),
                    new MenuKeyBind("rKey", "Semi-manual cast R key", KeyCode.S, KeybindType.Press),
                    new MenuSlider("rHit", "Semi-manual R enemy Hit Count", 1, 1, 5),
                    new MenuBool("disableAA", "Disable AutoAttacks", false)

                };
                aurelionsol.Add(combo);

                var harass = new Menu("harass", "Harass")
                {
                    new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                    new MenuBool("autoHarass", "Auto Harass", false),
                    new MenuSliderBool("q", "Harass use Q / if mana >= x%", true, 50, 0, 99),
                };
                aurelionsol.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%",true , 60, 0, 99),
                    new MenuSlider("UnitsQhit", "Q Hit x units minions >= x%", 3, 1, 6),
                };
                aurelionsol.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 30, 0, 99),
                };
                aurelionsol.Add(jungleclear);

                //var antiGapcloser = new Menu("antiGapcloser", "Aurelion Sol anti-gapcloser spells")
                //{
                //    new MenuBool("r", "Anti-gapcloser R")
                //};
                //aurelionsol.Add(antiGapcloser);
                //Gapcloser.Attach(aurelionsol, "Anti-gapcloser settings");
                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q"));
                    drawings.Add(new MenuBool("w", "Draw W", false));
                    drawings.Add(new MenuBool("eMini", "Draw E minimap", false));
                    drawings.Add(new MenuBool("r", "Draw R"));
                    drawings.Add(new MenuBool("eq", "Draw E-Q minimap (Default Range 3000)"));
                    var drawDamage = new Menu("drawDamage", "Use draw damage");
                    {
                        drawDamage.Add(new MenuBool("enabled", "Enabled"));
                        drawDamage.Add(new MenuBool("q", "Draw Q damage", false));
                        drawDamage.Add(new MenuBool("r", "Draw R damage"));

                    }
                    drawings.Add(drawDamage);
                }
                aurelionsol.Add(drawings);
            }
            Main.Add(aurelionsol);
            Main.Attach();
        
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            GameObject.OnCreate += SolOnCreate;
           // Gapcloser.OnGapcloser += AntiGapcloser;
            Render.OnPresent += SpellDraw;
            GameObject.OnDestroy += SolOnDestroy;
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
            if (Main["drawings"]["eq"].As<MenuBool>().Enabled && E.Ready)
            {
                Vector2 centre;
                Render.WorldToMinimap(ObjectManager.GetLocalPlayer().Position, out centre);
                var rangePosition = ObjectManager.GetLocalPlayer().Position;
                rangePosition.X += Main["combo"]["qEMax"].As<MenuSlider>().Value;
                Vector2 end;
                Render.WorldToMinimap(rangePosition, out end);
                var radius = Math.Abs(end.X - centre.X);
                DrawCircle(centre, radius, Color.Aqua);
            }
            if (Main["drawings"]["r"].As<MenuBool>().Enabled && R.Ready)
            {
                Render.Circle(Player.Position, R.Range, 180, Color.Green);
            }
            if (Main["drawings"]["eMini"].As<MenuBool>().Enabled && E.Ready)
            {
                var eRange = new[] { 3000, 4000, 5000, 6000, 7000 }[Player.SpellBook.GetSpell(SpellSlot.E).Level - 1];
                Vector2 centre;
                Render.WorldToMinimap(ObjectManager.GetLocalPlayer().Position, out centre);
                var rangePosition = ObjectManager.GetLocalPlayer().Position;
                rangePosition.X += eRange;
                Vector2 end;
                Render.WorldToMinimap(rangePosition, out end);
                var radius = Math.Abs(end.X - centre.X);
                DrawCircle(centre, radius, Color.Aqua);
            }
        }
        private static float QDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.Q);
        }
        private static float RDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.R);
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

            if (Main["combo"]["qEKey"].As<MenuKeyBind>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Main["combo"]["qEMax"].As<MenuSlider>().Value);
                if (target != null)
                {
                    if (Player.Distance(target.ServerPosition) > 800 && target.IsValidTarget(Main["combo"]["qEMax"].As<MenuSlider>().Value) && Player.Mana > Player.SpellBook.GetSpell(SpellSlot.Q).Cost + Player.SpellBook.GetSpell(SpellSlot.E).Cost && E.Ready && Q.Ready && IsEActive)
                    {
                        E.Cast(target.ServerPosition);
                        if (IsQActive)
                        {
                            Q.Cast(target.ServerPosition);
                        }
                    }
                    if (Player.Distance(target.ServerPosition) < 700 && Main["combo"]["qReles"].As<MenuBool>().Enabled && !IsEActive)
                    {
                        E.Cast();
                    }
                    if (!IsQActive && AurelionSolQMissile != null && target.IsValidTarget(250f, false, false, AurelionSolQMissile.ServerPosition))
                    {
                        Q.Cast();
                        if (!Main["combo"]["qReles"].As<MenuBool>().Enabled && !IsEActive)
                        {
                            E.Cast();
                        }
                    }               
                }
            }
            if (!IsEActive || !IsEActive)
            {
                var target = GetBestEnemyHeroTargetInRange(Main["combo"]["qEMax"].As<MenuSlider>().Value);

                    if (Player.Distance(target.ServerPosition) < 700 && Main["combo"]["qReles"].As<MenuBool>().Enabled && !IsEActive)
                    {
                        E.Cast();
                    }
                    if (!IsQActive && AurelionSolQMissile != null && target.IsValidTarget(250f, false, false, AurelionSolQMissile.ServerPosition))
                    {
                        Q.Cast();
                        if (!Main["combo"]["qReles"].As<MenuBool>().Enabled && !IsEActive)
                        {
                            E.Cast();
                        }
                    }                               
            }
            
            if (R.Ready && Main["combo"]["rKS"].As<MenuBool>().Enabled)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(R.Range - 100) && !x.IsDead &&  x.Health < RDamage(x)))
                {
                    var pred = R.GetPrediction(enemy);
                    if (pred.HitChance >= HitChance.High && !Check(enemy, DamageType.Magical) && (!Q.Ready && !IsWActive || Player.Distance(enemy) > 750))
                    {
                        R.Cast(enemy);
                    }        
                }
            }
            if (Main["harass"]["autoHarass"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Mixed)
            {
                Harass();
            }
            if (R.Ready && Main["combo"]["rKey"].As<MenuKeyBind>().Enabled)
            {
                var result = PolygonEnemy.GetLinearLocation(1300, 120);
                if (result?.EnemyHit >= Main["combo"]["rHit"].As<MenuSlider>().Value)
                {
                    R.Cast(result.CastPosition);
                }
            }
            
            Misc.Orbwalker.MovingEnabled = IsEActive;
            Misc.Orbwalker.AttackingEnabled = IsEActive;
        }
        public static bool IsWActive => Player.HasBuff("AurelionSolWActive");
        public static bool IsQActive => Player.SpellBook.GetSpell(SpellSlot.Q).SpellData.Name == "AurelionSolQ";
        public static bool IsEActive => Player.SpellBook.GetSpell(SpellSlot.E).SpellData.Name == "AurelionSolE";

        private static void Combo()
        {
            if (Q.Ready && Main["combo"]["q"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target != null && !Check(target, DamageType.Magical) && Player.Distance(target) > 250)
                {
                    if (IsQActive && GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(Q.Width, false, false, Q.GetPrediction(target).CastPosition)) >= 1)
                    {
                        var pred = Q.GetPrediction(target);
                        if (pred.HitChance >= HitChance.High)
                        {
                            Q.Cast(target.Position);
                        }
                    }
                    else if (!IsQActive && AurelionSolQMissile != null && target.IsValidTarget(250f, false, false, AurelionSolQMissile.ServerPosition))
                    {
                        Q.Cast();
                    }
                }
            }
            if (W.Ready && Main["combo"]["w"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(830);
                if (target != null && target.IsValid)
                {
                    var distence = target.Distance(Player);
                    if (Main["combo"]["wClose"].As<MenuBool>().Enabled)
                    {
                        if (distence < 650 && distence > 610 && target.MoveSpeed > Player.MoveSpeed && target.IsFacing(Player) && !IsWActive)
                        {
                            W.Cast();
                        }
                        else if (distence > 590 && distence < 650 && !IsWActive)
                        {
                            W.Cast();
                        }
                        else if (distence < 590 && distence > 475 && !target.IsFacing(Player) && !IsWActive)
                        {
                            W.Cast();
                        }
                        else if (distence < 400 && distence > 360 && target.IsFacing(Player) && IsWActive)
                        {
                            W.Cast();
                        }
                        else if (distence < 360 && IsWActive)
                        {
                            W.Cast();
                        }                
                    }
                    if (!Main["combo"]["wClose"].As<MenuBool>().Enabled)
                    {
                        if (distence < 650 && distence > 610 && target.MoveSpeed > Player.MoveSpeed && target.IsFacing(Player) && !IsWActive)
                        {
                            W.Cast();
                        }
                        else if (distence > 590 && distence < 610 && !IsWActive)
                        {
                            W.Cast();
                        }
                        else if (distence < 590 && distence > 475 && !target.IsFacing(Player) && !IsWActive)
                        {
                            W.Cast();
                        }
                    }          
                }
                if (Player.CountEnemyHeroesInRange(1400) == 0 && IsWActive && Main["combo"]["wClose"].As<MenuBool>().Enabled)
                {
                    W.Cast();
                }
            }
            if (R.Ready && Main["combo"]["r"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(830);
                if (target != null && target.IsValid && !Check(target, DamageType.Magical))
                {
                    if ( target.Distance(Player) < (target.IsMelee ? 340 : W.Range - 100) && Player.HealthPercent() <= 40)
                    {
                        var pred = R.GetPrediction(target);
                        if (pred.HitChance >= HitChance.High)
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
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target != null && !Check(target, DamageType.Magical))
                {
                    if (IsQActive && GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(Q.Width, false, false, Q.GetPrediction(target).CastPosition)) >= 1)
                    {
                        var pred = Q.GetPrediction(target);
                        if (pred.HitChance >= HitChance.Medium)
                        {
                            Q.Cast(target.Position);
                        }
                    }
                    else if (!IsQActive && AurelionSolQMissile != null && target.IsValidTarget(250f, false, false, AurelionSolQMissile.ServerPosition))
                    {
                        Q.Cast();
                    }
                }
            }
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value && Q.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range - 160)))
                {
                    if (target != null)
                    {
                        if (GameObjects.EnemyMinions.Count(x => x.IsValidTarget(220f, false, false, Q.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsQhit"].As<MenuSlider>().Value && IsQActive)
                        {
                            Q.Cast(Q.GetPrediction(target).CastPosition);
                        }
                        else if (!IsQActive && AurelionSolQMissile != null && target.IsValidTarget(220f, false, false, AurelionSolQMissile.ServerPosition)
                            && GameObjects.EnemyMinions.Count(x => x.IsValidTarget(220f, false, false, AurelionSolQMissile.ServerPosition)) >= Main["laneclear"]["UnitsQhit"].As<MenuSlider>().Value)
                        {
                            Q.Cast();
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var target in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (target != null)
                {
                    if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >
                        Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready && IsQActive)
                    {
                        Q.Cast(target.Position);
                    }
                    else if (!IsQActive && AurelionSolQMissile != null && target.IsValidTarget(Q.Width, false, false, AurelionSolQMissile.ServerPosition))
                    {
                        Q.Cast();
                    }
                }
            }
        }

        //private static void AntiGapcloser(Obj_AI_Hero target, GapcloserArgs args)
        //{          
        //    if (Main["antiGapcloser"]["r"].As<MenuBool>().Enabled && R.Ready && target != null && target.IsValidTarget(R.Range) && !args.HaveShield)
        //    {
        //        switch (args.Type)
        //        {
        //            case SpellType.SkillShot:
        //            {
        //                if (target.IsValidTarget(300))
        //                {
        //                    var rPred = R.GetPrediction(target);
        //                    R.Cast(rPred.UnitPosition);
        //                }
        //            }
        //                break;
        //            case SpellType.Melee:
        //            case SpellType.Dash:
        //            case SpellType.Targeted:
        //            {
        //                if (target.IsValidTarget(400))
        //                {
        //                    var rPred = R.GetPrediction(target);
        //                    R.Cast(rPred.UnitPosition);
        //                }
        //            }
        //                break;
        //        }
        //    }
        //}

        private static void DamageDraw()
        {
            if (!Main["drawDamage"]["enabled"].Enabled) return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.IsVisible && Player.Distance(x) < 1700))
            {
                float qdmgDraw = 0, rdmgDraw = 0;
                if (Q.Ready && Main["drawDamage"]["q"].As<MenuBool>().Enabled)
                {
                    qdmgDraw = QDamage(enemy);
                }
                if (R.Ready && Main["drawDamage"]["r"].As<MenuBool>().Enabled)
                {
                    rdmgDraw = RDamage(enemy);
                }
                var damage = qdmgDraw + rdmgDraw;

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
        private static void OnPreAttack(object sender, PreAttackEventArgs args)
        {
            switch (Misc.Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    if (Main["combo"]["disableAA"].As<MenuBool>().Enabled)
                    {
                        args.Cancel = true;
                    }
                    if (Main["combo"]["WActive"].As<MenuBool>().Enabled && IsWActive)
                    {
                        args.Cancel = true;
                    }
                    if (!IsEActive)
                    {
                        args.Cancel = true;
                    }
                    break;
            }
        }
    }
}
