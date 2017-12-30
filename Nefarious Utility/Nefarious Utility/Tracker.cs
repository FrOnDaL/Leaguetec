using System;
using Aimtec;
using System.Linq;
using System.Drawing;
using Aimtec.SDK.Extensions;
using Aimtec.SDK.Util.Cache;
using System.Collections.Generic;
using Nefarious_Utility.Properties;

namespace Nefarious_Utility
{
    internal class HiddenObjects
    {
        public int Type;
        public int Level;
        public string WardName;
        public float EndTime { get; set; }
        public Vector3 Location { get; set; }
    }
    internal class HeroInfo
    {
        public Texture Hud { get; }
        public TrackingSpell Summoner1 { get; }
        public TrackingSpell Summoner2 { get; }
        public TrackingSpell Summoner1Hud { get; }
        public TrackingSpell Summoner2Hud { get; }
        public TrackingSpell SummonerRHud { get; }
        public readonly List<TrackingSpell> Spells = new List<TrackingSpell>();
        public Obj_AI_Hero Hero { get; set; }
        public bool Displaying { get; set; } = true;
        public bool IsJungler { get; set; }
        public Vector3 LastVisablePos { get; set; }
        public int RecallTime { get; set; }
        public float RecallEnd { get; set; }
        public int TimeSpeed { private get; set; }
        public int TimeMia => Game.TickCount - TimeSpeed;
        public Vector3 LastWayPoint { get; set; }
        public float LastSeenWhen { get; set; }
        public float SecondsSinceSeen => Game.ClockTime - LastSeenWhen;
        public bool Abort { get; set; }
        public float StartRecallTime { get; set; }
        public float AbortRecallTime { get; set; }
        public float FinishRecallTime { get; set; }
        public float Start { get; set; }
        public float Duration { get; set; }
        public float TickCount { get; set; }
        public Texture HudSprite;
        public Texture MinimapSprite;
        public HeroInfo(Obj_AI_Hero hero)
        {
            Hero = hero;
            HudSprite = Program.HudIcon(hero.ChampionName, Color.DarkGoldenrod, 100);
            MinimapSprite = Program.MinimapIcon(hero.ChampionName);
            IsJungler = hero.SpellBook.Spells.Any(spell => spell.Name.ToLower().Contains("smite"));
            var enemySpawn = ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Type == GameObjectType.obj_HQ && x.Team != ObjectManager.GetLocalPlayer().Team);
            if (enemySpawn != null)
            {
                switch (enemySpawn.Team)
                {
                    case GameObjectTeam.Order:
                        LastVisablePos = enemySpawn.Position + new Vector3(-1000, -1000, -1000);
                        break;
                    case GameObjectTeam.Chaos:
                        LastVisablePos = enemySpawn.Position + new Vector3(1000, 1000, 1000);
                        break;
                }
            }
            StartRecallTime = 0;
            AbortRecallTime = 0;
            FinishRecallTime = 0;
            var bmpHud = Resources.Cd_Hud;
            Hud = new Texture(bmpHud);
            foreach (var spell in Hero.SpellBook.Spells)
            {
                TrackingSpell tspell;
                switch (spell.Slot)
                {
                    case SpellSlot.Summoner1:
                        tspell = new TrackingSpell(spell, true);
                        Summoner1 = tspell;
                        break;
                    case SpellSlot.Summoner2:
                        tspell = new TrackingSpell(spell, true);
                        Summoner2 = tspell;
                        break;
                    case SpellSlot.Q:
                    case SpellSlot.W:
                    case SpellSlot.E:
                    case SpellSlot.R:
                        tspell = new TrackingSpell(spell);
                        Spells.Add(tspell);
                        break;
                }
                switch (spell.Slot)
                {
                    case SpellSlot.Summoner1:
                        tspell = new TrackingSpell(spell, true);
                        Summoner1Hud = tspell;
                        break;
                    case SpellSlot.Summoner2:
                        tspell = new TrackingSpell(spell, true);
                        Summoner2Hud = tspell;
                        break;
                    case SpellSlot.R:
                        tspell = new TrackingSpell(spell);
                        SummonerRHud = tspell;
                        break;
                }
            }
        }
    }
    public class TrackingSpell
    {
        public Spell Spell { get; }
        private bool IsSummoner { get; }
        public int StatusBoxWidth { get; }
        public int StatusBoxHeight { get; }
        public float TimeUntilReady => Spell.CooldownEnd - Game.ClockTime;
        public int CurrentWidth
        {
            get
            {
                if (TimeUntilReady <= 0)
                {
                    return StatusBoxWidth;
                }
                var timeLeft = TimeUntilReady;
                var percent = timeLeft / Spell.Cooldown;
                var width = percent * StatusBoxWidth;
                return (int)width;
            }
        }

        public Color CurrentColor => TimeUntilReady <= 0 ? Color.Green : Color.LawnGreen;
        public TrackingSpell(Spell spell, bool summ = false)
        {
            Spell = spell;
            IsSummoner = summ;
            if (IsSummoner)
            {
                LoadTexture();
                LoadTextureSideHud();
                StatusBoxHeight = 20;
                StatusBoxWidth = 20;
            }
            else
            {
                StatusBoxHeight = 4;
                StatusBoxWidth = 19;
            }
            LoadUltiSideHud();
        }

        private void LoadTexture()
        {
            var name = Spell.Name;
            if (Spell.Name.ToLower().Contains("smite"))
            {
                name = "SummonerSmite";
            }
            Bitmap bmp;
            try
            {
                bmp = Program.ResizeImage((Bitmap)Resources.ResourceManager.GetObject(name),
                   new Size(19, 19));
            }
            catch (Exception)
            {
                Console.WriteLine($@"Texture not found for {Spell.Name} :(");
                bmp = Program.ResizeImage((Bitmap)Resources.ResourceManager.GetObject("SummonerDot"),
                    new Size(19, 19));
            }
            if (bmp != null)
            {
                SpellTexture = new Texture(bmp);
            }
        }
        private void LoadTextureSideHud()
        {
            var name = Spell.Name;
            if (Spell.Name.ToLower().Contains("smite"))
            {
                name = "SummonerSmite";
            }
            try
            {
                SpellTextureHud = Program.SummonerSpells(name);
            }
            catch (Exception)
            {
                Console.WriteLine($@"Texture not found for {Spell.Name} :(");
                SpellTextureHud = Program.SummonerSpells("SummonerDot");
            }
        }
        private void LoadUltiSideHud()
        {
            UltiTextureHud = Program.UltiIcon("Ulti_Side_Hud");
        }
        internal Texture SpellTexture { get; private set; }
        internal Texture SpellTextureHud { get; private set; }
        internal Texture UltiTextureHud { get; private set; }
    }
    internal class Tracker : Program
    {
        public static List<HiddenObjects> HiddenObjectsList = new List<HiddenObjects>();
        public static List<HeroInfo> HeroInfoList = new List<HeroInfo>();
        public static Texture PinkMiniMap, WardMiniMap;
        private readonly Vector3 _enemySpawn;
        public Tracker()
        {
            var enemySpawn = ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Type == GameObjectType.obj_HQ && x.Team != ObjectManager.GetLocalPlayer().Team);
            if (enemySpawn != null)
            {
                switch (enemySpawn.Team)
                {
                    case GameObjectTeam.Order:
                        _enemySpawn = enemySpawn.Position + new Vector3(-1000, -1000, -1000);
                        break;
                    case GameObjectTeam.Chaos:
                        _enemySpawn = enemySpawn.Position + new Vector3(1000, 1000, 1000);
                        break;
                }
            }
            foreach (var hero in GameObjects.Heroes)
            {
                if (hero.ChampionName == "PracticeTool_TargetDummy")
                {
                    continue;
                }
                HeroInfoList.Add(new HeroInfo(hero));
            }
            var bmpPinkMiniMap = Resources.Mini_Map_Pink;
            var bmpWardMiniMap = ResizeImage((Bitmap)Resources.ResourceManager.GetObject("Mini_Map_Ward"), new Size(12, 12));
            PinkMiniMap = new Texture(bmpPinkMiniMap);
            WardMiniMap = new Texture(bmpWardMiniMap);
            Game.OnUpdate += GameOnUpdate;
            Teleport.OnTeleport += OnRecall;
            Obj_AI_Base.OnProcessSpellCast += HiddenObjectsSpellCast;
            GameObject.OnDestroy += Destroy;
            GameObject.OnCreate += NewOnCreate;
        }
        private static void OnRecall(Obj_AI_Base sender, Teleport.TeleportEventArgs args)
        {
            var unit = sender as Obj_AI_Hero;
            if (unit == null || !unit.IsValid || unit.IsAlly) return;
            var championInfoOne = HeroInfoList.Find(x => x.Hero.NetworkId == sender.NetworkId);
            if (args.Type == TeleportType.Recall)
            {              
                switch (args.Status)
                {
                    case TeleportStatus.Start:
                        championInfoOne.Start = args.Start;
                        championInfoOne.Duration = args.Duration;
                        championInfoOne.TickCount = Game.TickCount;
                        championInfoOne.StartRecallTime = Game.ClockTime;
                        championInfoOne.RecallEnd = Game.ClockTime;
                        championInfoOne.Abort = false;
                        if (args.Duration == 8000)
                        {
                            championInfoOne.RecallTime = 8;
                        }
                        else if(args.Duration == 7000)
                        {
                            championInfoOne.RecallTime = 7;
                        }
                        else if (args.Duration == 4500)
                        {
                            championInfoOne.RecallTime = 4;
                        }
                        else if (args.Duration == 4000)
                        {
                            championInfoOne.RecallTime = 4;
                        }
                        break;
                    case TeleportStatus.Abort:
                        championInfoOne.AbortRecallTime = Game.ClockTime;
                        championInfoOne.Abort = true;
                        break;
                    case TeleportStatus.Finish:
                        var enemySpawn = ObjectManager.Get<GameObject>().FirstOrDefault(x => x.Type == GameObjectType.obj_HQ && x.Team != ObjectManager.GetLocalPlayer().Team);
                        championInfoOne.FinishRecallTime = Game.ClockTime;
                        championInfoOne.LastSeenWhen = Game.ClockTime;  
                        if (enemySpawn != null)
                        {
                            switch (enemySpawn.Team)
                            {
                                case GameObjectTeam.Order:
                                    championInfoOne.LastVisablePos = enemySpawn.Position + new Vector3(-1000, -1000, -1000);
                                    championInfoOne.LastWayPoint = enemySpawn.Position + new Vector3(-1000, -1000, -1000);
                                    break;
                                case GameObjectTeam.Chaos:
                                    championInfoOne.LastVisablePos = enemySpawn.Position + new Vector3(1000, 1000, 1000);
                                    championInfoOne.LastWayPoint = enemySpawn.Position + new Vector3(1000, 1000, 1000);
                                    break;
                            }

                            
                        }                          
                        break;
                }
            }
        }
        private void GameOnUpdate()
        {
            foreach (var extra in HeroInfoList.Where(x => x.Hero.IsEnemy))
            {
                var enemy = extra.Hero;
                var mpos = ObjectManager.GetLocalPlayer().ServerPosition;
                var dvpos = enemy.Position;
                var mcell = NavMesh.WorldToCell(mpos);
                var dvcell = NavMesh.WorldToCell(dvpos);
                var mgrass = mcell.Flags.HasFlag(NavCellFlags.Grass);
                var enemygrass = dvcell.Flags.HasFlag(NavCellFlags.Grass);
                var distance = mpos.Distance(dvpos);

                if (distance <= 400 && (!enemygrass || mgrass && distance < 350))
                {
                    if (!enemy.IsVisible)
                    {
                        extra.Displaying = false;
                    }
                }
                if (Game.ClockTime - extra.FinishRecallTime < 1)
                {
                    extra.LastVisablePos = _enemySpawn;
                    extra.TimeSpeed = Game.TickCount;
                    extra.LastWayPoint = _enemySpawn;
                    extra.LastSeenWhen = Game.ClockTime;
                }
                if (enemy.IsDead)
                {
                    extra.LastVisablePos = _enemySpawn;
                    extra.TimeSpeed = Game.TickCount;
                    extra.LastWayPoint = _enemySpawn;
                    extra.LastSeenWhen = Game.ClockTime;
                }
                else if (enemy.IsVisible)
                {
                    extra.LastWayPoint = extra.Hero.GetWaypoints().Last().To3D();
                    extra.LastVisablePos = enemy.Position;
                    extra.TimeSpeed = Game.TickCount;
                    extra.LastSeenWhen = Game.ClockTime;
                    extra.Displaying = true;
                }
                if (Game.ClockTime >= 0 && Game.ClockTime < 15)
                {
                    extra.TimeSpeed = Game.TickCount;
                }
            }

            HiddenObjectsList.RemoveAll(x => Game.ClockTime >= x.EndTime && x.WardName != "TrinketTotemLvl1" && (x.Type == 2 || x.Type == 3 || x.Type == 5));
            HiddenObjectsList.RemoveAll(x => Game.ClockTime >= x.EndTime + 3.5f * (x.Level - 1) && x.WardName == "TrinketTotemLvl1" && x.Type == 2);
        }

        #region - Game Hidden Objects Tracker -
        private static void HiddenObjectsSpellCast(Obj_AI_Base sender, Obj_AI_BaseMissileClientDataEventArgs args)
        {
            var hero = sender as Obj_AI_Hero;
            if (hero != null && hero.IsEnemy)
            {
                if (args.SpellData.Name == "TeemoRCast")
                {
                    var hit = HiddenObjectsList.Where(x => x.Location.Distance(args.End) <= 150).ToList();
                    if (hit.Count < 1)
                    {
                        WardSpells(args.SpellData.Name, args.End, hero.Level);
                    }
                }
                else
                {
                    WardSpells(args.SpellData.Name, args.End, hero.Level);
                }
            }
        }

        private static void NewOnCreate(GameObject sender)
        {
            if (!sender.IsEnemy || sender.IsAlly) return;
            if (sender.Name == "Ziggs_Base_E_placedMine.troy")
            {
                HiddenObjectsList.Add(new HiddenObjects { Type = 5, WardName = "ZiggsE", Location = sender.Position, EndTime = Game.ClockTime + 10 });
            }
            var missile = sender as MissileClient;
            if (missile != null)
            {
                if (missile.SpellData.Name == "BantamTrapBounceSpell" && !HiddenObjectsList.Exists(x => missile.EndPosition == x.Location))
                {
                    WardSpells("TeemoRCast", missile.EndPosition, 1);
                }
            }
            var minion = sender as Obj_AI_Minion;
            if (minion != null)
            {
                if ((sender.Name.ToLower() == "jammerdevice" || sender.Name.ToLower() == "sightward" || sender.Name.ToLower() == "visionward") && !HiddenObjectsList.Exists(x => x.Location.Distance(sender.Position) < 100))
                {
                    foreach (var ward in HiddenObjectsList)
                    {
                        if (ward.Location.Distance(sender.Position) < 400)
                        {
                            if (ward.Type == 0)
                            {
                                HiddenObjectsList.Remove(ward);
                                return;
                            }
                        }
                    }

                    var invisible = (Obj_AI_Minion)sender;
                    if (Math.Abs(invisible.Mana) <= 0)
                    {
                        if (sender.Name.ToLower() == "jammerdevice")
                        {
                            HiddenObjectsList.Add(new HiddenObjects { Type = 1, Location = sender.Position, EndTime = float.MaxValue });
                        }
                        if (sender.Name.ToLower() == "sightward")
                        {
                            HiddenObjectsList.Add(new HiddenObjects { Type = 4, Location = sender.Position, EndTime = float.MaxValue });
                        }
                    }
                    else
                    {
                        HiddenObjectsList.Add(invisible.UnitSkinName == "YellowTrinket" ? new HiddenObjects { Type = 2, Location = sender.Position, EndTime = Game.ClockTime + invisible.Mana + 60 } : new HiddenObjects
                        { Type = 2, Location = sender.Position, EndTime = Game.ClockTime + invisible.Mana });
                    }
                }

                if ((sender.Name.ToLower() == "noxious trap" || sender.Name.ToLower() == "cupcake trap" || sender.Name == "Jhin_Base_E_Trap_Idle.troy" || sender.Name == "Jhin_Base_E_Trap_indicator_enemy.troy" || sender.Name == "Nidalee_Base_W_TC_Red.troy" || sender.Name == "Nidalee_Base_W_Tar.troy" || sender.Name == "Teemo_Base_R_CollisionBox_RingEnemy.troy" || sender.Name == "caitlyn_Base_yordleTrap_idle_red.troy" || sender.Name == "caitlyn_Base_yordleTrap_idle_red_inbrush.troy" || sender.Name.ToLower() == "jack in the box") || sender.Name == "Jinx_Base_E_Mine_Ready_Red" || sender.Name == "Jinx_Base_E_Mine_Explosion" && !HiddenObjectsList.Exists(x => x.Location.Distance(sender.Position) < 100))
                {
                    foreach (var ward in HiddenObjectsList)
                    {
                        if (ward.Location.Distance(sender.Position) < 400)
                        {
                            if (ward.Type == 0)
                            {
                                HiddenObjectsList.Remove(ward);
                                return;
                            }
                        }
                    }
                    var unitSkinName = (Obj_AI_Minion)sender;
                    if (unitSkinName.UnitSkinName == "JhinTrap")
                    {
                        if (HeroInfoList.FirstOrDefault(x => x.Hero.ChampionName == "Jhin" && !x.Hero.IsVisible) != null)
                        {
                            HiddenObjectsList.Add(new HiddenObjects { Type = 3, WardName = "JhinE", Location = sender.Position, EndTime = Game.ClockTime + 120 });
                        }
                    }
                    if (unitSkinName.UnitSkinName == "TeemoMushroom")
                    {
                        if (HeroInfoList.FirstOrDefault(x => x.Hero.ChampionName == "Teemo" && !x.Hero.IsVisible) != null)
                        {
                            HiddenObjectsList.Add(new HiddenObjects { Type = 3, WardName = "TeemoRCast", Location = sender.Position, EndTime = Game.ClockTime + 300 });
                        }
                    }
                    if (unitSkinName.UnitSkinName == "NidaleeSpear")
                    {
                        if (HeroInfoList.FirstOrDefault(x => x.Hero.ChampionName == "Nidalee" && !x.Hero.IsVisible) != null)
                        {
                            HiddenObjectsList.Add(new HiddenObjects { Type = 3, WardName = "Bushwhack", Location = sender.Position, EndTime = Game.ClockTime + 120 });
                        }
                    }
                    if (unitSkinName.UnitSkinName == "ShacoBox")
                    {
                        if (HeroInfoList.FirstOrDefault(x => x.Hero.ChampionName == "Shaco" && !x.Hero.IsVisible) != null)
                        {
                            HiddenObjectsList.Add(new HiddenObjects { Type = 3, WardName = "JackInTheBox", Location = sender.Position, EndTime = Game.ClockTime + 60 });
                        }
                    }
                    if (unitSkinName.UnitSkinName == "CaitlynTrap")
                    {
                        if (HeroInfoList.FirstOrDefault(x => x.Hero.ChampionName == "Caitlyn" && !x.Hero.IsVisible) != null)
                        {
                            HiddenObjectsList.Add(new HiddenObjects { Type = 5, WardName = "CaitlynYordleTrap", Location = sender.Position, EndTime = Game.ClockTime + 50 });
                        }
                    }
                    if (unitSkinName.UnitSkinName == "JinxMine")
                    {
                        if (HeroInfoList.FirstOrDefault(x => x.Hero.ChampionName == "Jinx" && !x.Hero.IsVisible) != null)
                        {
                            HiddenObjectsList.Add(new HiddenObjects { Type = 5, WardName = "JinxE", Location = sender.Position, EndTime = Game.ClockTime + 5 });
                        }
                    }
                }
            }
        }

        private static void Destroy(GameObject sender)
        {
            if (sender.IsAlly) return;
            if (sender.Name.ToLower() == "sightward" || sender.Name.ToLower() == "visionward" || sender.Name.ToLower() == "jammerdevice")
            {
                HiddenObjectsList.RemoveAll(x => x.Location.Distance(sender.Position) <= 25 && x.Type != 5 && x.Type != 3); return;
            }
            if (sender.Name.ToLower() == "noxious trap" || sender.Name.ToLower() == "cupcake trap" || sender.Name == "Jhin_Base_E_Trap_Idle.troy" || sender.Name == "Jhin_Base_E_Trap_indicator_enemy.troy" || sender.Name == "Nidalee_Base_W_TC_Red.troy" || sender.Name == "Nidalee_Base_W_Tar.troy" || sender.Name == "Teemo_Base_R_CollisionBox_RingEnemy.troy" || sender.Name == "caitlyn_Base_yordleTrap_idle_red.troy" || sender.Name == "caitlyn_Base_yordleTrap_idle_red_inbrush.troy" || sender.Name == "Ziggs_Base_E_placedMine.troy" || sender.Name == "Jinx_Base_E_Mine_Ready_Red" || sender.Name == "Jinx_Base_E_Mine_Explosion")
            {
                /*Jhin Çalıda çalışmıyor çalıda görüş olması gerek.*/
                HiddenObjectsList.RemoveAll(x => x.Location.Distance(sender.Position) <= 25 && x.WardName == "JhinE");

                HiddenObjectsList.RemoveAll(x => x.Location.Distance(sender.Position) <= 25 && x.WardName == "TeemoRCast");

                /*Nidale Çalıda çalışmıyor çalıda görüş olması gerek.*/
                HiddenObjectsList.RemoveAll(x => x.Location.Distance(sender.Position) <= 25 && x.WardName == "Bushwhack");

                /*Caitlyn Çalıda çalışmıyor çalıda görüş olması gerek.*/
                HiddenObjectsList.RemoveAll(x => x.Location.Distance(sender.Position) <= 25 && x.WardName == "CaitlynYordleTrap");

                HiddenObjectsList.RemoveAll(x => x.Location.Distance(sender.Position) <= 25 && x.WardName == "ZiggsE");

                HiddenObjectsList.RemoveAll(x => x.Location.Distance(sender.Position) <= 25 && x.WardName == "JinxE");
            }
            if (sender.Name.ToLower() == "jack in the box")
            {
                HiddenObjectsList.RemoveAll(x => x.Location.Distance(sender.Position) <= 25 && x.WardName == "JackInTheBox");
            }
        }

        private static void WardSpells(string name, Vector3 lastSeenPos, int level)
        {
            switch (name)
            {
                case "JammerDevice":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 1, WardName = "JammerDevice", Location = lastSeenPos, EndTime = float.MaxValue });
                    break;
                case "TrinketOrbLvl3":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 4, WardName = "TrinketOrbLvl3", Location = lastSeenPos, EndTime = float.MaxValue });
                    break;
                case "TrinketTotemLvl1":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 2, WardName = "TrinketTotemLvl1", Location = lastSeenPos, Level = level, EndTime = Game.ClockTime + 60 });
                    break;
                case "ItemGhostWard":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 2, WardName = "ItemGhostWard", Location = lastSeenPos, EndTime = Game.ClockTime + 150 });
                    break;
                case "JhinE":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 3, WardName = "JhinE", Location = lastSeenPos, EndTime = Game.ClockTime + 120 });
                    break;
                case "TeemoRCast":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 3, WardName = "TeemoRCast", Location = lastSeenPos, EndTime = Game.ClockTime + 300 });
                    break;
                case "Bushwhack":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 3, WardName = "Bushwhack", Location = lastSeenPos, EndTime = Game.ClockTime + 120 });
                    break;
                case "JackInTheBox":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 3, WardName = "JackInTheBox", Location = lastSeenPos, EndTime = Game.ClockTime + 60 });
                    break;
                case "CaitlynYordleTrap": 
                    HiddenObjectsList.Add(new HiddenObjects { Type = 5, WardName = "CaitlynYordleTrap", Location = lastSeenPos, EndTime = Game.ClockTime + 50 });
                    break;
                case "JinxE":
                    HiddenObjectsList.Add(new HiddenObjects { Type = 5, WardName = "JinxE", Location = lastSeenPos, EndTime = Game.ClockTime + 5 });
                    break;
            }
        }
        #endregion
    }

    #region Recall(Start-Abort-Finish)
    public static class Teleport
    {
        public delegate void TeleportHandler(Obj_AI_Base sender, TeleportEventArgs args);
        internal static readonly Dictionary<int, TeleportEventArgs> TeleportDataNetId = new Dictionary<int, TeleportEventArgs>();
        static Teleport()
        {
            Obj_AI_Base.OnTeleport += OnUnitTeleport;
        }
        public static event TeleportHandler OnTeleport;
        private static void OnUnitTeleport(Obj_AI_Base sender, Obj_AI_BaseTeleportEventArgs e)
        {

            var eventArgs = new TeleportEventArgs
            {
                Status = TeleportStatus.Unknown,
                Type = TeleportType.Unknown
            };

            if (sender == null)
            {
                return;
            }

            if (!TeleportDataNetId.ContainsKey(sender.NetworkId))
            {
                TeleportDataNetId[sender.NetworkId] = eventArgs;
            }

            if (!string.IsNullOrEmpty(e.DisplayName))
            {
                eventArgs.Status = TeleportStatus.Start;
                eventArgs.Duration = GetDuration(e);
                eventArgs.Type = GetType(e);
                eventArgs.Start = Game.TickCount;

                TeleportDataNetId[sender.NetworkId] = eventArgs;
            }
            else
            {
                eventArgs = TeleportDataNetId[sender.NetworkId];
                eventArgs.Status = Game.TickCount - eventArgs.Start < eventArgs.Duration - 250 ? TeleportStatus.Abort : TeleportStatus.Finish;
            }

            OnTeleport?.Invoke(sender, eventArgs);
        }
        internal static TeleportType GetType(Obj_AI_BaseTeleportEventArgs args)
        {
            switch (args.DisplayName)
            {
                case "Recall": return TeleportType.Recall;
                default: return TeleportType.Recall;
            }
        }
        internal static int GetDuration(Obj_AI_BaseTeleportEventArgs args)
        {
            switch (GetType(args))
            {
                case TeleportType.Recall: return GetRecallDuration(args);
                default: return 3500;
            }
        }
        internal static int GetRecallDuration(Obj_AI_BaseTeleportEventArgs args)
        {
            switch (args.DisplayName.ToLower())
            {
                case "recall": return 8000;
                case "recallimproved": return 7000;
                case "odinrecall": return 4500;
                case "odinrecallimproved": return 4000;
                case "superrecall": return 4000;
                case "superrecallimproved": return 4000;
                default: return 8000;
            }
        }
        public class TeleportEventArgs : EventArgs
        {
            public int Start { get; internal set; }
            public int Duration { get; internal set; }
            public TeleportType Type { get; internal set; }
            public TeleportStatus Status { get; internal set; }
        }
    }
    public enum TeleportType
    {
        Recall,
        Unknown
    }
    public enum TeleportStatus
    {
        Start,
        Abort,
        Finish,
        Unknown
    }
    #endregion

}
