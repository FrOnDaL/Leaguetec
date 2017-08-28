using System;
using Aimtec;
using Aimtec.SDK.Events;

namespace FrOnDaL_Varus
{
    internal class Program
    {
        private static void Main()
        {
            GameEvents.GameStart += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {
            if (ObjectManager.GetLocalPlayer().ChampionName != "Varus") return;
            var unused = new FrOnDaLVarus();
            Console.WriteLine("FrOnDaL Varus loaded");
        }
    }
}
