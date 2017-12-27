using System;
using Aimtec;
using System.Drawing;
using Aimtec.SDK.Menu;
using Aimtec.SDK.Events;
using System.Diagnostics;
using Aimtec.SDK.Orbwalking;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using Nefarious_Utility.Properties;

namespace Nefarious_Utility
{
    internal class Program
    {
        public static Menu Menu;
        public static Obj_AI_Hero Player => ObjectManager.GetLocalPlayer();
        public static IOrbwalker Orbwalker => Aimtec.SDK.Orbwalking.Orbwalker.Implementation;
        private static void Main()
        {
            GameEvents.GameStart += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {
            Menu = new Menu("Index", "Nefarious Utility", true);
            try
            {
                // ReSharper disable ObjectCreationAsStatement
                new Tracker();
                new Utility();
                Menu.Attach();
                Console.WriteLine(@"Nefarious Utility loaded");
                // ReSharper restore ObjectCreationAsStatement
            }
            catch (Exception)
            {
                Console.WriteLine(@"EROR Nefarious Utility!");
            }
        }
        public static void DrawCircleOnMinimap(Vector3 center, float radius, Color color, int thickness, int quality = 100)
        {
            var pointList = new List<Vector3>();
            for (var i = 0; i < quality; i++)
            {
                var angle = i * Math.PI * 2 / quality;
                pointList.Add(
                    new Vector3(
                        center.X + radius * (float)Math.Cos(angle),
                        center.Y,
                        center.Z + radius * (float)Math.Sin(angle))
                );
            }

            for (var i = 0; i < pointList.Count; i++)
            {
                var a = pointList[i];
                var b = pointList[i == pointList.Count - 1 ? 0 : i + 1];
                Vector2 aonScreen;
                Vector2 bonScreen;
                Render.WorldToMinimap(a, out aonScreen);
                Render.WorldToMinimap(b, out bonScreen);
                Render.Line(aonScreen, bonScreen, thickness, true, color);
            }
        }

        #region Image Restoration
        public static Texture SummonerSpells(string name)
        {
            var srcBitmap = ResizeImage((Bitmap)Resources.ResourceManager.GetObject(name), new Size(19, 19));
            if (srcBitmap == null)
            {
                Console.WriteLine(@"Texture not found for " + name);
                srcBitmap = (Bitmap)Resources.ResourceManager.GetObject("Default");
            }
            Debug.Assert(srcBitmap != null, nameof(srcBitmap) + " != null");
            var img = new Bitmap(srcBitmap.Width + 2, srcBitmap.Width + 2);
            var cropRect = new System.Drawing.Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);

            using (var sourceImage = srcBitmap)
            {
                using (var croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(tb, 0, 0, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            var finalSprite = new Texture(img);
            return finalSprite;
        }
        public static Texture UltiIcon(string name)
        {
            var srcBitmap = ResizeImage((Bitmap)Resources.ResourceManager.GetObject(name), new Size(19, 19));
            if (srcBitmap == null)
            {
                Console.WriteLine(@"Texture not found for " + name);
                srcBitmap = (Bitmap)Resources.ResourceManager.GetObject("Default");
            }
            Debug.Assert(srcBitmap != null, nameof(srcBitmap) + " != null");
            var img = new Bitmap(srcBitmap.Width + 2, srcBitmap.Width + 2);
            var cropRect = new System.Drawing.Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);

            using (var sourceImage = srcBitmap)
            {
                using (var croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(tb, -1, -1, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            var finalSprite = new Texture(img);
            return finalSprite;
        }
        public static Texture MinimapIcon(string name)
        {
            var srcBitmap = ResizeImage((Bitmap)Resources.ResourceManager.GetObject(name), new Size(22, 22));
            if (srcBitmap == null)
            {
                Console.WriteLine(@"Texture not found for " + name);
                srcBitmap = (Bitmap)Resources.ResourceManager.GetObject("Default");
            }
            Debug.Assert(srcBitmap != null, nameof(srcBitmap) + " != null");
            var img = new Bitmap(srcBitmap.Width + 2, srcBitmap.Width + 2);
            var cropRect = new System.Drawing.Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);
            using (var sourceImage = srcBitmap)
            {
                using (var croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(tb, 0, 0, srcBitmap.Width, srcBitmap.Width);

                            var p = new Pen(Color.OrangeRed, 1) { Alignment = PenAlignment.Center };
                            g.DrawEllipse(p, 0, 0, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            var finalSprite = new Texture(img);
            return finalSprite;
        }
        public static Texture HudIcon(string name, Color color, int opacity = 60)
        {
            var srcBitmap = ResizeImage((Bitmap)Resources.ResourceManager.GetObject(name), new Size(42, 42));
            if (srcBitmap == null)
            {
                Console.WriteLine(@"Texture not found for " + name);
                srcBitmap = (Bitmap)Resources.ResourceManager.GetObject("Default");
            }
            Debug.Assert(srcBitmap != null, nameof(srcBitmap) + " != null");
            var img = new Bitmap(srcBitmap.Width + 2, srcBitmap.Width + 2);
            var cropRect = new System.Drawing.Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);

            using (var sourceImage = srcBitmap)
            {
                using (var croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(tb, 0, 0, srcBitmap.Width, srcBitmap.Width);

                            var p = new Pen(color, 2) { Alignment = PenAlignment.Center };
                            g.DrawEllipse(p, 0, 0, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            var finalSprite = new Texture(ChangeOpacity(img, opacity));
            return finalSprite;
        }
        public static Texture RadrarIcon(string name, Color color, int opacity = 60)
        {
            var srcBitmap = (Bitmap)Resources.ResourceManager.GetObject(name);
            if (srcBitmap == null)
            {
                Console.WriteLine(@"Texture not found for " + name);
                srcBitmap = (Bitmap)Resources.ResourceManager.GetObject("Default");
            }
            Debug.Assert(srcBitmap != null, nameof(srcBitmap) + " != null");
            var img = new Bitmap(srcBitmap.Width + 20, srcBitmap.Width + 20);
            var cropRect = new System.Drawing.Rectangle(0, 0, srcBitmap.Width, srcBitmap.Width);

            using (var sourceImage = srcBitmap)
            {
                using (var croppedImage = sourceImage.Clone(cropRect, sourceImage.PixelFormat))
                {
                    using (var tb = new TextureBrush(croppedImage))
                    {
                        using (var g = Graphics.FromImage(img))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.FillEllipse(tb, 0, 0, srcBitmap.Width, srcBitmap.Width);

                            var p = new Pen(color, 5) { Alignment = PenAlignment.Inset };
                            g.DrawEllipse(p, 0, 0, srcBitmap.Width, srcBitmap.Width);
                        }
                    }
                }
            }
            srcBitmap.Dispose();
            var finalSprite = new Texture(ChangeOpacity(img, opacity));
            return finalSprite;
        }
        public static Bitmap ChangeOpacity(Bitmap img, int opacity)
        {
            var iconOpacity = opacity / 100.0f;
            var bmp = new Bitmap(img.Width, img.Height);
            var graphics = Graphics.FromImage(bmp);
            var colormatrix = new ColorMatrix { Matrix33 = iconOpacity };
            var imgAttribute = new ImageAttributes();
            imgAttribute.SetColorMatrix(colormatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            graphics.DrawImage(
                img, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, img.Width, img.Height, GraphicsUnit.Pixel,
                imgAttribute);
            graphics.Dispose();
            img.Dispose();
            return bmp;
        }
        public static Bitmap ResizeImage(Bitmap imgToResize, Size size)
        {
            try
            {
                var b = new Bitmap(size.Width, size.Height);
                using (var g = Graphics.FromImage(b))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.DrawImage(imgToResize, 0, 0, size.Width, size.Height);
                }
                return b;
            }
            catch (Exception e)
            {
                Console.WriteLine(@"Bitmap could not be resized " + e);
                return imgToResize;
            }
        }
        #endregion

        #region X and Y Offset
        private static float _x, _y;

        public static float Xhud(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Annie": _x = 18f; break; // Yapıldı.
                case "Darius": _x = 30f; break; // Yapıldı.
                case "Renekton": _x = 29f; break; // Yapıldı.
                case "Ornn": _x = 29f; break; // Yapıldı.
                case "Sion": _x = 29f; break; // Yapıldı.
                case "Thresh": _x = 28f; break; // Yapıldı.
                default: _x = 23f; break;
            }
            return _x;
        }

        public static float XhudMe(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Annie": _x = 18f; break;  // Yapıldı.
                case "Ryze": _x = 23f; break; // Yapıldı.
                default: _x = 23f; break;
            }
            return _x;
        }

        /*Enemy Hud*/
        public static float Yenemy(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Aatrox": _y = 13f; break;
                case "Ahri": _y = 18f; break; // Yapıldı.
                case "Akali": _y = 18f; break; // Yapıldı.
                case "Alistar": _y = 11f; break; // Yapıldı.
                case "Amumu": _y = 18f; break; // Yapıldı.
                case "Anivia": _y = 13f; break; // Yapıldı.
                case "Annie": _y = 30f; break; // Yapıldı.
                case "Ashe": _y = 18f; break; // Yapıldı.
                case "AurelionSol": _y = 13f; break;
                case "Azir": _y = 13f; break; // Yapıldı.
                case "Bard": _y = 9f; break; // Yapıldı.
                case "Blitzcrank": _y = 11f; break;  // Yapıldı.
                case "Brand": _y = 13f; break; // Yapıldı.
                case "Braum": _y = 14f; break; // Yapıldı.
                case "Caitlyn": _y = 18f; break; // Yapıldı.
                case "Camille": _y = 13f; break;
                case "Cassiopeia": _y = 13f; break; // Yapıldı.
                case "Chogath": _y = 17f; break; // Yapıldı.
                case "Corki": _y = 30f; break; // Yapıldı.
                case "Darius": _y = 15f; break; // Yapıldı.
                case "Diana": _y = 27f; break; // Yapıldı.
                case "DrMundo": _y = 15f; break; // Yapıldı.
                case "Draven": _y = 18f; break; // Yapıldı.
                case "Ekko": _y = 18f; break; // Yapıldı.
                case "Elise": _y = 7f; break; // Yapıldı.
                case "Evelynn": _y = 13f; break; // Yapıldı.
                case "Ezreal": _y = 18f; break; // Yapıldı.
                case "FiddleSticks": _y = 16f; break; // Yapıldı.
                case "Fiora": _y = 14f; break; // Yapıldı.
                case "Fizz": _y = 18f; break; // Yapıldı.
                case "Galio": _y = 21f; break; // Yapıldı.
                case "Gangplank": _y = 13f; break; // Yapıldı.
                case "Garen": _y = 19f; break; // Yapıldı.
                case "Gnar": _y = 13f; break; // Yapıldı.
                case "Gragas": _y = 13f; break; // Yapıldı.
                case "Graves": _y = 18.5f; break; // Yapıldı.
                case "Hecarim": _y = 18f; break; // Yapıldı.
                case "Heimerdinger": _y = 18f; break; // Yapıldı.
                case "Illaoi": _y = 17f; break; // Yapıldı.
                case "Irelia": _y = 18f; break; // Yapıldı.
                case "Ivern": _y = -10f; break; // Yapıldı.
                case "Janna": _y = 14f; break; // Yapıldı.
                case "JarvanIV": _y = 20f; break; // Yapıldı.
                case "Jax": _y = 10f; break; // Yapıldı.
                case "Jayce": _y = 4f; break; // Yapıldı.
                case "Jhin": _y = 30f; break; // Yapıldı.
                case "Jinx": _y = 18.5f; break; // Yapıldı.
                case "Kalista": _y = 13f; break;
                case "Karma": _y = 13f; break; // Yapıldı.
                case "Karthus": _y = 17f; break; // Yapıldı.
                case "Kassadin": _y = 12f; break; // Yapıldı.
                case "Katarina": _y = 18f; break; // Yapıldı.
                case "Kayle": _y = 11f; break; // Yapıldı.
                case "Kayn": _y = 13f; break; // Yapıldı.
                case "Kennen": _y = 13f; break; // Yapıldı.
                case "Khazix": _y = 17f; break; // Yapıldı.
                case "Kindred": _y = 18f; break; // Yapıldı.
                case "Kled": _y = 4f; break; // Yapıldı.
                case "KogMaw": _y = 17f; break; // Yapıldı.
                case "Leblanc": _y = 20f; break; // Yapıldı.
                case "LeeSin": _y = 14f; break; // Yapıldı.
                case "Leona": _y = 19f; break; // Yapıldı.
                case "Lissandra": _y = 18f; break; // Yapıldı.
                case "Lucian": _y = 13f; break; // Yapıldı.
                case "Lulu": _y = 12f; break; // Yapıldı.
                case "Lux": _y = 11f; break; // Yapıldı.
                case "Malphite": _y = 13f; break; // Yapıldı.
                case "Malzahar": _y = 13f; break; // Yapıldı.
                case "Maokai": _y = 18f; break; // Yapıldı.
                case "MasterYi": _y = 15f; break; // Yapıldı.
                case "MissFortune": _y = 13f; break; // Yapıldı.
                case "Mordekaiser": _y = 18f; break; // Yapıldı.
                case "Morgana": _y = 13f; break; // Yapıldı.
                case "Nami": _y = 14f; break; // Yapıldı.
                case "Nasus": _y = 18f; break; // Yapıldı.
                case "Nautilus": _y = 12f; break; // Yapıldı.
                case "Nidalee": _y = 20f; break; // Yapıldı.
                case "Nocturne": _y = 18f; break; // Yapıldı.
                case "Nunu": _y = 13f; break; // Yapıldı.
                case "Olaf": _y = 13f; break;
                case "Orianna": _y = 13f; break; // Yapıldı.
                case "Ornn": _y = 14f; break; // Yapıldı.
                case "Pantheon": _y = 14f; break; // Yapıldı.
                case "Poppy": _y = 2f; break; // Yapıldı.
                case "Quinn": _y = 13f; break;
                case "Rakan": _y = 1f; break; // Yapıldı.
                case "Rammus": _y = 16f; break; // Yapıldı.
                case "RekSai": _y = 13f; break;
                case "Renekton": _y = 14f; break; // Yapıldı.
                case "Rengar": _y = 18f; break;
                case "Riven": _y = 11f; break; // Yapıldı.
                case "Rumble": _y = 11.5f; break; // Yapıldı.
                case "Ryze": _y = 32f; break; // Yapıldı.
                case "Sejuani": _y = 18.5f; break; // Yapıldı.
                case "Shaco": _y = 18f; break; // Yapıldı.
                case "Shen": _y = 11f; break; // Yapıldı.
                case "Shyvana": _y = 16f; break; // Yapıldı.
                case "Singed": _y = 18f; break; // Yapıldı.
                case "Sion": _y = 14f; break; // Yapıldı.
                case "Sivir": _y = 15f; break; // Yapıldı.
                case "Skarner": _y = 18f; break; // Yapıldı.
                case "Sona": _y = 9.5f; break; // Yapıldı.
                case "Soraka": _y = 9f; break; // Yapıldı.
                case "Swain": _y = 13f; break; // Yapıldı.
                case "Syndra": _y = 13f; break; // Yapıldı.
                case "TahmKench": _y = 18f; break; // Yapıldı.
                case "Taliyah": _y = 3f; break; // Yapıldı.
                case "Talon": _y = 18f; break; // Yapıldı.
                case "Taric": _y = 18f; break; // Yapıldı.
                case "Teemo": _y = 13f; break; // Yapıldı.
                case "Thresh": _y = 18f; break; // Yapıldı.
                case "Tristana": _y = 13f; break; // Yapıldı.
                case "Trundle": _y = 23f; break; // Yapıldı.
                case "Tryndamere": _y = 20f; break; // Yapıldı.
                case "TwistedFate": _y = 18f; break; // Yapıldı.
                case "Twitch": _y = 15f; break; // Yapıldı.
                case "Udyr": _y = 18f; break; // Yapıldı.
                case "Urgot": _y = 9f; break; // Yapıldı.
                case "Varus": _y = 18f; break; // Yapıldı.
                case "Vayne": _y = 18f; break; // Yapıldı.
                case "Veigar": _y = 18f; break; // Yapıldı.
                case "Velkoz": _y = -10f; break; // Yapıldı.
                case "Vi": _y = 15f; break; // Yapıldı.
                case "Viktor": _y = 28f; break; // Yapıldı.
                case "Vladimir": _y = 18f; break; // Yapıldı.
                case "Volibear": _y = 13f; break; // Yapıldı.
                case "Warwick": _y = 11f; break; // Yapıldı.
                case "MonkeyKing": _y = 18f; break; // Yapıldı.
                case "Xayah": _y = 20f; break; // Yapıldı.
                case "Xerath": _y = 13f; break; // Yapıldı.
                case "XinZhao": _y = 29f; break; // Yapıldı.
                case "Yasuo": _y = 11f; break; // Yapıldı.
                case "Yorick": _y = 18f; break; // Yapıldı.
                case "Zac": _y = 16f; break; // Yapıldı.
                case "Zed": _y = 18f; break; // Yapıldı.
                case "Ziggs": _y = 13f; break; // Yapıldı.
                case "Zilean": _y = 15f; break; // Yapıldı.
                case "Zoe": _y = 5f; break; // Yapıldı.
                case "Zyra": _y = 14f; break; // Yapıldı.
                default: _y = 13f; break;
            }
            return _y;
        }
        /*Ally Hud*/
        public static float Yally(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Aatrox": _y = 15f; break;
                case "Ahri": _y = 18f; break; // Yapıldı.
                case "Akali": _y = 19f; break; // Yapıldı.
                case "Alistar": _y = 14f; break; // Yapıldı.
                case "Amumu": _y = 19f; break; // Yapıldı.
                case "Anivia": _y = 15f; break; // Yapıldı.
                case "Annie": _y = 29f; break; // Yapıldı.
                case "Ashe": _y = 19f; break; // Yapıldı.
                case "AurelionSol": _y = 15f; break;
                case "Azir": _y = 15f; break; // Yapıldı.
                case "Bard": _y = 12f; break; // Yapıldı.
                case "Blitzcrank": _y = 14f; break; // Yapıldı.
                case "Brand": _y = 16f; break; // Yapıldı.
                case "Braum": _y = 16f; break; // Yapıldı.
                case "Caitlyn": _y = 19f; break; // Yapıldı.
                case "Camille": _y = 15f; break;
                case "Cassiopeia": _y = 15f; break; // Yapıldı.
                case "Chogath": _y = 19f; break; // Yapıldı.
                case "Corki": _y = 31f; break; // Yapıldı.
                case "Darius": _y = 17f; break; // Yapıldı.
                case "Diana": _y = 25f; break; // Yapıldı.
                case "DrMundo": _y = 17f; break; // Yapıldı.
                case "Draven": _y = 19f; break; // Yapıldı.
                case "Ekko": _y = 19f; break; // Yapıldı.
                case "Elise": _y = 11f; break; // Yapıldı.
                case "Evelynn": _y = 15f; break; // Yapıldı.
                case "Ezreal": _y = 19f; break; // Yapıldı.
                case "FiddleSticks": _y = 17f; break; // Yapıldı.
                case "Fiora": _y = 16f; break; // Yapıldı.
                case "Fizz": _y = 19f; break; // Yapıldı.
                case "Galio": _y = 21f; break; // Yapıldı.
                case "Gangplank": _y = 15f; break; // Yapıldı.
                case "Garen": _y = 20f; break; // Yapıldı.
                case "Gnar": _y = 15f; break; // Yapıldı.
                case "Gragas": _y = 15f; break; // Yapıldı.
                case "Graves": _y = 18.5f; break; // Yapıldı.
                case "Hecarim": _y = 19f; break; // Yapıldı.
                case "Heimerdinger": _y = 19f; break; // Yapıldı.
                case "Illaoi": _y = 18f; break; // Yapıldı.
                case "Irelia": _y = 19f; break; // Yapıldı.
                case "Ivern": _y = 0f; break; // Yapıldı.
                case "Janna": _y = 16f; break; // Yapıldı.
                case "JarvanIV": _y = 21f; break; // Yapıldı.
                case "Jax": _y = 14f; break; // Yapıldı.
                case "Jayce": _y = 9f; break; // Yapıldı.
                case "Jhin": _y = 30f; break; // Yapıldı.
                case "Jinx": _y = 18.5f; break; // Yapıldı.
                case "Kalista": _y = 15f; break;
                case "Karma": _y = 15f; break; // Yapıldı.
                case "Karthus": _y = 18f; break; // Yapıldı.
                case "Kassadin": _y = 14f; break; // Yapıldı.
                case "Katarina": _y = 19f; break; // Yapıldı.
                case "Kayle": _y = 14f; break; // Yapıldı.
                case "Kayn": _y = 15f; break; // Yapıldı.
                case "Kennen": _y = 15f; break; // Yapıldı.
                case "Khazix": _y = 17f; break; // Yapıldı.
                case "Kindred": _y = 19f; break; // Yapıldı.
                case "Kled": _y = 9f; break; // Yapıldı.
                case "KogMaw": _y = 18f; break; // Yapıldı.
                case "Leblanc": _y = 20f; break; // Yapıldı.
                case "LeeSin": _y = 16f; break; // Yapıldı.
                case "Leona": _y = 20f; break; // Yapıldı.
                case "Lissandra": _y = 19f; break; // Yapıldı.
                case "Lucian": _y = 15f; break; // Yapıldı.
                case "Lulu": _y = 14f; break; // Yapıldı.
                case "Lux": _y = 14f; break; // Yapıldı.
                case "Malphite": _y = 15f; break; // Yapıldı.
                case "Malzahar": _y = 15f; break; // Yapıldı.
                case "Maokai": _y = 19f; break; // Yapıldı.
                case "MasterYi": _y = 17f; break; // Yapıldı.
                case "MissFortune": _y = 15f; break; // Yapıldı.
                case "Mordekaiser": _y = 19f; break; // Yapıldı.
                case "Morgana": _y = 15f; break; // Yapıldı.
                case "Nami": _y = 16f; break; // Yapıldı.
                case "Nasus": _y = 19f; break; // Yapıldı.
                case "Nautilus": _y = 14f; break; // Yapıldı.
                case "Nidalee": _y = 21f; break; // Yapıldı.
                case "Nocturne": _y = 19f; break; // Yapıldı.
                case "Nunu": _y = 15f; break; // Yapıldı.
                case "Olaf": _y = 15f; break;
                case "Orianna": _y = 15f; break; // Yapıldı.
                case "Ornn": _y = 16f; break; // Yapıldı.
                case "Pantheon": _y = 15f; break; // Yapıldı.
                case "Poppy": _y = 7f; break; // Yapıldı.
                case "Quinn": _y = 15f; break;
                case "Rakan": _y = 7f; break; // Yapıldı.
                case "Rammus": _y = 18f; break; // Yapıldı.
                case "RekSai": _y = 15f; break;
                case "Renekton": _y = 16f; break; // Yapıldı.
                case "Rengar": _y = 19f; break; // Yapıldı.
                case "Riven": _y = 15f; break; // Yapıldı.
                case "Rumble": _y = 14f; break; // Yapıldı.
                case "Ryze": _y = 32f; break; // Yapıldı.
                case "Sejuani": _y = 18.5f; break; // Yapıldı.
                case "Shaco": _y = 19f; break; // Yapıldı.
                case "Shen": _y = 14f; break; // Yapıldı.
                case "Shyvana": _y = 17f; break; // Yapıldı.
                case "Singed": _y = 19f; break; // Yapıldı.
                case "Sion": _y = 16f; break; // Yapıldı.
                case "Sivir": _y = 18f; break; // Yapıldı.
                case "Skarner": _y = 19f; break; // Yapıldı.
                case "Sona": _y = 12.5f; break; // Yapıldı.
                case "Soraka": _y = 13f; break; // Yapıldı.
                case "Swain": _y = 16f; break; // Yapıldı.
                case "Syndra": _y = 15f; break; // Yapıldı.
                case "TahmKench": _y = 19f; break; // Yapıldı.
                case "Taliyah": _y = 9f; break; // Yapıldı.
                case "Talon": _y = 19f; break; // Yapıldı.
                case "Taric": _y = 20f; break; // Yapıldı.
                case "Teemo": _y = 15f; break; // Yapıldı.
                case "Thresh": _y = 19f; break; // Yapıldı.
                case "Tristana": _y = 16f; break; // Yapıldı.
                case "Trundle": _y = 23f; break; // Yapıldı.
                case "Tryndamere": _y = 20f; break; // Yapıldı.
                case "TwistedFate": _y = 19f; break; // Yapıldı.
                case "Twitch": _y = 17f; break; // Yapıldı.
                case "Udyr": _y = 19f; break; // Yapıldı.
                case "Urgot": _y = 12f; break; // Yapıldı.
                case "Varus": _y = 19f; break; // Yapıldı.
                case "Vayne": _y = 19f; break; // Yapıldı.
                case "Veigar": _y = 19f; break; // Yapıldı.
                case "Velkoz": _y = 0f; break; // Yapıldı.
                case "Vi": _y = 15f; break; // Yapıldı.
                case "Viktor": _y = 26f; break; // Yapıldı.
                case "Vladimir": _y = 19f; break; // Yapıldı.
                case "Volibear": _y = 15f; break; // Yapıldı.
                case "Warwick": _y = 14f; break; // Yapıldı.
                case "MonkeyKing": _y = 19f; break; // Yapıldı.
                case "Xayah": _y = 23f; break; // Yapıldı.
                case "Xerath": _y = 15f; break; // Yapıldı.
                case "XinZhao": _y = 27f; break; // Yapıldı.
                case "Yasuo": _y = 14f; break; // Yapıldı.
                case "Yorick": _y = 19f; break; // Yapıldı.
                case "Zac": _y = 17f; break; // Yapıldı.
                case "Zed": _y = 19f; break; // Yapıldı.
                case "Ziggs": _y = 15f; break; // Yapıldı.
                case "Zilean": _y = 17f; break; // Yapıldı.
                case "Zoe": _y = 11f; break; // Yapıldı.
                case "Zyra": _y = 16f; break; // Yapıldı.
                default: _y = 15f; break;
            }
            return _y;
        }
        /*Me Hud*/
        public static float Yme(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Aatrox": _y = 22f; break;
                case "Ahri": _y = 22f; break;
                case "Akali": _y = 22f; break;
                case "Alistar": _y = 22f; break;
                case "Amumu": _y = 22f; break;
                case "Anivia": _y = 22f; break;
                case "Annie": _y = 30f; break; // Yapıldı.
                case "Ashe": _y = 22f; break;
                case "AurelionSol": _y = 22f; break;
                case "Azir": _y = 22f; break;
                case "Bard": _y = 22f; break;
                case "Blitzcrank": _y = 22f; break;
                case "Brand": _y = 22f; break;
                case "Braum": _y = 22f; break;
                case "Caitlyn": _y = 22f; break;
                case "Camille": _y = 22f; break;
                case "Cassiopeia": _y = 22f; break;
                case "Chogath": _y = 22f; break;
                case "Corki": _y = 32f; break; // Yapıldı.
                case "Darius": _y = 22f; break;
                case "Diana": _y = 22f; break;
                case "DrMundo": _y = 22f; break;
                case "Draven": _y = 22f; break;
                case "Ekko": _y = 22f; break;
                case "Elise": _y = 22f; break;
                case "Evelynn": _y = 22f; break;
                case "Ezreal": _y = 22f; break;
                case "FiddleSticks": _y = 22f; break;
                case "Fiora": _y = 22f; break;
                case "Fizz": _y = 22f; break;
                case "Galio": _y = 22f; break;
                case "Gangplank": _y = 22f; break;
                case "Garen": _y = 22f; break;
                case "Gnar": _y = 22f; break;
                case "Gragas": _y = 22f; break;
                case "Graves": _y = 22f; break;
                case "Hecarim": _y = 22f; break;
                case "Heimerdinger": _y = 22f; break;
                case "Illaoi": _y = 22f; break;
                case "Irelia": _y = 22f; break;
                case "Ivern": _y = 22f; break;
                case "Janna": _y = 22f; break;
                case "JarvanIV": _y = 22f; break;
                case "Jax": _y = 22f; break;
                case "Jayce": _y = 22f; break;
                case "Jhin": _y = 32f; break; // Yapıldı.
                case "Jinx": _y = 22f; break;
                case "Kalista": _y = 22f; break;
                case "Karma": _y = 22f; break;
                case "Karthus": _y = 22f; break;
                case "Kassadin": _y = 22f; break;
                case "Katarina": _y = 22f; break;
                case "Kayle": _y = 22f; break;
                case "Kayn": _y = 22f; break;
                case "Kennen": _y = 22f; break;
                case "Khazix": _y = 22f; break;
                case "Kindred": _y = 22f; break;
                case "Kled": _y = 22f; break;
                case "KogMaw": _y = 22f; break;
                case "LeBlanc": _y = 22f; break;
                case "LeeSin": _y = 22f; break;
                case "Leona": _y = 22f; break;
                case "Lissandra": _y = 22f; break;
                case "Lucian": _y = 22f; break;
                case "Lulu": _y = 22f; break;
                case "Lux": _y = 22f; break;
                case "Malphite": _y = 22f; break;
                case "Malzahar": _y = 22f; break;
                case "Maokai": _y = 22f; break;
                case "MasterYi": _y = 22f; break;
                case "MissFortune": _y = 22f; break;
                case "Mordekaiser": _y = 22f; break;
                case "Morgana": _y = 22f; break;
                case "Nami": _y = 22f; break;
                case "Nasus": _y = 22f; break;
                case "Nautilus": _y = 22f; break;
                case "Nidalee": _y = 22f; break;
                case "Nocturne": _y = 22f; break;
                case "Nunu": _y = 22f; break;
                case "Olaf": _y = 22f; break;
                case "Orianna": _y = 22f; break;
                case "Ornn": _y = 22f; break;
                case "Pantheon": _y = 22f; break;
                case "Poppy": _y = 22f; break;
                case "Quinn": _y = 22f; break;
                case "Rakan": _y = 22f; break;
                case "Rammus": _y = 22f; break;
                case "RekSai": _y = 22f; break;
                case "Renekton": _y = 22f; break;
                case "Rengar": _y = 22f; break;
                case "Riven": _y = 22f; break;
                case "Rumble": _y = 22f; break;
                case "Ryze": _y = 34f; break; // Yapıldı.
                case "Sejuani": _y = 22f; break;
                case "Shaco": _y = 22f; break;
                case "Shen": _y = 22f; break;
                case "Shyvana": _y = 22f; break;
                case "Singed": _y = 22f; break;
                case "Sion": _y = 22f; break;
                case "Sivir": _y = 22f; break;
                case "Skarner": _y = 22f; break;
                case "Sona": _y = 22f; break;
                case "Soraka": _y = 22f; break;
                case "Swain": _y = 22f; break;
                case "Syndra": _y = 22f; break;
                case "TahmKench": _y = 22f; break;
                case "Taliyah": _y = 22f; break;
                case "Talon": _y = 22f; break;
                case "Taric": _y = 22f; break;
                case "Teemo": _y = 22f; break;
                case "Thresh": _y = 22f; break;
                case "Tristana": _y = 22f; break;
                case "Trundle": _y = 22f; break;
                case "Tryndamere": _y = 22f; break;
                case "TwistedFate": _y = 22f; break;
                case "Twitch": _y = 22f; break;
                case "Udyr": _y = 22f; break;
                case "Urgot": _y = 22f; break;
                case "Varus": _y = 22f; break;
                case "Vayne": _y = 22f; break;
                case "Veigar": _y = 22f; break;
                case "Velkoz": _y = 22f; break; // Yapıldı.
                case "Vi": _y = 22f; break;
                case "Viktor": _y = 22f; break;
                case "Vladimir": _y = 22f; break;
                case "Volibear": _y = 22f; break;
                case "Warwick": _y = 22f; break;
                case "MonkeyKing": _y = 22f; break;
                case "Xayah": _y = 35f; break; // Yapıldı.
                case "Xerath": _y = 22f; break;
                case "XinZhao": _y = 22f; break;
                case "Yasuo": _y = 22f; break;
                case "Yorick": _y = 22f; break;
                case "Zac": _y = 22f; break;
                case "Zed": _y = 22f; break;
                case "Ziggs": _y = 22f; break;
                case "Zilean": _y = 22f; break;
                case "Zoe": _y = 22f; break;
                case "Zyra": _y = 22f; break;
                default: _y = 22f; break;
            }
            return _y;
        }

        /*Spells Me*/
        public static float XspellsMe(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Annie": _x = 129f; break; // Yapıldı.
                case "Ryze": _x = 122f; break; // Yapıldı.
                case "Corki": _x = 122f; break; // Yapıldı.
                case "Jhin": _x = 123f; break; // Yapıldı.
                case "Xayah": _x = 122f; break; // Yapıldı.
                default: _x = 122f; break;
            }
            return _x;
        }

        public static float YspellsMe(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Annie": _y = -25f; break; // Yapıldı.
                case "Ryze": _y = -29f; break; // Yapıldı.
                case "Corki": _y = -27f; break; // Yapıldı.
                case "Jhin": _y = -26f; break; // Yapıldı.
                case "Xayah": _y = -29f; break; // Yapıldı.
                default: _y = -17f; break;
            }
            return _y;
        }

        /*Spells Enemy*/
        public static float XspellsEnemy(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Annie": _x = 129f; break;  // Yapıldı.
                case "Ryze": _x = 123.5f; break; // Yapıldı.
                case "Corki": _x = 122f; break; // Yapıldı.
                case "Jhin": _x = 123f; break; // Yapıldı.
                case "Xayah": _x = 123f; break; // Yapıldı.
                default: _x = 122f; break;
            }
            return _x;
        }

