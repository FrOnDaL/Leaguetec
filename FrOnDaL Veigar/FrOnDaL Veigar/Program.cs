using System;
using Aimtec;
using Aimtec.SDK.Events;

namespace FrOnDaL_Veigar
{
    internal class Program
    {
        private static void Main()
        {
            GameEvents.GameStart += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {
            if (ObjectManager.GetLocalPlayer().ChampionName != "Veigar") return;
            var unused = new FrOnDaLVeigar();
            Console.WriteLine("FrOnDaL Veigar loaded");
        }
    }
}
