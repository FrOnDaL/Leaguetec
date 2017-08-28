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
    internal class Jhin
    {              
        public Jhin()
        {           
            Q = new Spell(SpellSlot.Q, 620);
            W = new Spell(SpellSlot.W, 3000);
            E = new Spell(SpellSlot.E, 750);
            R = new Spell(SpellSlot.R, 3500);

            W.SetSkillshot(0.75f, 40, float.MaxValue, false, SkillshotType.Line);
            E.SetSkillshot(1f, 120, 1600, false, SkillshotType.Circle);
            R.SetSkillshot(0.24f, 80, 5000, false, SkillshotType.Line);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var jhin = new Menu("jhin", "Jhin");
            {
                var combo = new Menu("combo", "Combo")
                {
                    new MenuBool("q", "Use combo Q"),
                    new MenuBool("qAfter", "Fast Q and fourth shot"),
                    new MenuSliderBool("qEE", "Auto Q logic minion --> Enemy / if mana >= x% (beta)", true, 30, 0, 99),
                    new MenuSlider("qHit", "Minimum enemies for Q", 1, 1, 4),
                    new MenuBool("w", "Use combo W"),
                    new MenuBool("wKS", "Auto W kill-steal"),
                    new MenuSlider("wMin", "W minimum range to cast", 450, 100, 650),
                    new MenuSlider("wMax", "W maximum range to cast", 2500, 600, 3000),
                    new MenuBool("e", "Use combo E"),
                    new MenuBool("eCC", "Auto E on CC"),
                    new MenuSlider("eHit", "Minimum enemies for E", 1, 1, 3),
                    new MenuBool("r", "Use combo R"),
                    new MenuKeyBind("rKey", "Semi-manual cast R key", KeyCode.T, KeybindType.Press),
                    new MenuBool("rKS", "Auto R kill-steal"),
                    new MenuBool("rVisable", "Don't shot if enemy is not visable", false),
                    new MenuSlider("rMin", "R minimum range to cast", 1000, 100, 3500),
                    new MenuSlider("rMax", "R maximum range to cast", 3000, 600, 3500),
                    new MenuSlider("rSafe", "R safe area range", 1000, 600, 2000)
                };
                jhin.Add(combo);

                var harass = new Menu("harass", "Harass")
                {
                    new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                    new MenuSliderBool("q", "Harass use Q / if mana >= x%", true, 50, 0, 99),
                    new MenuSliderBool("qA", "Auto use Q / if mana >= x%", false, 50, 0, 99),
                    new MenuSlider("qHit", "Minimum enemies for auto Q", 2, 1, 4),
                    new MenuSliderBool("w", "Harass use W / if mana >= x%", false, 50, 0, 99),
                    new MenuBool("wCC", "Harass use W CC enemy (If harass W is active)"),
                    new MenuSliderBool("e", "Harass use E / if mana >= x%", false, 50, 0, 99)
                };
                jhin.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 60, 0, 99),
                    new MenuBool("qFirst", "Q - If the first minion kill"),
                    new MenuSliderBool("w", "Use W / if mana >= x%", false, 60, 0, 99),
                    new MenuSlider("UnitsWhit", "W Hit x units minions >= x%", 3, 1, 6),
                    new MenuSliderBool("e", "Use E / if mana >= x%", false, 60, 0, 99),
                    new MenuSlider("UnitsEhit", "E Hit x units minions >= x%", 3, 1, 4)
                };
                jhin.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", false, 30, 0, 99)
                };
                jhin.Add(jungleclear);

                var antiGapcloser = new Menu("antiGapcloser", "Jhin anti-gapcloser spells")
                {
                    new MenuBool("w", "Anti-gapcloser W"),
                    new MenuBool("e", "Anti-gapcloser E")
                };
                jhin.Add(antiGapcloser);
                Gapcloser.Attach(jhin, "Anti-gapcloser settings");
                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q"));
                    drawings.Add(new MenuBool("w", "Draw W", false));
                    drawings.Add(new MenuBool("e", "Draw E"));
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
                jhin.Add(drawings);
            }
            Main.Add(jhin);
            Main.Attach();
            Render.OnPresent += SpellDraw;
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Gapcloser.OnGapcloser += AntiGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
        }
      
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender.IsMe && args.SpellData.Name.ToLower() == "jhinr")
            {
                _rPosCast = args.End;
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
                if (!R.Ready) return 0;
                var damage = (-25 + 75 * Player.SpellBook.GetSpell(SpellSlot.R).Level + 0.2 * Player.FlatPhysicalDamageMod) *
                             (1 + (100 - d.HealthPercent()) * 0.02);
                return (float)Player.CalculateDamage(d, DamageType.Physical, damage);        
        }
 
        private static Obj_AI_Hero _rTargetLast;
        private static Vector3 _rPosLast;
        #pragma warning disable 649
        private static Vector3 _rPosCast;
        #pragma warning restore 649
        private static bool FourthShot() => Player.HasBuff("jhinpassiveattackbuff");
        private static bool IsCastingR() => Player.SpellBook.GetSpell(SpellSlot.R).Name.Equals("JhinRShot");
                
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
            if (Q.Ready && Main["combo"]["qEE"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["combo"]["qEE"].As<MenuSliderBool>().Value)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range, true)))
                {
                    if (minion != null)
                    {
                        var enemyInBounceRange = GameObjects.EnemyHeroes.FirstOrDefault(x => x.IsValidTarget(800, false, false, minion.ServerPosition));
                        if (enemyInBounceRange != null && enemyInBounceRange.Distance(Player) > 620)
                        {
                            if (minion.Distance(enemyInBounceRange) < 350 & minion.CountEnemyHeroesInRange(350) <= 3)
                            {
                                Q.CastOnUnit(minion);
                            }
                        }
                    }
                }
            }

            if (R.Ready && Main["combo"]["rKS"].As<MenuBool>().Enabled)
            {
                JhinR();
            }
            
             if (W.Ready && Main["combo"]["wKS"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Combo && !IsCastingR())
            {
                var target = TargetSelector.GetTarget(2800);
                if (target != null && target.Health <= WDamage(target))
                {
                    W.Cast(target);
                }
            }
            if (E.Ready && Main["combo"]["eCC"].As<MenuBool>().Enabled && !IsCastingR())
            {               
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsImmobile() && x.Distance(Player) < E.Range))
                {
                    E.Cast(target);
                }
                foreach (var minion in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsEnemy && x.Distance(Player) <= W.Range && x.ValidActiveBuffs().Any(y => y.Name.Equals("teleport_target"))))
                {
                    E.Cast(minion.ServerPosition);
                }
            }
            if (Q.Ready && Main["harass"]["qA"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["qA"].As<MenuSliderBool>().Value && !IsCastingR())
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target != null)
                {
                    var qHit = GameObjects.EnemyHeroes.Where(x => x.Distance(target) <= 400f).ToList();
                    if (qHit.Count >= Main["harass"]["qHit"].As<MenuSlider>().Value || target.Health <= QDamage(target))
                    {
                        Q.CastOnUnit(target);
                    }
                }             
            }

             if (R.Ready && Main["combo"]["rKey"].As<MenuKeyBind>().Enabled)
             {
                R.Range = !IsCastingR() ? Main["combo"]["rMax"].As<MenuSlider>().Value : 3500;
                var targetR = GetBestEnemyHeroTargetInRange(R.Range);
                if (targetR != null && targetR.IsValidTarget())
                {
                    _rPosLast = R.GetPrediction(targetR).CastPosition;

                    if (!IsCastingR() && Player.CountEnemyHeroesInRange(800) == 0 && !Player.IsUnderEnemyTurret() && !Check(targetR) && !IsSpellHeroCollision(targetR, R))
                    {
                        R.Cast(_rPosLast); _rTargetLast = targetR;
                    }
                    if (IsCastingR())
                    {
                        if (InCone(targetR.ServerPosition))
                        {
                            R.Cast(targetR);
                        }
                        else
                        {
                            foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(R.Range) && InCone(targetR.ServerPosition)).OrderBy(enemy => enemy.Health))
                            {
                                R.Cast(targetR); _rPosLast = R.GetPrediction(enemy).CastPosition; _rTargetLast = enemy;
                            }
                        }
                    }
                }
                else if (IsCastingR() && _rTargetLast != null && !_rTargetLast.IsDead)
                {
                    if (!Main["combo"]["rVisable"].As<MenuBool>().Enabled && InCone(_rTargetLast.Position) && InCone(_rPosLast))
                    {
                        R.Cast(_rPosLast);
                    }
                }
            }

            Misc.Orbwalker.MovingEnabled = !IsCastingR();
            Misc.Orbwalker.AttackingEnabled = !IsCastingR();
        }

        private static void Combo()
        {
            var target = GetBestEnemyHeroTargetInRange(800);

            if (Q.Ready && Main["combo"]["q"].As<MenuBool>().Enabled && target.IsValidTarget(Q.Range) && !Check(target, DamageType.Magical, false) && !IsCastingR())
            {
                if(target == null) return;
                var qHit = GameObjects.EnemyHeroes.Where(x => x.Distance(target) <= 400f).ToList();
                if (qHit.Count >= Main["combo"]["qHit"].As<MenuSlider>().Value && !Main["combo"]["qAfter"].As<MenuBool>().Enabled || target.Health <= QDamage(target))
                {
                        Q.CastOnUnit(target);                                  
                }
                if (Main["combo"]["qAfter"].As<MenuBool>().Enabled && FourthShot() || target.Health <= QDamage(target))
                {
                    DelayAction.Queue(300, () => Q.CastOnUnit(target));
                }
            }

            if (E.Ready && Main["combo"]["e"].As<MenuBool>().Enabled && target.IsValidTarget(E.Range) && !Check(target, DamageType.Magical, false) && !IsCastingR())
            {
                if (target == null) return;
                if (GameObjects.EnemyHeroes.Count(x => x.IsValidTarget(235, false, false, E.GetPrediction(target).CastPosition)) >= Main["combo"]["eHit"].As<MenuSlider>().Value)
                {
                    E.Cast(target);
                }               
            }

            if (W.Ready && Main["combo"]["w"].As<MenuBool>().Enabled && !IsCastingR())
            {
                foreach (var targetW in GameObjects.EnemyHeroes.Where(x => x.HasBuff("jhinespotteddebuff") && x.IsValidTarget(Main["combo"]["wMax"].As<MenuSlider>().Value) && x.Distance(Player) > Main["combo"]["wMin"].As<MenuSlider>().Value && !Check(x, DamageType.Magical, false) || !Check(x, DamageType.Magical, false) && x.IsValidTarget(2500) && x.Health <= WDamage(target)))
                {
                   W.Cast(targetW);
                }
            }

            if (R.Ready && Main["combo"]["r"].As<MenuBool>().Enabled)
            {
                JhinR();
            }
        }
        private static void JhinR()
        {
            R.Range = !IsCastingR() ? Main["combo"]["rMax"].As<MenuSlider>().Value : 3500;
            var targetR = GetBestEnemyHeroTargetInRange(R.Range);
            if (targetR != null && targetR.IsValidTarget())
            {
                _rPosLast = R.GetPrediction(targetR).CastPosition;

                if (!IsCastingR() && RDamage(targetR) * 4 > targetR.Health && Player.CountEnemyHeroesInRange(Main["combo"]["rSafe"].As<MenuSlider>().Value) == 0 && Player.Distance(targetR) > Main["combo"]["rMin"].As<MenuSlider>().Value && !Player.IsUnderEnemyTurret() && !Check(targetR) && !IsSpellHeroCollision(targetR, R))
                {
                    R.Cast(_rPosLast); _rTargetLast = targetR;
                }
                if (!IsCastingR()) return;
                if (InCone(targetR.ServerPosition))
                {
                    R.Cast(targetR);
                }
                else
                {
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(R.Range) && InCone(targetR.ServerPosition)).OrderBy(enemy => enemy.Health))
                    {
                        R.Cast(targetR); _rPosLast = R.GetPrediction(enemy).CastPosition; _rTargetLast = enemy;
                    }
                }
            }
            else if (IsCastingR() && _rTargetLast != null && !_rTargetLast.IsDead)
            {
                if (!Main["combo"]["rVisable"].As<MenuBool>().Enabled && InCone(_rTargetLast.Position) && InCone(_rPosLast))
                {
                    R.Cast(_rPosLast);
                }
            }
        }

        private static void Harass()
        {
            var target = GetBestEnemyHeroTargetInRange(800);

            if (Q.Ready && Main["harass"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value && target.IsValidTarget(Q.Range) && !Check(target, DamageType.Magical, false) && !IsCastingR())
            {
                if (target == null) return; Q.CastOnUnit(target);              
            }

            if (E.Ready && Main["harass"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["e"].As<MenuSliderBool>().Value && target.IsValidTarget(E.Range) && !Check(target, DamageType.Magical, false) && !IsCastingR())
            {
                if (target == null) return; E.Cast(target);               
            }

            if (!W.Ready || !Main["harass"]["w"].As<MenuSliderBool>().Enabled || !(Player.ManaPercent() > Main["harass"]["w"].As<MenuSliderBool>().Value) || IsCastingR()) return;

            foreach (var targetW in GameObjects.EnemyHeroes.Where(x => x.HasBuff("jhinespotteddebuff") && !Check(x, DamageType.Magical, false) && x.IsValidTarget(2800) || x.IsImmobile() && x.IsValidTarget(2800) && Main["harass"]["wCC"].As<MenuBool>().Enabled && x.Distance(Player) > 400 && !Check(x, DamageType.Magical, false) || !Check(x, DamageType.Magical, false) && x.IsValidTarget(2800) && !Main["harass"]["wCC"].As<MenuBool>().Enabled))
            {
                W.Cast(targetW);
            }
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Q.Ready && Player.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(m =>  m.IsValidTarget(Q.Range) && m.Health < QDamage(m) && Main["laneclear"]["qFirst"].As<MenuBool>().Enabled || m.IsValidTarget(Q.Range) && !Main["laneclear"]["qFirst"].As<MenuBool>().Enabled))
                {
                    var qHit = GameObjects.EnemyMinions.Where(x => x.Distance(minion) <= 400f).ToList();
                    if (qHit.Count >= 3)
                    {
                        Q.CastOnUnit(minion);
                    }
                }                
            }

            if (Main["laneclear"]["w"].As<MenuSliderBool>().Enabled && W.Ready && Player.ManaPercent() > Main["laneclear"]["w"].As<MenuSliderBool>().Value)
            {
                var result = Polygon.GetLinearLocation(1000, W.Width);
                if(result == null) return;
                if (result.NumberOfMinionsHit >= Main["laneclear"]["UnitsWhit"].As<MenuSlider>().Value )
                {
                    W.Cast(result.CastPosition);
                }
            }

            if (!Main["laneclear"]["e"].As<MenuSliderBool>().Enabled || !(Player.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value) || !E.Ready) return;
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range)))
                {
                    if (minion == null) continue;
                    if (GameObjects.EnemyMinions.Count(m => m.IsValidTarget(E.Width, false, false, E.GetPrediction(minion).CastPosition)) >= Main["laneclear"]["UnitsEhit"].As<MenuSlider>().Value)
                    {
                        E.Cast(E.GetPrediction(minion).CastPosition);
                    }
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var target in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready)
                {
                    Q.CastOnUnit(target);
                }

                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready)
                {
                        W.Cast(target.Position);                 
                }

                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready && !GameObjects.JungleLegendary.Contains(target))
                {
                    E.Cast(target.Position.Extend(Player.ServerPosition, Vector3.Distance(target.Position, Player.Position) - 300));
                }
                else if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready && GameObjects.JungleLegendary.Contains(target))
                {
                    E.Cast(target.Position);
                }
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
                    edmgDraw = EDamage(enemy);
                }
                if (R.Ready && Main["drawDamage"]["r"].As<MenuBool>().Enabled)
                {
                    rdmgDraw = RDamage(enemy) * 4;
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

            //foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && Player.Distance(x) < 1700 && x.IsFloatingHealthBarActive && x.IsVisible))
            //{
            //    var xOffset = X(enemy);
            //    var yOffset = Y(enemy);
            //    var damage = (float)(QDamage(enemy) + WDamage(enemy) + EDamage(enemy) + (RDamage(enemy) * 4));
            //    var qdmgDraw = (float)QDamage(enemy) / damage;
            //    var wdmgDraw = (float)WDamage(enemy) / damage;
            //    var edmgDraw = (float)EDamage(enemy) / damage;
            //    var rdmgDraw = (float)(RDamage(enemy) * 4) / damage;
            //    var percentHealthAfterDamage = Math.Max(0, enemy.Health - damage) / enemy.MaxHealth;
            //    var barPos = enemy.FloatingHealthBarPosition + 3;
            //    var yPos = barPos.Y + yOffset;
            //    var xPosDamage = barPos.X + xOffset + 103 * percentHealthAfterDamage;
            //    var xPosCurrentHp = barPos.X + xOffset + 103 * enemy.Health / enemy.MaxHealth - 8;
            //    var differenceInHp = xPosCurrentHp - xPosDamage;
            //    var pos1 = barPos.X + xOffset + (107 * percentHealthAfterDamage);

            //    for (var i = 0; i < differenceInHp; i++)
            //    {
            //        if (i < qdmgDraw * differenceInHp)
            //            Render.Line(pos1 + i, yPos, pos1 + i, yPos + 9, 9, true, enemy.Health < damage ? Color.GreenYellow : Color.Cyan);

            //        else if (i < (qdmgDraw + wdmgDraw) * differenceInHp)
            //            Render.Line(pos1 + i, yPos, pos1 + i, yPos + 9, 9, true, enemy.Health < damage ? Color.GreenYellow : Color.Orange);

            //        else if (i < (qdmgDraw + wdmgDraw + edmgDraw) * differenceInHp)
            //            Render.Line(pos1 + i, yPos, pos1 + i, yPos + 9, 9, true, enemy.Health < damage ? Color.GreenYellow : Color.Yellow);

            //        else if (i < (qdmgDraw + wdmgDraw + edmgDraw + rdmgDraw) * differenceInHp)
            //            Render.Line(pos1 + i, yPos, pos1 + i, yPos + 9, 9, true, enemy.Health < damage ? Color.GreenYellow : Color.Violet);
            //    }
            //}
        }

        private static void AntiGapcloser(Obj_AI_Hero target, GapcloserArgs args)
        {
            if (target == null || !target.IsValidTarget(E.Range) || args.HaveShield || IsCastingR()) return;

            switch (args.Type)
            {
                case SpellType.SkillShot:
                {
                    if (target.IsValidTarget(300))
                    {
                        if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && Game.TickCount > 2500)
                        {
                            var ePred = E.GetPrediction(target);

                            E.Cast(ePred.CastPosition);
                        }

                        if (Main["antiGapcloser"]["w"].As<MenuBool>().Enabled && W.Ready && target.HasBuff("jhinespotteddebuff"))
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
                        if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && Game.TickCount > 2500)
                        {
                            var ePred = E.GetPrediction(target);

                            E.Cast(ePred.CastPosition);
                        }

                        if (Main["antiGapcloser"]["w"].As<MenuBool>().Enabled && W.Ready && target.HasBuff("jhinespotteddebuff"))
                        {
                            var wPred = W.GetPrediction(target);
                            W.Cast(wPred.UnitPosition);
                        }
                    }
                }
                    break;
            }
        }
     
        private static bool InCone(Vector3 position)
        {
            var range = R.Range;
            const float angle = 70f * (float)Math.PI / 180;
            var end2 =  _rPosCast.To2D() - Player.Position.To2D();
            var edge1 = end2.Rotated(-angle / 2);
            var edge2 = edge1.Rotated(angle);
            var point = position.To2D() - Player.Position.To2D();
            return point.Distance(new Vector2()) < range * range && edge1.CrossProduct(point) > 0 && point.CrossProduct(edge2) > 0;
        }
    }
}
