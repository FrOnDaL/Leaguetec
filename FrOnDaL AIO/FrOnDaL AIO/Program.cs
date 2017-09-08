using System;
using Aimtec;
using Aimtec.SDK.Events;
using FrOnDaL_AIO.Champions;

namespace FrOnDaL_AIO
{
    internal class Program
    {
        
        private static void Main()
        {
            GameEvents.GameStart += OnLoadingComplete;
        }
        private static void OnLoadingComplete()
        {        
            try
            {
                switch (ObjectManager.GetLocalPlayer().ChampionName)
                {
                    case "Jhin":
                        var unused = new Jhin();       
                        Console.WriteLine("FrOnDaL AIO Jhin loaded");
                        break;
                    case "Veigar":
                        var unused1 = new Veigar();
                        Console.WriteLine("FrOnDaL AIO Veigar loaded");
                        break;
                    case "Swain":
                        var unused2 = new Swain();
                        Console.WriteLine("FrOnDaL AIO Swain loaded");
                        break;
                    case "Varus":
                        var unused3 = new Varus();
                        Console.WriteLine("FrOnDaL AIO Varus loaded");
                        break;
                    case "Thresh":
                        var unused4 = new Thresh();
                        Console.WriteLine("FrOnDaL AIO Thresh loaded");
                        break;
                    case "Lux":
                        var unused5 = new Lux();
                        Console.WriteLine("FrOnDaL AIO Lux loaded");
                        break;
                    case "Shen":
                        var unused6 = new Shen();
                        Console.WriteLine("FrOnDaL AIO Shen loaded");
                        break;
                    case "DrMundo":
                        var unused7 = new DrMundo();
                        Console.WriteLine("FrOnDaL AIO Dr.Mundo loaded");
                        break;
                    case "JarvanIV":
                        var unused8 = new JarvanIv();
                        Console.WriteLine("FrOnDaL AIO Jarvan IV loaded");
                        break;
                    case "AurelionSol":
                        var unused9 = new AurelionSol();
                        Console.WriteLine("FrOnDaL AIO AurelionSol loaded");
                        break;
                    case "Ziggs":
                        var unused10 = new Ziggs();
                        Console.WriteLine("FrOnDaL AIO Ziggs loaded");
                        break;
                }
            }
            catch (Exception)
            {            
                    Console.WriteLine("Champion not supported " + ObjectManager.GetLocalPlayer().ChampionName);               
            }
        }
    }
}
