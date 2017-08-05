using System;
using Aimtec;
using System.Linq;
using System.Drawing;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Util;
using Aimtec.SDK.Damage;
using Aimtec.SDK.Orbwalking;
using Aimtec.SDK.Extensions;
using Spell = Aimtec.SDK.Spell;
using Aimtec.SDK.Menu.Components;
using System.Collections.Generic;
using Aimtec.SDK.Prediction.Skillshots;

namespace FrOnDaL_Veigar
{
    internal class FrOnDaLVeigar
    {
        public static Menu Main = new Menu("Index", "FrOnDaL Veigar", true);
        public static Orbwalker Orbwalker = new Orbwalker();
        public static Obj_AI_Hero Veigar => ObjectManager.GetLocalPlayer();
        private static Spell _q, _w, _e, _e2, _r;
        public static readonly List<string> SpecialChampions = new List<string> { "Annie", "Jhin" };
        public static int SxOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 1 : 10;
        }
        public static int SyOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 7 : 15;
        }
        public FrOnDaLVeigar()
        {
            /*Spells*/
            _q = new Spell(SpellSlot.Q, 900f);
            _w = new Spell(SpellSlot.W, 900f);
            _e = new Spell(SpellSlot.E, 725f);
            _e2 = new Spell(SpellSlot.E, 725f);
            _r = new Spell(SpellSlot.R, 650f) { Delay = 0.25f, Speed = 1400 };

            _q.SetSkillshot(0.25f, 70f, 2000f, true, SkillshotType.Line);
            _w.SetSkillshot(1.35f, 225f, float.MaxValue, false, SkillshotType.Circle);
            _e.SetSkillshot(.8f, 350f, float.MaxValue, false, SkillshotType.Circle);
            _e2.SetSkillshot(.8f, 350f, float.MaxValue, false, SkillshotType.Circle);


            Orbwalker.Attach(Main);

            /*Combo Menu*/
            var combo = new Menu("combo", "Combo")
            {
                new MenuBool("q", "Use Combo Q"),
                new MenuBool("w2", "Use Combo W (On/Off)"),
                new MenuList("w", "Use Combo W", new []{ "Normal", "Stun" }, 0),
                new MenuBool("e2", "Use Combo E (On/Off)"),
                new MenuList("e", "Use Combo E", new []{ "Normal", "Stun","Normal 2" }, 0),
                new MenuSlider("UnitsEhit", "Normal Mod Minimum enemies for E", 1, 1, 4),
                new MenuSliderBool("r", "Use Combo R / Enemies Health", false, 30, 1, 99),
                new MenuBool("rKillSteal", "Use Combo R KillSteal"),
                new MenuBool("disableAA", "Disable AutoAttacks", false)
            };
            Main.Add(combo);

            /*Harass Menu*/
            var harass = new Menu("harass", "Harass")
            {
                new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                new MenuSlider("mana","Harass Mana Control", 30, 1 , 99),
                new MenuBool("q", "Use harass Q"),
                new MenuBool("w2", "Use harass W (On/Off)"),
                new MenuList("w", "Use harass W", new []{ "Normal", "Stun" }, 1),
                new MenuBool("e2", "Use harass E (On/Off)"),
                new MenuList("e", "Use harass E", new []{ "Normal", "Stun", "Normal 2" }, 0),
                new MenuSlider("UnitsEhit", "Normal Mod Minimum enemies for E", 1, 1, 4),
            };
            Main.Add(harass);

            /*LaneClear Menu*/
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuSliderBool("q", "Use Q (stack) / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("w", "Use W / if Mana >= x%", true, 60, 0, 99),
                new MenuSlider("UnitsWhit", "W Hit x Units minions >= x%", 3, 1, 4)         
            };
            Main.Add(laneclear);

            /*JungleClear Menu*/
            var jungleclear = new Menu("jungleclear", "Jungle Clear")
            {
                new MenuSliderBool("q", "Use Q / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("w", "Use W / if Mana >= x%", true, 30, 0, 99),
                new MenuSliderBool("e", "Use E / if Mana >= x%", true, 30, 0, 99),
            };
            Main.Add(jungleclear);

            /*Drawings Menu*/
            var drawings = new Menu("drawings", "Drawings")
            {
                new MenuBool("q", "Draw Q", false),
                new MenuBool("w", "Draw W", false),
                new MenuBool("e", "Draw E", false),
                new MenuBool("r", "Draw R"),
                new MenuBool("drawDamage", "Use Draw R Damage")
            };
            Main.Add(drawings);
            Main.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Render.OnPresent += SpellDraw;
            Orbwalker.PreAttack += OnPreAttack;
        }


        /*Drawings*/
        private static void SpellDraw()
        {
            if (Main["drawings"]["q"].As<MenuBool>().Enabled)
            {
                Render.Circle(Veigar.Position, _q.Range, 180, Color.Green);
            }
            if (Main["drawings"]["w"].As<MenuBool>().Enabled)
            {
                Render.Circle(Veigar.Position, _w.Range, 180, Color.Green);
            }
            if (Main["drawings"]["e"].As<MenuBool>().Enabled)
            {
                Render.Circle(Veigar.Position, _e.Range, 180, Color.Green);
            }
            if (Main["drawings"]["r"].As<MenuBool>().Enabled)
            {
                Render.Circle(Veigar.Position, _r.Range, 180, Color.Green);
            }
        }

        private static void Game_OnUpdate()
        {
            if (Veigar.IsDead || MenuGUI.IsChatOpen()) return;
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
                case OrbwalkingMode.Lasthit:
                    foreach (var minion in GameObjects.EnemyMinions.Where(m => Veigar.Distance(m.Position) <= _q.Range && m.Health < Veigar.GetSpellDamage(m, SpellSlot.Q)))
                    {
                        if (Main["combo"]["q"].As<MenuBool>().Enabled && _q.Ready && minion.IsValidTarget(_q.Range))
                        {
                            _q.Cast(minion);
                        }
                    }
                    break;
            }
        }

        public static void CastE(Obj_AI_Base target)
        {
            var pred = Prediction.GetPrediction(target, _e.Delay);
            var castVec = pred.UnitPosition.To2D() - Vector2.Normalize(pred.UnitPosition.To2D() - Veigar.Position.To2D()) * _e.Width;
            if (pred.HitChance >= HitChance.VeryHigh && _e.Ready)
            {
                _e.Cast(castVec);
            }
        }

        public static void CastE(Obj_AI_Hero target)
        {
            var pred = Prediction.GetPrediction(target, _e.Delay);
            var castVec = pred.UnitPosition.To2D() - Vector2.Normalize(pred.UnitPosition.To2D() - Veigar.Position.To2D()) * _e.Width;
            if (pred.HitChance >= HitChance.VeryHigh && _e.Ready)
            {
                _e.Cast(castVec);
            }
        }

        public static void CastE(Vector3 pos)
        {
            var castVec = pos.To2D() - Vector2.Normalize(pos.To2D() - Veigar.Position.To2D()) * _e.Width;
            if (_e.Ready)
            {
                _e.Cast(castVec);
            }
        }

        public static void CastE(Vector2 pos)
        {
            var castVec = pos;
            if (_e.Ready)
            {
                _e.Cast(castVec);
            }
        }

        private static void Combo()
        {
            foreach (var targetC in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1000)))
            {
                if (targetC == null) return;

                switch (Main["combo"]["e"].As<MenuList>().Value)
                {
                    case 0:
                        if (Veigar.CountEnemyHeroesInRange(800) >= Main["combo"]["UnitsEhit"].As<MenuSlider>().Value && Main["combo"]["e2"].As<MenuBool>().Enabled && _e.Ready)
                        {
                            if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(_e2.Width, false, true,
                                    _e2.GetPrediction(targetC).CastPosition)) >= Main["combo"]["UnitsEhit"].As<MenuSlider>().Value)
                            {
                                _e2.Cast(targetC.Position);
                            }
                        }
                        break;
                    case 1:
                        if (Veigar.Distance(targetC.Position) <= 900 && Main["combo"]["e2"].As<MenuBool>().Enabled && _e.Ready)
                        {                            
                                CastE(targetC);                          
                        }
                        break;
                    case 2:
                        if (Veigar.Distance(targetC.Position) <= 900 && Main["combo"]["e2"].As<MenuBool>().Enabled && _e.Ready)
                        {
                            CastE2(targetC);
                        }
                        break;
                }

                switch (Main["combo"]["w"].As<MenuList>().Value)
                {
                    case 0:
                        var predW = _w.GetPrediction(targetC, Veigar.Position, targetC.Position);

                        if (targetC.IsInRange(_w.Range) && Main["combo"]["w2"].As<MenuBool>().Enabled && _w.Ready)
                        {
                            if (predW.HitChance >= HitChance.VeryHigh)
                            {
                                _w.Cast(predW.CastPosition);
                            }
                        }
                        break;
                    case 1:                    
                        if (targetC.IsInRange(_w.Range) && Main["combo"]["w2"].As<MenuBool>().Enabled && _w.Ready && TargetStun(targetC) >= _w.Delay)
                        {                           
                            _w.Cast(targetC.Position);
                        }
                        break;
                }

                if (Main["combo"]["q"].As<MenuBool>().Enabled && _q.Ready && targetC.IsInRange(_q.Range))
                {
                    var pred = _q.GetPrediction(targetC);
                    if (pred.HitChance >= HitChance.High && pred.CollisionObjects.Count == 0)
                    {
                        _q.Cast(pred.CastPosition);
                    }
                }

                if (Main["combo"]["rKillSteal"].As<MenuBool>().Enabled && _r.Ready && targetC.IsValidTarget(_r.Range))
                {
                    if (Veigar.GetSpellDamage(targetC, SpellSlot.R) > targetC.Health)
                    {                     
                            _r.CastOnUnit(targetC);                      
                    }
                }
                if (Main["combo"]["r"].As<MenuSliderBool>().Enabled && !Main["combo"]["rKillSteal"].As<MenuBool>().Enabled && _r.Ready && targetC.IsValidTarget(_r.Range))
                {
                    if (targetC.HealthPercent() < Main["combo"]["r"].As<MenuSliderBool>().Value)
                    {
                        _r.CastOnUnit(targetC);
                    }
                }
            }            
        }

        /*Harass*/
        private static void Harass()
        {
            foreach (var targetH in GameObjects.EnemyHeroes.Where(x => x.IsValidTarget(1000)))
            {
                if (targetH == null) return;
                if (!(Veigar.ManaPercent() >= Main["harass"]["mana"].As<MenuSlider>().Value)) continue;
                switch (Main["harass"]["e"].As<MenuList>().Value)
                {
                    case 0:
                        if (Veigar.CountEnemyHeroesInRange(800) >= Main["harass"]["UnitsEhit"].As<MenuSlider>().Value && Main["harass"]["e2"].As<MenuBool>().Enabled && _e.Ready)
                        {
                            if (GameObjects.EnemyHeroes.Count(t => t.IsValidTarget(_e2.Width, false, true,
                                    _e2.GetPrediction(targetH).CastPosition)) >= Main["harass"]["UnitsEhit"].As<MenuSlider>().Value)
                            {
                                _e2.Cast(targetH.Position);
                            }
                        }
                        break;
                    case 1:
                        if (Veigar.Distance(targetH.Position) <= 900 && Main["harass"]["e2"].As<MenuBool>().Enabled && _e.Ready)
                        {
                            var predE = _e.GetPrediction(targetH, Veigar.Position, targetH.Position);
                            if (predE.HitChance == HitChance.VeryHigh)
                            {
                                CastE(targetH);
                            }
                        }
                        break;
                    case 2:
                        if (Veigar.Distance(targetH.Position) <= 900 && Main["harass"]["e2"].As<MenuBool>().Enabled && _e.Ready)
                        {
                            var predE = _e.GetPrediction(targetH, Veigar.Position, targetH.Position);
                            if (predE.HitChance == HitChance.VeryHigh)
                            {
                                CastE2(targetH);
                            }
                        }
                        break;
                }

                switch (Main["harass"]["w"].As<MenuList>().Value)
                {
                    case 0:
                        var predW = _w.GetPrediction(targetH, Veigar.Position, targetH.Position);

                        if (targetH.IsInRange(_w.Range) && Main["harass"]["w2"].As<MenuBool>().Enabled && _w.Ready)
                        {
                            if (predW.HitChance >= HitChance.VeryHigh)
                            {
                                _w.Cast(predW.CastPosition);
                            }
                        }
                        break;
                    case 1:
                        if (targetH.IsInRange(_w.Range) && Main["harass"]["w2"].As<MenuBool>().Enabled && _w.Ready && TargetStun(targetH) >= _w.Delay)
                        {
                            _w.Cast(targetH.Position);
                        }
                        break;
                }

                if (Main["harass"]["q"].As<MenuBool>().Enabled && _q.Ready && targetH.IsInRange(_q.Range))
                {
                    var pred = _q.GetPrediction(targetH);
                    if (pred.HitChance >= HitChance.VeryHigh && pred.CollisionObjects.Count == 0)
                    {
                        _q.Cast(pred.CastPosition);
                    }
                }
            }

        }

        private static void LaneClear()
        {
            foreach (var targetL in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(1000)))
            {
                if (targetL == null) return;

                if (Main["laneclear"]["w"].As<MenuSliderBool>().Enabled && Veigar.ManaPercent() >= Main["laneclear"]["w"].As<MenuSliderBool>().Value && _w.Ready && targetL.IsValidTarget(_w.Range))
                {

                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(_w.Width - 80, false, false, _w.GetPrediction(targetL).CastPosition)) >= Main["laneclear"]["UnitsWhit"].As<MenuSlider>().Value)
                    {
                        _w.Cast(_e.GetPrediction(targetL).CastPosition);
                    }
                }

                if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Veigar.ManaPercent() >= Main["laneclear"]["q"].As<MenuSliderBool>().Value && _q.Ready && targetL.IsValidTarget(_q.Range))
                {
                    foreach (var minion in GameObjects.EnemyMinions.Where(m => Veigar.Distance(m.Position) <= _q.Range && m.Health < Veigar.GetSpellDamage(m, SpellSlot.Q)))
                    {
                        _q.Cast(minion);
                    }                 
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var targetJ in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Veigar.ManaPercent() >= Main["jungleclear"]["e"].As<MenuSliderBool>().Value && _e.Ready)
                {
                    CastE(targetJ.Position);
                }
                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Veigar.ManaPercent() >= Main["jungleclear"]["w"].As<MenuSliderBool>().Value && _w.Ready)
                {
                    _w.Cast(targetJ.Position);
                }               
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Veigar.ManaPercent() >= Main["jungleclear"]["q"].As<MenuSliderBool>().Value && _q.Ready && (targetJ.HasBuffOfType(BuffType.Stun) || !_e.Ready))
                {
                    _q.Cast(targetJ.Position);                    
                }
            }
        }

        public static void CastE2(Obj_AI_Base target)
        {
            var pred = Prediction.GetPrediction(target, _e.Delay);
            var castVec = pred.UnitPosition.To2D() - Vector2.Normalize(pred.UnitPosition.To2D() - Veigar.Position.To2D()) * _e.Width + 60;
            if (pred.HitChance >= HitChance.VeryHigh && _e.Ready)
            {
                _e.Cast(castVec);
            }
        }

        public static void CastE2(Obj_AI_Hero target)
        {
            var pred = Prediction.GetPrediction(target, _e.Delay);
            var castVec = pred.UnitPosition.To2D() - Vector2.Normalize(pred.UnitPosition.To2D() - Veigar.Position.To2D()) * _e.Width + 60;
            if (pred.HitChance >= HitChance.VeryHigh && _e.Ready)
            {
                _e.Cast(castVec);
            }
        }

        public static void CastE2(Vector3 pos)
        {
            var castVec = pos.To2D() - Vector2.Normalize(pos.To2D() - Veigar.Position.To2D()) * _e.Width + 60;
            if (_e.Ready)
            {
                _e.Cast(castVec);
            }
        }

        public static void CastE2(Vector2 pos)
        {
            var castVec = pos;
            if (_e.Ready)
            {
                _e.Cast(castVec);
            }
        }
        /*Draw Damage Q*/
        private static void DamageDraw()
        {
            if (!Main["drawings"]["drawDamage"].Enabled || Veigar.SpellBook.GetSpell(SpellSlot.R).Level <= 0) return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && Veigar.Distance(x) < 1700 && x.IsFloatingHealthBarActive && x.IsVisible))
            {
                const int width = 103;
                var xOffset = SxOffset(enemy);
                var yOffset = SyOffset(enemy);
                var barPos = enemy.FloatingHealthBarPosition;
                barPos.X += xOffset;
                barPos.Y += yOffset;
                var drawEndXPos = barPos.X + width * (enemy.HealthPercent() / 100);
                var drawStartXPos = (float)(barPos.X + (enemy.Health > Veigar.GetSpellDamage(enemy, SpellSlot.R) ? width * ((enemy.Health - Veigar.GetSpellDamage(enemy, SpellSlot.R)) / enemy.MaxHealth * 100 / 100) : 0));
                Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 9, true, enemy.Health < Veigar.GetSpellDamage(enemy, SpellSlot.R) ? Color.GreenYellow : Color.ForestGreen);
            }
        }
        public static void OnPreAttack(object sender, PreAttackEventArgs args)
        {
            switch (Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    if (Main["combo"]["disableAA"].As<MenuBool>().Enabled)
                    {
                        args.Cancel = true;
                    }
                    break;
            }
        }
        private static float TargetStun(Obj_AI_Base target)
        {
            return target.Buffs.Where(t => t.IsActive && Game.ClockTime < t.EndTime && (t.Type == BuffType.Charm || t.Type == BuffType.Stun || t.Type == BuffType.Stun || t.Type == BuffType.Fear || t.Type == BuffType.Snare || t.Type == BuffType.Taunt || t.Type == BuffType.Knockback || t.Type == BuffType.Suppression)).Aggregate(0f, (current, buff) => Math.Max(current, buff.EndTime)) - Game.ClockTime;
        }

    }
}
