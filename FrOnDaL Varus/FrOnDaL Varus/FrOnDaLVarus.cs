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
using Aimtec.SDK.Menu.Components;
using System.Collections.Generic;
using Aimtec.SDK.Prediction.Skillshots;

namespace FrOnDaL_Varus
{
    internal class FrOnDaLVarus
    {
        public static Menu Main = new Menu("Index", "FrOnDaL Varus", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Varus => ObjectManager.GetLocalPlayer();
        private static Spell _q, _e, _r;
        internal static bool IsPreAa;
        internal static bool IsAfterAa;
       /* public static bool BuffW(Obj_AI_Base unit) => unit.Buffs.Any(x => x.Name.Equals("VarusW", StringComparison.CurrentCultureIgnoreCase));
        public static Buff GoBuffW(Obj_AI_Base unit) => BuffW(unit) ? unit.Buffs.First(x => x.Name.Equals("VarusW", StringComparison.CurrentCultureIgnoreCase)) : null;
        public static int GetBuffCount(Obj_AI_Hero target) => target.GetBuffCount("VarusW");*/
        public static double QDamage(Obj_AI_Base d)
        {
            var damageQ = Varus.CalculateDamage(d, DamageType.Physical, (float)new double[] { 12, 58, 104, 150, 196 }[Varus.SpellBook.GetSpell(SpellSlot.Q).Level - 1] + Varus.TotalAttackDamage / 100 * 132); return damageQ;
        }     

        public static readonly List<string> SpecialChampions = new List<string> { "Annie", "Jhin" };
        public static int SxOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 1 : 10;
        }
        public static int SyOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 7 : 15;
        }

