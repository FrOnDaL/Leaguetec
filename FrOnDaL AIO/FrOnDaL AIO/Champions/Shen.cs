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

namespace FrOnDaL_AIO.Champions
{
    internal class Shen
    {
        public Shen()
        {
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 600);
            R = new Spell(SpellSlot.R, 35000);
            E.SetSkillshot(0.20f, 100f, float.MaxValue, false, SkillshotType.Line);
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
            var shen = new Menu("shen", "Shen");
            {
                var combo = new Menu("combo", "Combo");
                {
                    combo.Add(new MenuBool("q", "Use combo Q"));
                    combo.Add(new MenuBool("w", "Use combo W"));
                    combo.Add(new MenuSliderBool("autoW", "Auto W Protect / if energy >= x% (AA)", true, 0, 0, 99));
                    combo.Add(new MenuBool("autoWS", "Use Auto W Protect / Shen sword in", false));
                    combo.Add(new MenuBool("e", "Use combo E"));
                    combo.Add(new MenuKeyBind("eF", "Use combo flash E key:", KeyCode.T, KeybindType.Press));
                    var whiteList2 = new Menu("ehiteList2", "Flash E settings");
                    {
                        whiteList2.Add(new MenuSliderBool("myHeal", "My health >= x%", true, 0, 0, 100));
                        whiteList2.Add(new MenuSeperator("ehiteList2.Seperator1"));
                        foreach (var enemies2 in GameObjects.EnemyHeroes)
                        {
                            whiteList2.Add(new MenuSliderBool("eWhiteList2" + enemies2.ChampionName.ToLower(), enemies2.ChampionName + " health <= x%", true, 50, 0, 101));
                        }
                    }
                    combo.Add(whiteList2);
                    var whiteList = new Menu("ehiteList", "E white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteList.Add(new MenuBool("eWhiteList" + enemies.ChampionName.ToLower(), enemies.ChampionName));
                        }
                    }
                    combo.Add(whiteList);
                }
                shen.Add(combo);

                var rProtect = new Menu("rProtect", "Ulti Protect");
                {
                    rProtect.Add(new MenuBool("autoR", "Auto R Protect"));
                    rProtect.Add(new MenuSlider("autoRmyHeal", "Auto R / if my health >= x%", 25, 1, 99));
                    rProtect.Add(new MenuSlider("autoREnemy", "Auto R / if Count Enemy Heroes In Minimum Range = 0", 800, 600, 1200));
                    rProtect.Add(new MenuKeyBind("r", "Semi-manual cast R key", KeyCode.R, KeybindType.Press));
                    rProtect.Add(new MenuSlider("allHeal", "Ally health percent for Ult", 25, 1, 101));
                    var rwhiteList = new Menu("rhiteList", "R white list");
                    {
                        foreach (var ally in GameObjects.AllyHeroes.Where(x => !x.IsMe))
                        {
                            rwhiteList.Add(new MenuBool("allyR" + ally.ChampionName.ToLower(), ally.ChampionName));
                        }
                    }
                    rProtect.Add(rwhiteList);

                }
                shen.Add(rProtect);

                var laneclear = new Menu("laneclear", "Lane Clear")
                {
                    new MenuSliderBool("q", "Use Q / if energy >= x%", true, 60, 0, 99)
                };
                shen.Add(laneclear);

                var jungleclear = new Menu("jungleclear", "Jungle Clear")
                {
                    new MenuSliderBool("q", "Use Q / if energy >= x%", true, 30, 0, 99),
                    new MenuSliderBool("w", "Use W / if energy >= x%", true, 30, 0, 99),
                    new MenuSliderBool("e", "Use E / if energy >= x%", true, 30, 0, 99)
                };
                shen.Add(jungleclear);

