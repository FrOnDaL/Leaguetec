using System;
using Aimtec;
using System.Linq;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Util;
using Aimtec.SDK.Damage;
using Font = Aimtec.Font;
using System.Globalization;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util.Cache;
using Aimtec.SDK.Damage.JSON;
using Spell = Aimtec.SDK.Spell;
using Aimtec.SDK.TargetSelector;
using System.Collections.Generic;
using Aimtec.SDK.Menu.Components;
using Aimtec.SDK.Prediction.Skillshots;
using Color = System.Drawing.Color;
using Rectangle = Aimtec.Rectangle;

namespace Nefarious_Utility
{
    internal class Utility : Program
    {
        private static int[] _level;
        private static Spell _smite;
        private const int Delay = 500;
        private static bool _avoiding;
        private static Spell Q { get; set; }
        private static Spell W { get; set; }
        private static Spell E { get; set; }
        private static Spell R { get; set; }     
        private static bool LevelFull { get; set; }
        private static int _lvl1, _lvl2, _lvl3, _lvl4;
        private static int PlayerLevel => Player.Level + 1;
        private static Font _font1, _font2, _font3, _font4, _font5, _font6, _font7, _font8;
        private static Aimtec.Spell IsSmite => Player.SpellBook.Spells.Where(o => o?.SpellData != null).FirstOrDefault(o => o.SpellData.Name.Contains("Smite"));
        public Utility()
        {
            var enabled = new Menu("enabled", "Utility On/Off");
            {
                enabled.Add(new MenuBool("uEnabled", "Enabled"));
            }

            #region - Map Hack Menu -
            var mapHack = new Menu("mapHack", "Map Hack");
            {
                mapHack.Add(new MenuBool("mapE", "Enabled"));
                mapHack.Add(new MenuBool("mapCircle", "Draw circle minimap"));
                mapHack.Add(new MenuBool("mapLine", "Draw Line minimap"));
                mapHack.Add(new MenuBool("visableT", "Last visable time", false));
            }
            #endregion

            #region - Enemy Show Click Menu -
            var enemyClick = new Menu("enemyClick", "Enemy Show Click");
            {
                enemyClick.Add(new MenuBool("enable", "Enabled"));
                enemyClick.Add(new MenuBool("heroName", "Hero name text"));
            }
            #endregion

            #region - Auto Smite -
            var autosmite = new Menu("autosmite", "Auto Smite");
            {
                if (IsSmite != null)
                {
                    autosmite.Add(new MenuBool("autosmiteE", "Enabled"));
                    autosmite.Add(new MenuKeyBind("autosmiteKey", "Auto smite (on/off)", KeyCode.M, KeybindType.Toggle, true));
                    autosmite.Add(new MenuBool("smitedrawE", "Smite range draw"));
                    autosmite.Add(new MenuBool("smitedamgeE", "Smite damage indicator"));
                    autosmite.Add(new MenuSeperator("Seperator1", "Enemy hero settings"));
                    autosmite.Add(new MenuBool("autosmiteH", "Use enemy hero smite on/off"));
                    autosmite.Add(new MenuBool("cmoboAutoSmiteH", "İf combo is active auto smite"));
                    var whiteListSmite = new Menu("whiteListQ", "Q white list");
                    {
                        foreach (var enemies in GameObjects.EnemyHeroes)
                        {
                            whiteListSmite.Add(new MenuSliderBool("smite" + enemies.ChampionName.ToLower(), enemies.ChampionName + "use smite / Enemies health", true, 100, 1));
                        }
                    }
                    autosmite.Add(whiteListSmite);
                    autosmite.Add(new MenuSeperator("Seperator2", "Enable auto smite for monsters"));
                    autosmite.Add(new MenuBool("blueE", "Use auto smite Blue"));
                    autosmite.Add(new MenuBool("redE", "Use auto smite Red"));
                    autosmite.Add(new MenuBool("grompE", "Use auto smite Gromp"));
                    autosmite.Add(new MenuBool("murkWolfE", "Use auto smite MurkWolf"));
                    autosmite.Add(new MenuBool("razorbeakE", "Use auto smite Razorbeak"));
                    autosmite.Add(new MenuBool("krugsE", "Use auto smite Krugs"));
                    autosmite.Add(new MenuBool("crabE", "Use auto smite Crab", false));
                    autosmite.Add(new MenuBool("dragonE", "Use auto smite Dragon"));
                    autosmite.Add(new MenuBool("riftHeraldE", "Use auto smite RiftHerald"));
                    autosmite.Add(new MenuBool("baronE", "Use auto smite Baron"));
                }
                else if (IsSmite == null)
                {
                    autosmite.Add(new MenuSeperator("Seperator1", "Smite not detected"));
                    autosmite.Add(new MenuBool("smitedamgeE", "Smite damage indicator", false));
                }

            }
            #endregion

            #region - Auto Level Menu -
            var autoLevel = new Menu(Player.ChampionName + "autoLevel", "Auto Level");
            {
                autoLevel.Add(new MenuList("levelMode", "Auto level mode", new[] { "Disable", "Priority", "Meta", "OnlyUlti(R)" }, 0));
                autoLevel.Add(new MenuSlider("levelStart", "Auto leveler start", 2, 2, 6));
                autoLevel.Add(new MenuSeperator("Seperator1", "Meta : " + MetaString(Player)));
                autoLevel.Add(new MenuSeperator("Seperator2", Player.ChampionName + " Priority"));
                autoLevel.Add(new MenuList("priority1", "1", new[] { "Q", "W", "E", "R" }, 3));
                autoLevel.Add(new MenuList("priority2", "2", new[] { "Q", "W", "E", "R" }, 0));
                autoLevel.Add(new MenuList("priority3", "3", new[] { "Q", "W", "E", "R" }, 1));
                autoLevel.Add(new MenuList("priority4", "4", new[] { "Q", "W", "E", "R" }, 2));
            }
            #endregion

            #region - Damage Indicator Menu -
            var drawDamage = new Menu("drawDamage", "Damage Indicator");
            {
                drawDamage.Add(new MenuBool("enabled", "Enabled"));
                drawDamage.Add(new MenuBool("q", "Draw Q damage"));
                drawDamage.Add(new MenuBool("w", "Draw W damage"));
                drawDamage.Add(new MenuBool("e", "Draw E damage"));
                drawDamage.Add(new MenuBool("r", "Draw R damage"));
            }
            #endregion

            #region - Ward Tracker Menu -
            var wardTracker = new Menu("wardTracker", "Ward Tracker");
            {
                wardTracker.Add(new MenuBool("wardE", "Enabled"));
                wardTracker.Add(new MenuSeperator("Seperator1", "Disable or enable"));
                wardTracker.Add(new MenuBool("pinkE", "Pink Ward - Color : DeepPink"));
                wardTracker.Add(new MenuBool("jhinE", "Jhin E skill - Color : Orange"));
                wardTracker.Add(new MenuBool("teemoE", "Teemo R skill - Color : Red"));
                wardTracker.Add(new MenuBool("nidaleeE", "Nidalee W skill - Color : GreenYellow"));
                wardTracker.Add(new MenuBool("shacoE", "Shaco W skill - Color : Green"));
                wardTracker.Add(new MenuBool("caitlynE", "Caitlyn W skill - Color : Red"));
                wardTracker.Add(new MenuSeperator("Seperator2", "Text disable or enable"));
                wardTracker.Add(new MenuBool("wardT", "Ward text"));
                wardTracker.Add(new MenuBool("jhinT", "Jhin E skill text", false));
                wardTracker.Add(new MenuBool("teemoT", "Teemo R skill text"));
                wardTracker.Add(new MenuBool("nidaleeT", "Nidalee W skill text", false));
                wardTracker.Add(new MenuBool("shacoT", "Shaco W skill text", false));
            }
            #endregion

            #region - Cooldown Tracker Menu -
            var cdTracker = new Menu("cdTracker", "Cooldown Tracker");
            {
                cdTracker.Add(new MenuBool("trackerE", "CD tracker"));
                cdTracker.Add(new MenuBool("allies", "Track allies"));
                cdTracker.Add(new MenuBool("enemies", "Track enemies"));
                cdTracker.Add(new MenuBool("me", "Track me"));
                cdTracker.Add(new MenuSeperator("Seperator1", "Summoner Spells Settings"));
                cdTracker.Add(new MenuBool("spells", "Summoner spells icon"));
                cdTracker.Add(new MenuBool("spellsAllies", "Summoner spells icon allies", false));
                cdTracker.Add(new MenuBool("spellsEnemies", "Summoner spells icon enemies"));
                cdTracker.Add(new MenuBool("spellsMe", "Summoner spells icon me", false));
                cdTracker.Add(new MenuSeperator("Seperator2", "Time Settings"));
                cdTracker.Add(new MenuBool("spellsTime", "Summoner spells time"));
                cdTracker.Add(new MenuBool("skillsTime", "Summoner skills time"));
            }
            #endregion

            #region - Hero Side Hud Menu -
            var sideHud = new Menu("sideHud", "Side Hud");
            {
                sideHud.Add(new MenuBool("hudE", "Side Hud"));
                sideHud.Add(new MenuBool("spellsTime", "Summoner spells time"));
                sideHud.Add(new MenuSeperator("Seperator1", "Hud X and Y offset"));
                sideHud.Add(new MenuSlider("xOffset", "X Offset", 0, 0, 2000));
                sideHud.Add(new MenuBool("invertX", "Invert X"));
                sideHud.Add(new MenuSlider("yOffset", "Y Offset", 0, 0, 2000));
                sideHud.Add(new MenuBool("invertY", "Invert Y"));
            }
            #endregion

            #region - Tower and Auto Attack Range Menu -
            var taRange = new Menu("taRange", "Tower and auto attack range");
            {
                taRange.Add(new MenuBool("taE", "Enabled"));
                taRange.Add(new MenuBool("trE", "Enemy tower range"));
                taRange.Add(new MenuBool("trA", "Allies tower range", false));
                taRange.Add(new MenuBool("aaE", "Enemy auto attack range", false));
                taRange.Add(new MenuBool("aaA", "Allies auto attack range", false));
            }
            #endregion

            #region - Avoiders Menu -
            var avoider = new Menu("avoider", "Avoiders");
            {
                avoider.Add(new MenuKeyBind("Key", "Auto avoid (on/off)", KeyCode.N, KeybindType.Toggle, true));
                avoider.Add(new MenuBool("caitlynW", "Caitlyn W"));
                avoider.Add(new MenuBool("nidaleeW", "Nidalee W"));
                avoider.Add(new MenuBool("teemoR", "Teemo R"));
                avoider.Add(new MenuBool("jhinE", "Jhin E"));
                avoider.Add(new MenuBool("ziggsE", "Ziggs E"));
                avoider.Add(new MenuBool("ziggsD", "Ziggs E mine draw"));
                avoider.Add(new MenuBool("jinxE", "Jinx E"));
                avoider.Add(new MenuBool("jinxD", "Jinx E mine draw"));
            }
            #endregion

            #region - Gank Alerter -
            var gankAlerter = new Menu("gankAlerter", "Gank Alerter");
            {
                gankAlerter.Add(new MenuBool("gankAlerterE", "Enabled"));
                gankAlerter.Add(new MenuSeperator("Seperator1", "Rectangle X and Y offset"));
                gankAlerter.Add(new MenuSlider("xOffsetG", "X Offset", 0, 0, 1000));
                gankAlerter.Add(new MenuBool("invertXG", "Invert X"));
                gankAlerter.Add(new MenuSlider("yOffsetG", "Y Offset", 0, 0, 1000));
                gankAlerter.Add(new MenuBool("invertYG", "Invert Y"));

            }
            #endregion

            #region - Base Ulti -
            var baseUlti = new Menu("baseUlti", "Base Ulti");
            {
                baseUlti.Add(new MenuKeyBind("baseUltiE", "Base ulti press U", KeyCode.U, KeybindType.Press));
                baseUlti.Add(new MenuBool("baseUltiD", "Base ulti draw"));
                var whiteListUlti = new Menu("whiteListUlti", "Ulti white list");
                {
                    foreach (var enemies in GameObjects.EnemyHeroes)
                    {
                        whiteListUlti.Add(new MenuBool("ulti" + enemies.ChampionName.ToLower(), enemies.ChampionName + " use ulti"));
                    }
                }
                baseUlti.Add(whiteListUlti);
                baseUlti.Add(new MenuSeperator("Seperator1", "No Collision"));
                baseUlti.Add(new MenuSlider("scalingB", "Scaling", 0, 0, 200));
                baseUlti.Add(new MenuSeperator("Seperator2", "Rectangle X and Y offset"));
                baseUlti.Add(new MenuSlider("xOffsetB", "X Offset", 0, 0, 1000));
                baseUlti.Add(new MenuBool("invertXB", "Invert X"));
                baseUlti.Add(new MenuSlider("yOffsetB", "Y Offset", 0, 0, 1000));
                baseUlti.Add(new MenuBool("invertYB", "Invert Y"));
            }
            #endregion


            Menu.Add(enabled);
            Menu.Add(mapHack);
            Menu.Add(enemyClick);
            Menu.Add(autosmite);
            Menu.Add(autoLevel);
            Menu.Add(drawDamage);
            Menu.Add(wardTracker);
            Menu.Add(cdTracker);
            Menu.Add(sideHud);
            Menu.Add(taRange);
            Menu.Add(avoider);
            Menu.Add(gankAlerter);
            Menu.Add(baseUlti);
            Q = new Spell(SpellSlot.Q);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R);
            if (IsSmite != null) _smite = new Spell(IsSmite.Slot, 570);
            #region - Text Font -
            _font1 = new Font("Tahoma", 14, 0){ OutputPrecision = FontOutputPrecision.Default, Quality = FontQuality.AntiAliased };
            _font2 = new Font("Tahoma", 12, 0) { Weight = FontWeight.Bold, OutputPrecision = FontOutputPrecision.Default, Quality = FontQuality.AntiAliased };
            _font3 = new Font("Tahoma", 11, 0) { Weight = FontWeight.Normal, OutputPrecision = FontOutputPrecision.Default, Quality = FontQuality.AntiAliased };
            _font4 = new Font("Tahoma", 12, 0) { Weight = FontWeight.Normal, OutputPrecision = FontOutputPrecision.Default, Quality = FontQuality.AntiAliased };
            _font5 = new Font("Tahoma", 13, 0) { Weight = FontWeight.Normal, OutputPrecision = FontOutputPrecision.Default, Quality = FontQuality.AntiAliased };
            _font6 = new Font("Calibri", 13, 6) { Weight = FontWeight.Normal, OutputPrecision = FontOutputPrecision.Default, Quality = FontQuality.NonAntiAliased };
            _font7 = new Font("Calibri", 13, 6) { Weight = FontWeight.Bold, OutputPrecision = FontOutputPrecision.Default, Quality = FontQuality.NonAntiAliased };
            _font8 = new Font("Tahoma", 13, 0) { Weight = FontWeight.Normal, OutputPrecision = FontOutputPrecision.Default, Quality = FontQuality.AntiAliased };
            #endregion
            Game.OnUpdate += GameOnUpdate;
            Obj_AI_Base.OnLevelUp += On_Level_Up;
            Render.OnPresent += Render_OnPresent;
            Obj_AI_Base.OnIssueOrder += OnIssueOrder;
            Orbwalker.PreAttack += Orbwalker_OnPreAttack;
        }

