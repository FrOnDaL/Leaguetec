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
using System.Collections.Generic;
using static FrOnDaL_AIO.Common.Misc;
using Aimtec.SDK.Prediction.Collision;
using Aimtec.SDK.Prediction.Skillshots;
using static FrOnDaL_AIO.Common.Utils.XyOffset;
using static FrOnDaL_AIO.Common.Utils.Extensions;
using static FrOnDaL_AIO.Common.Utils.Invulnerable;

namespace FrOnDaL_AIO.Champions
{
    internal class Lux
    {
        public Lux()
        {
            Q = new Spell(SpellSlot.Q, 1175);
            W = new Spell(SpellSlot.W, 1075);
            E = new Spell(SpellSlot.E, 1075);
            R = new Spell(SpellSlot.R, 3000);

            Q.SetSkillshot(0.25f, 80f, 1200f, false, SkillshotType.Line);
            W.SetSkillshot(0.25f, 110f, 1200f, false, SkillshotType.Line);
            E.SetSkillshot(0.3f, 250f, 1050f, false, SkillshotType.Circle);
            R.SetSkillshot(1.35f, 190f, float.MaxValue, false, SkillshotType.Line);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var lux = new Menu("lux", "Lux");
            {
                var combo = new Menu("combo", "Combo");
                {
                   combo.Add(new MenuBool("q", "Use combo Q"));
                   combo.Add(new MenuBool("qCC", "Auto Q on CC"));
                   combo.Add(new MenuBool("qKS", "Auto Q kill-steal"));
                   var whiteList = new Menu("whiteList", "Q white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteList.Add(new MenuBool("qWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                        }
                    }
                   combo.Add(whiteList);
                   combo.Add(new MenuBool("e", "Use combo E"));
                   combo.Add(new MenuBool("eCC", "Auto E on CC", false));
                   combo.Add(new MenuBool("eKS", "Auto E kill-steal"));
                   combo.Add(new MenuBool("eSD", "E slow logic detonate"));                
                   combo.Add(new MenuSlider("UnitsEhit", "E hit x units enemy", 1, 1, 3));
                   combo.Add(new MenuBool("r", "Use combo R", false));
                   combo.Add(new MenuSliderBool("UnitsRhit", "R enemy Hit Count (Only combo mod)", false, 2, 1, 5));
                   combo.Add(new MenuBool("rKillSteal", "Auto R kill-steal"));
                   var jungSteal = new Menu("jungSteal", "R jungle stealer");
                    {
                        jungSteal.Add(new MenuBool("autoJ", "Auto R jungle stealer", false));
                        jungSteal.Add(new MenuKeyBind("keyJR", "R jungle stealer key", KeyCode.S, KeybindType.Press));
                        jungSteal.Add(new MenuBool("allyJ", "Ally R jungle stealer", false));
                        jungSteal.Add(new MenuBool("baronR", "Baron"));
                        jungSteal.Add(new MenuBool("dragonR", "Dragon"));
                        jungSteal.Add(new MenuBool("riftR", "Rift Herald"));
                        jungSteal.Add(new MenuBool("lueR", "Blue"));
                        jungSteal.Add(new MenuBool("redR", "Red"));
                    }
                   combo.Add(jungSteal);
                   combo.Add(new MenuKeyBind("keyR", "Semi-manual cast R key", KeyCode.T, KeybindType.Press));
                   
                }
                lux.Add(combo);

                var wProtect = new Menu("wProtect", "Auto W Protect");
                {
                    wProtect.Add(new MenuBool("enabled", "Enabled"));
                    wProtect.Add(new MenuSliderBool("autoW", "Auto W Protect / if Mana >= x%", false, 50, 0, 99));
                    var wCc = new Menu("wCC", "Auto W on CC");
                    {
                        foreach (var ally in GameObjects.AllyHeroes)
                        {
                            wCc.Add(new MenuBool("wccAlly" + ally.ChampionName.ToLower(), ally.ChampionName));
                        }
                    }
                    wProtect.Add(wCc);
                    var wPoison = new Menu("wP", "Auto W If there is poison");
                    {
                        foreach (var ally in GameObjects.AllyHeroes)
                        {
                            wPoison.Add(new MenuBool("wpAlly" + ally.ChampionName.ToLower(), ally.ChampionName));
                        }
                    }
                    wProtect.Add(wPoison);
                    var whiteList = new Menu("helSet", "Ally health setting");
                    {
                        foreach (var ally in GameObjects.AllyHeroes)
                        {
                            whiteList.Add(new MenuSliderBool("allyW" + ally.ChampionName.ToLower(), ally.ChampionName, true, 50, 0, 101));
                        }
                    }
                    wProtect.Add(whiteList);
                }
                lux.Add(wProtect);

                var harass = new Menu("harass", "Harass")
                {                  
                    new MenuKeyBind("keyHarass", "Harass key:", KeyCode.C, KeybindType.Press),
                    new MenuBool("autoHarass", "Auto harass", false),
                    new MenuSliderBool("q", "Use Q / if mana >= x%", false, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", true, 30, 0, 99),
                    new MenuSlider("UnitsEhit", "E hit x Units Enemy", 1, 1, 3),
                };
                lux.Add(harass);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", false, 60, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", true, 60, 0, 99),
                    new MenuSlider("UnitsEhit", "E hit x units minions >= x%", 3, 1, 7)
                };
                lux.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if mana >= x%", true, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", true, 30, 0, 99)
                };
                lux.Add(jungleclear);

                var antiGapcloser = new Menu("antiGapcloser", "Lux anti-gapcloser spells")
                {
                    new MenuBool("q", "Anti-gapcloser Q"),
                    new MenuBool("w", "Anti-gapcloser W ally"),
                    new MenuBool("w2", "Anti-gapcloser W lux"),
                    new MenuBool("e", "Anti-gapcloser E")
                };
                lux.Add(antiGapcloser);
                Gapcloser.Attach(lux, "Anti-gapcloser settings");
                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q", false));
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
                lux.Add(drawings);
            }
            Main.Add(lux);
            Main.Attach();

            Render.OnPresent += SpellDraw;
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Gapcloser.OnGapcloser += AntiGapcloser;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;       

        }
        private static float _jDmg;
        private static double _jTime;
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
                Render.WorldToMinimap(Player.Position, out centre);
                var rangePosition = Player.Position;
                rangePosition.X += R.Range;
                Vector2 end;
                Render.WorldToMinimap(rangePosition, out end);
                var radius = Math.Abs(end.X - centre.X);
                DrawCircle(centre, radius, Color.Aqua);
            }
        }
        private static Vector3 _eposition = Vector3.Zero;
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
            if (Main["harass"]["autoHarass"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Mixed && Misc.Orbwalker.Mode != OrbwalkingMode.Combo)
            {
                Harass();
            }
            if (Q.Ready && Main["combo"]["qCC"].As<MenuBool>().Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsImmobile() && x.IsValidTarget(Q.Range) && !Check(x, DamageType.Magical)))
                {
                    CastQ(target);
                }
            }
            if (Q.Ready && Main["combo"]["qKS"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target != null)
                if (target.IsValidTarget() && target.Health < QDamage(target) && !Check(target, DamageType.Magical))
                {
                    CastQ(target);
                }
            }           
            if (E.Ready && Main["combo"]["eCC"].As<MenuBool>().Enabled)
            {
                foreach (var target in GameObjects.EnemyHeroes.Where(x => x.IsImmobile() && x.IsValidTarget(E.Range) && !Check(x, DamageType.Magical)))
                {
                    E.Cast(target, true);
                }
            }
            
            if (E.Ready && Main["combo"]["eKS"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range - 100);
                if (target != null)
                if (target.IsValidTarget() && target.Health < QDamage(target) && !Check(target, DamageType.Magical))
                {
                    var epred = E.GetPrediction(target);
                    if (epred.HitChance >= HitChance.VeryHigh)
                    {
                        E.Cast(epred.CastPosition);
                    }                   
                }
            }
            
            if (Player.HasBuff("LuxLightStrikeKugel"))
            {
                var ePosition = _eposition.CountEnemyHeroesInRange(350f);
                if (Main["combo"]["eSD"].As<MenuBool>().Enabled)
                {
                    var detonate = ePosition - _eposition.CountEnemyHeroesInRange(200f);

                    if (detonate > 0 || ePosition > 1)
                    {
                        E.Cast();
                    }
                }
                else
                {
                    if (ePosition > 0)
                    {
                        E.Cast();
                    }
                }
            }

            if (R.Ready && Main["combo"]["keyR"].As<MenuKeyBind>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target != null && target.IsValidTarget(R.Range))
                {
                    R.Cast(target, true);
                }
            }
            if (R.Ready && Main["combo"]["rKillSteal"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                var rpred = R.GetPrediction(target);
                if (target != null)
                {
                    if (target.IsValidTarget() && Player.Distance(target) >= 800 && target.Health < RDamage(target) && !Check(target, DamageType.Magical))
                    {
                        if (rpred.HitChance >= HitChance.VeryHigh)
                        {
                            R.Cast(target);
                        }
                    }
                    if (target.IsImmobile() && target.IsValidTarget(E.Range))
                    {
                        var rDamage = RDamage(target);
                        if (E.Ready)
                        {
                            var eDamage = EDamage(target);
                            if (eDamage > target.Health) return;
                            rDamage += eDamage;
                        }
                        if (target.IsValidTarget(800)) rDamage += BonusDmg(target);
                        if (rDamage > target.Health)
                        {
                            R.CastIfWillHit(target, 1);
                            R.Cast(target);
                        }
                    }
                }             
            }

            if (R.Ready && (Main["jungSteal"]["keyJR"].As<MenuKeyBind>().Enabled || Main["jungSteal"]["autoJ"].As<MenuBool>().Enabled) )
            {        
                foreach (var jungSteal in GameObjects.Jungle.Where(x => x.IsValidTarget(R.Range)))
                {
                    if ((jungSteal.UnitSkinName.StartsWith("SRU_Dragon") || jungSteal.UnitSkinName.StartsWith("SRU_Baron") ||
                        jungSteal.UnitSkinName.StartsWith("SRU_RiftHerald") || jungSteal.UnitSkinName.StartsWith("SRU_Blue") ||
                        jungSteal.UnitSkinName.StartsWith("SRU_Red")) && (jungSteal.CountAllyHeroesInRange(1000) == 0 || Main["jungSteal"]["allyJ"].As<MenuBool>().Enabled) && jungSteal.Health < jungSteal.MaxHealth && jungSteal.Distance(Player.Position) > 750)
                    {
                        if (Math.Abs(_jDmg) <= 0)
                            _jDmg = jungSteal.Health;

                        if (Game.ClockTime - _jTime > 3)
                        {
                            if (_jDmg - jungSteal.Health > 0)
                            {
                                _jDmg = jungSteal.Health;
                            }
                            _jTime = Game.ClockTime;
                        }
                        else
                        {
                            var damageS = (_jDmg - jungSteal.Health) * (Math.Abs(_jTime - Game.ClockTime) / 3);
                            if (_jDmg - jungSteal.Health > 0)
                            {
                                var timeTravel = R.Delay;
                                var timeR = (jungSteal.Health - RDamage(jungSteal)) / (damageS / 3);
                                if (timeTravel > timeR)
                                {
                                    R.Cast(jungSteal.Position);
                                }
                            }
                            else
                            {
                                _jDmg = jungSteal.Health;
                            }

                        }
                    }
                }
            }
            if (Main["wProtect"]["enabled"].As<MenuBool>().Enabled)
            {
                AutoProtect();
            }
        }

        private static void Obj_AI_Base_OnProcessSpellCast(GameObject sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender.IsMe && args.SpellData.Name == "LuxLightStrikeKugel")
            {
                _eposition = args.End;
            }
        }
        private static float BonusDmg(Obj_AI_Base target)
        {
            var damage = 10 + (Player.Level) * 8 + 0.2f * Player.FlatMagicDamageMod;
            if (Player.HasBuff("lichbane"))
            {
                damage += (Player.BaseAttackDamage * 0.75f) + ((Player.BaseAbilityDamage + Player.FlatMagicDamageMod) * 0.5f);
            }
            return (float)(Player.GetAutoAttackDamage(target) + Player.CalculateDamage(target, DamageType.Magical, damage));
        }
        private static float QDamage(Obj_AI_Base d)
        {
            return (float)Player.GetSpellDamage(d, SpellSlot.Q);
        }
        private static float WDamage()
        {
            return 0;
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
            if (Main["combo"]["q"].As<MenuBool>().Enabled && Q.Ready)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && !Check(enemy, DamageType.Magical) && EDamage(enemy) + QDamage(enemy) + BonusDmg(enemy) > enemy.Health))
                {
                        CastQ(enemy); return;
                }

                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target == null) return;
 
                if (target.IsValidTarget() && !Check(target, DamageType.Magical) && (Main["whiteList"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled || target.Health < QDamage(target)))
                {
                    CastQ(target);                
                }
            }

            if (Main["combo"]["e"].As<MenuBool>().Enabled && E.Ready)
            {                          
                    var target = GetBestEnemyHeroTargetInRange(E.Range);
                    if (target == null) return;
                    var ePosition = _eposition.CountEnemyHeroesInRange(350f);
                    var epred = E.GetPrediction(target);
                    if (target.IsValidTarget() && GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(E.Width, false, false, E.GetPrediction(target).CastPosition)) >= Main["combo"]["UnitsEhit"].As<MenuSlider>().Value )
                    {
                        if (epred.HitChance >= HitChance.VeryHigh)
                        {
                            if (Main["combo"]["eSD"].As<MenuBool>().Enabled && ePosition == 0)
                            {
                                 E.Cast(epred.CastPosition);
                            }
                            if (!Main["combo"]["eSD"].As<MenuBool>().Enabled)
                            {
                                 E.Cast(epred.CastPosition);
                            }
                        }
                    }                              
            }

            if (Main["combo"]["r"].As<MenuBool>().Enabled && R.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(R.Range);
                if (target == null) return;
                var rpred = R.GetPrediction(target);
                if ((target.IsValidTarget(R.Range) && Player.Distance(target) >= 250 && target.Health < RDamage(target) && 
                    !Check(target, DamageType.Magical) && !Main["combo"]["UnitsRhit"].As<MenuSliderBool>().Enabled) ||
                    (target.IsValidTarget(R.Range) && Player.Distance(target) >= 250 && !Check(target, DamageType.Magical) && Main["combo"]["UnitsRhit"].As<MenuSliderBool>().Enabled))
                {
                    if (rpred.HitChance >= HitChance.VeryHigh)
                    {                                         
                        if (!Main["combo"]["UnitsRhit"].As<MenuSliderBool>().Enabled)
                        {
                            R.Cast(target);
                        }
                        if (Main["combo"]["UnitsRhit"].As<MenuSliderBool>().Enabled)
                        {
                            R.CastIfWillHit(target, Main["combo"]["UnitsRhit"].As<MenuSliderBool>().Value - 1);
                        }                       
                    }
                }
            }

        }

        private static void Harass()
        {
            if (Main["harass"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["q"].As<MenuSliderBool>().Value && Q.Ready)
            {
                foreach (var enemy in GameObjects.EnemyHeroes.Where(enemy => enemy.IsValidTarget(Q.Range) && !Check(enemy, DamageType.Magical) && EDamage(enemy) + QDamage(enemy) + BonusDmg(enemy) > enemy.Health))
                {
                    CastQ(enemy); return;
                }

                var target = GetBestEnemyHeroTargetInRange(Q.Range);
                if (target == null) return;

                if (target.IsValidTarget() && !Check(target, DamageType.Magical) && (Main["whiteList"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled || target.Health < QDamage(target)))
                {
                    CastQ(target);
                }
            }

            if (Main["harass"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["harass"]["e"].As<MenuSliderBool>().Value && E.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target == null) return;
                var ePosition = _eposition.CountEnemyHeroesInRange(350f);
                var epred = E.GetPrediction(target);
                if (target.IsValidTarget() && GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(E.Width + 50, false, false, E.GetPrediction(target).CastPosition)) >= Main["harass"]["UnitsEhit"].As<MenuSlider>().Value)
                {
                    if (epred.HitChance >= HitChance.VeryHigh)
                    {
                        if (Main["combo"]["eSD"].As<MenuBool>().Enabled && ePosition == 0)
                        {
                            E.Cast(epred.CastPosition);
                        }
                        if (!Main["combo"]["eSD"].As<MenuBool>().Enabled)
                        {
                            E.Cast(epred.CastPosition);
                        }
                    }
                }
            }
        }

        private static void AutoProtect()
        {
            foreach (var ally in GameObjects.AllyHeroes.Where(ally => ally.IsValid && !ally.IsDead && Player.ServerPosition.Distance(ally.ServerPosition) < W.Range))
            {
                if (W.Ready && Main["wProtect"]["wccAlly" + ally.ChampionName.ToLower()].As<MenuBool>().Enabled)
                {
                    if (ally.IsImmobile())
                    {
                       W.CastOnUnit(ally);
                    }                   
                }
                if (W.Ready && Main["wProtect"]["wpAlly" + ally.ChampionName.ToLower()].As<MenuBool>().Enabled && !Player.IsRecalling())
                {
                    if (ally.HasBuffOfType(BuffType.Poison))
                    {
                        W.Cast(ally.ServerPosition);
                    }            
                }
                if (Main["wProtect"]["autoW"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["wProtect"]["autoW"].As<MenuSliderBool>().Value && W.Ready && Main["wProtect"]["allyW" + ally.ChampionName.ToLower()].As<MenuSliderBool>().Enabled && ally.HealthPercent() <= Main["wProtect"]["allyW" + ally.ChampionName.ToLower()].As<MenuSliderBool>().Value && ally.CountEnemyHeroesInRange(750) >= 1)
                {
                    if (ally.IsInRange(W.Range) && !Player.IsRecalling() && !ally.IsMe)
                    {
                        W.Cast(ally.ServerPosition);
                    }
                    if (ally.IsMe && !Player.IsRecalling())
                    {
                        foreach (var isme in GameObjects.AllyHeroes.Where(isme => isme.IsValid && !isme.IsDead && Player.ServerPosition.Distance(isme.ServerPosition) < W.Range && !isme.IsMe))
                        {
                            W.Cast(isme.ServerPosition);
                        }
                        if (Player.CountAllyHeroesInRange(750) == 0)
                        {
                            W.Cast(Player.ServerPosition);
                        }
                    }              
                }
            }
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value && Q.Ready)
            {
                foreach (var minion in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(Q.Range)).ToList())
                {
                    if (minion != null)
                    {
                        var colQ = Q.GetPrediction(minion);
                        var collision = Collision.GetCollision(new List<Vector3> { minion.ServerPosition }, Q.GetPredictionInput(minion));
                        var col = collision.Count(x => x.IsMinion);
                        if (col >= 2 && colQ.HitChance >= HitChance.High)
                        {
                            Q.Cast(minion);
                        }
                    }
                }
            }
            if (Main["laneclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value && E.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(E.Range)))
                {
                    if (target != null)
                    {
                        if (GameObjects.EnemyMinions.Count(x => x.IsValidTarget(320f, false, false, E.GetPrediction(target).CastPosition)) >= Main["laneclear"]["UnitsEhit"].As<MenuSlider>().Value)
                        {
                            E.Cast(E.GetPrediction(target).CastPosition);
                        }
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
                    Q.Cast(target);
                }
                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready)
                {
                    if (Player.CountAllyHeroesInRange(W.Range) >= 1)
                    {
                        var allyHero = GameObjects.AllyHeroes.Where(x => x.Distance(Player) <= W.Range + 550 && !x.IsMe)
                            .OrderBy(x => Player.Distance(x.Position) < 900).FirstOrDefault();
                        if (allyHero != null)
                        {
                            if (allyHero.Distance(target) < 350)
                            {
                                W.Cast(allyHero.ServerPosition);
                            }
                            else
                            {
                                if (Player.Distance(target) < 350)
                                {
                                    W.Cast(allyHero.ServerPosition);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Player.Distance(target) < 350)
                        {
                            W.Cast(Player.ServerPosition);
                        }                       
                    }                  
                }
                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready)
                {
                    E.Cast(target.Position);
                }
            }
        }
        private static void CastQ(Obj_AI_Base t)
        {
            var colQ = Q.GetPrediction(t);
            var collision = Collision.GetCollision(new List<Vector3> {t.ServerPosition}, Q.GetPredictionInput(t));
            var col = collision.Count(x => x.IsEnemy && x.IsMinion);
            if (col <= 1 && colQ.HitChance >= HitChance.VeryHigh)
            {
                Q.Cast(t);
            }              
        }

        private static void AntiGapcloser(Obj_AI_Hero target, GapcloserArgs args)
        {
            if (target != null && Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && target.IsValidTarget(E.Range))
            {
                E.Cast(args.EndPosition);            
            }
            if (target != null && Main["antiGapcloser"]["q"].As<MenuBool>().Enabled && Q.Ready && target.IsValidTarget(Q.Range))
            {
                var rPred = Q.GetPrediction(target);
                Q.Cast(rPred.UnitPosition);
            }
            if (target != null && Main["antiGapcloser"]["w2"].As<MenuBool>().Enabled && W.Ready &&
                args.EndPosition.Distance(Player) < W.Range)
            {
                var isme2 = GameObjects.AllyHeroes.Where(ally => ally.Distance(Player) <= W.Range)
                    .OrderBy(ally => ally.Distance(args.EndPosition)).FirstOrDefault();
                if (isme2 != null && isme2.IsMe)
                {
                    foreach (var isme in GameObjects.AllyHeroes.Where(isme => isme.IsValid && !isme.IsDead && Player.ServerPosition.Distance(isme.ServerPosition) < W.Range && !isme.IsMe))
                    {
                        W.Cast(isme.ServerPosition);
                    }
                    if (Player.CountAllyHeroesInRange(W.Range) == 0)
                    {
                        W.Cast(Player.ServerPosition);
                    }
                }             
            }
            if (target != null && Main["antiGapcloser"]["w"].As<MenuBool>().Enabled && W.Ready && args.EndPosition.Distance(Player) < W.Range)
            {
                var allyHero = GameObjects.AllyHeroes.Where(ally => ally.Distance(Player) <= W.Range && !ally.IsMe)
                    .OrderBy(ally => ally.Distance(args.EndPosition)).FirstOrDefault();
                if (allyHero != null)
                {
                    W.Cast(allyHero.Position);
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
                    wdmgDraw = WDamage();
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
