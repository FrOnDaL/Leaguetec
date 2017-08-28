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
    internal class Thresh
    {
        public Thresh()
        {
            Q = new Spell(SpellSlot.Q, 1070);
            W = new Spell(SpellSlot.W, 950);
            E = new Spell(SpellSlot.E, 500);
            E2 = new Spell(SpellSlot.E, 500);
            R = new Spell(SpellSlot.R, 450);

            Q.SetSkillshot(0.5f, 70, 1900f, true, SkillshotType.Line);
            W.SetSkillshot(0.2f, 50f, float.MaxValue, false, SkillshotType.Circle);
            E2.SetSkillshot(0f, 50, float.MaxValue, false, SkillshotType.Line);

            Main = new Menu("Index", "FrOnDaL AIO", true);
            Misc.Orbwalker.Attach(Main);
            var thresh = new Menu("thresh", "Thresh");
            {
                var combo = new Menu("combo", "Combo");
                {
                    combo.Add(new MenuBool("q", "Use combo Q"));
                    combo.Add(new MenuBool("qCC", "Auto Q on CC"));
                    combo.Add(new MenuBool("qDash", "Auto Q dash"));
                    combo.Add(new MenuBool("q2", "Use combo Q2"));
                    combo.Add(new MenuBool("q2Turret", "Use Q2 Under Enemy Turret", false));
                    combo.Add(new MenuSlider("QMinimumRange", "Q minimum range to cast", 250, 125, 600));
                    combo.Add(new MenuSlider("QMaximumRange", "Q Maximum range to cast", 1070, 600, 1070));
                    var whiteList = new Menu("whiteList", "Q white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteList.Add(new MenuBool("qWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                        }
                    }
                    combo.Add(whiteList);
                    combo.Add(new MenuBool("w", "Use combo W"));
                    combo.Add(new MenuBool("e", "Use combo E"));
                    combo.Add(new MenuList("ePP", "E Push/Pull", new[] {"Push", "Pull"}, 1));
                    combo.Add(new MenuSlider("mPull", "Minimum pull range E", 250, 0, 400));
                    combo.Add(new MenuSliderBool("r", "Use Combo R - Minimum enemies for R", true, 3, 1, 5));
                    combo.Add(new MenuBool("rKs", "Auto R kill-steal", false));
                    combo.Add(new MenuBool("disableAA", "Disable AutoAttacks", false));
                }
                thresh.Add(combo);

                var harass = new Menu("harass", "Harass")
                {
                    new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                    new MenuBool("autoHarass", "Auto Harass", false),
                    new MenuSliderBool("q", "Auto Use Q / if Mana >= x%", true, 50, 0, 99),
                    new MenuSliderBool("e", "Auto Use E / if Mana >= x%", false, 50, 0, 99),
                    new MenuSliderBool("EqHarass", "E after Use Q (E and Q If ready. Key press C)", false, 50, 0, 99)
                };
                thresh.Add(harass);

                var wProtect = new Menu("wProtect", "Auto W Protect");
                {
                    wProtect.Add(new MenuBool("enabled", "Enabled"));
                    wProtect.Add(new MenuSliderBool("autoW", "Auto W Protect / if Mana >= x%", false, 30, 0, 99)); 
                    wProtect.Add(new MenuBool("wCC", "Auto W - Slows/Stuns")); 
                    wProtect.Add(new MenuBool("wBc", "Auto W - Blitz Hook")); 
                    wProtect.Add(new MenuSliderBool("Wnear", "Auto W if x enemies near ally",true, 3, 1, 5)); 
                    var whiteList = new Menu("helSet", "Ally health setting"); 
                    {
                        foreach (var ally in GameObjects.AllyHeroes)
                        {
                            whiteList.Add(new MenuSliderBool("allyW" + ally.ChampionName.ToLower(), ally.ChampionName, true, 50, 0, 101));
                        }
                    }
                    wProtect.Add(whiteList);
                }
                thresh.Add(wProtect);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", false, 60, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", false, 60, 0, 99),
                    new MenuSlider("UnitsEhit", "E Hit x units minions >= x%", 2, 1, 4)
                };
                thresh.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if mana >= x%", false, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if mana >= x%", false, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if mana >= x%", false, 30, 0, 99)
                };
                thresh.Add(jungleclear);

                var antiGapcloser = new Menu("antiGapcloser", "Thresh anti-gapcloser spells")
                {
                    new MenuBool("w", "Anti-gapcloser W ally"),
                    new MenuBool("w2", "Anti-gapcloser W thresh"),
                    new MenuBool("e", "Anti-gapcloser E")
                };
                thresh.Add(antiGapcloser);
                Gapcloser.Attach(thresh, "Anti-gapcloser settings");
                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("q", "Draw Q"));
                    drawings.Add(new MenuBool("w", "Draw W", false));
                    drawings.Add(new MenuBool("e", "Draw E", false));
                    drawings.Add(new MenuBool("r", "Draw R", false));
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
                thresh.Add(drawings);
            }
            Main.Add(thresh);
            Main.Attach();
            Render.OnPresent += SpellDraw;
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Misc.Orbwalker.PreAttack += OnPreAttack;
            Gapcloser.OnGapcloser += AntiGapcloser;
        }
        private static bool IsQActive => Player.SpellBook.GetSpell(SpellSlot.Q).SpellData.Name == "threshQ";
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
                case OrbwalkingMode.Mixed:
                    Harass();
                    break;
                case OrbwalkingMode.Laneclear:
                    LaneClear();
                    JungleClear();
                    break;
            }
            if(Main["wProtect"]["enabled"].As<MenuBool>().Enabled)
            {
                AutoProtect();
            }
            if (Main["combo"]["rKs"].As<MenuBool>().Enabled && R.Ready)
            {
                var target = TargetSelector.GetTarget(R.Range - 100);
                if (target != null && target.Health <= RDamage(target) && Player.Distance(target) < 300)
                {
                    R.Cast();
                }
            }
            if (Main["harass"]["autoHarass"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Combo && Misc.Orbwalker.Mode != OrbwalkingMode.Mixed)
            {              
                    if (Main["harass"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["harass"]["q"].As<MenuSliderBool>().Value && Q.Ready)
                    {
                        var target = GetBestEnemyHeroTargetInRange(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value);
                        if (target != null)
                        {
                        var pred = Q.GetPrediction(target);

                            if (Main["whiteList"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled &&
                                target.IsValidTarget(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value) &&
                                !target.HasBuff("threshQ") && Player.Distance(target.ServerPosition) > Main["combo"]["QMinimumRange"].As<MenuSlider>().Value && !Check(target, DamageType.Magical))
                            {
                                if (!target.HasBuffOfType(BuffType.SpellImmunity) && !target.HasBuffOfType(BuffType.SpellShield) && GameObjects.EnemyMinions.Count(x => Player.Distance(x.Position) <= 200) == 0)
                                {
                                    if (pred.HitChance >= HitChance.High && pred.CollisionObjects.Count == 0)
                                    {
                                        Q.Cast(pred.CastPosition);
                                    }
                                }
                            }
                            if (target.HasBuff("threshQ") && Main["combo"]["q2"].As<MenuBool>().Enabled && !target.IsUnderEnemyTurret())
                            {
                                if (target.Distance(Player.ServerPosition) >= 400)
                                {
                                    DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                                }
                            }
                            if (target.HasBuff("threshQ") && target.IsUnderEnemyTurret() && Main["combo"]["q2Turret"].As<MenuBool>().Enabled)
                            {
                                if (target.Distance(Player.ServerPosition) >= 400)
                                {
                                    DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                                }
                            }
                            if (W.Ready)
                            {
                                var ally = GameObjects.AllyHeroes.Where(x => x.IsInRange(1700) && !x.IsDead && x.IsAlly && !x.IsMe).FirstOrDefault(x => x.Distance(Player.Position) <= 1700);
                                if (ally != null)
                                {
                                    if (ally.Distance(Player.ServerPosition) <= 700) return;
                                    if (target.HasBuff("threshQ"))
                                    {
                                        W.Cast(ally.ServerPosition);
                                    }
                                }
                            }
                    }                       
                    }
                    if (Main["harass"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["harass"]["e"].As<MenuSliderBool>().Value && E.Ready)
                    {
                        var target = GetBestEnemyHeroTargetInRange(E.Range);
                        if (target == null) return;
                        switch (Main["combo"]["ePP"].As<MenuList>().Value)
                        {
                            case 0:
                                if (target.IsInRange(E.Range) && !IsQActive)
                                {
                                    E.Cast(target.Position);
                                }
                                break;
                            case 1:
                                if (target.IsInRange(E.Range) && Player.Distance(target) > Main["combo"]["mPull"].As<MenuSlider>().Value && !IsQActive)
                                {
                                    E.Cast(target.Position.Extend(Player.ServerPosition, Vector3.Distance(target.Position, Player.Position) + 400));
                                }
                                break;
                        }
                    }               
            }

            if (Main["combo"]["qCC"].As<MenuBool>().Enabled || Main["combo"]["qDash"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value);
                if (target == null) return;
                var pred = Q.GetPrediction(target);
                if (Main["whiteList"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled &&
                    target.IsValidTarget(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value) && !target.HasBuff("threshQ") &&
                    Q.Ready && Player.Distance(target.ServerPosition) > Main["combo"]["QMinimumRange"].As<MenuSlider>().Value && !Check(target, DamageType.Magical))
                {
                    if (!target.HasBuffOfType(BuffType.SpellImmunity) && !target.HasBuffOfType(BuffType.SpellShield) && GameObjects.EnemyMinions.Count(x => Player.Distance(x.Position) <= 200) == 0)
                    {
                        if (Main["combo"]["qCC"].As<MenuBool>().Enabled && pred.HitChance == HitChance.Immobile && pred.CollisionObjects.Count == 0 && target.IsImmobile())
                        {
                            Q.Cast(pred.CastPosition);
                        }
                        if (Main["combo"]["qDash"].As<MenuBool>().Enabled && pred.HitChance == HitChance.Dashing && pred.CollisionObjects.Count == 0)
                        {
                            Q.Cast(pred.CastPosition);
                        }
                    }
                }
                if (target.HasBuff("threshQ") && Main["combo"]["q2"].As<MenuBool>().Enabled && !target.IsUnderEnemyTurret())
                {
                    if (target.Distance(Player.ServerPosition) >= 400)
                    {
                        DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                    }
                }
                if (target.HasBuff("threshQ") && target.IsUnderEnemyTurret() && Main["combo"]["q2Turret"].As<MenuBool>().Enabled)
                {
                    if (target.Distance(Player.ServerPosition) >= 400)
                    {
                        DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                    }
                }
            }
        }

        private static void Combo()
        {
            if (Main["combo"]["q"].As<MenuBool>().Enabled || Main["combo"]["w"].As<MenuBool>().Enabled)
            {
                var target = GetBestEnemyHeroTargetInRange(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value);
                if (target == null) return;
                var pred = Q.GetPrediction(target);

                if (Main["combo"]["q"].As<MenuBool>().Enabled &&
                    Main["whiteList"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled &&
                    target.IsValidTarget(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value) &&
                    !target.HasBuff("threshQ") &&
                    Q.Ready && Player.Distance(target.ServerPosition) > Main["combo"]["QMinimumRange"].As<MenuSlider>().Value && !Check(target, DamageType.Magical))
                {
                    if (!target.HasBuffOfType(BuffType.SpellImmunity) && !target.HasBuffOfType(BuffType.SpellShield) && GameObjects.EnemyMinions.Count(x => Player.Distance(x.Position) <= 200) == 0)
                    {
                        if (pred.HitChance == HitChance.High && pred.CollisionObjects.Count == 0)
                        {
                            Q.Cast(pred.CastPosition);
                        }
                    }
                }
                if (target.HasBuff("threshQ") && Main["combo"]["q2"].As<MenuBool>().Enabled && !target.IsUnderEnemyTurret())
                {
                    if (target.Distance(Player.ServerPosition) >= 400)
                    {
                        DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                    }
                }
                if (target.HasBuff("threshQ") && target.IsUnderEnemyTurret() && Main["combo"]["q2Turret"].As<MenuBool>().Enabled)
                {
                    if (target.Distance(Player.ServerPosition) >= 400)
                    {
                        DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                    }
                }

                if (Main["combo"]["w"].As<MenuBool>().Enabled && W.Ready)
                {
                    var ally = GameObjects.AllyHeroes.Where(x => x.IsInRange(1700) && !x.IsDead && x.IsAlly && !x.IsMe).FirstOrDefault(x => x.Distance(Player.Position) <= 1700);
                    if (ally != null)
                    {
                        if (ally.Distance(Player.ServerPosition) <= 700) return;
                        if (target.HasBuff("threshQ"))
                        {
                            W.Cast(ally.ServerPosition);
                        }
                    }
                }
            }
            if (Main["combo"]["e"].As<MenuBool>().Enabled && E.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(E.Range);
                if (target == null) return;
                switch (Main["combo"]["ePP"].As<MenuList>().Value)
                {
                    case 0:
                        if (target.IsInRange(E.Range) && !IsQActive)
                        {
                            E.Cast(target.Position);
                        }
                        break;
                    case 1:
                        if (target.IsInRange(E.Range) && Player.Distance(target) > Main["combo"]["mPull"].As<MenuSlider>().Value && !IsQActive)
                        {
                            E.Cast(target.Position.Extend(Player.ServerPosition, Vector3.Distance(target.Position, Player.Position) + 400));
                        }
                        break;
                }
            }

            if (Main["combo"]["r"].As<MenuSliderBool>().Enabled && R.Ready)
            {
                if (Player.CountEnemyHeroesInRange(R.Range) < Player.CountEnemyHeroesInRange(200)) return;
                if (Player.CountEnemyHeroesInRange(R.Range) >= Main["combo"]["r"].As<MenuSliderBool>().Value && Main["combo"]["r"].As<MenuSliderBool>().Value > 0)
                {
                    R.Cast();
                }

            }
        }

        private static void Harass()
        {
            if (!Main["harass"]["EqHarass"].As<MenuSliderBool>().Enabled)
            {
                if (Main["harass"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["harass"]["q"].As<MenuSliderBool>().Value && Q.Ready)
                {
                    var target = GetBestEnemyHeroTargetInRange(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value);
                    if (target == null) return;
                    var pred = Q.GetPrediction(target);

                    if (Main["whiteList"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled &&
                        target.IsValidTarget(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value) &&
                        !target.HasBuff("threshQ") && Player.Distance(target.ServerPosition) > Main["combo"]["QMinimumRange"].As<MenuSlider>().Value && !Check(target, DamageType.Magical))
                    {
                        if (!target.HasBuffOfType(BuffType.SpellImmunity) && !target.HasBuffOfType(BuffType.SpellShield) && GameObjects.EnemyMinions.Count(x => Player.Distance(x.Position) <= 200) == 0)
                        {
                            if (pred.HitChance >= HitChance.High && pred.CollisionObjects.Count == 0)
                            {
                                Q.Cast(pred.CastPosition);
                            }
                        }
                    }
                    if (target.HasBuff("threshQ") && Main["combo"]["q2"].As<MenuBool>().Enabled && !target.IsUnderEnemyTurret())
                    {
                        if (target.Distance(Player.ServerPosition) >= 400)
                        {
                            DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                        }
                    }
                    if (target.HasBuff("threshQ") && target.IsUnderEnemyTurret() && Main["combo"]["q2Turret"].As<MenuBool>().Enabled)
                    {
                        if (target.Distance(Player.ServerPosition) >= 400)
                        {
                            DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                        }
                    }
                    if (W.Ready)
                    {
                        var ally = GameObjects.AllyHeroes.Where(x => x.IsInRange(1700) && !x.IsDead && x.IsAlly && !x.IsMe).FirstOrDefault(x => x.Distance(Player.Position) <= 1700);
                        if (ally != null)
                        {
                            if (ally.Distance(Player.ServerPosition) <= 700) return;
                            if (target.HasBuff("threshQ"))
                            {
                                W.Cast(ally.ServerPosition);
                            }
                        }
                    }
                }
                if (Main["harass"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["harass"]["e"].As<MenuSliderBool>().Value && E.Ready)
                {
                    var target = GetBestEnemyHeroTargetInRange(E.Range);
                    if (target == null) return;
                    switch (Main["combo"]["ePP"].As<MenuList>().Value)
                    {
                        case 0:
                            if (target.IsInRange(E.Range) && !IsQActive)
                            {
                                E.Cast(target.Position);
                            }
                            break;
                        case 1:
                            if (target.IsInRange(E.Range) && Player.Distance(target) > Main["combo"]["mPull"].As<MenuSlider>().Value && !IsQActive)
                            {
                                E.Cast(target.Position.Extend(Player.ServerPosition, Vector3.Distance(target.Position, Player.Position) + 400));
                            }
                            break;
                    }
                }
            }

            if (Main["harass"]["EqHarass"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["harass"]["EqHarass"].As<MenuSliderBool>().Value)
            {
                var target = TargetSelector.GetTarget(Main["combo"]["QMaximumRange"].As<MenuSlider>().Value);
                if (target == null) return;
                if (target.IsInRange(E.Range) && !target.HasBuff("threshQ") && E.Ready)
                {
                    E.Cast(target.Position.Extend(Player.ServerPosition, Vector3.Distance(target.Position, Player.Position) + 400));
                }
                var prediction = Q.GetPrediction(target);

                if (Main["whiteList"]["qWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled && target.IsInRange(Q.Range) && target.IsValidTarget() && !target.HasBuff("threshQ") && Q.Ready && target.Distance(Player) > E.Range && !E.Ready && !Check(target, DamageType.Magical))
                {
                    if (prediction.HitChance >= HitChance.High)
                    {
                        Q.Cast(prediction.CastPosition);
                    }
                }
                if (target.HasBuff("threshQ") && Main["combo"]["q2"].As<MenuBool>().Enabled && !target.IsUnderEnemyTurret())
                {
                    if (target.Distance(Player.ServerPosition) >= 400)
                    {
                        DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                    }
                }
                if (target.HasBuff("threshQ") && target.IsUnderEnemyTurret() && Main["combo"]["q2Turret"].As<MenuBool>().Enabled)
                {
                    if (target.Distance(Player.ServerPosition) >= 400)
                    {
                        DelayAction.Queue(1000, () => Q.CastOnUnit(Player));
                    }
                }

                if (W.Ready)
                {
                    var ally = GameObjects.AllyHeroes.Where(x => x.IsInRange(1700) && !x.IsDead && x.IsAlly && !x.IsMe).FirstOrDefault(x => x.Distance(Player.Position) <= 1700);
                    if (ally != null)
                    {
                        if (ally.Distance(Player.ServerPosition) <= 700) return;
                        if (target.HasBuff("threshQ"))
                        {
                            W.Cast(ally.ServerPosition);
                        }
                    }
                }
            }
        }
        private static void AutoProtect()
        {
            if (Main["wProtect"]["wBc"].As<MenuBool>().Enabled)
            {
                var saveAlly = GameObjects.AllyHeroes.FirstOrDefault(ally => ally.HasBuff("rocketgrab2") && !ally.IsMe);
                if (saveAlly != null)
                {
                    var blitz = saveAlly.GetBuff("rocketgrab2").Caster;
                    if (Player.Distance(blitz.Position) <= W.Range + 550 && W.Ready)
                    {
                        CastW(blitz.Position);
                    }
                }
            }

            foreach (var ally in GameObjects.AllyHeroes.Where(ally => ally.IsValid && !ally.IsDead && Player.Distance(ally) < W.Range + 400))
            {
                if (Main["wProtect"]["wCC"].As<MenuBool>().Enabled && !ally.IsMe && W.Ready)
                {
                    if (ally.Distance(Player) <= W.Range)
                    {
                        if (ally.HasBuffOfType(BuffType.Stun) || ally.HasBuffOfType(BuffType.Slow))
                        {
                            W.Cast(ally.Position);
                        }
                    }
                }

                var nearEnemys = ally.CountEnemyHeroesInRange(900);
                if (Main["wProtect"]["Wnear"].As<MenuSliderBool>().Enabled && nearEnemys >= Main["wProtect"]["Wnear"].As<MenuSliderBool>().Value && Main["wProtect"]["Wnear"].As<MenuSliderBool>().Value > 0 && W.Ready)
                {
                    CastW(W.GetPrediction(ally).CastPosition);                
                }

                if (Main["wProtect"]["autoW"].As<MenuSliderBool>().Enabled && Player.Distance(ally) < W.Range + 100 && Player.ManaPercent() >= Main["wProtect"]["autoW"].As<MenuSliderBool>().Value && W.Ready)
                {
                    if (!Player.IsRecalling() && Main["wProtect"]["allyW" + ally.ChampionName.ToLower()].As<MenuSliderBool>().Enabled && ally.HealthPercent() <= Main["wProtect"]["allyW" + ally.ChampionName.ToLower()].Value && ally.CountEnemyHeroesInRange(700) >= 1)
                    {
                        W.Cast(ally.Position);
                    }
                    if (ally.IsMe && Player.CountAllyHeroesInRange(1400) == 0 && !Player.IsRecalling() && Main["wProtect"]["allyW" + ally.ChampionName.ToLower()].As<MenuSliderBool>().Enabled && ally.HealthPercent() <= Main["wProtect"]["allyW" + ally.ChampionName.ToLower()].Value && ally.CountEnemyHeroesInRange(700) >= 1)
                    {
                        W.Cast(ally.Position);
                    }
                }
            }
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Q.Ready && Player.ManaPercent() > Main["laneclear"]["q"].As<MenuSliderBool>().Value)
            {
                var result = Polygon.GetLinearLocation(Q.Range, Q.Width);
                if (result == null) return;
                if (result.NumberOfMinionsHit >= 1)
                {
                    Q.Cast(result.CastPosition);
                }
            }

            if (Main["laneclear"]["e"].As<MenuSliderBool>().Enabled && E.Ready && Player.ManaPercent() > Main["laneclear"]["e"].As<MenuSliderBool>().Value)
            {
                var result = Polygon.GetLinearLocation(E.Range, 100);
                if (result == null) return;
                if (result.NumberOfMinionsHit >= Main["laneclear"]["UnitsEhit"].As<MenuSlider>().Value)
                {
                    E2.Cast(result.CastPosition);
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var target in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready)
                {
                    Q.Cast(target.Position);
                }

                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready && Player.Distance(target) < 350)
                {
                    W.Cast(Player.Position);
                }

                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready && target.IsValidTarget(E.Range - 50))
                {
                    E2.Cast(target.Position);
                }

            }
        }

        private static void AntiGapcloser(Obj_AI_Hero target, GapcloserArgs args)
        {
            if (target == null) return;

            switch (args.Type)
            {
                case SpellType.Dash:
                    if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && args.EndPosition.DistanceToPlayer() <= 350 && target.IsValidTarget(E.Range))
                    {
                        var ePred = E.GetPrediction(target);
                        E.Cast(ePred.UnitPosition);
                    }
                    break;
                case SpellType.SkillShot:
                {
                    if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && target.IsValidTarget(E.Range))
                    {
                        E.Cast(target.Position);
                    }
                    if (Main["antiGapcloser"]["w2"].As<MenuBool>().Enabled && W.Ready && target.IsValidTarget(W.Range) && Player.CountEnemyHeroesInRange(250) >= 1)
                    {
                        W.Cast(Player.Position);
                    }
                    }
                    break;
                case SpellType.Targeted:
                {
                    if (Main["antiGapcloser"]["w2"].As<MenuBool>().Enabled && W.Ready && target.IsValidTarget(300))
                    {
                        W.Cast(Player.Position);
                    }
                    if (Main["antiGapcloser"]["w"].As<MenuBool>().Enabled && W.Ready && target.IsValidTarget(W.Range + 550))
                    {
                        var allyHero = GameObjects.AllyHeroes.Where(ally => ally.Distance(Player) <= W.Range + 550 && !ally.IsMe)
                            .OrderBy(ally => ally.Distance(args.EndPosition)).FirstOrDefault();
                        if (allyHero != null)
                        {
                            CastW(allyHero.Position);
                        }
                    }
                        if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready && Game.TickCount > 2500 && target.IsValidTarget(E.Range))
                    {
                        var ePred = E.GetPrediction(target);

                        E.Cast(ePred.UnitPosition);
                    }
                }
                    break;
            }
        }
        private static void CastW(Vector3 pos)
        {
            W.Cast(Player.Distance(pos) < W.Range ? pos : Player.Position.Extend(pos, W.Range));
        }
        public static void OnPreAttack(object sender, PreAttackEventArgs args)
        {
            switch (Misc.Orbwalker.Mode)
            {

                case OrbwalkingMode.Mixed:
                    if (!E.Ready) return;
                    args.Cancel = true;
                    break;

                case OrbwalkingMode.Combo:
                case OrbwalkingMode.Lasthit:
                case OrbwalkingMode.Laneclear:
                    if (GameObjects.EnemyMinions.Contains(args.Target) && Main["combo"]["disableAA"].As<MenuBool>().Enabled)
                    {
                        args.Cancel = GameObjects.AllyHeroes.Any(a => !a.IsMe && a.Distance(Player) < 2500);
                    }
                    break;
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
