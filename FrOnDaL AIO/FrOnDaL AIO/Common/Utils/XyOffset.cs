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
                case "Soraka": _x = 10; break;
                case "Jax": _x = 10; break;
                case "XinZhao": _x = 10; break;
                case "Irelia": _x = 10; break;

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

                default: _y = 15; break;
            }
            return _y;
        }
    }
}
