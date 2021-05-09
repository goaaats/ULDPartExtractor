using System;
using Lumina;
using Lumina.Data.Files;

namespace ULDPartExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new GameData(args[0]);

            var e = new UldExtractor(game);
            e.HandleUld(args[1]);
        }

        
    }
}
