using System.Linq;
using Aimtec;


namespace FrOnDaL_AIO.Common.Utils
{
    internal class XyOffset
    {
        private static float _x, _y;
        public static float X (Obj_AI_Hero target)
        {
            switch (target.ChampionName)
            {
                case "Annie": _x = 1; break;

                default: _x = 10; break;
            }
            return _x;
        }
        public static float Y (Obj_AI_Hero target)
        {
            switch (target.ChampionName)
            {
                case "Soraka": _y = 10; break;
                case "Jax": _y = 12; break;
                case "XinZhao": _y = 30; break;
                case "Irelia": _y = 19; break;
                case "Ashe": _y = 19; break;
                case "Amumu": _y = 19; break;
                case "Alistar": _y = 12; break;
                case "Annie": _y = 7; break;
                case "Brand": _y = 15; break;
                case "Blitzcrank": _y = 12; break;
                case "Caitlyn": _y = 19; break;
                case "Cassiopeia": _y = 15; break;
                case "Chogath": _y = 19; break;
                case "Darius": _y = 16; break;
                case "DrMundo": _y = 17; break;
                case "Ezreal": _y = 19; break;
                case "FiddleSticks": _y = 17; break;
                case "Galio": _y = 22; break;
                case "Garen": _y = 21; break;
                case "Graves": _y = 19; break;
                case "JarvanIV": _y = 22; break;
                case "Karthus": _y = 19; break;
                case "Kayle": _y = 13; break;
                case "KogMaw": _y = 19; break;
                case "Leona": _y = 21; break;
                case "Nidalee": _y = 23; break;
                case "Morgana": _y = 15; break;
                case "Nasus": _y = 19; break;
                case "Nunu": _y = 15; break;
                case "Rammus": _y = 18f; break;
                case "Ryze": _y = 14; break;
                case "Renekton": _y = 16f; break;
                case "Shen": _y = 12; break;
                case "Shyvana": _y = 17; break;
                case "Sivir": _y = 17f; break;
                case "Swain": _y = 15; break;
                case "Taric": _y = 19; break;
                case "Tristana": _y = 15; break;
                case "Trundle": _y = 24; break;
                case "Udyr": _y = 19; break;
                case "Veigar": _y = 19; break;
                case "Vladimir": _y = 19; break;
                case "Warwick": _y = 12; break;
                case "MonkeyKing": _y = 19; break;
                case "Ziggs": _y = 15; break;
                case "Zyra": _y = 15; break;
                case "Zilean": _y = 16; break;

                default: _y = 18; break;
            }
            return _y;
        }
    }
}
