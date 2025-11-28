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
        public bool isDiscovered = false;
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
                        if (choice > 0 && choice < players.Count && players[choice].isAlive)
                        {
                            if (role == Role.Werewolf && players[choice].role == Role.Werewolf)
                            {
                                ConsoleDisplay.ClearLine(2);
                                Console.WriteLine("Invalid input : you cannot choose a fellow werewolf");
                            }
                            else
                            {
                                ConsoleDisplay.ClearLine(2);
                                break;
                            }
                        }
                        else if (choice == 0)
                        {
                            ConsoleDisplay.ClearLine(2);
                            Console.WriteLine("Invalid input : you cannot choose yourself");
                        }
                        else if (choice >= players.Count)
                        {
                            ConsoleDisplay.ClearLine(2);
                            Console.WriteLine($"Invalid input : pLease enter a number between 0 and {players.Count - 1}");
                        }
                        else
                        {
                            ConsoleDisplay.ClearLine(2);
                            Console.WriteLine($"Invalid input : you cannot vote for a dead person");
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
                if (role != Role.Werewolf)
                {
                    if (preferedChoice > -1)
                    {
                        if (!players[preferedChoice].isAlive)
                        {
                            choice = preferedChoice;
                        }
                        else // reset preferedChoice & random vote
                        {
                            preferedChoice = -1;
                            do
                            {
                                choice = GlobalRandom.GetRandom(players.Count);
                            } while (choice == indexInPlayerList || !players[choice].isAlive);
                        }
                    }
                    else
                    {
                        do
                        {
                            choice = GlobalRandom.GetRandom(players.Count);
                        } while (choice == indexInPlayerList || !players[choice].isAlive);
                    }
                }
                else // if werewolf, choose an non-werewolf player to kill
                {
                    do
                    {
                        choice = GlobalRandom.GetRandom(players.Count);
                    } while (players[choice].role == Role.Werewolf || !players[choice].isAlive);
                }
            }
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"[DEBUG] The {role} {name} has voted for {players[choice].role} {players[choice].name} by voting {choice}.");
            Console.ForegroundColor = ConsoleColor.White;
            return choice;
        }

        public int VoteFromIndex(List<int> playersIndex)
        {
            int choice;
            if (isHumain)
            {
                while (true)
                {
                    string? answer = Console.ReadLine();
                    if (int.TryParse(answer, out choice))
                    {
                        if (playersIndex.Contains(choice))
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
                choice = playersIndex[GlobalRandom.GetRandom(playersIndex.Count)];
            }
            Console.WriteLine($"{name} as voted {choice}.");
            return choice;
        }

        public (bool displayResult, int index) SeeCard(List<Player> players)
        {
            int choice;
            if (isHumain)
            {
                while (true)
                {
                    string? answer = Console.ReadLine();
                    if (int.TryParse(answer, out choice))
                    {
                        if (choice >= 0 && choice < players.Count)
                        {
                            if (choice == indexInPlayerList)
                            {
                                ConsoleDisplay.ClearLine(2);
                                Console.WriteLine("Invalid input : you cannot choose yourself");
                            }
                            else if (players[choice].isDiscovered)
                            {
                                ConsoleDisplay.ClearLine(2);
                                Console.WriteLine("Invalid input : you have already chosen this person");
                            }
                            else
                            {
                                ConsoleDisplay.ClearLine(2);
                                break;
                            }
                        }
                        else
                        {
                            ConsoleDisplay.ClearLine(2);
                            Console.WriteLine($"Invalid input : pLease enter a number between 0 and {players.Count - 1}");
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
            else
            {
                do
                {
                    choice = GlobalRandom.GetRandom(players.Count);
                } while (choice == indexInPlayerList || !players[choice].isAlive || players[choice].isDiscovered);
                if (players[choice].role == Role.Werewolf)
                {
                    preferedChoice = choice;
                }
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"[DEBUG] The {role} {name} as discovered that {players[choice].name} is a {players[choice].role}.\n");
                Console.ForegroundColor = ConsoleColor.White;
                return (false, choice);
            }
        }
    }
}