        public FrOnDaLVarus()
        {
            /*Spells*/
            _q = new Spell(SpellSlot.Q, 1000);
            _e = new Spell(SpellSlot.E, 925);
            _r = new Spell(SpellSlot.R, 1200);

            _q.SetSkillshot(0.25f, 70, 1900, false, SkillshotType.Line);
            _q.SetCharged("VarusQ", "VarusQ", 1000, 1600, 1.3f);           
            _e.SetSkillshot(250, 235, 1500f, false, SkillshotType.Circle);
            _r.SetSkillshot(250, 120, 1950f, false, SkillshotType.Line);

            Orbwalker.Attach(Main);

            /*Combo Menu*/
            var combo = new Menu("combo", "Combo");
            {
                combo.Add(new MenuBool("q", "Use Combo Q"));
                //combo.Add(new MenuSliderBool("qstcW", "Minimum W stack for Q", false, 2, 1, 3));
                var whiteListQ = new Menu("whiteListQ", "Q White List");
                {
                    foreach (var enemies in GameObjects.EnemyHeroes)
                    {
                        whiteListQ.Add(new MenuBool("qWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                    }
                }              
                combo.Add(whiteListQ);
                combo.Add(new MenuBool("e", "Use Combo E"));
                combo.Add(new MenuSlider("UnitsEhit", "E Hit x Units Enemy", 1, 1, 3));
                //combo.Add(new MenuSliderBool("eStcW", "Minimum W stack for E", false, 2, 1, 3));
                combo.Add(new MenuKeyBind("keyR", "R Key:", KeyCode.T, KeybindType.Press));
                combo.Add(new MenuSlider("rHit", "Minimum enemies for R", 2, 1, 5));
                var whiteListR = new Menu("whiteListR", "R White List");
                {
                    foreach (var enemies in GameObjects.EnemyHeroes)
                    {
                        whiteListR.Add(new MenuBool("rWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                    }
                }
                combo.Add(whiteListR);
            }                      
            Main.Add(combo);

            /*Harass Menu*/
            var harass = new Menu("harass", "Harass");
            {
                harass.Add(new MenuBool("autoHarass", "Auto Harass", false));
                harass.Add(new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press));
                harass.Add(new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 70, 0, 99));
                var whiteListQ = new Menu("whiteListQ", "Q White List");
                {
                    foreach (var enemies in GameObjects.EnemyHeroes)
                    {
                        whiteListQ.Add(new MenuBool("qWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                    }
                }
                harass.Add(whiteListQ);
                harass.Add(new MenuSliderBool("e", "Use E / if Mana >= x%", false, 70, 0, 99));
            }
            Main.Add(harass);

            /*LaneClear Menu*/
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 60, 0, 99),
                new MenuSlider("UnitsQhit", "Q Hit x Units minions >= x%", 2, 1, 3),
                new MenuSliderBool("e", "Use E / if Mana >= x%", false, 60, 0, 99),
                new MenuSlider("UnitsEhit", "E Hit x Units minions >= x%", 3, 1, 4)
            };
            Main.Add(laneclear);

            /*JungleClear Menu*/
            var jungleclear = new Menu("jungleclear", "Jungle Clear")
            {
                new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("e", "Use E / if Mana >= x%", true, 30, 0, 99)
            };
            Main.Add(jungleclear);

            /*Drawings Menu*/
            var drawings = new Menu("drawings", "Drawings")
            {
                new MenuBool("q", "Draw Q"),
                new MenuBool("e", "Draw E", false),
                new MenuBool("r", "Draw R", false),
                new MenuBool("drawDamage", "Use Draw Q Damage")
            };
            Main.Add(drawings);
            Main.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Render.OnPresent += SpellDraw;
            
            Orbwalker.PreAttack += (a, b) => IsPreAa = true;
            Orbwalker.PostAttack += (a, b) => { IsPreAa = false; IsAfterAa = true; };
        }

        /*Drawings*/
        private static void SpellDraw()
        {
            if (Main["drawings"]["q"].As<MenuBool>().Enabled)
            {
                Render.Circle(Varus.Position, _q.ChargedMinRange, 180, Color.Green);
            }
            if (Main["drawings"]["e"].As<MenuBool>().Enabled)
            {
                Render.Circle(Varus.Position, _e.Range, 180, Color.Green);
            }
            if (Main["drawings"]["r"].As<MenuBool>().Enabled)
            {
                Render.Circle(Varus.Position, _r.Range, 180, Color.Green);
            }
        }

        private static void Game_OnUpdate()
        {
            if (Varus.IsDead || MenuGUI.IsChatOpen()) return;
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

            if (Main["combo"]["keyR"].As<MenuKeyBind>().Enabled)
            {
                ManualR();
            }
        }

        /*Comob*/
        private static void Combo()
        {

            var target = TargetSelector.GetTarget(_q.ChargedMaxRange);
            if (target == null) return;

            if (Main["combo"]["q"].As<MenuBool>().Enabled && Main["combo"]["whiteListQ"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && _q.Ready)
            {
                /*if (Main["combo"]["qstcW"].As<MenuSliderBool>().Enabled && BuffW(target) && GoBuffW(target).Count >= Main["combo"]["qstcW"].As<MenuSliderBool>().Value || !Main["combo"]["qstcW"].As<MenuSliderBool>().Enabled)
                {*/                                               
                if (!_q.IsCharging && !IsPreAa)
                {
                    _q.StartCharging(_q.GetPrediction(target).CastPosition); return;
                }
                if (!_q.IsCharging) return;              
                    if (Varus.CountEnemyHeroesInRange(700) == 0 && _q.ChargePercent >= 100)
                    {
                        var prediction = _q.GetPrediction(target);

                        if (prediction.HitChance >= HitChance.Medium)
                        {
                            _q.Cast(_q.GetPrediction(target).CastPosition);
                        }
                           
                    }
                    else if (Varus.CountEnemyHeroesInRange(700) >= 1 && _q.ChargePercent >= 30)
                    {
                        var prediction = _q.GetPrediction(target);

                        if (prediction.HitChance >= HitChance.Medium)
                        {
                            _q.Cast(_q.GetPrediction(target).CastPosition);
                        }

                    }
               //}
        }

            if (Main["combo"]["e"].As<MenuBool>().Enabled && target.IsValidTarget(_e.Range) && _e.Ready)
            {
               /* if (Main["combo"]["eStcW"].As<MenuSliderBool>().Enabled && BuffW(target) && GoBuffW(target).Count >=
                    Main["combo"]["eStcW"].As<MenuSliderBool>().Value || !Main["combo"]["eStcW"].As<MenuSliderBool>().Enabled)
                { */                 
                    foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_e.Range)))
                    {
                        if (enemy == null) continue;
                        if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(_e.Width, false, false, _e.GetPrediction(enemy).CastPosition)) >= Main["combo"]["UnitsEhit"].As<MenuSlider>().Value)
                        {
                            _e.Cast(_e.GetPrediction(target).CastPosition);
                        }
                    }
                //}
            }

        }

        /*Harass*/
        private static void Harass()
        {
            var target = TargetSelector.GetTarget(_q.ChargedMaxRange - 100);
            if (target == null) return;
            if (Main["harass"]["q"].As<MenuSliderBool>().Enabled && Varus.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value && Main["harass"]["whiteListQ"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && _q.Ready && !Varus.IsUnderEnemyTurret())
            {
                if (!_q.IsCharging && !IsPreAa)
                {
                    _q.StartCharging(_q.GetPrediction(target).CastPosition); return;
                }
                if (!_q.IsCharging) return;
                if (Varus.CountEnemyHeroesInRange(700) == 0 && _q.ChargePercent >= 100)
                {
                    _q.Cast(_q.GetPrediction(target).CastPosition);
                }
                else if (Varus.CountEnemyHeroesInRange(700) >= 1 && _q.ChargePercent >= 30)
                {
                    _q.Cast(_q.GetPrediction(target).CastPosition);
                }
            }

            if (Main["harass"]["e"].As<MenuSliderBool>().Enabled && Varus.ManaPercent() > Main["harass"]["e"].As<MenuSliderBool>().Value && !Varus.IsUnderEnemyTurret() && target.IsValidTarget(_e.Range) && _e.Ready)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(_e.Range)))
                {
                    if (enemy == null) continue;
                    if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(_e.Width, false, false, _e.GetPrediction(enemy).CastPosition)) >= 1)
                    {
                        _e.Cast(enemy.Position);
                    }
                }
            }
        }
    
        /*Lane Clear*/
        private static void LaneClear()
        {         
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Varus.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value && _q.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_q.ChargedMaxRange)))
                {
                    if (target == null) return;              
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(150, false, false, _q.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsQhit"].As<MenuSlider>().Value && !Varus.IsUnderEnemyTurret() && !_q.IsCharging && !IsPreAa)
                    {
                        _q.StartCharging(_q.GetPrediction(target).CastPosition); return;
                    }
                    if (!_q.IsCharging) return;
                    if (Varus.Distance(target) > 700 && _q.ChargePercent >= 90)
                    {
                        _q.Cast(target.Position);
                    }
                    else if (Varus.Distance(target) < 700 && _q.ChargePercent >= 40)
                    {
                        _q.Cast(target.Position);
                    }
                }
            }

            if (Main["laneclear"]["e"].As<MenuSliderBool>().Enabled && Varus.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value && _e.Ready && Varus.CountEnemyHeroesInRange(_e.Range) == 0)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(_e.Range)))
                {
                    if (target == null) continue;
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(_e.Width, false, false, _e.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsEhit"].As<MenuSlider>().Value && !Varus.IsUnderEnemyTurret())
                    {
                        _e.Cast(_e.GetPrediction(target).CastPosition);
                    }
                }
            }
        }  

        /*Jungle Clear*/
        private static void JungleClear()
        {
            foreach (var target in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(_q.ChargedMaxRange)))
            {
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Varus.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && target.IsValidTarget(1000) && _q.Ready)
                {
                                                            
                    if (!_q.IsCharging)
                    {
                        if (!IsPreAa)
                            _q.StartCharging(target.Position);
                    }
                    else if (_q.IsCharging && _q.ChargePercent >= 100)
                    {
                        _q.Cast(target.Position);
                    }  
                    else if (Varus.Distance(target) < 700 && _q.ChargePercent >= 40)
                    {
                        _q.Cast(target.Position);
                    }               
                }               

                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Varus.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && target.IsValidTarget(_e.Range) && _e.Ready)
                {
                    _e.Cast(target.Position);
                }
            }          
        }

        private static void ManualR()
        {
            var target = TargetSelector.GetTarget(_r.Range);
            if (target == null) return;
            var rHit = GameObjects.EnemyHeroes.Where(x => x.Distance(target) <= 450f).ToList();
            if (Main["combo"]["whiteListR"]["rWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && _r.Ready &&
                target.IsInRange(_r.Range - 100) && target.IsValidTarget(_r.Range - 100) && rHit.Count >= Main["combo"]["rHit"].As<MenuSlider>().Value)
            {              
                    _r.Cast(_r.GetPrediction(target).CastPosition);             
            }
        }

        /*Draw Damage Q */
        private static void DamageDraw()
        {
            if (Main["drawings"]["drawDamage"].Enabled)
            {
                ObjectManager.Get<Obj_AI_Base>().Where(h => h is Obj_AI_Hero && h.IsValidTarget(1700)).ToList().ForEach(unit =>
                {
                    var heroUnit = unit as Obj_AI_Hero;
                    const int width = 103;
                    var xOffset = SxOffset(heroUnit);
                    var yOffset = SyOffset(heroUnit);
                    var barPos = unit.FloatingHealthBarPosition;
                    barPos.X += xOffset;
                    barPos.Y += yOffset;
                    var drawEndXPos = barPos.X + width * (unit.HealthPercent() / 100);
                    var drawStartXPos = (float)(barPos.X + (unit.Health > QDamage(unit) ? width * ((unit.Health - QDamage(unit)) / unit.MaxHealth * 100 / 100) : 0));
                    Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 9, true, unit.Health < QDamage(unit) ? Color.GreenYellow : Color.ForestGreen);
                });
            }
        }

        //private static float StacksWDamage(Obj_AI_Base unit)
        //{
        //    if (!BuffW(unit)) return 0;
        //    float[] damageStackW = { 0, 0.02f, 0.0275f, 0.035f, 0.0425f, 0.05f };
        //    var stacksWCount = GoBuffW(unit).Count;
        //    var extraDamage = 2 * (Varus.FlatMagicDamageMod / 100);
        //    var damageW = unit.MaxHealth * damageStackW[Varus.SpellBook.GetSpell(SpellSlot.W).Level] * stacksWCount + (extraDamage - extraDamage % 2);
        //    var expiryDamage = Varus.CalculateDamage(unit, DamageType.Magical, damageW > 360 && unit.GetType() != typeof(Obj_AI_Hero) ? 360 : damageW);
        //    return (float) expiryDamage;
        //}       
    }
}
