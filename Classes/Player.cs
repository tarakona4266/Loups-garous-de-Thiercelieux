using Loups_Garous_de_Thiercelieux_console.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loups_Garous_de_Thiercelieux_console.Classes
{
    // IDEA : separate PlayerHumain & PlayerBot in 2 classes with interface IPlayer ?
    
    public class Player
    {
        public bool isHumain;
        public bool isAlive = true;
        public string name;
        public Role role;
        public int indexInPlayerList;
        private int preferedChoice = -1; // allow the IA to vote for a specific player
        public Player(string name, bool isHumain, int index)
        {
            this.isHumain = isHumain;
            this.name = name;
            indexInPlayerList = index;
        }

        public void PrintRole()
        {
            switch (role)
            {
                case Role.Werewolf:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Role.Cupido:
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    break;
                case Role.FortuneTeller:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case Role.Hunter:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case Role.Witch:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case Role.LittleGirl:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    break;
                case Role.Sheriff:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default: Console.ForegroundColor = ConsoleColor.White;
                    break;
            }
            Console.Write(role);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public int Vote(List<Player> players)
        {
            int choice;
            if (isHumain)
            {
                while (true)
                {
                    string? answer = Console.ReadLine();
                    if (int.TryParse(answer, out choice))
                    {
                        if (choice > 0 && choice < players.Count)
                        {
                            ConsoleDisplay.ClearLine(2);
                            break;
                        }
                        else if (choice == 0)
                        {
                            ConsoleDisplay.ClearLine(2);
                            Console.WriteLine("Invalid input : you cannot choose yourself");
                        }
                        else
                        {
                            ConsoleDisplay.ClearLine(2);
                            Console.WriteLine($"Invalid input : pLease enter a number between 0 and {players.Count}");
                        }
                    }
                    else
                    {
                        ConsoleDisplay.ClearLine(2);
                        Console.WriteLine("Invalid input : pLease enter a number");
                    }
                }
                return choice;
            }
            else // AI random vote
            {
                do
                { choice = GlobalRandom.GetRandom(players.Count);
                } while (choice == indexInPlayerList);
            }
            Console.WriteLine($"{name} has voted {choice}");
            return choice;
        }

        public void VoteFromIndex(List<int> playersIndex)
        {
            int choice;
            if (isHumain)
            {
                while (true)
                {
                    string? answer = Console.ReadLine();
                    if (int.TryParse(answer, out choice))
                    {
                        if (playersIndex.Contains(choice)) // no check for choosing self, might add one later
                        {
                            ConsoleDisplay.ClearLine(2);
                            break;
                        }
                        else
                        {
                            ConsoleDisplay.ClearLine(2);
                            Console.WriteLine($"Invalid input : please chose an existing player ID");
                        }
                    }
                    else
                    {
                        ConsoleDisplay.ClearLine(2);
                        Console.WriteLine("Invalid input : pLease enter a number");
                    }
                }
            }
            else // AI random vote
            {
                do
                {
                    choice = playersIndex[GlobalRandom.GetRandom(playersIndex.Count)];
                } while (choice != indexInPlayerList);
            }
        }


        public (bool displayResult, int index) SeeCard(List<Player> players)
        {
            int choice;
            if (isHumain)
            {
                Console.WriteLine("Choose someone's card to see :");
                while (true)
                {
                    string? answer = Console.ReadLine();
                    if (int.TryParse(answer, out choice))
                    {
                        if (choice >= 0 && choice < players.Count)
                        {
                            if (choice != indexInPlayerList)
                            {
                                ConsoleDisplay.ClearLine(2);
                                break;
                            }
                            else
                            {
                                ConsoleDisplay.ClearLine(2);
                                Console.WriteLine("Invalid input : you cannot choose yourself");
                            }
                        }
                        else
                        {
                            ConsoleDisplay.ClearLine(2);
                            Console.WriteLine($"Invalid input : pLease enter a number between 0 and {players.Count}");
                        }
                    }
                    else
                    {
                        ConsoleDisplay.ClearLine(2);
                        Console.WriteLine("Invalid input : pLease enter a number");
                    }
                }
                return (true, choice);
            }
            else { return (false, 0); }
        }
    }
}