        public static float YspellsEnemy(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Annie": _y = -25f; break; // Yapıldı.
                case "Ryze": _y = -27f; break; // Yapıldı.
                case "Corki": _y = -25f; break; // Yapıldı.
                case "Jhin": _y = -25f; break; // Yapıldı.
                case "Xayah": _y = -31f; break; // Yapıldı.
                default: _y = -17f; break;
            }
            return _y;
        }

        /*Spells Ally*/
        public static float XspellsAlly(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Annie": _x = 129f; break; // Yapıldı.
                case "Ryze": _x = 124f; break; // Yapıldı.
                case "Corki": _x = 123f; break; // Yapıldı.
                case "Jhin": _x = 123f; break; // Yapıldı.
                case "Xayah": _x = 122f; break; // Yapıldı.
                default: _x = 122f; break;
            }
            return _x;
        }

        public static float YspellsAlly(Obj_AI_Hero champion)
        {
            switch (champion.ChampionName)
            {
                case "Annie": _y = -25f; break; // Yapıldı.
                case "Ryze": _y = -27f; break; // Yapıldı.
                case "Corki": _y = -26f; break; // Yapıldı.
                case "Jhin": _y = -25f; break; // Yapıldı.
                case "Xayah": _y = -29f; break; // Yapıldı.
                default: _y = -17f; break;
            }
            return _y;
        }

        /*Damage Indicator*/
        public static float X(Obj_AI_Hero target)
        {
            switch (target.ChampionName)
            {
                case "Darius": _x = 36.5f; break; // Yapıldı.
                case "Renekton": _x = 36.5f; break; // Yapıldı.
                case "Corki": _x = 31f; break; // Yapıldı.
                case "Jhin": _x = 31f; break; // Yapıldı.
                case "Ornn": _x = 38f; break; // Yapıldı.
                case "Sion": _x = 37.5f; break; // Yapıldı.
                case "Thresh": _x = 34f; break; // Yapıldı.
                default: _x = 29.5f; break;
            }
            return _x;
        }

        public static float Y(Obj_AI_Hero target)
        {
            switch (target.ChampionName)
            {
                case "Aatrox": _y = 0; break;
                case "Ahri": _y = 2f; break; // Yapıldı.
                case "Akali": _y = 2f; break; // Yapıldı.
                case "Alistar": _y = -5f; break; // Yapıldı.
                case "Amumu": _y = 2f; break; // Yapıldı.
                case "Anivia": _y = -3f; break; // Yapıldı.
                case "Annie": _y = 6f; break; // Yapıldı.
                case "Ashe": _y = 2f; break;  // Yapıldı.
                case "AurelionSol": _y = 0; break;
                case "Azir": _y = -3f; break; // Yapıldı.
                case "Bard": _y = -8f; break; // Yapıldı.
                case "Blitzcrank": _y = -5f; break; // Yapıldı.
                case "Brand": _y = -2f; break; // Yapıldı.
                case "Braum": _y = -2.5f; break; // Yapıldı.
                case "Caitlyn": _y = 2f; break; // Yapıldı.
                case "Camille": _y = 0; break;
                case "Cassiopeia": _y = -2f; break; // Yapıldı.
                case "Chogath": _y = 1.5f; break; // Yapıldı.
                case "Corki": _y = 6f; break; // Yapıldı.
                case "Darius": _y = -1.5f; break; // Yapıldı.
                case "Diana": _y = 10f; break; // Yapıldı.
                case "DrMundo": _y = 0; break; // Yapıldı.
                case "Draven": _y = 1f; break; // Yapıldı.
                case "Ekko": _y = 1f; break; // Yapıldı.
                case "Elise": _y = -9f; break; // Yapıldı.
                case "Evelynn": _y = -3f; break; // Yapıldı.
                case "Ezreal": _y = 2f; break; // Yapıldı.
                case "FiddleSticks": _y = 0; break; // Yapıldı.
                case "Fiora": _y = -2.5f; break; // Yapıldı.
                case "Fizz": _y = 1f; break; // Yapıldı.
                case "Galio": _y = 5f; break; // Yapıldı.
                case "Gangplank": _y = -4f; break; // Yapıldı.
                case "Garen": _y = 4.5f; break; // Yapıldı.
                case "Gnar": _y = -3f; break; // Yapıldı.
                case "Gragas": _y = -5f; break; // Yapıldı.
                case "Graves": _y = 1.5f; break; // Yapıldı.
                case "Hecarim": _y = 2f; break; // Yapıldı.
                case "Heimerdinger": _y = 1.5f; break; // Yapıldı.
                case "Illaoi": _y = -1; break; // Yapıldı.
                case "Irelia": _y = 2f; break; // Yapıldı.
                case "Ivern": _y = -26f; break; // Yapıldı.
                case "Janna": _y = -2.5f; break; // Yapıldı.
                case "JarvanIV": _y = 5f; break; // Yapıldı.
                case "Jax": _y = -4.5f; break; // Yapıldı.
                case "Jayce": _y = -13f; break; // Yapıldı.
                case "Jhin": _y = 6f; break; // Yapıldı.
                case "Jinx": _y = 2f; break; // Yapıldı.
                case "Kalista": _y = 0; break;
                case "Karma": _y = -3; break; // Yapıldı.
                case "Karthus": _y = 2f; break; // Yapıldı.
                case "Kassadin": _y = -5f; break; // Yapıldı.
                case "Katarina": _y = 2f; break; // Yapıldı.
                case "Kayle": _y = -4.5f; break; // Yapıldı.
                case "Kayn": _y = -3.5f; break; // Yapıldı.
                case "Kennen": _y = -3f; break; // Yapıldı.
                case "Khazix": _y = -1f; break; // Yapıldı.
                case "Kindred": _y = 1f; break; // Yapıldı.
                case "Kled": _y = -12f; break; // Yapıldı.
                case "KogMaw": _y = 2f; break; // Yapıldı.
                case "Leblanc": _y = 4f; break; // Yapıldı.
                case "LeeSin": _y = -2f; break; // Yapıldı.
                case "Leona": _y = 3.5f; break; // Yapıldı.
                case "Lissandra": _y = 1.5f; break; // Yapıldı.
                case "Lucian": _y = -2.5f; break; // Yapıldı.
                case "Lulu": _y = -4.5f; break; // Yapıldı.
                case "Lux": _y = -4.5f; break; // Yapıldı.
                case "Malphite": _y = -3.5f; break; // Yapıldı.
                case "Malzahar": _y = -2.5f; break; // Yapıldı.
                case "Maokai": _y = 1f; break; // Yapıldı.
                case "MasterYi": _y = -0.5f; break; // Yapıldı.
                case "MissFortune": _y = -2.5f; break; // Yapıldı.
                case "Mordekaiser": _y = 2f; break; // Yapıldı.
                case "Morgana": _y = -2.5f; break; // Yapıldı.
                case "Nami": _y = -1.5f; break; // Yapıldı.
                case "Nasus": _y = 1.5f; break; // Yapıldı.
                case "Nautilus": _y = -5f; break; // Yapıldı.
                case "Nidalee": _y = 5f; break; // Yapıldı.
                case "Nocturne": _y = 2f; break; // Yapıldı.
                case "Nunu": _y = -2.5f; break; // Yapıldı.
                case "Olaf": _y = 0; break;
                case "Orianna": _y = -3f; break; // Yapıldı.
                case "Ornn": _y = -1.5f; break; // Yapıldı.
                case "Pantheon": _y = -2.5f; break; // Yapıldı.
                case "Poppy": _y = -15f; break; // Yapıldı.
                case "Quinn": _y = 0; break;
                case "Rakan": _y = -15f; break; // Yapıldı.
                case "Rammus": _y = 0.5f; break; // Yapıldı.
                case "RekSai": _y = 0; break;
                case "Renekton": _y = -1.5f; break; // Yapıldı.
                case "Rengar": _y = 2f; break; // Yapıldı.
                case "Riven": _y = -5f; break; // Yapıldı.
                case "Rumble": _y = -5f; break; // Yapıldı.
                case "Ryze": _y = 6f; break; // Yapıldı.
                case "Sejuani": _y = 2f; break; // Yapıldı.
                case "Shaco": _y = 1.5f; break;
                case "Shen": _y = -5f; break; // Yapıldı.
                case "Shyvana": _y = -0.5f; break; // Yapıldı.
                case "Singed": _y = 2f; break; // Yapıldı.
                case "Sion": _y = -2f; break; // Yapıldı.
                case "Sivir": _y = 0; break; // Yapıldı.
                case "Skarner": _y = 2f; break; // Yapıldı.
                case "Sona": _y = -7.5f; break; // Yapıldı.
                case "Soraka": _y = -6.8f; break; // Yapıldı.
                case "Swain": _y = -3f; break; // Yapıldı.
                case "Syndra": _y = -2.5f; break; // Yapıldı.
                case "TahmKench": _y = 1.5f; break; // Yapıldı.
                case "Taliyah": _y = -13f; break; // Yapıldı.
                case "Talon": _y = 1.5f; break; // Yapıldı.
                case "Taric": _y = 2f; break; // Yapıldı.
                case "Teemo": _y = -3f; break; // Yapıldı.
                case "Thresh": _y = 1.5f; break; // Yapıldı.
                case "Tristana": _y = -2.5f; break; // Yapıldı.
                case "Trundle": _y = 7.5f; break; // Yapıldı.
                case "Tryndamere": _y = 4f; break; // Yapıldı.
                case "TwistedFate": _y = 2; break; // Yapıldı.
                case "Twitch": _y = -1f; break; // Yapıldı.
                case "Udyr": _y = 2f; break; // Yapıldı.
                case "Urgot": _y = -7f; break; // Yapıldı.
                case "Varus": _y = 2f; break; // Yapıldı.
                case "Vayne": _y = 2f; break; // Yapıldı.
                case "Veigar": _y = 2f; break; // Yapıldı.
                case "Velkoz": _y = -26f; break; // Yapıldı.
                case "Vi": _y = -1f; break; // Yapıldı.
                case "Viktor": _y = 11f; break; // Yapıldı.
                case "Vladimir": _y = 1.5f; break; // Yapıldı.
                case "Volibear": _y = -2.5f; break; // Yapıldı.
                case "Warwick": _y = -4.5f; break; // Yapıldı.
                case "MonkeyKing": _y = 2f; break; // Yapıldı.
                case "Xayah": _y = -10f; break; // Yapıldı.
                case "Xerath": _y = -2.5f; break; // Yapıldı.
                case "XinZhao": _y = 12.5f; break; // Yapıldı.
                case "Yasuo": _y = -5; break; // Yapıldı.
                case "Yorick": _y = 2f; break; // Yapıldı.
                case "Zac": _y = 1f; break; // Yapıldı.
                case "Zed": _y = 1.5f; break; // Yapıldı.
                case "Ziggs": _y = -3f; break; // Yapıldı.
                case "Zilean": _y = -1f; break; // Yapıldı.
                case "Zoe": _y = -11f; break; // Yapıldı.
                case "Zyra": _y = -2.5f; break; // Yapıldı.

                default: _y = 0; break;
            }
            return _y;
        }
        #endregion
    }
}
