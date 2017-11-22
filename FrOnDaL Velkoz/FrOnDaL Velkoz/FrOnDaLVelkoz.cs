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
using Aimtec.SDK.Util.ThirdParty;
using Aimtec.SDK.Prediction.Skillshots;

namespace FrOnDaL_Velkoz
{
    internal class FrOnDaLVelkoz
    {
        private static readonly Menu Main = new Menu("Index", "FrOnDaL Vel'koz", true);
        private static readonly Orbwalker Orbwalker = new Orbwalker();
        private static Obj_AI_Hero Velkoz => ObjectManager.GetLocalPlayer();
        private static Spell _q, _w, _e, _r;

        private static Vector3 _rCastPos;
        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            if (sender.IsMe && args.SpellSlot == SpellSlot.R) _rCastPos = sender.ServerPosition.Extend(args.End, _r.Range);
        }
        private static bool IsQActive => Velkoz.SpellBook.GetSpell(SpellSlot.Q).SpellData.Name == "VelkozQ";
        private static double RDamage(Obj_AI_Base d)
        {
            var damageR = Velkoz.CalculateDamage(d, DamageType.Magical, (float)new double[] { 450, 625, 800 }[Velkoz.SpellBook.GetSpell(SpellSlot.R).Level - 1] + Velkoz.TotalAbilityDamage / 100 * 125); return damageR;
        }
        private static readonly List<string> SpecialChampions = new List<string> { "Annie", "Jhin" };
        private static int SxOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 1 : 10;
        }
        private static int SyOffset(Obj_AI_Hero target)
        {
            return SpecialChampions.Contains(target.ChampionName) ? 7 : 15;
        }
        public FrOnDaLVelkoz()
        {
            /*Spells*/
            _q = new Spell(SpellSlot.Q, 1050);
            _w = new Spell(SpellSlot.W, 1050);
            _e = new Spell(SpellSlot.E, 810);
            _r = new Spell(SpellSlot.R, 1575);


            _q.SetSkillshot(0.25f, 50f, 1300f, true, SkillshotType.Line);
            _w.SetSkillshot(0.25f, 85f, 1700f, false, SkillshotType.Line);
            _e.SetSkillshot(0.5f, 120f, 1500f, false, SkillshotType.Circle);
            _r.SetSkillshot(0.3f, 1f, float.MaxValue, false, SkillshotType.Line);

            Orbwalker.Attach(Main);

            /*Combo Menu*/
            var combo = new Menu("combo", "Combo")
            {
                new MenuBool("q", "Use Combo Q"),
                new MenuBool("w", "Use Combo W"),
                new MenuBool("e", "Use Combo E"),
                new MenuBool("r", "Use Combo R",false),
                new MenuBool("rKillSteal", "Auto R KillSteal"),
                new MenuKeyBind("keyR", "R Key:", KeyCode.T, KeybindType.Press),
                new MenuBool("disableAA", "Disable AutoAttacks", false)
            };
            Main.Add(combo);

            /*Harass Menu*/
            var harass = new Menu("harass", "Harass")
            {
                new MenuKeyBind("keyHarass", "Harass Key:", KeyCode.C, KeybindType.Press),
                new MenuSliderBool("q", "Use Q / if Mana >= x%", false, 50, 0, 99),
                new MenuSliderBool("w", "Use W / if Mana >= x%", false, 50, 0, 99),
                new MenuSliderBool("e", "Use E / if Mana >= x%", false, 50, 0, 99)
            };
            Main.Add(harass);

            /*LaneClear Menu*/
            var laneclear = new Menu("laneclear", "Lane Clear")
            {
                new MenuSliderBool("q", "Use Q / if Mana >= x%", false, 60, 0, 99),
                new MenuSliderBool("w", "Use W / if Mana >= x%", true, 60, 0, 99),
                new MenuSlider("UnitsWhit", "W Hit x Units minions >= x%", 5, 1, 6),
                new MenuSliderBool("e", "Use E / if Mana >= x%", false, 60, 0, 99),
                new MenuSlider("UnitsEhit", "E Hit x Units minions >= x%", 3, 1, 4)
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
                new MenuBool("e", "Draw E"),
                new MenuBool("r", "Draw R", false),
                new MenuBool("drawDamage", "Use Draw R Damage")
            };
            Main.Add(drawings);
            Main.Attach();

            Game.OnUpdate += Game_OnUpdate;
            Render.OnPresent += DamageDraw;
            Render.OnPresent += SpellDraw;
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            Orbwalker.PreAttack += OnPreAttack;
        }


        /*Drawings*/
        private static void SpellDraw()
        {
            if (Main["drawings"]["q"].As<MenuBool>().Enabled)
            {
                Render.Circle(Velkoz.Position, _q.Range, 180, Color.Green);
            }
            if (Main["drawings"]["w"].As<MenuBool>().Enabled)
            {
                Render.Circle(Velkoz.Position, _w.Range, 180, Color.Green);
            }
            if (Main["drawings"]["e"].As<MenuBool>().Enabled)
            {
                Render.Circle(Velkoz.Position, _e.Range, 180, Color.Green);
            }
            if (Main["drawings"]["r"].As<MenuBool>().Enabled)
            {
                Render.Circle(Velkoz.Position, _r.Range, 180, Color.Green);
            }
        }

        private static void Game_OnUpdate()
        {
            if (Velkoz.IsDead || MenuGUI.IsChatOpen()) return;
            switch (Orbwalker.Mode)
            {
                case OrbwalkingMode.Combo:
                    Combo();
                    ManuelR();
                    break;
                case OrbwalkingMode.Laneclear:
                    LaneClear();
                    JungleClear();
                    break;
                case OrbwalkingMode.Mixed:
                    Harass();
                    break;
            }
            if (_r.Ready && Main["combo"]["keyR"].As<MenuKeyBind>().Enabled)
            {
                var target = TargetSelector.GetTarget(_r.Range);
                if (target != null)
                {
                    _r.Cast(target.Position);
                }         
            }

            if (_r.Ready && Main["combo"]["rKillSteal"].As<MenuBool>().Enabled)
            {
                var target = TargetSelector.Implementation.GetOrderedTargets(_r.Range).FirstOrDefault(x => x.Health < RDamage(x));
                if (target != null && Velkoz.Distance(target) > 400 && target.IsValidTarget(1200))
                {
                    _r.Cast(target.Position);
                }
            }

            var followR = GameObjects.EnemyHeroes.OrderBy(t => t.Distance(_rCastPos)).FirstOrDefault(t => t.IsValidTarget(_r.Range));
            if (Velkoz.SpellBook.IsChanneling && followR != null)
            {
                Velkoz.SpellBook.UpdateChargedSpell(SpellSlot.R, followR.ServerPosition, false);
            }
        }
        /*Combo*/
        private static void Combo()
        {
            var targetQ = TargetSelector.GetTarget(_q.Range);
            if (targetQ != null)
            {
                if (Main["combo"]["q"].As<MenuBool>().Enabled && _q.Ready)
                {
                    if (IsQActive)
                    {
                        var prediction = _q.GetPrediction(targetQ, Velkoz.Position);
                        if (prediction.HitChance >= HitChance.Medium)
                        {
                            _q.Cast(prediction.CastPosition);
                        }
                    }
                }
            }

            var targetW = TargetSelector.GetTarget(_w.Range);
            if (targetW != null)
            {
                if (Main["combo"]["w"].As<MenuBool>().Enabled && _w.Ready)
                {
                    var prediction = _w.GetPrediction(targetW, Velkoz.Position);
                    if (prediction.HitChance >= HitChance.High)
                    {
                        _w.Cast(prediction.CastPosition);
                    }
                }
            }

            var targetE = TargetSelector.GetTarget(_e.Range);
            if (targetE != null)
            {
                if (Main["combo"]["e"].As<MenuBool>().Enabled && _e.Ready)
                {
                    var prediction = _e.GetPrediction(targetE, Velkoz.Position, targetE.Position);
                    if (prediction.HitChance >= HitChance.High)
                    {
                        _e.Cast(prediction.CastPosition);
                    }
                }
            }
        }
        /*ManuelR*/
        private static void ManuelR()
        {
            if (_r.Ready && Main["combo"]["r"].As<MenuBool>().Enabled)
            {
                var target = TargetSelector.Implementation.GetOrderedTargets(_r.Range).FirstOrDefault(x => x.Health < RDamage(x));
                if (target != null && Velkoz.Distance(target) > 350 && target.IsValidTarget(1300))
                {
                    _r.Cast(target.Position);
                }
            }
        }
        /*ManuelR*/
        /*Harass*/
        private static void Harass()
        {
            var targetQ = TargetSelector.GetTarget(_q.Range);
            if (targetQ != null)
            {
                if (Main["harass"]["q"].As<MenuSliderBool>().Enabled && _q.Ready)
                {
                    if (IsQActive)
                    {
                        var prediction = _q.GetPrediction(targetQ, Velkoz.Position);
                        if (prediction.HitChance >= HitChance.High)
                        {
                            _q.Cast(prediction.CastPosition);
                        }
                    }
                }
            }

            var targetW = TargetSelector.GetTarget(_w.Range);
            if (targetW != null)
            {
                if (Main["harass"]["w"].As<MenuSliderBool>().Enabled && _w.Ready)
                {
                    var prediction = _w.GetPrediction(targetW, Velkoz.Position);
                    if (prediction.HitChance >= HitChance.High)
                    {
                        _w.Cast(prediction.CastPosition);
                    }
                }
            }

            var targetE = TargetSelector.GetTarget(_e.Range);
            if (targetE != null)
            {
                if (Main["harass"]["e"].As<MenuSliderBool>().Enabled && _e.Ready )
                {
                    var prediction = _e.GetPrediction(targetE, Velkoz.Position, targetE.Position - 100);
                    if (prediction.HitChance >= HitChance.High)
                    {
                        _e.Cast(prediction.CastPosition);
                    }
                }
            }
        }
        /*Lane Clear*/
        private static void LaneClear()
        {
            foreach (var targetL in GameObjects.EnemyMinions.Where(x => x.IsValidTarget(1300)))
            {
                if (targetL == null) continue;
                if (Main["laneclear"]["w"].As<MenuSliderBool>().Enabled && Velkoz.ManaPercent() >= Main["laneclear"]["w"].As<MenuSliderBool>().Value && _w.Ready && targetL.IsValidTarget(_w.Range))
                {
                    var result = GetLinearLocation(_w.Range, _w.Width + 20);
                    if (result == null) continue;
                    if (result.NumberOfMinionsHit >= Main["laneclear"]["UnitsWhit"].As<MenuSlider>().Value && !Velkoz.IsUnderEnemyTurret() && _w.Ready)
                    {
                        _w.Cast(result.CastPosition);
                    }
                }

                if (Main["laneclear"]["e"].As<MenuSliderBool>().Enabled && Velkoz.ManaPercent() >= Main["laneclear"]["e"].As<MenuSliderBool>().Value && _e.Ready && targetL.IsValidTarget(_e.Range))
                {

                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(_e.Width + 20, false, false, _e.GetPrediction(targetL).CastPosition)) >= Main["laneclear"]["UnitsEhit"].As<MenuSlider>().Value && !Velkoz.IsUnderEnemyTurret())
                    {
                        _e.Cast(_e.GetPrediction(targetL).CastPosition);
                    }
                }

                if (Main["laneclear"]["q"].As<MenuSliderBool>().Enabled && Velkoz.ManaPercent() >= Main["laneclear"]["q"].As<MenuSliderBool>().Value && _q.Ready && targetL.IsValidTarget(_q.Range))
                {
                    if (GameObjects.EnemyMinions.Count(t => t.IsValidTarget(120, false, false, _q.GetPrediction(targetL).CastPosition)) >= 2 && !Velkoz.IsUnderEnemyTurret())
                    {
                        if (IsQActive)
                        {
                            _q.Cast(_q.GetPrediction(targetL).CastPosition);
                        }
                    }
                }
            }
        }

        private static void JungleClear()
        {
            foreach (var targetJ in GameObjects.Jungle.Where(x => !GameObjects.JungleSmall.Contains(x) && (GameObjects.JungleLarge.Contains(x) || GameObjects.JungleLegendary.Contains(x)) && x.IsValidTarget(900)))
            {
                
                if (Main["jungleclear"]["w"].As<MenuSliderBool>().Enabled && Velkoz.ManaPercent() >= Main["jungleclear"]["w"].As<MenuSliderBool>().Value && _w.Ready)
                {
                    _w.Cast(targetJ.Position);
                }
                if (Main["jungleclear"]["e"].As<MenuSliderBool>().Enabled && Velkoz.ManaPercent() >= Main["jungleclear"]["e"].As<MenuSliderBool>().Value && _e.Ready)
                {
                    _e.Cast(targetJ.Position);
                }
                if (Main["jungleclear"]["q"].As<MenuSliderBool>().Enabled && Velkoz.ManaPercent() >= Main["jungleclear"]["q"].As<MenuSliderBool>().Value && _q.Ready)
                {
                    if (IsQActive)
                    {
                        _q.Cast(targetJ.Position);
                    }
                }
            }
        }
        /*Draw Damage R*/
        private static void DamageDraw()
        {
            if (!Main["drawings"]["drawDamage"].Enabled || Velkoz.SpellBook.GetSpell(SpellSlot.R).Level <= 0) return;
            foreach (var enemy in GameObjects.EnemyHeroes.Where(x => !x.IsDead && Velkoz.Distance(x) < 1700 && x.IsFloatingHealthBarActive && x.IsVisible))
            {
                const int width = 103;
                var xOffset = SxOffset(enemy);
                var yOffset = SyOffset(enemy);
                var barPos = enemy.FloatingHealthBarPosition;
                barPos.X += xOffset;
                barPos.Y += yOffset;
                var drawEndXPos = barPos.X + width * (enemy.HealthPercent() / 100);
                var drawStartXPos = (float)(barPos.X + (enemy.Health > RDamage(enemy) ? width * ((enemy.Health - RDamage(enemy)) / enemy.MaxHealth * 100 / 100) : 0));
                Render.Line(drawStartXPos, barPos.Y, drawEndXPos, barPos.Y, 9, true, enemy.Health < RDamage(enemy) ? Color.GreenYellow : Color.ForestGreen);
            }
        }
        public static void OnPreAttack(object sender, PreAttackEventArgs args)
        {
            switch (Orbwalker.Mode)
            {

                case OrbwalkingMode.Combo:
                    if (Velkoz.HasBuff("VelkozR"))
                    {
                        args.Cancel = true;
                    }
                    if (Main["combo"]["disableAA"].As<MenuBool>().Enabled)
                    {
                        args.Cancel = true;
                    }
                    break;
                case OrbwalkingMode.Mixed:
                case OrbwalkingMode.Lasthit:
                case OrbwalkingMode.Laneclear:
                    if (Velkoz.HasBuff("VelkozR"))
                    {
                        args.Cancel = true;
                    }
                    break;
            }
        }
        /*Polygon*/
        public abstract class Polygon
        {
            public List<Vector3> Points = new List<Vector3>();

            public List<IntPoint> ClipperPoints
            {
                get
                {
                    return Points.Select(p => new IntPoint(p.X, p.Z)).ToList();
                }
            }

            public bool Contains(Vector3 point)
            {
                var p = new IntPoint(point.X, point.Z);
                var inpolygon = Clipper.PointInPolygon(p, ClipperPoints);
                return inpolygon == 1;
            }
        }
        public class Rectangle : Polygon
        {
            public Rectangle(Vector3 startPosition, Vector3 endPosition, float width)
            {
                var direction = (startPosition - endPosition).Normalized();
                var perpendicular = Perpendicular(direction);

                var leftBottom = startPosition + width * perpendicular;
                var leftTop = startPosition - width * perpendicular;

                var rightBottom = endPosition - width * perpendicular;
                var rightLeft = endPosition + width * perpendicular;

                Points.Add(leftBottom);
                Points.Add(leftTop);
                Points.Add(rightBottom);
                Points.Add(rightLeft);
            }

            public Vector3 Perpendicular(Vector3 v)
            {
                return new Vector3(-v.Z, v.Y, v.X);
            }
        }

        /*Q Minions Location*/
        public class LaneclearResult
        {
            public LaneclearResult(int hit, Vector3 cp)
            {
                NumberOfMinionsHit = hit;
                CastPosition = cp;
            }

            public int NumberOfMinionsHit;
            public Vector3 CastPosition;
        }

        public static LaneclearResult GetLinearLocation(float range, float width)
        {
            var minions = ObjectManager.Get<Obj_AI_Base>().Where(x => x.IsValidSpellTarget(range));

            var objAiBases = minions as Obj_AI_Base[] ?? minions.ToArray();
            var positions = objAiBases.Select(x => x.ServerPosition).ToList();

            var locations = new List<Vector3>();

            locations.AddRange(positions);

            var max = positions.Count;

            for (var i = 0; i < max; i++)
            {
                for (var j = 0; j < max; j++)
                {
                    if (positions[j] != positions[i])
                    {
                        locations.Add((positions[j] + positions[i]) / 2);
                    }
                }
            }

            var results = new HashSet<LaneclearResult>();

            foreach (var p in locations)
            {
                var rect = new Rectangle(Velkoz.Position, p, width);

                var count = objAiBases.Count(m => rect.Contains(m.Position));

                results.Add(new LaneclearResult(count, p));
            }

            var maxhit = results.MaxBy(x => x.NumberOfMinionsHit);

            return maxhit;
        }
    }
}