        private static void GameOnUpdate()
        {
            #region - Auto Level Up Value -
            if (Menu[Player.ChampionName + "autoLevel"]["levelMode"].As<MenuList>().SelectedItem != "Disable" && Menu["enabled"]["uEnabled"].As<MenuBool>().Enabled)
            {
                _lvl1 = Menu[Player.ChampionName + "autoLevel"]["priority1"].As<MenuList>().Value;
                _lvl2 = Menu[Player.ChampionName + "autoLevel"]["priority2"].As<MenuList>().Value;
                _lvl3 = Menu[Player.ChampionName + "autoLevel"]["priority3"].As<MenuList>().Value;
                _lvl4 = Menu[Player.ChampionName + "autoLevel"]["priority4"].As<MenuList>().Value;
            }
            #endregion

            #region - Auto Smite -
            if (IsSmite != null)
            {
                if (Menu["autosmite"]["autosmiteE"].As<MenuBool>().Enabled && Menu["autosmite"]["autosmiteKey"].As<MenuKeyBind>().Enabled && _smite.Ready && Menu["enabled"]["uEnabled"].As<MenuBool>().Enabled)
                {
                    if (Menu["autosmite"]["autosmiteH"].As<MenuBool>().Enabled)
                    {
                        var smiteHero = TargetSelector.GetTarget(_smite.Range);
                        if (smiteHero != null)
                        {
                            if (Menu["autosmite"]["smite" + smiteHero.ChampionName.ToLower()].As<MenuSliderBool>().Enabled && smiteHero.HealthPercent() <= Menu["autosmite"]["smite" + smiteHero.ChampionName.ToLower()].As<MenuSliderBool>().Value)
                            {
                                if (Menu["autosmite"]["cmoboAutoSmiteH"].As<MenuBool>().Enabled && Orbwalker.Mode == OrbwalkingMode.Combo)
                                {
                                    _smite.CastOnUnit(smiteHero);
                                }
                                if (!Menu["autosmite"]["cmoboAutoSmiteH"].As<MenuBool>().Enabled)
                                {
                                    _smite.CastOnUnit(smiteHero);
                                }                               
                            }
                        }
                    }
                    foreach (var monster in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValidTarget(_smite.Range) && (x.UnitSkinName == "SRU_Blue" && Menu["autosmite"]["blueE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Red" && Menu["autosmite"]["redE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Baron" && Menu["autosmite"]["baronE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_RiftHerald" && Menu["autosmite"]["riftHeraldE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Krug" && Menu["autosmite"]["krugsE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Razorbeak" && Menu["autosmite"]["razorbeakE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Murkwolf" && Menu["autosmite"]["murkWolfE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Gromp" && Menu["autosmite"]["grompE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "Sru_Crab" && Menu["autosmite"]["crabE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Dragon_Earth" && Menu["autosmite"]["dragonE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Dragon_Air" && Menu["autosmite"]["dragonE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Dragon_Fire" && Menu["autosmite"]["dragonE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Dragon_Water" && Menu["autosmite"]["dragonE"].As<MenuBool>().Enabled
                    || x.UnitSkinName == "SRU_Dragon_Elder" && Menu["autosmite"]["dragonE"].As<MenuBool>().Enabled)))
                    {
                        if (monster.Health <= SmiteDamages)
                        {
                            _smite.CastOnUnit(monster);
                        }
                    }
                }
            }
            #endregion

            #region - Avoiders -
            if (Player.IsDead || !Menu["avoider"]["Key"].As<MenuKeyBind>().Enabled || !Menu["enabled"]["uEnabled"].As<MenuBool>().Enabled) return;

            if (Player.HasBuffOfType(BuffType.Stun) || Player.HasBuffOfType(BuffType.Snare))
            {
                return;
            }

            foreach (var t in Tracker.HiddenObjectsList)
            {
                if (Player.Distance(t.Location) > 1000)
                {
                    continue;
                }
                if (GameObjects.Heroes.FirstOrDefault(x => x.IsEnemy) != null)
                {
                    if (t.WardName == "ZiggsE" && Menu["avoider"]["ziggsD"].As<MenuBool>().Enabled)
                    {
                        Render.Circle(t.Location, 65, 30, Color.Red);
                    }
                    if (t.WardName == "JinxE" && Menu["avoider"]["jinxD"].As<MenuBool>().Enabled)
                    {
                        Render.Circle(t.Location, 70, 30, Color.Red);
                    }
                    if (Player.Distance(t.Location) < 200)
                    {
                        if (t.WardName == "CaitlynYordleTrap" && Menu["avoider"]["caitlynW"].As<MenuBool>().Enabled)
                        {
                            Avoid(t.Location, 180);
                        }
                        if (t.WardName == "Bushwhack" && Menu["avoider"]["nidaleeW"].As<MenuBool>().Enabled)
                        {
                            Avoid(t.Location, 220);
                        }
                        if (t.WardName == "TeemoRCast" && Menu["avoider"]["teemoR"].As<MenuBool>().Enabled)
                        {
                            Avoid(t.Location, 220);
                        }
                        if (t.WardName == "JhinE" && Menu["avoider"]["jhinE"].As<MenuBool>().Enabled)
                        {
                            Avoid(t.Location, 250);
                        }
                        if (t.WardName == "ZiggsE" && Menu["avoider"]["ziggsE"].As<MenuBool>().Enabled)
                        {
                            Avoid(t.Location, 220);
                        }
                        if (t.WardName == "JinxE" && Menu["avoider"]["jinxE"].As<MenuBool>().Enabled)
                        {
                            Avoid(t.Location, 220);
                        }
                    }
                }  
            }
            #endregion
        }

        private static void Render_OnPresent()
        {
            if (!Menu["enabled"]["uEnabled"].As<MenuBool>().Enabled) return;
            var flashing = (int)(Game.ClockTime * 10) % 2 != 0;      
            foreach (var hero in Tracker.HeroInfoList.Where(x => !x.Hero.IsDead))
            {
                #region - Map Hack -
                if (Menu["mapHack"]["mapE"].As<MenuBool>().Enabled)
                {
                    if (!hero.Hero.IsVisible && hero.Displaying && hero.Hero != null && hero.Hero.IsEnemy)
                    {
                        Vector2 enemyPos;
                        Vector2 enemylastPos;
                        Render.WorldToMinimap(hero.LastVisablePos, out enemylastPos);
                        Render.WorldToMinimap(hero.LastWayPoint, out enemyPos);
                        if (hero.TimeMia < 25000)
                        {
                            if (Menu["mapHack"]["mapCircle"].As<MenuBool>().Enabled)
                            {
                                if (!hero.LastWayPoint.IsZero)
                                {
                                    if (Menu["mapHack"]["mapLine"].As<MenuBool>().Enabled)
                                    {
                                        var distanceTravelled2 = (hero.TimeMia / 82000f) * hero.Hero.MoveSpeed;
                                        enemyPos = enemylastPos.Extend(enemyPos, distanceTravelled2);
                                        if (Game.ClockTime - hero.StartRecallTime < hero.RecallTime && !hero.Abort)
                                        {
                                            if (flashing)
                                            {
                                                Render.Line(enemylastPos, enemyPos, 1, true, Color.Yellow);
                                            }
                                        }
                                        else
                                        {
                                            Render.Line(enemylastPos, enemyPos, 1, true, Color.OrangeRed);
                                        }
                                    }
                                }
                                var speed = hero.Hero.MoveSpeed;
                                if (Math.Abs(hero.Hero.MoveSpeed) <= 0 && Game.ClockTime >= 15 && Game.ClockTime < 35)
                                {
                                    speed = 600;
                                }
                                var distanceTravelled = (hero.TimeMia / 1000f) * speed;
                                
                                if (Game.ClockTime - hero.StartRecallTime < hero.RecallTime && !hero.Abort)
                                {
                                    DrawCircleOnMinimap(hero.LastVisablePos, distanceTravelled, Color.Yellow, 1);
                                }
                                else
                                {
                                    DrawCircleOnMinimap(hero.LastVisablePos, distanceTravelled, Color.DarkGray, 1);
                                }
                            }   
                        }
                        hero.MinimapSprite.Draw(enemylastPos + new Vector2(-11, -11));
                        hero.MinimapSprite.Draw(enemylastPos + new Vector2(-11, -11),
                            Menu["mapHack"]["visableT"].As<MenuBool>().Enabled
                                ? Color.FromArgb(115, 0, 0, 0)
                                : Color.FromArgb(40, 0, 0, 0));

                        if (Game.ClockTime - hero.StartRecallTime < hero.RecallTime && !hero.Abort)
                        {
                            DrawCircleOnMinimap(hero.LastVisablePos, 890, Color.Yellow, 1);
                        }
                        if (Menu["mapHack"]["visableT"].As<MenuBool>().Enabled)
                        {
                            RenderFontText1(_font8, Math.Floor(hero.SecondsSinceSeen) > 240 || Math.Abs(Math.Floor(hero.SecondsSinceSeen)) <= 0 ? " " : TextCenter(hero.SecondsSinceSeen), enemylastPos + new Vector2(-7, -6), Color.White);
                        }
                    }
                }
                #endregion

                #region - Enemy Show Click -
                if (Menu["enemyClick"]["enable"].As<MenuBool>().Enabled && hero.Hero.IsValid && hero.Hero.IsVisible && !hero.Hero.FloatingHealthBarPosition.IsZero && hero.Hero.IsEnemy && hero.Hero.IsMoving)
                {
                    var waypoints = hero.Hero.GetWaypoints();
                    Vector2 heroScreenPos;
                    Vector2 heroScreenPos2;
                    Render.WorldToScreen(hero.Hero.Position, out heroScreenPos);
                    Render.WorldToScreen(waypoints.Last().To3D(), out heroScreenPos2);

                    Render.Line(heroScreenPos, heroScreenPos2, 1, true, Color.Red);

                    if (Menu["enemyClick"]["heroName"].As<MenuBool>().Enabled)
                    {
                        RenderFontText1(_font1, hero.Hero.ChampionName, heroScreenPos2, Color.WhiteSmoke);
                    }   
                }
                #endregion

                #region - Damage Indicator -
                if (Menu["drawDamage"]["enabled"].Enabled && hero.Hero.IsEnemy && hero.Hero.IsValid && hero.Hero.IsVisible && !hero.Hero.IsDead && !hero.Hero.FloatingHealthBarPosition.IsZero)
                {
                    const int width = 103;
                    const int height = 11;

                    var xOffset = X(hero.Hero);
                    var yOffset = Y(hero.Hero);
                    if (hero.Hero.ChampionName == "Nidalee" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown")
                    {
                        xOffset = 29.5f;
                        yOffset = 2f;
                    }
                    if (hero.Hero.ChampionName == "Elise" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ")
                    {
                        xOffset = 29.5f;
                        yOffset = 1.5f;
                    }
                    var barPos = hero.Hero.FloatingHealthBarPosition;

                    float qdmgDraw = 0, wdmgDraw = 0, edmgDraw = 0, rdmgDraw = 0;

                    var qRdy = Q.Ready;
                    var wRdy = W.Ready;
                    var eRdy = E.Ready;
                    var rRdy = R.Ready;

                    float qDmg = 0;
                    float wDmg = 0;
                    float eDmg = 0;
                    float rDmg = 0;

                    if (qRdy && Menu["drawDamage"]["q"].Enabled)
                    {
                        qDmg = (float)Player.GetSpellDamage(hero.Hero, SpellSlot.Q);
                    }

                    if (wRdy && Menu["drawDamage"]["w"].Enabled)
                    {
                        wDmg = (float)Player.GetSpellDamage(hero.Hero, SpellSlot.W);
                    }

                    if (eRdy && Menu["drawDamage"]["e"].Enabled)
                    {
                        eDmg = (float)Player.GetSpellDamage(hero.Hero, SpellSlot.E);
                    }

                    if (rRdy && Menu["drawDamage"]["r"].Enabled)
                    {
                        rDmg = (float)Player.GetSpellDamage(hero.Hero, SpellSlot.R);
                    }

                    switch (Player.ChampionName)
                    {
                        case "Ahri":
                            if (rRdy && Menu["drawDamage"]["r"].Enabled)
                            {
                                rDmg = rDmg * 3;
                            }
                            break;
                        case "Jhin":
                            if (rRdy && Menu["drawDamage"]["r"].Enabled)
                            {
                                rDmg = rDmg * 4;
                            }
                            break;
                        case "Kennen":
                            if (rRdy && Menu["drawDamage"]["r"].Enabled)
                            {
                                rDmg = rDmg * 6;
                            }
                            break;
                        case "Lucian":
                            if (rRdy && Menu["drawDamage"]["r"].Enabled)
                            {
                                rDmg = rDmg * 10;
                            }
                            break;
                        case "FiddleSticks":
                            if (rRdy && Menu["drawDamage"]["r"].Enabled)
                            {
                                rDmg = rDmg * 5;
                            }
                            break;
                        case "MissFortune":
                            if (eRdy && Menu["drawDamage"]["e"].Enabled)
                            {
                                eDmg = eDmg * 2;
                            }
                            if (rRdy && Menu["drawDamage"]["r"].Enabled)
                            {
                                rDmg = rDmg * 12;
                            }
                            break;
                        case "Gangplank":
                            if (rRdy && Menu["drawDamage"]["r"].Enabled)
                            {
                                rDmg = rDmg * 12;
                            }
                            break;
                        case "Swain":
                            if (qRdy && Menu["drawDamage"]["q"].Enabled)
                            {
                                qDmg = qDmg * 3;
                            }
                            if (eRdy && Menu["drawDamage"]["e"].Enabled)
                            {
                                eDmg = eDmg * 4;
                            }
                            if (rRdy && Menu["drawDamage"]["r"].Enabled)
                            {
                                rDmg = rDmg * 3;
                            }
                            break;
                        case "Twitch":
                            if (eRdy && Menu["drawDamage"]["e"].Enabled)
                            {
                                eDmg = eDmg + (float)Player.GetSpellDamage(hero.Hero, SpellSlot.E, DamageStage.Buff);
                            }
                            break;
                    }

                    var damage = qDmg + wDmg + eDmg + rDmg;

                    if (qRdy)
                    {
                        qdmgDraw = (qDmg / damage);
                    }

                    if (wRdy && Player.ChampionName != "Kalista")
                    {
                        wdmgDraw = (wDmg / damage);
                    }

                    if (eRdy)
                    {
                        edmgDraw = (eDmg / damage);
                    }

                    if (rRdy)
                    {
                        rdmgDraw = (rDmg / damage);
                    }                    

                    var percentHealthAfterDamage = Math.Max(0, hero.Hero.Health - damage) / hero.Hero.MaxHealth;
                    var yPos = barPos.Y + yOffset;
                    var xPosDamage = barPos.X + xOffset + width * percentHealthAfterDamage;
                    var xPosCurrentHp = barPos.X + xOffset + width * hero.Hero.Health / hero.Hero.MaxHealth;
                    var differenceInHp = xPosCurrentHp - xPosDamage;
                    var pos1 = barPos.X + xOffset + (107 * percentHealthAfterDamage);

                    for (var i = 0; i < differenceInHp; i++)
                    {
                        if (qRdy && i < qdmgDraw * differenceInHp)
                        {
                            Render.Line(pos1 + i, yPos, pos1 + i, yPos + height, 1, true, Color.FromArgb(0, 240, 240));
                        }
                        else if (wRdy && i < (qdmgDraw + wdmgDraw) * differenceInHp)
                        {
                            Render.Line(pos1 + i, yPos, pos1 + i, yPos + height, 1, true, Color.FromArgb(240, 150, 10));
                        }
                        else if (eRdy && i < (qdmgDraw + wdmgDraw + edmgDraw) * differenceInHp)
                        {
                            Render.Line(pos1 + i, yPos, pos1 + i, yPos + height, 1, true, Color.FromArgb(240, 240, 0));
                        }
                        else if (rRdy && i < (qdmgDraw + wdmgDraw + edmgDraw + rdmgDraw) * differenceInHp)
                        {
                            Render.Line(pos1 + i, yPos, pos1 + i, yPos + height, 1, true, Color.FromArgb(195, 30, 180));
                        }
                    }
                }
                #endregion

                #region - Cooldown Tracker -

                if (hero.Hero.IsValid && hero.Hero.IsVisible && !hero.Hero.FloatingHealthBarPosition.IsZero)
                {
                    if (hero.Hero.IsAlly && !hero.Hero.IsMe && !Menu["cdTracker"]["allies"].As<MenuBool>().Enabled)
                    {
                        continue;
                    }
                    if (hero.Hero.IsEnemy && !Menu["cdTracker"]["enemies"].As<MenuBool>().Enabled)
                    {
                        continue;
                    }
                    if (hero.Hero.IsMe && !Menu["cdTracker"]["me"].As<MenuBool>().Enabled)
                    {
                        continue;
                    }

                    if (Menu["cdTracker"]["trackerE"].As<MenuBool>().Enabled)
                    {
                        if (hero.Hero.IsEnemy)
                        {
                            var yEnemy = Yenemy(hero.Hero);
                            if (hero.Hero.ChampionName == "Nidalee" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown")
                            {
                                yEnemy = 18f;
                            }
                            if (hero.Hero.ChampionName == "Elise" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ")
                            {
                                yEnemy = 18f;
                            }
                            var bpos = hero.Hero.FloatingHealthBarPosition + new Vector2(Xhud(hero.Hero), yEnemy);

                            hero.Hud?.Draw(bpos);

                            var nSpellStart = bpos + new Vector2(15.5f, 9.5f);
                            for (var i = 0; i < hero.Spells.Count; i++)
                            {
                                var sp = hero.Spells[i];
                                var position = nSpellStart + new Vector2(i * 24, 0);

                                if (!sp.Spell.State.HasFlag(SpellState.Unknown) && !sp.Spell.State.HasFlag(SpellState.NotLearned) && !sp.Spell.State.HasFlag(SpellState.Disabled))
                                {
                                    Render.Rectangle(position, sp.CurrentWidth, sp.StatusBoxHeight, sp.CurrentColor);
                                }
                                if (sp.Spell.State.HasFlag(SpellState.Cooldown))
                                {
                                    var cdString = sp.TimeUntilReady < 1
                                        ? sp.TimeUntilReady.ToString("0.0")
                                        : Math.Ceiling(sp.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                    var pos = position + new Vector2((int)((sp.StatusBoxWidth + 6) / 2) - Render.MeasureText(cdString) / 2, 9);
                                    if (Menu["cdTracker"]["skillsTime"].As<MenuBool>().Enabled)
                                    {
                                        if (sp.TimeUntilReady > 0)
                                        {
                                            RenderFontText1(_font5, cdString, pos, Color.White);
                                        }
                                    }
                                }
                            }
                        }
                        else if (hero.Hero.IsAlly && !hero.Hero.IsMe)
                        {
                            var yAlly = Yally(hero.Hero);
                            if (hero.Hero.ChampionName == "Nidalee" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown")
                            {
                                yAlly = 19f;
                            }
                            if (hero.Hero.ChampionName == "Elise" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ")
                            {
                                yAlly = 18.5f;
                            }
                            var bpos = hero.Hero.FloatingHealthBarPosition + new Vector2(Xhud(hero.Hero), yAlly);

                            hero.Hud?.Draw(bpos);

                            var nSpellStart = bpos + new Vector2(15.5f, 9.5f);
                            for (var i = 0; i < hero.Spells.Count; i++)
                            {
                                var sp = hero.Spells[i];
                                var position = nSpellStart + new Vector2(i * 24, 0);

                                if (!sp.Spell.State.HasFlag(SpellState.Unknown) && !sp.Spell.State.HasFlag(SpellState.NotLearned) && !sp.Spell.State.HasFlag(SpellState.Disabled))
                                {
                                    Render.Rectangle(position, sp.CurrentWidth, sp.StatusBoxHeight, sp.CurrentColor);
                                }
                                if (sp.Spell.State.HasFlag(SpellState.Cooldown))
                                {
                                    var cdString = sp.TimeUntilReady < 1
                                        ? sp.TimeUntilReady.ToString("0.0")
                                        : Math.Ceiling(sp.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                    var pos = position + new Vector2((int)((sp.StatusBoxWidth + 6) / 2) - Render.MeasureText(cdString) / 2, 9);
                                    if (Menu["cdTracker"]["skillsTime"].As<MenuBool>().Enabled)
                                    {
                                        if (sp.TimeUntilReady > 0)
                                        {
                                            RenderFontText1(_font5, cdString, pos, Color.White);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            var bpos = hero.Hero.FloatingHealthBarPosition + new Vector2(XhudMe(hero.Hero), Yme(hero.Hero));

                            hero.Hud?.Draw(bpos);

                            var nSpellStart = bpos + new Vector2(15.5f, 9.5f);
                            for (var i = 0; i < hero.Spells.Count; i++)
                            {
                                var sp = hero.Spells[i];
                                var position = nSpellStart + new Vector2(i * 24, 0);

                                if (!sp.Spell.State.HasFlag(SpellState.Unknown) && !sp.Spell.State.HasFlag(SpellState.NotLearned) && !sp.Spell.State.HasFlag(SpellState.Disabled))
                                {
                                    Render.Rectangle(position, sp.CurrentWidth, sp.StatusBoxHeight, sp.CurrentColor);
                                }
                                if (sp.Spell.State.HasFlag(SpellState.Cooldown))
                                {
                                    var cdString = sp.TimeUntilReady < 1
                                        ? sp.TimeUntilReady.ToString("0.0")
                                        : Math.Ceiling(sp.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                    var pos = position + new Vector2((int)((sp.StatusBoxWidth + 6) / 2) - Render.MeasureText(cdString) / 2, 9);
                                    if (Menu["cdTracker"]["skillsTime"].As<MenuBool>().Enabled)
                                    {
                                        if (sp.TimeUntilReady > 0)
                                        {
                                            RenderFontText1(_font5, cdString, pos, Color.White);
                                        }
                                    }
                                }
                            }
                        }
                        if (hero.Hero.IsAlly && !hero.Hero.IsMe && !Menu["cdTracker"]["spellsAllies"].As<MenuBool>().Enabled)
                        {
                            continue;
                        }
                        if (hero.Hero.IsEnemy && !Menu["cdTracker"]["spellsEnemies"].As<MenuBool>().Enabled)
                        {
                            continue;
                        }
                        if (hero.Hero.IsMe && !Menu["cdTracker"]["spellsMe"].As<MenuBool>().Enabled)
                        {
                            continue;
                        }
                        if (Menu["cdTracker"]["spells"].As<MenuBool>().Enabled)
                        {
                            if (hero.Hero.IsEnemy)
                            {
                                var yEnemy = Yenemy(hero.Hero);
                                if (hero.Hero.ChampionName == "Nidalee" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown")
                                {
                                    yEnemy = 18f;
                                }
                                if (hero.Hero.ChampionName == "Elise" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ")
                                {
                                    yEnemy = 18f;
                                }
                                var bpos = hero.Hero.FloatingHealthBarPosition + new Vector2(Xhud(hero.Hero), yEnemy);
                                var summonerStart = bpos + new Vector2(XspellsEnemy(hero.Hero), YspellsEnemy(hero.Hero));

                                var s1Bar = summonerStart + new Vector2(-1f, -1f);
                                if (hero.Summoner1?.SpellTexture != null)
                                {
                                    hero.Summoner1.SpellTexture.Draw(summonerStart);

                                    if (hero.Summoner1.Spell.State.HasFlag(SpellState.Cooldown) && hero.Summoner1.TimeUntilReady > 0.0f)
                                    {
                                        Render.Rectangle(s1Bar, hero.Summoner1.CurrentWidth, hero.Summoner1.StatusBoxHeight, Color.FromArgb(150, 0, 0, 0));
                                        var cdString = hero.Summoner1.TimeUntilReady < 1
                                            ? hero.Summoner1.TimeUntilReady.ToString("0.0")
                                            : Math.Ceiling(hero.Summoner1.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                        var tpos = s1Bar + new Vector2((int)((hero.Summoner1.StatusBoxWidth + 7) / 2) - Render.MeasureText(cdString) / 2, 23);
                                        if (Menu["cdTracker"]["spellsTime"].As<MenuBool>().Enabled)
                                        {
                                            RenderFontText1(_font4, cdString,tpos,Color.White);
                                        }
                                    }
                                }

                                var s2Bar = s1Bar + new Vector2(24, 0);
                                if (hero.Summoner2?.SpellTexture != null)
                                {
                                    var s2Position = summonerStart + new Vector2(24, 0);
                                    hero.Summoner2.SpellTexture.Draw(s2Position);

                                    if (hero.Summoner2.Spell.State.HasFlag(SpellState.Cooldown) && hero.Summoner2.TimeUntilReady > 0.0f)
                                    {
                                        Render.Rectangle(s2Bar, hero.Summoner2.CurrentWidth, hero.Summoner2.StatusBoxHeight, Color.FromArgb(150, 0, 0, 0));
                                        var cdString = hero.Summoner2.TimeUntilReady < 1
                                            ? hero.Summoner2.TimeUntilReady.ToString("0.0")
                                            : Math.Ceiling(hero.Summoner2.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                        var tpos = s2Bar + new Vector2((int)((hero.Summoner2.StatusBoxWidth + 7) / 2) - Render.MeasureText(cdString) / 2, 23);
                                        if (Menu["cdTracker"]["spellsTime"].As<MenuBool>().Enabled)
                                        {
                                            RenderFontText1(_font4, cdString, tpos, Color.White);
                                        }
                                    }
                                }
                            }
                            else if (hero.Hero.IsAlly && !hero.Hero.IsMe)
                            {
                                var yAlly = Yally(hero.Hero);
                                if (hero.Hero.ChampionName == "Nidalee" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "Takedown")
                                {
                                    yAlly = 19f;
                                }
                                if (hero.Hero.ChampionName == "Elise" && hero.Hero.SpellBook.GetSpell(SpellSlot.Q).Name == "EliseHumanQ")
                                {
                                    yAlly = 18.5f;
                                }
                                var bpos = hero.Hero.FloatingHealthBarPosition + new Vector2(Xhud(hero.Hero), yAlly);
                                var summonerStart = bpos + new Vector2(XspellsAlly(hero.Hero), YspellsAlly(hero.Hero));

                                var s1Bar = summonerStart + new Vector2(-1f, -1f);
                                if (hero.Summoner1?.SpellTexture != null)
                                {
                                    hero.Summoner1.SpellTexture.Draw(summonerStart);

                                    if (hero.Summoner1.Spell.State.HasFlag(SpellState.Cooldown) && hero.Summoner1.TimeUntilReady > 0.0f)
                                    {
                                        Render.Rectangle(s1Bar, hero.Summoner1.CurrentWidth, hero.Summoner1.StatusBoxHeight, Color.FromArgb(150, 0, 0, 0));
                                        var cdString = hero.Summoner1.TimeUntilReady < 1
                                            ? hero.Summoner1.TimeUntilReady.ToString("0.0")
                                            : Math.Ceiling(hero.Summoner1.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                        var tpos = s1Bar + new Vector2((int)((hero.Summoner1.StatusBoxWidth + 7) / 2) - Render.MeasureText(cdString) / 2, 23);
                                        if (Menu["cdTracker"]["spellsTime"].As<MenuBool>().Enabled)
                                        {
                                            RenderFontText1(_font4, cdString, tpos, Color.White);
                                        }
                                    }
                                }

                                var s2Bar = s1Bar + new Vector2(24, 0);
                                if (hero.Summoner2?.SpellTexture != null)
                                {
                                    var s2Position = summonerStart + new Vector2(24, 0);
                                    hero.Summoner2.SpellTexture.Draw(s2Position);

                                    if (hero.Summoner2.Spell.State.HasFlag(SpellState.Cooldown) && hero.Summoner2.TimeUntilReady > 0.0f)
                                    {
                                        Render.Rectangle(s2Bar, hero.Summoner2.CurrentWidth, hero.Summoner2.StatusBoxHeight, Color.FromArgb(150, 0, 0, 0));
                                        var cdString = hero.Summoner2.TimeUntilReady < 1
                                            ? hero.Summoner2.TimeUntilReady.ToString("0.0")
                                            : Math.Ceiling(hero.Summoner2.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                        var tpos = s2Bar + new Vector2((int)((hero.Summoner2.StatusBoxWidth + 7) / 2) - Render.MeasureText(cdString) / 2, 23);
                                        if (Menu["cdTracker"]["spellsTime"].As<MenuBool>().Enabled)
                                        {
                                            RenderFontText1(_font4, cdString, tpos, Color.White);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var bpos = hero.Hero.FloatingHealthBarPosition + new Vector2(XhudMe(hero.Hero), Yme(hero.Hero));
                                var summonerStart = bpos + new Vector2(XspellsMe(hero.Hero), YspellsMe(hero.Hero));

                                var s1Bar = summonerStart + new Vector2(-1f, -1f);
                                if (hero.Summoner1?.SpellTexture != null)
                                {
                                    hero.Summoner1.SpellTexture.Draw(summonerStart);

                                    if (hero.Summoner1.Spell.State.HasFlag(SpellState.Cooldown) && hero.Summoner1.TimeUntilReady > 0.0f)
                                    {
                                        Render.Rectangle(s1Bar, hero.Summoner1.CurrentWidth, hero.Summoner1.StatusBoxHeight, Color.FromArgb(150, 0, 0, 0));
                                        var cdString = hero.Summoner1.TimeUntilReady < 1
                                            ? hero.Summoner1.TimeUntilReady.ToString("0.0")
                                            : Math.Ceiling(hero.Summoner1.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                        var tpos = s1Bar + new Vector2((int)((hero.Summoner1.StatusBoxWidth + 7) / 2) - Render.MeasureText(cdString) / 2, 23);
                                        if (Menu["cdTracker"]["spellsTime"].As<MenuBool>().Enabled)
                                        {
                                            RenderFontText1(_font4, cdString, tpos, Color.White);
                                        }
                                    }
                                }

                                var s2Bar = s1Bar + new Vector2(24, 0);
                                if (hero.Summoner2?.SpellTexture != null)
                                {
                                    var s2Position = summonerStart + new Vector2(24, 0);
                                    hero.Summoner2.SpellTexture.Draw(s2Position);

                                    if (hero.Summoner2.Spell.State.HasFlag(SpellState.Cooldown) && hero.Summoner2.TimeUntilReady > 0.0f)
                                    {
                                        Render.Rectangle(s2Bar, hero.Summoner2.CurrentWidth, hero.Summoner2.StatusBoxHeight, Color.FromArgb(150, 0, 0, 0));
                                        var cdString = hero.Summoner2.TimeUntilReady < 1
                                            ? hero.Summoner2.TimeUntilReady.ToString("0.0")
                                            : Math.Ceiling(hero.Summoner2.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                                        var tpos = s2Bar + new Vector2((int)((hero.Summoner2.StatusBoxWidth + 8) / 2) - Render.MeasureText(cdString) / 2, 23);
                                        if (Menu["cdTracker"]["spellsTime"].As<MenuBool>().Enabled)
                                        {
                                            RenderFontText1(_font4, cdString, tpos, Color.White);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region - Auto Attack Range -
                if (Menu["taRange"]["taE"].As<MenuBool>().Enabled)
                {
                    var attackRange = hero.Hero.AttackRange + hero.Hero.BoundingRadius;
                    if (hero.Hero.IsEnemy && hero.Hero.IsVisible && Menu["taRange"]["aaE"].As<MenuBool>().Enabled)
                    {
                        Render.Circle(hero.Hero.Position, attackRange, 180, Player.Distance(hero.Hero) < attackRange ? Color.FromArgb(150, Color.Red) : Color.FromArgb(150, Color.ForestGreen));
                    }
                    if (hero.Hero.IsAlly && hero.Hero.IsVisible && !hero.Hero.IsMe && Menu["taRange"]["aaA"].As<MenuBool>().Enabled)
                    {
                        Render.Circle(hero.Hero.Position, attackRange, 180, Player.Distance(hero.Hero) < attackRange ? Color.FromArgb(150, Color.Red) : Color.FromArgb(150, Color.ForestGreen));
                    }
                }
                #endregion

                #region - Base Ulti -
                if (Menu["baseUlti"]["baseUltiE"].As<MenuKeyBind>().Enabled || Menu["baseUlti"]["baseUltiD"].As<MenuBool>().Enabled && hero.Hero.IsEnemy)
                {
                    var secondsLeft = hero.RecallEnd + hero.RecallTime - Game.ClockTime;
                    var secondsPassed = hero.RecallTime - secondsLeft;
                    var percent = secondsPassed / hero.RecallTime * 100;
                    var delay = 0;
                    var speed = 0;
                    if (Player.ChampionName == "Ashe")
                    {
                        R.SetSkillshot(250, 130, 1600, true, SkillshotType.Line);
                        delay = 250;
                        speed = 1600;
                    }
                    if (Player.ChampionName == "Draven")
                    {
                        R.SetSkillshot(300, 160, 2000, true, SkillshotType.Line);
                        delay = 300;
                        speed = 2000;
                    }
                    if (Player.ChampionName == "Ezreal")
                    {
                        R.SetSkillshot(1000, 160, 2000, false, SkillshotType.Line);
                        delay = 160;
                        speed = 2000;
                    }
                    if (Player.ChampionName == "Jinx")
                    {
                        R.SetSkillshot(500, 140, 2200, true, SkillshotType.Line);
                        delay = 140;
                        speed = 2200;
                    }
                    const int baseultiBarWidth = 220;
                    var baseultiStartX = (Menu["baseUlti"]["invertXB"].Enabled ? -Menu["baseUlti"]["xOffsetB"].Value : Menu["baseUlti"]["xOffsetB"].Value) + (Render.Width / 2 - baseultiBarWidth / 2);
                    var baseultiStartY = (Menu["baseUlti"]["invertYB"].Enabled ? -Menu["baseUlti"]["yOffsetB"].Value : Menu["baseUlti"]["yOffsetB"].Value) + (Render.Height - 157);

                    Vector3 xy;
                    if (Game.Type == GameType.Normal && Player.Team == GameObjectTeam.Order)
                    {
                        xy = new Vector3(14340, 171.9777f, 14390);
                    }
                    else if (Game.Type == GameType.Normal && Player.Team == GameObjectTeam.Chaos)
                    {
                        xy = new Vector3(396, 185.1325f, 462);
                    }
                    else
                    {
                        xy = Vector3.Zero;
                    }

                    var travelTime = Player.Distance(xy) / speed * 1000 + delay + Game.Ping / 2f;
                    var castTime = (int)(-(hero.TickCount - (hero.Start + hero.Duration)) - travelTime);
                    var castUltiTime = (int)(-(Game.TickCount - (hero.Start + hero.Duration)) - travelTime);
                    if (Game.ClockTime - hero.StartRecallTime < hero.RecallTime && !hero.Abort)
                    {
                        if (Game.ClockTime >= 10)
                        {
                            if (Player.GetSpellDamage(hero.Hero, SpellSlot.R) < hero.Hero.Health + 50 && Menu["baseUlti"]["baseUltiD"].As<MenuBool>().Enabled)
                            {
                                var w1 = 222 + Menu["baseUlti"]["scalingB"].Value - (222 + Menu["baseUlti"]["scalingB"].Value) * (percent / 100);
                                var h1 = 7 + Menu["baseUlti"]["scalingB"].Value / 4;
                                DrawRect(baseultiStartX, baseultiStartY, w1, h1, 1, Color.FromArgb((int)(100f * 1f), Color.White));

                                var w2 = 221 + Menu["baseUlti"]["scalingB"].Value - (221 + Menu["baseUlti"]["scalingB"].Value) * (percent / 100);
                                DrawRect(baseultiStartX + w2, baseultiStartY - 7, 1, 6, 2, Color.FromArgb((int)(255f * 1f), Color.White));
                                
                                var w3 = 221 + Menu["baseUlti"]["scalingB"].Value - (221 + Menu["baseUlti"]["scalingB"].Value) * (percent / 100) - (float)hero.Hero.ChampionName.Length * _font6.Width / 2;
                                RenderFontText1(_font6, hero.Hero.ChampionName, new Vector2(baseultiStartX + w3, baseultiStartY - 19), Color.WhiteSmoke);
                            }                           
                            var barY = (Menu["baseUlti"]["invertYB"].Enabled ? -Menu["baseUlti"]["yOffsetB"].Value : Menu["baseUlti"]["yOffsetB"].Value) + (Render.Height * 0.8f);
                            const int barHeight = 6;
                            var barX = Render.Width * 0.580f;
                            var barWidth = Render.Width - 2 * barX;
                            var scale =  barWidth / 8000;
                            if (Player.GetSpellDamage(hero.Hero, SpellSlot.R) > hero.Hero.Health + 50)
                            {
                                if (Menu["baseUlti"]["baseUltiD"].As<MenuBool>().Enabled)
                                {
                                    if (castTime >= 0)
                                    {
                                        if (Player.ChampionName == "Ashe" || Player.ChampionName == "Draven" ||                         Player.ChampionName == "Ezreal" || Player.ChampionName == "Jinx")
                                        {
                                            Render.Rectangle(
                                                (Menu["baseUlti"]["invertXB"].Enabled ? -Menu["baseUlti"]["xOffsetB"].Value : Menu["baseUlti"]["xOffsetB"].Value) + Menu["baseUlti"]["scalingB"].Value + barX + scale * castTime,
                                                barY + 5 + barHeight - 7 + (float)Menu["baseUlti"]["scalingB"].Value / 4, 
                                                2, 10, Color.Orange);
                                        }
                                    }
                                    var w1 = 222 + Menu["baseUlti"]["scalingB"].Value - (222 + Menu["baseUlti"]["scalingB"].Value) * (percent / 100);
                                    var h1 = 7 + Menu["baseUlti"]["scalingB"].Value / 4;
                                    DrawRect(baseultiStartX, baseultiStartY, w1, h1, 1, Color.FromArgb(255, Color.Red));

                                    var w2 = 221 + Menu["baseUlti"]["scalingB"].Value - (221 + Menu["baseUlti"]["scalingB"].Value) * (percent / 100);
                                    DrawRect(baseultiStartX + w2, baseultiStartY + 9 + (float)Menu["baseUlti"]["scalingB"].Value / 4, 1, 7, 2, Color.FromArgb((int)(255f * 1f), Color.Orange));

                                    var w3 = 221 + Menu["baseUlti"]["scalingB"].Value - (221 + Menu["baseUlti"]["scalingB"].Value) * (percent / 100) - (float)hero.Hero.ChampionName.Length * _font6.Width / 2;
                                    RenderFontText1(_font7, hero.Hero.ChampionName, new Vector2(baseultiStartX + w3, baseultiStartY + 18 + (float)Menu["baseUlti"]["scalingB"].Value / 4), Color.DarkOrange);

                                }
                                if (castUltiTime <= Game.Ping && Menu["baseUlti"]["baseUltiE"].As<MenuKeyBind>().Enabled)
                                {
                                    if (Menu["baseUlti"]["ulti" + hero.Hero.ChampionName.ToLower()].As<MenuBool>().Enabled)
                                    {
                                        R.Cast(xy);
                                    }
                                }
                            }

                            if (Menu["baseUlti"]["baseUltiD"].As<MenuBool>().Enabled)
                            {
                                /*Zemin*/
                                var w4 = 222 + Menu["baseUlti"]["scalingB"].Value;
                                var h4 = 7 + Menu["baseUlti"]["scalingB"].Value / 4;
                                DrawRect(baseultiStartX, baseultiStartY, w4, h4, 1, Color.FromArgb((int)(40f * 1f), Color.White));
                                /*Zemin*/
                                /*Çerçeve*/
                                var w5 = 222 + Menu["baseUlti"]["scalingB"].Value;
                                DrawRect(baseultiStartX, baseultiStartY - 1, w5, 1, 1, Color.FromArgb((int)(255f * 1f), Color.White));

                                var w6 = 222 + Menu["baseUlti"]["scalingB"].Value;
                                var h6 = 7 + Menu["baseUlti"]["scalingB"].Value / 4;
                                DrawRect(baseultiStartX, baseultiStartY + h6, w6, 1, 1, Color.FromArgb((int)(255f * 1f), Color.White));

                                var h7 = 7 + Menu["baseUlti"]["scalingB"].Value / 4;
                                DrawRect(baseultiStartX, baseultiStartY, 1, h7, 1, Color.FromArgb((int)(255f * 1f), Color.White));

                                var w8 = 221 + Menu["baseUlti"]["scalingB"].Value;
                                var h8 = 7 + Menu["baseUlti"]["scalingB"].Value / 4;
                                DrawRect(baseultiStartX + w8, baseultiStartY, 1, h8, 1, Color.FromArgb((int)(255f * 1f), Color.White));
                                /*Çerçeve*/
                            }
                        }
                    }
                }
                #endregion
            }

            #region - Auto Level Up Error -
            if (Menu[Player.ChampionName + "autoLevel"]["levelMode"].As<MenuList>().SelectedItem == "Priority")
            {
                const int barWidth = 240;
                const int barHeight = 20;
                var startX = Render.Width / 2 - barWidth / 2;
                var startY = Render.Height - 157;
                if (_lvl2 == _lvl3 || _lvl2 == _lvl4 || _lvl3 == _lvl4)
                {
                    var drawY = startY - (barHeight + 5);
                    var rect = new Rectangle(startX, drawY, startX + barWidth, drawY + barHeight);
                    Render.Rectangle(startX, drawY, barWidth, barHeight, Color.FromArgb(150, 0, 0, 0));
                    Render.Text("Priority can not have the same values!", rect, RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                }
            }

            if (Menu[Player.ChampionName + "autoLevel"]["levelMode"].As<MenuList>().SelectedItem == "Disable" && Game.ClockTime >= 0 && Game.ClockTime < 15)
            {
                const int barWidth = 295;
                const int barHeight = 20;
                var startX = Render.Width / 2 - barWidth / 2;
                var startY = Render.Height - 157;
                var drawY = startY - (barHeight + 5);
                var rect = new Rectangle(startX, drawY, startX + barWidth, drawY + barHeight);
                Render.Rectangle(startX, drawY, barWidth, barHeight, Color.FromArgb(150, 0, 0, 0));
                Render.Text("AutoLevel disabled by default for every champion!", rect, RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
            }
            #endregion

            #region - Game Hidden Objects Tracker -
            if (Menu["wardTracker"]["wardE"].As<MenuBool>().Enabled)
            {
                foreach (var ward in Tracker.HiddenObjectsList)
                {
                    if (ward.Type == 2)
                    {
                        float levelCal = 0;
                        if (ward.WardName == "TrinketTotemLvl1")
                        {
                            levelCal = 3.5f * (ward.Level - 1);
                        }
                        Render.Circle(ward.Location, 75, 50, Color.Yellow);
                        Tracker.WardMiniMap.Draw(ward.Location.ToMiniMapPosition() + new Vector2(-6, -6));
                        if (Menu["wardTracker"]["wardT"].As<MenuBool>().Enabled)
                        {
                            Render.Text($"{TextCenter(ward.EndTime + levelCal - Game.ClockTime)}", ward.Location.ToScreenPosition() + new Vector2(-9, -9), RenderTextFlags.Center, Color.White);
                        }
                    }
                    else if (ward.Type == 1 && Menu["wardTracker"]["pinkE"].As<MenuBool>().Enabled)
                    {
                        Render.Circle(ward.Location, 75, 50, Color.DeepPink);
                        Tracker.PinkMiniMap.Draw(ward.Location.ToMiniMapPosition() + new Vector2(-6, -6));
                    }
                    else if (ward.Type == 4)
                    {
                        Render.Circle(ward.Location, 75, 50, Color.Aqua);
                    }
                    else if (ward.Type == 3)
                    {
                        if (ward.WardName == "JhinE" && Menu["wardTracker"]["jhinE"].As<MenuBool>().Enabled)
                        {
                            Render.Circle(ward.Location, 150, 50, Color.Orange);
                        }
                        else if (ward.WardName == "TeemoRCast" && Menu["wardTracker"]["teemoE"].As<MenuBool>().Enabled)
                        {
                            Render.Circle(ward.Location, 125, 50, Color.Red);
                        }
                        else if (ward.WardName == "Bushwhack" && Menu["wardTracker"]["nidaleeE"].As<MenuBool>().Enabled)
                        {
                            Render.Circle(ward.Location, 100, 50, Color.GreenYellow);
                        }
                        else if (ward.WardName == "JackInTheBox" && Menu["wardTracker"]["shacoE"].As<MenuBool>().Enabled)
                        {
                            Render.Circle(ward.Location, 75, 50, Color.LimeGreen);
                        }

                        if (ward.WardName == "JhinE" && Menu["wardTracker"]["jhinT"].As<MenuBool>().Enabled)
                        {
                            Render.Text($"{TextCenter(ward.EndTime - Game.ClockTime)}", ward.Location.ToScreenPosition() + new Vector2(-9, -9), RenderTextFlags.Center, Color.White);
                        }
                        else if (ward.WardName == "TeemoRCast" && Menu["wardTracker"]["teemoT"].As<MenuBool>().Enabled)
                        {
                            Render.Text($"{TextCenter(ward.EndTime - Game.ClockTime)}", ward.Location.ToScreenPosition() + new Vector2(-9, -9), RenderTextFlags.Center, Color.White);
                        }
                        else if (ward.WardName == "Bushwhack" && Menu["wardTracker"]["nidaleeT"].As<MenuBool>().Enabled)
                        {
                            Render.Text($"{TextCenter(ward.EndTime - Game.ClockTime)}", ward.Location.ToScreenPosition() + new Vector2(-9, -9), RenderTextFlags.Center, Color.White);
                        }
                        else if (ward.WardName == "JackInTheBox" && Menu["wardTracker"]["shacoT"].As<MenuBool>().Enabled)
                        {
                            Render.Text($"{TextCenter(ward.EndTime - Game.ClockTime)}", ward.Location.ToScreenPosition() + new Vector2(-9, -9), RenderTextFlags.Center, Color.White);
                        }
                    }
                    else if (ward.Type == 5 && ward.WardName == "CaitlynYordleTrap" && Menu["wardTracker"]["caitlynE"].As<MenuBool>().Enabled)
                    {
                        Render.Circle(ward.Location, 75, 50, Color.Red);
                    }
                }
            }
            #endregion

            #region - Hero Side Hud -
            if (Menu["sideHud"]["hudE"].As<MenuBool>().Enabled)
            {
                var enemies = Tracker.HeroInfoList.Where(x => x.Hero.IsEnemy && !x.Hero.IsDead).ToList();
                var enemiesDead = Tracker.HeroInfoList.Where(x => x.Hero.IsEnemy && x.Hero.IsDead).ToList();
                var dead = 0;
                var deadX = 0;
                for (var i = 0; i < enemiesDead.Count; i++)
                {
                    dead++;
                }
                if (dead == 1)
                {
                    deadX = 46;
                }
                else if (dead == 2)
                {
                    deadX = 92;
                }
                else if (dead == 3)
                {
                    deadX = 138;
                }
                else if (dead == 4)
                {
                    deadX = 184;
                }
                else if (dead == 5)
                {
                    deadX = 0;
                }
                for (var i = 0; i < enemies.Count; i++)
                {
                    var champion = enemies[i];
                    var startX = Render.Width - 45 * (i + 1) - deadX + (Menu["sideHud"]["invertX"].Enabled ? -Menu["sideHud"]["xOffset"].Value : Menu["sideHud"]["xOffset"].Value);
                    var startY = Render.Height - 325 + (Menu["sideHud"]["invertY"].Enabled ? -Menu["sideHud"]["yOffset"].Value
                                     : Menu["sideHud"]["yOffset"].Value);
                    var championRect = new Rectangle(startX + i * -1, startY, startX + 48, startY + 48);
                    var summoner1HudRect = new Rectangle(championRect.Left, championRect.Top - 19, championRect.Left + 21, championRect.Top - 19);
                    var summoner2HudRect = new Rectangle(summoner1HudRect.Left + 21, summoner1HudRect.Top, summoner1HudRect.Left + 19 + 19, summoner1HudRect.Top + 19 + 19);
                    var summonerUltiRect = new Rectangle(summoner1HudRect.Left + 21, summoner1HudRect.Top, summoner1HudRect.Left + 19 + 19, summoner1HudRect.Top + 19 + 19);
                    if (champion.Hero.IsVisible)
                    {
                        champion.HudSprite.Draw(new Vector2(championRect.Left, championRect.Top));
                    }
                    else
                    {
                        champion.HudSprite.Draw(new Vector2(championRect.Left, championRect.Top), Color.DimGray);
                    }
                    if (champion.Summoner1Hud?.SpellTextureHud != null)
                    {
                        if (!champion.Summoner1Hud.Spell.State.HasFlag(SpellState.Cooldown))
                        {
                            champion.Summoner1Hud.SpellTextureHud.Draw(new Vector2(summoner1HudRect.Left + 1, summoner1HudRect.Top + 6));
                        }

                        if (champion.Summoner1Hud.Spell.State.HasFlag(SpellState.Cooldown) && champion.Summoner1Hud.TimeUntilReady > 0.0f)
                        {
                            champion.Summoner1Hud.SpellTextureHud.Draw(new Vector2(summoner1HudRect.Left + 1, summoner1HudRect.Top + 6), Color.DimGray);

                            var timer = champion.Summoner1Hud.TimeUntilReady < 1 ? champion.Summoner1Hud.TimeUntilReady.ToString("0.0") : Math.Ceiling(champion.Summoner1Hud.TimeUntilReady).ToString(CultureInfo.InvariantCulture);

                            if (Menu["sideHud"]["spellsTime"].As<MenuBool>().Enabled)
                            {
                                RenderFontText2(_font3, timer, new Rectangle(summoner1HudRect.Left, summoner1HudRect.Top, summoner1HudRect.Left + 23, summoner1HudRect.Top + 31), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                            }
                        }
                    }
                    if (champion.Summoner2Hud?.SpellTextureHud != null)
                    {
                        if (!champion.Summoner2Hud.Spell.State.HasFlag(SpellState.Cooldown))
                        {
                            champion.Summoner2Hud.SpellTextureHud.Draw(new Vector2(summoner2HudRect.Left - 1, summoner2HudRect.Top + 6));
                        }

                        if (champion.Summoner2Hud.Spell.State.HasFlag(SpellState.Cooldown) && champion.Summoner2Hud.TimeUntilReady > 0.0f)
                        {
                            champion.Summoner2Hud.SpellTextureHud.Draw(new Vector2(summoner2HudRect.Left - 1, summoner2HudRect.Top + 6), Color.DimGray);
                            var timer = champion.Summoner2Hud.TimeUntilReady < 1 ? champion.Summoner2Hud.TimeUntilReady.ToString("0.0") : Math.Ceiling(champion.Summoner2Hud.TimeUntilReady).ToString(CultureInfo.InvariantCulture);
                            if (Menu["sideHud"]["spellsTime"].As<MenuBool>().Enabled)
                            {
                                RenderFontText2(_font3, timer, new Rectangle(summoner2HudRect.Left, summoner2HudRect.Top, summoner2HudRect.Left + 19, summoner2HudRect.Top + 31), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                            }
                        }
                    }
                    if (!champion.SummonerRHud.Spell.State.HasFlag(SpellState.Cooldown) && !champion.SummonerRHud.Spell.State.HasFlag(SpellState.NotLearned))
                    {
                        champion.SummonerRHud.UltiTextureHud.Draw(new Vector2(summonerUltiRect.Left - 10, summonerUltiRect.Top - 9));
                    }
                    if (champion.SummonerRHud.Spell.State.HasFlag(SpellState.NotLearned))
                    {
                        champion.SummonerRHud.UltiTextureHud.Draw(new Vector2(summonerUltiRect.Left - 10, summonerUltiRect.Top - 9), Color.DimGray);
                    }
                    if (champion.SummonerRHud.Spell.State.HasFlag(SpellState.Cooldown) && champion.SummonerRHud.TimeUntilReady > 0.0f)
                    {
                        champion.SummonerRHud.UltiTextureHud.Draw(new Vector2(summonerUltiRect.Left - 10, summonerUltiRect.Top - 9), Color.DimGray);
                        var timer = champion.SummonerRHud.TimeUntilReady < 1 ? champion.SummonerRHud.TimeUntilReady.ToString("0.0") : Math.Ceiling(champion.SummonerRHud.TimeUntilReady).ToString(CultureInfo.InvariantCulture);
                        if (Menu["sideHud"]["spellsTime"].As<MenuBool>().Enabled)
                        {
                            RenderFontText2(_font3, timer, new Rectangle(summonerUltiRect.Left - 20, summonerUltiRect.Top - 30, summonerUltiRect.Left + 19, summonerUltiRect.Top + 31), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                        }
                    }
                    var levelRect = new Rectangle(championRect.Right - 30 - i * 2, championRect.Bottom - 26, championRect.Right, championRect.Bottom);
                    RenderFontText2(_font2, champion.Hero.Level.ToString(), levelRect, RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                    var healthRect = new Rectangle(championRect.Left + 1, championRect.Top + 44, championRect.Left + 48, championRect.Top + 48 + 5);
                    Render.Rectangle(new Vector2(healthRect.Left, healthRect.Top), 40, 13, Color.FromArgb(150, Color.Black));
                    var healthPct = champion.Hero.HealthPercent();
                    var color = Color.FromArgb(255, 35, 193, 26);
                    if (healthPct > 30 && healthPct < 50)
                    {
                        color = Color.DarkOrange;
                    }
                    else if (healthPct < 30)
                    {
                        color = Color.OrangeRed;
                    }
                    Render.Rectangle(new Vector2(healthRect.Left + 1, healthRect.Top + 1), 38 * (healthPct / 100), 5, color);
                    var manaPct = champion.Hero.ManaPercent();
                    Render.Rectangle(new Vector2(healthRect.Left + 1, healthRect.Top + 7), 38 * (manaPct / 100), 5, Color.FromArgb(255, 75, 155, 238));
                    var secondsLeft = champion.RecallEnd + champion.RecallTime - Game.ClockTime;
                    var secondsPassed = champion.RecallTime - secondsLeft;
                    var percent = secondsPassed / champion.RecallTime * 100;
                    var reCall = new Rectangle(healthRect.Left, healthRect.Top + 13, healthRect.Left + 48, healthRect.Top + 5 + 5);    
                    if (Game.ClockTime - champion.FinishRecallTime < 4)
                    {
                        if (Game.ClockTime >= 10)
                        {
                            RenderFontText2(_font2, "Finish", new Rectangle(reCall.Left - 35, reCall.Top - 40, reCall.Left + 75, reCall.Top - 30), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.Yellow);
                        }
                    }
                    else if (champion.StartRecallTime <= champion.AbortRecallTime && Game.ClockTime - champion.AbortRecallTime < 4)
                    {
                        if (Game.ClockTime >= 10)
                        {
                            RenderFontText2(_font2, "Abort", new Rectangle(reCall.Left - 35, reCall.Top - 40, reCall.Left + 75, reCall.Top - 30), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.Yellow);
                        }
                    }
                    else if (Game.ClockTime - champion.StartRecallTime < champion.RecallTime && !champion.Abort)
                    {
                        if (Game.ClockTime >= 10)
                        {
                            Render.Rectangle(new Vector2(reCall.Left, reCall.Top), 40, 6, Color.FromArgb(150, Color.Black));
                            Render.Rectangle(new Vector2(reCall.Left + 1, reCall.Top), 38 * (percent / 100), 5, Color.Yellow);
                            if (flashing)
                            {
                                RenderFontText2(_font2, "Recall", new Rectangle(reCall.Left - 35, reCall.Top - 40, reCall.Left + 75, reCall.Top - 30), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);
                            }
                        }      
                    }
                    else if (!champion.Hero.IsVisible)
                    {
                        var visibleRect = new Rectangle(championRect.Left, championRect.Top - 19, championRect.Left + 21, championRect.Top - 19);
                        var timeV = Math.Floor(champion.SecondsSinceSeen).ToString(CultureInfo.InvariantCulture);
                        if (Math.Floor(champion.SecondsSinceSeen) > 240 || Math.Abs(Math.Floor(champion.SecondsSinceSeen)) <= 0)
                        {
                            timeV = "";
                        }
                        RenderFontText2(_font4, timeV, new Rectangle(visibleRect.Left + 8, visibleRect.Top + 35, visibleRect.Left + 35, visibleRect.Top + 45), RenderTextFlags.VerticalCenter | RenderTextFlags.HorizontalCenter, Color.White);      
                    }
                    #region - Hero Exp -
                    if (Game.ClockTime - champion.StartRecallTime < champion.RecallTime && !champion.Abort)
                    {
                        
                    }
                    else
                    {
                        Render.Rectangle(new Vector2(reCall.Left, reCall.Top), 40, 3, Color.FromArgb(150, Color.Black));
                        var actualExp = champion.Hero.Exp;
                        if (champion.Hero.Level > 1)
                        {
                            actualExp -= (280 + 80 + 100 * champion.Hero.Level) / 2 * (champion.Hero.Level - 1);
                        }
                        
                        var levelLimit = champion.Hero.HasBuff("AwesomeBuff") ? 30 : 18;
                        if (champion.Hero.Level < levelLimit)
                        {
                            var neededExp = 180 + 100 * champion.Hero.Level;
                            var expPercent = (int)(actualExp / neededExp * 100);
                            if (expPercent > 0)
                            {
                                Render.Rectangle(new Vector2(reCall.Left + 1, reCall.Top), 0 + (float)(0.38 * expPercent), 2, Color.FromArgb(156, 82, 238));
                            }
                        }
                    }
                    #endregion
                }
            }
            #endregion

            #region - Tower Attack Range -
            if (Menu["taRange"]["taE"].As<MenuBool>().Enabled)
            {        
                foreach (var tower in ObjectManager.Get<Obj_AI_Turret>().Where(t => !t.IsDead && t.IsVisible && t.Name != "Turret_ChaosTurretShrine_A" && t.Name != "Turret_OrderTurretShrine_A"))
                {
                    if (tower.IsEnemy && !Menu["taRange"]["trE"].As<MenuBool>().Enabled)
                    {
                        continue;
                    }

                    if (tower.IsAlly && !Menu["taRange"]["trA"].As<MenuBool>().Enabled)
                    {
                        continue;
                    }
                    var towerAutoAttackRange = 775f + tower.BoundingRadius + Player.BoundingRadius - 15f;
                    Render.Circle(tower.ServerPosition, towerAutoAttackRange, 180, tower.IsEnemy && Player.Distance(tower) <= towerAutoAttackRange ? Color.FromArgb(150, Color.Red) : Color.FromArgb(150, Color.ForestGreen));   
                }
            }
            #endregion

            #region - Gank Alerter -
            if (Menu["gankAlerter"]["gankAlerterE"].As<MenuBool>().Enabled)
            {
                var jungler = Tracker.HeroInfoList.FirstOrDefault(x => x.Hero.IsEnemy && x.IsJungler);                       
                var startX = ((Menu["gankAlerter"]["invertXG"].Enabled ? - Menu["gankAlerter"]["xOffsetG"].Value : Menu["gankAlerter"]["xOffsetG"].Value) + 720) * 0.001f * Render.Width;
                var startY = ((Menu["gankAlerter"]["invertYG"].Enabled ? - Menu["gankAlerter"]["yOffsetG"].Value : Menu["gankAlerter"]["yOffsetG"].Value) + 900) * 0.001f * Render.Height;

                if (jungler != null)
                {
                    string info;
                    float percent;
                    var distance = jungler.LastVisablePos.Distance(Player.Position);
                    Render.Rectangle(new Vector2(startX -1, startY - 1), 120, 18, Color.FromArgb(150, Color.Black));
                    Render.Rectangle(new Vector2(startX, startY), 118, 16, Color.DarkGreen);  
                    if (Game.ClockTime - jungler.FinishRecallTime < 4)
                    {
                        info = "Jungler in base";
                        percent = 0;
                    }
                    else if (jungler.Hero.IsDead)
                    {
                        info = "Jungler dead";
                        percent = 0;
                    }
                    else if (distance < 3500)
                    {
                        info = "   Jungler Near you";
                        percent = 1;
                    }
                    else if (jungler.Hero.IsVisible)
                    {
                        info = "Jungler visable";
                        percent = 0;
                    }
                    else
                    {
                        var timer = jungler.LastVisablePos.Distance(Player.Position) / 330;
                        var time2 = timer - (Game.ClockTime - jungler.LastSeenWhen);
                        if ((int)time2 < 0 && (int)time2 > -350)
                        {
                            info = "Jungler in jungle " + (int)time2;
                        }
                        else if ((int) time2 >= 0)
                        {
                            info = "Jungler in jungle " + (int) time2;
                        }
                        else
                        {
                            info = "Jungler in jungle ---";
                        }            
                        time2 = time2 - 10;
                        if (time2 > 0)
                        {
                            percent = 0;
                        }
                        else
                        {
                            percent = (-time2) * 0.05f;
                        }
                        percent = Math.Min(percent, 1);
                    }
                    if (Math.Abs(percent) > 0 || Math.Abs(percent) < 0)
                    {
                        Render.Rectangle(new Vector2(startX, startY), 118 * percent, 16, distance < 3500 ? Color.Green : Color.DimGray);
                    }
                    RenderFontText1(_font2, info, new Vector2(startX + 5, startY + 2), Color.WhiteSmoke);
                }
                else
                {
                    if (Game.ClockTime >= 0 && Game.ClockTime < 35)
                    {
                        Render.Rectangle(new Vector2(startX - 1, startY - 1), 120, 18, Color.FromArgb(150, Color.Black));
                        RenderFontText1(_font2, " Jungler not detected", new Vector2(startX + 5, startY + 2), Color.White);
                    }
                }       
            }
            #endregion

            #region - Auto Smite Draw and Monster Damage Indicator -
            if (IsSmite != null)
            {
                if (Menu["autosmite"]["smitedrawE"].As<MenuBool>().Enabled && Menu["autosmite"]["autosmiteE"].As<MenuBool>().Enabled)
                {
                    Render.Circle(Player.Position, _smite.Range, 100, Menu["autosmite"]["autosmiteKey"].As<MenuKeyBind>().Enabled ? Color.FromArgb(150, Color.White) : Color.FromArgb(150, Color.DimGray));
                }
            }
            if (Menu["autosmite"]["smitedamgeE"].As<MenuBool>().Enabled)
            {
                foreach (var jung in ObjectManager.Get<Obj_AI_Minion>().Where(x => x.IsValid && x.IsVisible && !x.IsDead && !x.FloatingHealthBarPosition.IsZero && (x.UnitSkinName == "SRU_Blue" || x.UnitSkinName == "SRU_Red" || x.UnitSkinName == "SRU_Baron"  || x.UnitSkinName == "SRU_RiftHerald" || x.UnitSkinName == "SRU_Krug" || x.UnitSkinName == "SRU_Razorbeak" || x.UnitSkinName == "SRU_Murkwolf" || x.UnitSkinName == "SRU_Gromp" || x.UnitSkinName == "Sru_Crab" || x.UnitSkinName == "SRU_Dragon_Earth" || x.UnitSkinName == "SRU_Dragon_Air" || x.UnitSkinName == "SRU_Dragon_Fire" || x.UnitSkinName == "SRU_Dragon_Water" || x.UnitSkinName == "SRU_Dragon_Elder")))
                {
                    float xOffset = 0;
                    float yOffset = 0;
                    float healtBarW = 0;
                    float healtBarH = 0;

                    if (jung.UnitSkinName == "SRU_Blue" || jung.UnitSkinName == "SRU_Red")
                    {
                        xOffset = 7.9f;
                        yOffset = 25.82f;
                        healtBarW = 144;
                        healtBarH = 9;
                    }
                    if (jung.UnitSkinName == "SRU_Gromp" || jung.UnitSkinName == "SRU_Murkwolf" || jung.UnitSkinName == "SRU_Razorbeak")
                    {
                        xOffset = 0.9f;
                        yOffset = 8.82f;
                        healtBarW = 90;
                        healtBarH = 3.999f;
                    }
                    if (jung.UnitSkinName == "SRU_Krug")
                    {
                        xOffset = -14.9f;
                        yOffset = 8.82f;
                        healtBarW = 90;
                        healtBarH = 3.999f;
                    }
                    if (jung.UnitSkinName == "SRU_Baron")
                    {
                        xOffset = 27.9f;
                        yOffset = 12.82f;
                        healtBarW = 170;
                        healtBarH = 13;
                    }
                    if (jung.UnitSkinName == "SRU_RiftHerald")
                    {
                        xOffset = 6.9f;
                        yOffset = 25.82f;
                        healtBarW = 145;
                        healtBarH = 10;
                    }
                    if (jung.UnitSkinName == "SRU_Dragon_Earth" || jung.UnitSkinName == "SRU_Dragon_Air" || jung.UnitSkinName == "SRU_Dragon_Fire" || jung.UnitSkinName == "SRU_Dragon_Water")
                    {
                        xOffset = 7.3f;
                        yOffset = 24.5f;
                        healtBarW = 145;
                        healtBarH = 12;
                    }
                    if (jung.UnitSkinName == "SRU_Dragon_Elder")
                    {
                        xOffset = -3.9f;
                        yOffset = 12.82f;
                        healtBarW = 170;
                        healtBarH = 13;
                    }
                    var barPos = jung.FloatingHealthBarPosition;
                    barPos.X += xOffset;
                    barPos.Y += yOffset;
                    var drawEndXPos = barPos.X + healtBarW * (jung.HealthPercent() / 100);
                    var drawStartXPos = (barPos.X + (jung.Health > SmiteDamages ? healtBarW * ((jung.Health - SmiteDamages) / jung.MaxHealth * 100 / 100) : 0));
                    Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, healtBarH, true, Color.FromArgb(180, Color.Green));
                }
            }
            #endregion
        }

        #region - Auto Level Up -    
        private static void On_Level_Up(Obj_AI_Base sender, Obj_AI_BaseLevelUpEventArgs args)
        {
            if (!sender.IsMe || Menu[Player.ChampionName + "autoLevel"]["levelMode"].As<MenuList>().SelectedItem == "Disable" || PlayerLevel < Menu[Player.ChampionName + "autoLevel"]["levelStart"].Value || !Menu["enabled"]["uEnabled"].As<MenuBool>().Enabled) return;
            if (Menu[Player.ChampionName + "autoLevel"]["levelMode"].As<MenuList>().SelectedItem == "Priority")
            {
                if (_lvl2 == _lvl3 || _lvl2 == _lvl4 || _lvl3 == _lvl4) return;
                DelayAction.Queue(Delay, () => LevelUp(_lvl1));
                DelayAction.Queue(Delay + 50, () => LevelUp(_lvl2));
                DelayAction.Queue(Delay + 100, () => LevelUp(_lvl3));
                DelayAction.Queue(Delay + 150, () => LevelUp(_lvl4));
            }
            if (Menu[Player.ChampionName + "autoLevel"]["levelMode"].As<MenuList>().SelectedItem == "Meta" && LevelFull == false)
            {
                var i = 0;
                foreach (var level in Meta(Player))
                {
                    i++;
                    switch (PlayerLevel)
                    {
                        case 1:
                            if (i == 1)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 2:
                            if (i == 2)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 3:
                            if (i == 3)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 4:
                            if (i == 4)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 5:
                            if (i == 5)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 6:
                            if (i == 6)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 7:
                            if (i == 7)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 8:
                            if (i == 8)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 9:
                            if (i == 9)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 10:
                            if (i == 10)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 11:
                            if (i == 11)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 12:
                            if (i == 12)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 13:
                            if (i == 13)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 14:
                            if (i == 14)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 15:
                            if (i == 15)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 16:
                            if (i == 16)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 17:
                            if (i == 17)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            break;
                        case 18:
                            if (i == 18)
                            {
                                switch (level)
                                {
                                    case 0: DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.Q)); break;
                                    case 1: DelayAction.Queue(Delay + 50, () => Player.SpellBook.LevelSpell(SpellSlot.W)); break;
                                    case 2: DelayAction.Queue(Delay + 100, () => Player.SpellBook.LevelSpell(SpellSlot.E)); break;
                                    case 3: DelayAction.Queue(Delay + 150, () => Player.SpellBook.LevelSpell(SpellSlot.R)); break;
                                }
                            }
                            LevelFull = false;
                            break;
                    }
                }
            }
            if (Menu[Player.ChampionName + "autoLevel"]["levelMode"].As<MenuList>().SelectedItem == "OnlyUlti(R)" && PlayerLevel > 5)
            {
                DelayAction.Queue(Delay, () => Player.SpellBook.LevelSpell(SpellSlot.R));
            }
        }

        private static void LevelUp(int indx)
        {
            if (PlayerLevel < 5)
            {
                if (indx == 0 && Player.SpellBook.GetSpell(SpellSlot.Q).Level == 0)
                {
                    Player.SpellBook.LevelSpell(SpellSlot.Q);
                }
                if (indx == 1 && Player.SpellBook.GetSpell(SpellSlot.W).Level == 0)
                {
                    Player.SpellBook.LevelSpell(SpellSlot.W);
                }
                if (indx == 2 && Player.SpellBook.GetSpell(SpellSlot.E).Level == 0)
                {
                    Player.SpellBook.LevelSpell(SpellSlot.E);
                }
            }
            else
            {
                switch (indx)
                {
                    case 0: Player.SpellBook.LevelSpell(SpellSlot.Q); break;
                    case 1: Player.SpellBook.LevelSpell(SpellSlot.W); break;
                    case 2: Player.SpellBook.LevelSpell(SpellSlot.E); break;
                    case 3: Player.SpellBook.LevelSpell(SpellSlot.R); break;
                }
            }
        }

        private static IEnumerable<int> Meta(Obj_AI_Hero champion)
        {
            const int q = 0, w = 1, e = 2, r = 3;
            switch (champion.ChampionName)
            {
                case "Aatrox":
                    _level = new[] { w, q, e, w, w, r, w, e, w, e, r, e, e, q, q, r, q, q };
                    break;
                case "Ahri":
                    _level = new[] { q, e, w, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Akali":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Alistar":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Amumu":
                    _level = new[] { w, e, q, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Anivia":
                    _level = new[] { q, e, e, w, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Annie":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Ashe":
                    _level = new[] { w, q, q, w, e, r, w, w, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "AurelionSol":
                    _level = new[] { w, q, e, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Azir":
                    _level = new[] { w, q, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Bard":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Blitzcrank":
                    _level = new[] { q, e, w, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Brand":
                    _level = new[] { e, q, w, w, w, r, w, e, w, e, r, e, e, q, q, r, q, q };
                    break;
                case "Braum":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Caitlyn":
                    _level = new[] { q, w, e, q, w, r, w, w, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Camille":
                    _level = new[] { q, w, e, e, q, r, q, q, q, w, r, w, w, w, e, r, e, e };
                    break;
                case "Cassiopeia":
                    _level = new[] { e, q, w, q, e, r, q, e, w, q, r, e, q, e, w, r, w, w };
                    break;
                case "Chogath":
                    _level = new[] { e, w, q, e, e, r, e, w, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "Corki":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Darius":
                    _level = new[] { q, w, e, w, q, r, q, w, q, w, r, w, e, e, e, r, e, q };
                    break;
                case "Diana":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "DrMundo":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Draven":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Ekko":
                    _level = new[] { q, e, w, e, e, r, q, q, q, e, r, e, q, w, w, r, w, w };
                    break;
                case "Elise":
                    _level = new[] { w, q, e, e, w, r, w, w, w, q, r, e, q, e, e, r, q, q };
                    break;
                case "Evelynn":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Ezreal":
                    _level = new[] { q, e, q, w, q, r, q, e, q, e, r, e, e, w, w, w, r, w };
                    break;
                case "FiddleSticks":
                    _level = new[] { w, e, q, w, q, r, e, q, w, w, r, q, q, e, e, r, w, e };
                    break;
                case "Fiora":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Fizz":
                    _level = new[] { e, w, q, e, e, r, e, w, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "Galio":
                    _level = new[] { q, e, w, w, q, r, q, e, e, e, q, e, r, w, q, r, w, w };
                    break;
                case "Gangplank":
                    _level = new[] { q, e, w, e, e, r, q, e, q, e, q, r, q, w, w, w, w, r };
                    break;
                case "Garen":
                    _level = new[] { q, e, w, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Gnar":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Gragas":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Graves":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Hecarim":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Heimerdinger":
                    _level = new[] { q, e, w, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Illaoi":
                    _level = new[] { w, q, e, w, e, r, e, e, q, e, q, r, q, q, w, w, r, w };
                    break;
                case "Irelia":
                    _level = new[] { q, w, e, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Ivern":
                    _level = new[] { q, e, w, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Janna":
                    _level = new[] { e, w, q, e, e, r, e, w, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "JarvanIV":
                    _level = new[] { e, q, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Jax":
                    _level = new[] { e, q, w, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Jayce":
                    _level = new[] { q, w, e, e, q, q, q, w, q, w, q, w, w, w, e, e, e, e };
                    break;
                case "Jhin":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Jinx":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Kalista":
                    _level = new[] { e, q, w, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Karma":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Karthus":
                    _level = new[] { q, e, q, w, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Kassadin":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Katarina":
                    _level = new[] { q, e, w, w, w, r, e, w, w, e, r, q, q, q, e, r, q, e };
                    break;
                case "Kayle":
                    _level = new[] { e, w, q, w, e, r, e, e, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "Kayn":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Kennen":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Khazix":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Kindred":
                    _level = new[] { w, q, e, e, e, r, q, e, q, q, r, e, q, w, w, r, w, w };
                    break;
                case "Kled":
                    _level = new[] { q, w, e, e, w, r, w, q, w, w, r, q, q, q, e, r, e, e };
                    break;
                case "KogMaw":
                    _level = new[] { w, q, e, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Leblanc":
                    _level = new[] { q, w, e, w, q, q, r, q, q, w, r, e, w, w, e, r, e, e };
                    break;
                case "LeeSin":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Leona":
                    _level = new[] { q, e, w, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Lissandra":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Lucian":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Lulu":
                    _level = new[] { e, q, w, e, e, r, e, w, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "Lux":
                    _level = new[] { e, q, w, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Malphite":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Malzahar":
                    _level = new[] { q, e, w, e, q, r, q, q, q, w, r, e, e, e, w, r, w, w };
                    break;
                case "Maokai":
                    _level = new[] { w, q, e, q, q, r, q, e, q, w, r, e, e, e, w, r, w, w };
                    break;
                case "MasterYi":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "MissFortune":
                    _level = new[] { q, e, w, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Mordekaiser":
                    _level = new[] { e, w, q, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Morgana":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Nami":
                    _level = new[] { q, w, w, e, w, r, w, e, w, e, r, e, e, q, q, r, q, q };
                    break;
                case "Nasus":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Nautilus":
                    _level = new[] { q, e, w, e, e, r, e, w, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "Nidalee":
                    _level = new[] { q, e, w, e, q, q, r, q, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Nocturne":
                    _level = new[] { e, q, w, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Nunu":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Olaf":
                    _level = new[] { q, e, w, q, e, r, q, q, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Orianna":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Ornn":
                    _level = new[] { q, w, e, e, q, r, q, q, w, w, r, q, w, w, e, r, e, e };
                    break;
                case "Pantheon":
                    _level = new[] { q, e, w, q, e, r, q, q, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Poppy":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Quinn":
                    _level = new[] { e, q, w, e, q, r, q, e, q, e, r, q, e, w, w, r, w, w };
                    break;
                case "Rakan":
                    _level = new[] { w, e, q, w, w, r, w, e, w, e, r, e, e, q, q, r, q, q };
                    break;
                case "Rammus":
                    _level = new[] { e, w, q, e, w, r, e, e, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "RekSai":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Renekton":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Rengar":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Riven":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Rumble":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Ryze":
                    _level = new[] { e, q, w, w, q, r, q, e, e, e, r, e, q, q, q, w, w, w };
                    break;
                case "Sejuani":
                    _level = new[] { e, w, q, w, w, r, w, e, w, e, r, e, e, q, q, r, q, q };
                    break;
                case "Shaco":
                    _level = new[] { w, q, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Shen":
                    _level = new[] { e, q, w, q, q, r, e, e, e, e, r, q, q, w, w, r, w, w };
                    break;
                case "Shyvana":
                    _level = new[] { w, e, q, w, w, r, w, e, w, e, r, e, e, q, q, r, q, q };
                    break;
                case "Singed":
                    _level = new[] { q, e, q, w, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Sion":
                    _level = new[] { q, e, w, e, e, r, e, w, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "Sivir":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Skarner":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Sona":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Soraka":
                    _level = new[] { q, w, e, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Swain":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Syndra":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "TahmKench":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Taliyah":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Talon":
                    _level = new[] { w, q, e, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Taric":
                    _level = new[] { e, w, q, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Teemo":
                    _level = new[] { q, e, e, q, w, r, e, e, e, q, r, w, w, w, q, r, w, q };
                    break;
                case "Thresh":
                    _level = new[] { q, e, w, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Tristana":
                    _level = new[] { e, w, q, e, q, r, e, e, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Trundle":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Tryndamere":
                    _level = new[] { e, q, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "TwistedFate":
                    _level = new[] { w, q, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Twitch":
                    _level = new[] { e, w, q, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Udyr":
                    _level = new[] { q, w, e, q, q, e, q, e, q, e, e, w, w, w, w, r, r, r };
                    break;
                case "Urgot":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Varus":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Vayne":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Veigar":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Velkoz":
                    _level = new[] { w, q, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Vi":
                    _level = new[] { w, e, q, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Viktor":
                    _level = new[] { q, e, e, w, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Vladimir":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Volibear":
                    _level = new[] { w, e, q, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Warwick":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "MonkeyKing":
                    _level = new[] { e, w, q, e, e, r, e, q, e, q, r, q, q, w, w, r, w, w };
                    break;
                case "Xayah":
                    _level = new[] { q, e, w, e, e, r, e, w, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "Xerath":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "XinZhao":
                    _level = new[] { e, q, w, w, w, r, w, q, w, q, r, q, q, e, e, r, e, e };
                    break;
                case "Yasuo":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Yorick":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Zac":
                    _level = new[] { w, q, e, e, e, r, e, w, e, w, r, w, w, q, q, r, q, q };
                    break;
                case "Zed":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Ziggs":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Zilean":
                    _level = new[] { q, w, e, q, q, r, q, w, q, w, r, w, w, e, e, r, e, e };
                    break;
                case "Zoe":
                    _level = new[] { q, e, w, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                case "Zyra":
                    _level = new[] { q, w, e, q, q, r, q, e, q, e, r, e, e, w, w, r, w, w };
                    break;
                default:
                    _level = new[] { q, w, e, q, w, r, e, q, w, e, r, q, w, e, q, r, w, e };
                    break;
            }
            return _level;
        }

        private static string MetaString(Obj_AI_Hero champion)
        {
            string meta;
            switch (champion.ChampionName)
            {
                case "Aatrox":
                    meta = " W Q E W W R W E W E R E E Q Q R Q Q ";
                    break;
                case "Ahri":
                    meta = " Q E W Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Akali":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Alistar":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Amumu":
                    meta = " W E Q E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Anivia":
                    meta = " Q E E W E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Annie":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Ashe":
                    meta = " W Q Q W E R W W W Q R Q Q E E R E E ";
                    break;
                case "AurelionSol":
                    meta = " W Q E W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Azir":
                    meta = " W Q E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Bard":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Blitzcrank":
                    meta = " Q E W Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Brand":
                    meta = " E Q W W W R W E W E R E E Q Q R Q Q ";
                    break;
                case "Braum":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Caitlyn":
                    meta = " Q W E Q W R W W W Q R Q Q E E R E E ";
                    break;
                case "Camille":
                    meta = " Q W E E Q R Q Q Q W R W W W E R E E ";
                    break;
                case "Cassiopeia":
                    meta = " E Q W Q E R Q E W Q R E Q E W R W W ";
                    break;
                case "Chogath":
                    meta = " E W Q E E R E W E W R W W Q Q R Q Q ";
                    break;
                case "Corki":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Darius":
                    meta = " Q W E W Q R Q W Q W R W E E E R E Q ";
                    break;
                case "Diana":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "DrMundo":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Draven":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Ekko":
                    meta = " Q E W E E R Q Q Q E R E Q W W R W W ";
                    break;
                case "Elise":
                    meta = " W Q E E W R W W W Q R E Q E E R Q Q ";
                    break;
                case "Evelynn":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Ezreal":
                    meta = " Q E Q W Q R Q E Q E R E E W W W R W ";
                    break;
                case "FiddleSticks":
                    meta = " W E Q W Q R E Q W W R Q Q E E R W E ";
                    break;
                case "Fiora":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Fizz":
                    meta = " E W Q E E R E W E W R W W Q Q R Q Q ";
                    break;
                case "Galio":
                    meta = " Q E W W Q R Q E E E Q E R W Q R W W ";
                    break;
                case "Gangplank":
                    meta = " Q E W E E R Q E Q E Q R Q W W W W R ";
                    break;
                case "Garen":
                    meta = " Q E W E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Gnar":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Gragas":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Graves":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Hecarim":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Heimerdinger":
                    meta = " Q E W Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Illaoi":
                    meta = " W Q E W E R E E Q E Q R Q Q W W R W ";
                    break;
                case "Irelia":
                    meta = " Q W E E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Ivern":
                    meta = " Q E W E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Janna":
                    meta = " E W Q E E R E W E W R W W Q Q R Q Q ";
                    break;
                case "JarvanIV":
                    meta = " E Q W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Jax":
                    meta = " E Q W W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Jayce":
                    meta = " Q W E E Q Q Q W Q W Q W W W E E E E ";
                    break;
                case "Jhin":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Jinx":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Kalista":
                    meta = " E Q W E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Karma":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Karthus":
                    meta = " Q E Q W Q R Q E Q E R E E W W R W W ";
                    break;
                case "Kassadin":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Katarina":
                    meta = " Q E W W W R E W W E R Q Q Q E R Q E ";
                    break;
                case "Kayle":
                    meta = " E W Q W E R E E E W R W W Q Q R Q Q ";
                    break;
                case "Kayn":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Kennen":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Khazix":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Kindred":
                    meta = " W Q E E E R Q E Q Q R E Q W W R W W ";
                    break;
                case "Kled":
                    meta = " Q W E E W R W Q W W R Q Q Q E R E E ";
                    break;
                case "KogMaw":
                    meta = " W Q E W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Leblanc":
                    meta = " Q W E W Q Q R Q Q W R E W W E R E E ";
                    break;
                case "LeeSin":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Leona":
                    meta = " Q E W W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Lissandra":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Lucian":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Lulu":
                    meta = " E Q W E E R E W E W R W W Q Q R Q Q ";
                    break;
                case "Lux":
                    meta = " E Q W E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Malphite":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Malzahar":
                    meta = " Q E W E Q R Q Q Q W R E E E W R W W ";
                    break;
                case "Maokai":
                    meta = " W Q E Q Q R Q E Q W R E E E W R W W ";
                    break;
                case "MasterYi":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "MissFortune":
                    meta = " Q E W Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Mordekaiser":
                    meta = " E W Q W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Morgana":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Nami":
                    meta = " Q W W E W R W E W E R E E Q Q R Q Q ";
                    break;
                case "Nasus":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Nautilus":
                    meta = " Q E W E E R E W E W R W W Q Q R Q Q ";
                    break;
                case "Nidalee":
                    meta = " Q E W E Q Q R Q Q E R E E W W R W W ";
                    break;
                case "Nocturne":
                    meta = " E Q W Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Nunu":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Olaf":
                    meta = " Q E W Q E R Q Q Q E R E E W W R W W ";
                    break;
                case "Orianna":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Ornn":
                    meta = " Q W E E Q R Q Q W W R Q W W E R E E ";
                    break;
                case "Pantheon":
                    meta = " Q E W Q E R Q Q Q E R E E W W R W W ";
                    break;
                case "Poppy":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Quinn":
                    meta = " E Q W E Q R Q E Q E R Q E W W R W W ";
                    break;
                case "Rakan":
                    meta = " W E Q W W R W E W E R E E Q Q R Q Q ";
                    break;
                case "Rammus":
                    meta = " E W Q E W R E E E W R W W Q Q R Q Q ";
                    break;
                case "RekSai":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Renekton":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Rengar":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Riven":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Rumble":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Ryze":
                    meta = " E Q W W Q R Q E E E R E Q Q Q W W W ";
                    break;
                case "Sejuani":
                    meta = " E W Q W W R W E W E R E E Q Q R Q Q ";
                    break;
                case "Shaco":
                    meta = " W Q E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Shen":
                    meta = " E Q W Q Q R E E E E R Q Q W W R W W ";
                    break;
                case "Shyvana":
                    meta = " W E Q W W R W E W E R E E Q Q R Q Q ";
                    break;
                case "Singed":
                    meta = " Q E Q W Q R Q E Q E R E E W W R W W ";
                    break;
                case "Sion":
                    meta = " Q E W E E R E W E W R W W Q Q R Q Q ";
                    break;
                case "Sivir":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Skarner":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Sona":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Soraka":
                    meta = " Q W E W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Swain":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Syndra":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "TahmKench":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Taliyah":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Talon":
                    meta = " W Q E W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Taric":
                    meta = " E W Q E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Teemo":
                    meta = " Q E E Q W R E E E Q R W W W Q R W Q ";
                    break;
                case "Thresh":
                    meta = " Q E W E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Tristana":
                    meta = " E W Q E Q R E E E Q R Q Q W W R W W ";
                    break;
                case "Trundle":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Tryndamere":
                    meta = " E Q W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "TwistedFate":
                    meta = " W Q E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Twitch":
                    meta = " E W Q E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Udyr":
                    meta = " Q W E Q Q E Q E Q E E W W W W R R R ";
                    break;
                case "Urgot":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Varus":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Vayne":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Veigar":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Velkoz":
                    meta = " W Q E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Vi":
                    meta = " W E Q Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Viktor":
                    meta = " Q E E W E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Vladimir":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Volibear":
                    meta = " W E Q W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Warwick":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "MonkeyKing":
                    meta = " E W Q E E R E Q E Q R Q Q W W R W W ";
                    break;
                case "Xayah":
                    meta = " Q E W E E R E W E W R W W Q Q R Q Q ";
                    break;
                case "Xerath":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "XinZhao":
                    meta = " E Q W W W R W Q W Q R Q Q E E R E E ";
                    break;
                case "Yasuo":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Yorick":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Zac":
                    meta = " W Q E E E R E W E W R W W Q Q R Q Q ";
                    break;
                case "Zed":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Ziggs":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Zilean":
                    meta = " Q W E Q Q R Q W Q W R W W E E R E E ";
                    break;
                case "Zoe":
                    meta = " Q E W Q Q R Q E Q E R E E W W R W W ";
                    break;
                case "Zyra":
                    meta = " Q W E Q Q R Q E Q E R E E W W R W W ";
                    break;
                default:
                    meta = " Q W E Q W R E Q W E R Q W E Q R W E ";
                    break;
            }
            return meta;
        }
        #endregion

        #region - Avoiders -
        private static void OnIssueOrder(Obj_AI_Base sender, Obj_AI_BaseIssueOrderEventArgs args)
        {
            if (!sender.IsMe || !Menu["avoider"]["Key"].As<MenuKeyBind>().Enabled || !Menu["enabled"]["uEnabled"].As<MenuBool>().Enabled) return;
            if (args.OrderType == OrderType.MoveTo)
            {
                if (Tracker.HiddenObjectsList == null) return;
                var movePos = args.Position.To2D();
                foreach (var t in Tracker.HiddenObjectsList)
                {
                    if (movePos.Distance(t.Location) < Player.BoundingRadius)
                    {
                        args.ProcessEvent = false;
                    }
                }
            }
        }

        public static void Orbwalker_OnPreAttack(object sender, PreAttackEventArgs args)
        {
            if (Orbwalker.Mode == OrbwalkingMode.None) return;
            if (_avoiding)
            {
                args.Cancel = true;
            }
            _avoiding = false;
        }

        public static List<Vector3> Pathing(float radius, Vector3 position)
        {
            var points = new List<Vector3>();
            for (var i = 1; i <= 360; i++)
            {
                var angle = i * 2 * Math.PI / 360;

                foreach (var t in Tracker.HiddenObjectsList)
                {
                    if (t.WardName == "ZiggsE" || t.WardName == "JinxE")
                    {
                        angle = i * Math.PI / 360;
                    }
                }
                var point = new Vector3(position.X + radius * (float)Math.Cos(angle), position.Y + radius * (float)Math.Sin(angle), position.Z + radius * (float)Math.Sin(angle));

                points.Add(point);
            }
            return points;
        }

        private static void Avoid(Vector3 position, float range)
        {
            _avoiding = true;
            var nextPoints = Pathing(100, Player.Position);
            var getPoint = nextPoints.Where(x => x.Distance(position) > range).OrderBy(y => y.Distance(Game.CursorPos)).FirstOrDefault();
            Orbwalker.Move(getPoint);
        }
        #endregion

        #region - Render Text Font -    
        private static void RenderFontText1(Font renderText, string text, Vector2 pos, Color color)
        {
            renderText.Draw(text, pos, color);
        }

        private static void RenderFontText2(Font renderText, string text, Rectangle rectangle, RenderTextFlags flags, Color color)
        {
            renderText.Draw(text, rectangle, flags, color);
        }

        private static string TextCenter(float text)
        {
            var center = (int)text;
            if (center >= 10 && center < 100)
            {
                return " " + center;
            }
            if (center < 10)
            {
                return "  " + center;
            }

            return center.ToString();
        }
        #endregion

        #region - Smite Damages -  
        private static int SmiteDamages
        {
            get
            {
                var smite = new[] { 390, 410, 430, 450, 480, 510, 540, 570, 600, 640, 680, 720, 760, 800, 850, 900, 950, 1000 };
                return smite[Player.Level - 1];
            }
        }
        #endregion
        public static void DrawRect(float x, float y, float width, int height, float thickness, Color color)
        {
            for (var i = 0; i < height; i++)
                Render.Line(x, y + i, x + width, y + i, thickness, true, color);
        }
    }
}

