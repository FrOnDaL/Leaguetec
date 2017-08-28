using Aimtec;
using Aimtec.SDK.Events;

namespace FrOnDaL_Twitch
{
    internal class Program
    {       
        private static void Main()
        {
            GameEvents.GameStart += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {
            if (ObjectManager.GetLocalPlayer().ChampionName != "Twitch") return;
            var unused = new FrOnDaLTwitch();           
        }     
    }
}
