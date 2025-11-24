using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loups_Garoups_de_Thiercelieux_console.Classes
{
    public static class ConsoleDisplay
    {
        public static void MainTitle()
        {
            Console.WriteLine(" +------------------------------+");
            Console.WriteLine(" | Loups-Garous de Thiercelieux |");
            Console.WriteLine(" +------------------------------+\n");
        }

        public static void ClearLine(int nbLines = 1)
        {
            int lineNb = Console.CursorTop;
            if (lineNb >= nbLines)
            {
                lineNb -= nbLines;
            }
            Console.SetCursorPosition(0, lineNb);
            for (int i = 0; i <= nbLines; i++)
            {
                Console.Write(new string(' ', Console.BufferWidth));
                Console.SetCursorPosition(0, lineNb + i);
            }
            Console.SetCursorPosition(0, lineNb);

        }

    }
}
