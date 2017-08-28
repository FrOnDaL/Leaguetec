using System;
using Aimtec;
using Aimtec.SDK.Events;

namespace FrOnDaL_Velkoz
{
    internal class Program
    {
        private static void Main()
        {
            GameEvents.GameStart += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {
            if (ObjectManager.GetLocalPlayer().ChampionName != "Velkoz") return;
            var unused = new FrOnDaLVelkoz();
            Console.WriteLine("FrOnDaL Vel'koz loaded");
        }
    }
}
