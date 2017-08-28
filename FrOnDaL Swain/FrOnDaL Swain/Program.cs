using System;
using Aimtec;
using Aimtec.SDK.Events;
namespace FrOnDaL_Swain
{
    internal class Program
    {
        private static void Main()
        {
            GameEvents.GameStart += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {
            if (ObjectManager.GetLocalPlayer().ChampionName != "Swain") return;
            var unused = new FrOnDaLSwain();
            Console.WriteLine("FrOnDaL Swain loaded");           
        }
    }
}
