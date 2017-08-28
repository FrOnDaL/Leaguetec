using System;
using Aimtec;
using Aimtec.SDK.Events;

namespace FrOnDaL_Thresh
{
    internal class Program
    {
        private static void Main()
        {
            GameEvents.GameStart += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {
            if (ObjectManager.GetLocalPlayer().ChampionName != "Thresh") return;
            var unused = new FrOnDaLThresh();
            Console.WriteLine("FrOnDaL Thresh loaded");
        }
    }
}