                //var antiGapcloser = new Menu("antiGapcloser", "Shen anti-gapcloser spells")
                //{
                //    new MenuBool("e", "Anti-gapcloser E (Game Cursor Position)")
                //};
                //shen.Add(antiGapcloser);
                //Gapcloser.Attach(shen, "Anti-gapcloser settings");
                var drawings = new Menu("drawings", "Drawings");
                {
                    drawings.Add(new MenuBool("e", "Draw E"));
                    drawings.Add(new MenuBool("fE", "Draw Flash + E"));
                    drawings.Add(new MenuBool("sW", "Draw Sword"));

                    var allyHealt = new Menu("allyHealt", "Ally health indicator");
                    {
                        allyHealt.Add(new MenuBool("enabled", "Enabled"));
                        allyHealt.Add(new MenuSlider("xpos", "X Position", 250, 0, 2000));
                        allyHealt.Add(new MenuSlider("ypos", "Y Position", 65, 0, 2000));
                        foreach (var ally in GameObjects.AllyHeroes.Where(x => !x.IsMe))
                        {
                            allyHealt.Add(new MenuBool("allyHb" + ally.ChampionName.ToLower(), ally.ChampionName));
                        }
                    }
                    drawings.Add(allyHealt);

                    var drawDamage = new Menu("drawDamage", "Use draw damage");
                    {
                        drawDamage.Add(new MenuBool("enabled", "Enabled"));
                        drawDamage.Add(new MenuBool("q", "Draw Q active damage (3-AA)"));
                        drawDamage.Add(new MenuBool("e", "Draw E damage"));
                    }
                    drawings.Add(drawDamage);
                }
                shen.Add(drawings);
            }
            Main.Add(shen);
            Main.Attach();
            Render.OnPresent += SpellDraw;
            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Render.OnPresent += AllyHealt;
            //Gapcloser.OnGapcloser += AntiGapcloser;
            Obj_AI_Base.OnProcessAutoAttack += AutoAttack;
        }

        private static void SpellDraw()
        {

            if (Main["drawings"]["e"].As<MenuBool>().Enabled && E.Ready)
            {
                Render.Circle(Player.Position, E.Range, 180, Color.Green);
            }
            if (Main["drawings"]["fE"].As<MenuBool>().Enabled && E.Ready && Flash.Ready)
            {
                Render.Circle(Player.Position, 1025, 180, Color.Green);
            }
            if (Main["drawings"]["sW"].As<MenuBool>().Enabled)
            {
                foreach (var sword in GameObjects.AllGameObjects)
                {
                    if (sword.Name == "ShenSpiritUnit" && sword.IsValid && !sword.IsDead && sword.Team == Player.Team)
                    {
                        Render.Circle(sword.ServerPosition, 350, 180, Color.Aquamarine);
                    }
                }
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
        private static int HudOffsetRight => Main["xpos"].As<MenuSlider>().Value;

        private static int HudOffsetTop => Main["ypos"].As<MenuSlider>().Value;

        private static void DrawRect(float x, float y, int width, float height, Color color)
        {
            for (var i = 0; i < height; i++)
            {
                Render.Line(x, y + i, x + width, y + i, color);
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
                case OrbwalkingMode.Laneclear:
                    LaneClear();
                    JungleClear();
                    break;
            }

            if (Main["combo"]["eF"].As<MenuKeyBind>().Enabled && E.Ready && Flash.Ready)
            {
                var target = GetBestEnemyHeroTargetInRange(975);
                if (target != null)
                {
                    var xpos = target.Position.Extend(target.ServerPosition, E.Range);
                    var predepos = E.GetPrediction(target).UnitPosition;
                    {
                        if (target.IsValidTarget(975) && Player.Distance(target.ServerPosition) < 975 && Player.Distance(target.ServerPosition) > 600 && Main["ehiteList2"]["eWhiteList2" + target.ChampionName.ToLower()].As<MenuSliderBool>().Enabled && target.HealthPercent() <= Main["ehiteList2"]["eWhiteList2" + target.ChampionName.ToLower()].As<MenuSliderBool>().Value && Player.HealthPercent() >= Main["ehiteList2"]["myHeal"].As<MenuSliderBool>().Value)
                        {
                            Flash.Cast(xpos);
                            E.Cast(predepos);
                        }
                    }
                }        
            }

            if (Main["rProtect"]["autoR"].As<MenuBool>().Enabled && Misc.Orbwalker.Mode != OrbwalkingMode.Combo)
            {
                var target = GetBestEnemyHeroTargetInRange(1200);
                if (Player.HealthPercent() >= Main["rProtect"]["autoRmyHeal"].As<MenuSlider>().Value && (Player.CountEnemyHeroesInRange(Main["rProtect"]["autoREnemy"].As<MenuSlider>().Value) == 0 ||
                    ObjectManager.Get<Obj_AI_Turret>().FirstOrDefault(x => x.Distance(Player.Position) < 600 && x.Distance(target) > 700 && x.IsAlly) != null))
                {
                    UltiProtec();
                }             
            }

            if (Main["rProtect"]["r"].As<MenuKeyBind>().Enabled)
            {
                UltiProtec();
            }

        }

        private static void Combo()
        {
            var target = GetBestEnemyHeroTargetInRange(E.Range);
            if (target == null) return;
            if (Main["combo"]["q"].As<MenuBool>().Enabled && Q.Ready)
            {            
                if (Q.Ready && target.IsValidTarget(600))
                {       
                   if (target.Distance(Player) < 200)
                   {
                       Q.Cast();
                       if (Main["combo"]["w"].As<MenuBool>().Enabled && W.Ready)
                       {
                           W.Cast();
                       }
                   }           
                }
            }

            if (Main["combo"]["e"].As<MenuBool>().Enabled && E.Ready)
            {
                if (Main["ehiteList"]["eWhiteList" + target.ChampionName.ToLower()].As<MenuBool>().Enabled || target.HealthPercent() <= 15)
                {
                    CastE(target);                              
                }
            }

            if (Main["combo"]["w"].As<MenuBool>().Enabled && W.Ready && target.Distance(Player) < 200 && !Q.Ready)
            {
                W.Cast();
            }
        }

        private static void AutoAttack(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            var attack = sender as Obj_AI_Hero;
            var target = args.Target as Obj_AI_Hero;
            if (attack != null && attack.IsEnemy && attack.IsHero && Main["combo"]["autoW"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["combo"]["autoW"].As<MenuSliderBool>().Value && W.Ready)
            {
                foreach (var ally in GameObjects.AllyHeroes.Where(x => !x.IsDead && x.IsMe))
                {
                    if (target != null)
                    {
                        foreach (var sword in GameObjects.AllGameObjects)
                        {
                            if (sword.Name == "ShenSpiritUnit" && ally.Distance(sword.Position) <= 350 && Main["combo"]["autoWS"].As<MenuBool>().Enabled)
                            {
                                W.Cast();
                            }
                            else if(!Main["combo"]["autoWS"].As<MenuBool>().Enabled && Player.CountEnemyHeroesInRange(650) >= 1)
                            {
                                W.Cast();
                            }   
                        }                       
                    }
                }
            }
        }

        private static void UltiProtec()
        {
            if (R.Ready)
            {
                foreach (var ally in GameObjects.AllyHeroes.Where(x => !x.IsDead && !x.IsMe))
                {
                    if (Main["rProtect"]["allyR" + ally.ChampionName.ToLower()].As<MenuBool>().Enabled && 
                        ally.HealthPercent() <= Main["rProtect"]["allHeal"].As<MenuSlider>().Value)
                    {
                        if (ally.CountEnemyHeroesInRange(ally.IsUnderAllyTurret() ? 550 : 350 + 350) >= 1)
                        {
                            R.CastOnUnit(ally);
                            if (GetProtection(ally.ChampionName.ToLower()) == 3)
                            {
                                R.CastOnUnit(ally);
                            }
                            else if (GetProtection(ally.ChampionName.ToLower()) == 2)
                            {
                                R.CastOnUnit(ally);
                            }
                            else if (GetProtection(ally.ChampionName.ToLower()) == 1)
                            {
                                R.CastOnUnit(ally);
                            }
                        }
                    }
                }
            }            
        }

        private static void LaneClear()
        {
            if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() >= Main["laneclear"]["q"].As<MenuSliderBool>().Value && Q.Ready)
            {
                foreach (var target in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(600)))
                {
                    if (Player.Distance(target.Position) < 250)
                    {          
                        Q.Cast();                   
                    }
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var target in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["q"].As<MenuSliderBool>().Value && Q.Ready && Player.Distance(target.Position) < 150)
                {
                    Q.Cast();
                }

                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["w"].As<MenuSliderBool>().Value && W.Ready && Player.Distance(target.Position) < 250)
                {
                    foreach (var sword in GameObjects.AllGameObjects)
                    {
                        if (sword.Name == "ShenSpiritUnit" && Player.Distance(sword.Position) <= 350)
                        {
                            W.Cast();
                        }
                    }                  
                }

                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Player.ManaPercent() > Main["jungleclear"]["e"].As<MenuSliderBool>().Value && E.Ready && target.IsValidTarget(600))
                {
                    CastE(target);
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
        //            if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready &&
        //                target.IsValidTarget(E.Range))
        //            {
        //                E.Cast(Game.CursorPos);
        //            }
        //        }
        //            break;
        //        case SpellType.Targeted:
        //        {
        //            if (Main["antiGapcloser"]["e"].As<MenuBool>().Enabled && E.Ready &&
        //                target.IsValidTarget(E.Range))
        //            {
        //                E.Cast(Game.CursorPos);
        //            }
        //        }
        //            break;
        //    }
            
        // }

        public static int GetProtection(string championName)
        {
            string[] lowProtection =
            {
                "alistar", "amumu", "bard", "blitzcrank", "braum", "chogath", "drmundo", "garen", "gnar",
                "hecarim", "janna", "jarvaniv", "leona", "lulu", "malphite", "nami", "nasus", "nautilus", "nunu",
                "olaf", "rammus", "renekton", "sejuani", "shen", "shyvana", "singed", "sion", "skarner", "sona",
                "soraka", "tahmkench", "taric", "thresh", "volibear", "warwick", "monkeyking", "yorick", "zac", "zyra",
                "rakan", "illaoi", "ornn", "kled"
            };

            string[] mediumProtection =
            {
                "aatrox", "akali", "darius", "diana", "ekko", "elise", "evelynn", "fiddlesticks", "fiora", "fizz",
                "galio", "gangplank", "gragas", "heimerdinger", "irelia", "jax", "jayce", "kassadin", "kayle", "khazix",
                "leesin", "lissandra", "maokai", "mordekaiser", "morgana", "nocturne", "nidalee", "pantheon", "poppy",
                "rekSai", "rengar", "riven", "rumble", "ryze", "shaco", "swain", "trundle", "tryndamere", "udyr",
                "urgot", "vladimir", "vi", "xinzhao", "yasuo", "zilean", "kayn", "ivern"
            };

            string[] highProtection =
            {
                "ahri", "anivia", "annie", "ashe", "azir", "brand", "caitlyn", "cassiopeia", "corki", "draven", "ezreal",
                "graves", "jhin", "jinx", "kalista", "karma", "karthus", "katarina", "kennen", "kogmaw", "leblanc",
                "lucian", "lux", "malzahar", "masteryi", "missfortune", "orianna", "quinn", "sivir", "syndra", "talon",
                "teemo", "tristana", "twistedfate", "twitch", "varus", "vayne", "veigar", "velkoz", "viktor", "xerath",
                "zed", "ziggs", "xayah", "kindred", "aurelionsol", "taliyah"
            };

            if (highProtection.Contains(championName))
            {
                return 3;
            }

            if (mediumProtection.Contains(championName))
            {
                return 2;
            }

            if (lowProtection.Contains(championName))
            {
                return 1;
            }
            return 0;
        }
   
        private static void AllyHealt()
        {
            if (!Main["allyHealt"]["enabled"].As<MenuBool>().Enabled || Player.SpellBook.GetSpell(SpellSlot.R).Level < 1) return;
            
            float i = 0;
            foreach (var hero in GameObjects.AllyHeroes.Where(x => !x.IsMe && !x.IsDead && Main["allyHealt"]["allyHb" + x.ChampionName.ToLower()].As<MenuBool>().Enabled && x.HealthPercent() <= Main["rProtect"]["allHeal"].As<MenuSlider>().Value))
            {
                var champion = hero.ChampionName;
                if (champion.Length > 20)
                {
                    champion = champion.Remove(7) + "...";
                }

                var healthPercent = (int)(hero.Health / hero.MaxHealth * 100);
                var championInfo = $"{champion} ({healthPercent}%)";
                const int height = 25;
                DrawRect(Render.Width - HudOffsetRight, HudOffsetTop + i, 200, height, Color.FromArgb(175, 51, 55, 51));
                DrawRect(Render.Width - HudOffsetRight + 2, HudOffsetTop + i - -2,healthPercent <= 0 ? 100 : healthPercent * 2 - 4,
                    height - 4, healthPercent < 30 && healthPercent > 0 ? Color.FromArgb(255, 250, 0, 23) : healthPercent < 50
                    ? Color.FromArgb(255, 230, 169, 14) : Color.FromArgb(255, 2, 157, 10));
                Render.Text((int)((Render.Width - HudOffsetRight) + 10f), (int)(HudOffsetTop + i + 6), Color.AliceBlue , championInfo);
                i += 20f + 5;
            }
        }
        public static void CastE(Obj_AI_Base t)
        {

            var hithere = t.Position + Vector3.Normalize(t.ServerPosition - Player.Position) * 60;
            if (hithere.Distance(Player.Position) < E.Range)
            {
                E.Cast(hithere);
            }
        }
        private static void DamageDraw()
        {
            if (!Main["drawDamage"]["enabled"].Enabled) return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && x.IsVisible && Player.Distance(x) < 1700))
            {
                float qdmgDraw = 0, edmgDraw = 0;
                if (Q.Ready && Main["drawDamage"]["q"].As<MenuBool>().Enabled)
                {
                    qdmgDraw = QDamage(enemy) * 3;
                }
                if (E.Ready && Main["drawDamage"]["e"].As<MenuBool>().Enabled)
                {
                    edmgDraw = EDamage(enemy);
                }           
                var damage = qdmgDraw  + edmgDraw;

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
