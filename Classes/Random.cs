using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loups_Garoups_de_Thiercelieux_console.Classes
{
    public static class GlobalRandom
    {
        private static Random rdm = new Random();
        public static int GetRandom(int max)
        {
            return rdm.Next(max);
        }
        public static int GetRandom(int min, int max)
        {
            return rdm.Next(min, max);
        }

    }
}
