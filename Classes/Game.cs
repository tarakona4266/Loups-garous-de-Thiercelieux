using Loups_Garoups_de_Thiercelieux_console.Classes;
using Loups_Garous_de_Thiercelieux_console.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace Loups_Garous_de_Thiercelieux_console.Classes
{
    public class Game
    {
        private int nbPlayer;
        private int nbWerewolves;
        private bool simpleGame;
        private bool endGame = false;
        bool humanIsWerewolf = false;
        private (int aliveWerewolves, int aliveTownfolks) endGameResult;

        private List<Player> allPlayers = [];
        private List<int> discoveredByFT = [];

        // for calling special players
        Player? fortuneTeller;
        Player? thief;
        Player? witch;
        Player? littleGirl;
        Player? hunter;

        public Game(int nbPlayer, bool simpleGame = false)
        {
            this.nbPlayer = nbPlayer;
            this.simpleGame = simpleGame;
        }

        public void Run()
        {
            #region SETUP

            // --- Players creation ---
            bool valid = false;
            Console.WriteLine("What is your name ?");
            string pattern = @"^(\w){3,12}$";
            string debugPattern = @"^Debug$";
            while (!valid)
            {
                string? answer = Console.ReadLine();
                if (answer != null && Regex.IsMatch(answer, pattern, RegexOptions.CultureInvariant))
                {
                    ConsoleDisplay.ClearLine(2);
                    allPlayers.Add(new Player(answer, true, 0));
                    allPlayers[0].isDiscovered = true; // allow to print role
                    valid = true;
                }
                else
                {
                    ConsoleDisplay.ClearLine(2);
                    Console.WriteLine("Invalid input : please enter 3 to 12 alphanumerical characters.");
                    continue;
                }
            }
            Console.WriteLine($"Your name is {allPlayers[0].name}.");

            for (int i = 0; i < nbPlayer - 1; i++)  // AI players
            {
                allPlayers.Add(new Player(Enum.GetName(typeof(Name), i), false, i + 1));
            }

            // --- Assign roles ---
            if (simpleGame)
            {
                List<int> availableRoles = [];
                nbWerewolves = (int)Math.Round(allPlayers.Count * 0.2f);  // 20% of werewolves
                for (int i = 0; i < nbWerewolves; i++)
                {
                    availableRoles.Add(1);  // werewolves
                }
                availableRoles.Add(2);      // Fortune teller
                int nbOrdinaryTownFolks = allPlayers.Count - nbWerewolves - 1;
                for (int i = 0; i < nbOrdinaryTownFolks; i++)
                {
                    availableRoles.Add(0);  // ordinary townfolks
                }

                foreach (Player player in allPlayers)
                {
                    int i = GlobalRandom.GetRandom(availableRoles.Count);
                    player.role = (Role)availableRoles[i];
                    availableRoles.RemoveAt(i);
                }
            }
            else
            {
                List<int> availableRoles = [];
                nbWerewolves = (int)Math.Round(allPlayers.Count * 0.2f);  // 20% of werewolves
                for (int i = 0; i < nbWerewolves; i++)
                {
                    availableRoles.Add(1);  // werewolves
                }
                availableRoles.Add(2);      // Fortune teller
                availableRoles.Add(3);      // Hunter
                availableRoles.Add(4);      // Cupido
                availableRoles.Add(5);      // Witch
                availableRoles.Add(6);      // LittleGirl
                availableRoles.Add(8);      // Thief

                int nbOrdinaryTownFolks = allPlayers.Count - nbWerewolves - 6;
                for (int i = 0; i < nbOrdinaryTownFolks; i++)
                {
                    availableRoles.Add(0);  // ordinary townfolks
                }
                foreach (Player player in allPlayers)
                {
                    int i = GlobalRandom.GetRandom(availableRoles.Count);
                    player.role = (Role)availableRoles[i];
                    availableRoles.RemoveAt(i);
                }
            }

            

            #endregion

            // --- Game start ---

            Console.Write("You are a ");
            allPlayers[0].PrintRole();
            Console.WriteLine(" !\n");
            Wait(1000);

            if (Regex.IsMatch(allPlayers[0].name, debugPattern)) // secret debug feature
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("(i) debug mode enabled\n");
                Console.ForegroundColor = ConsoleColor.White;
                ConsoleDisplay.printDebug = true;
            }

            ConsoleDisplay.Next();

            #region THIEF

            if (!simpleGame)
            {
                Player thief = GetSpecialPlayer(Role.Thief);
                InvokeThief(thief);
                thief = GetSpecialPlayer(Role.Thief);
            }

            #endregion

            if (allPlayers[0].role == Role.Werewolf) // print the role of all werewolves
            {
                humanIsWerewolf = true;
                foreach (Player player in allPlayers)
                {
                    if (player.role == Role.Werewolf)
                    {
                        player.isDiscovered = true;
                    }
                }
            }

            // get a ref of all special players
            fortuneTeller = GetSpecialPlayer(Role.FortuneTeller);
            if (!simpleGame)
            {
                littleGirl = GetSpecialPlayer(Role.LittleGirl);
                witch = GetSpecialPlayer(Role.Witch);
                hunter = GetSpecialPlayer(Role.Hunter);
            }

            // --- game loop ---

            while (!endGame)
            {
                List<int> votes = [];
                List<VoteData> voteResults;
                int victimIndex;

                ConsoleDisplay.Narrate("The night is approaching. Everyone goes to sleep.\n");

                #region FORTUNE_TELLER

                if (fortuneTeller.isAlive)
                {
                    bool canInvokeFT = false;
                    foreach (Player player in allPlayers) // avoid calling FT if everyone's role is already exposed
                    {
                        if (player.role != Role.FortuneTeller)
                        {
                            if (player.isAlive && !discoveredByFT.Contains(player.indexInPlayerList))
                            {
                                canInvokeFT = true;
                            }
                        }
                    }
                    if (canInvokeFT)
                    {
                        ConsoleDisplay.Narrate("The Fortune Teller awakes.\n");
                        InvokeFortuneTeller(fortuneTeller);
                        ConsoleDisplay.Narrate("The Fortune Teller goes back to sleep.\n");
                        ConsoleDisplay.Next();
                    }
                }

                #endregion

                #region WEREWOLVES_VOTE

                CheckForEndGame();

                ConsoleDisplay.Narrate("The full moon is shining. The werewolves are awakening.\n");

                List<Player> werewolves = GetWerewolves();

                if (allPlayers[0].role == Role.Werewolf)
                {
                    ConsoleDisplay.PrintPlayers(allPlayers);
                    Console.WriteLine("Who will you devour this night ?");
                }
                else
                {
                    ConsoleDisplay.Narrate("Wolf howls echo through the night.\n");
                    ConsoleDisplay.Narrate("You struggle to sleep while they are debating about their next victim.");
                    ConsoleDisplay.DebugPrint("", true);
                }

                foreach (Player werewolf in werewolves) // first vote
                {
                    if (werewolf.isAlive)
                    {
                        votes.Add(werewolf.Vote(allPlayers));
                    }
                }
                Console.WriteLine();

                voteResults = GetWeightedVotes(votes, allPlayers);
                victimIndex = GetVictimFromVotes(voteResults);
                if (humanIsWerewolf) { ConsoleDisplay.PrintVotes(voteResults, allPlayers); }

                while (victimIndex == -1) // if voters don't agree
                {
                    if (humanIsWerewolf && allPlayers[0].isAlive)
                    {
                        ConsoleDisplay.PrintSeparation();
                        ConsoleDisplay.PrintPlayers(allPlayers, votes);
                        Console.WriteLine("The werewolves couldn't agree. Choose again among those savorous victims :");
                    }
                    votes.Clear();
                    votes = NewVote(voteResults, werewolves);
                    ConsoleDisplay.DebugPrint("", true);
                    voteResults = GetWeightedVotes(votes, allPlayers);
                    victimIndex = GetVictimFromVotes(voteResults);
                    if (humanIsWerewolf) { ConsoleDisplay.PrintVotes(voteResults, allPlayers); }
                }
                Kill(allPlayers[victimIndex]);

                if (allPlayers[0].role == Role.Werewolf)
                {
                    ConsoleDisplay.Narrate($"The werewolves have chosen to devour {allPlayers[victimIndex].name}\n");
                }
                ConsoleDisplay.Narrate("Someone screams. The werewolves' hunger is satisfied for this night.\n");

                endGameResult = CheckForEndGame();
                if (endGameResult.aliveWerewolves == 1 && endGameResult.aliveTownfolks == 1) { endGame = true; }
                ConsoleDisplay.Next();
                if (endGame) { break; }

                #endregion

                #region TOWN_VOTE

                ConsoleDisplay.Narrate("The sun rises. The town wakes up.\n");
                ConsoleDisplay.Narrate($"The werewolves have striked tonight. {allPlayers[victimIndex].name} has been eaten.\n");
                ConsoleDisplay.Narrate("The townfolks gather at the village square.\n");
                ConsoleDisplay.Narrate("In an attempt to get rid of the werewolves, everybody vote for a scapegoat to sacrifice.\n");
                Wait();
                ConsoleDisplay.PrintPlayers(allPlayers);

                if (allPlayers[0].isAlive) { Console.WriteLine("Who do you think should be sacrificed ?"); }
                votes.Clear();
                foreach (Player player in allPlayers)
                {
                    if (player.isAlive)
                    {
                        votes.Add(player.Vote(allPlayers));
                    }
                }
                Console.WriteLine();
                voteResults = GetWeightedVotes(votes, allPlayers);
                victimIndex = GetVictimFromVotes(voteResults);
                ConsoleDisplay.PrintVotes(voteResults, allPlayers);

                while (victimIndex == -1) // if voters don't agree
                {
                    if (allPlayers[0].isAlive)
                    {
                        ConsoleDisplay.PrintSeparation();
                        ConsoleDisplay.PrintPlayers(allPlayers, votes);
                        Console.WriteLine("The villagers couldn't agree. Choose again among the suspects :");
                    }
                    votes.Clear();
                    votes = NewVote(voteResults, allPlayers);
                    voteResults = GetWeightedVotes(votes, allPlayers);
                    victimIndex = GetVictimFromVotes(voteResults);
                    ConsoleDisplay.PrintVotes(voteResults, allPlayers);
                }
                Console.WriteLine();
                Kill(allPlayers[victimIndex]);
                ConsoleDisplay.Narrate($"The assembly has spoken : the scapegoat is {allPlayers[victimIndex].name}.");
                ConsoleDisplay.Narrate($"This person was a ", false);
                allPlayers[victimIndex].PrintRole();
                Console.WriteLine("\n");

                endGameResult = CheckForEndGame();
                ConsoleDisplay.Next();
                if (endGame) { break; }

                #endregion

            }

            #region ENDGAME

            ConsoleDisplay.DebugPrint("end of the game\n");

            if (endGameResult.aliveWerewolves > 0 && endGameResult.aliveTownfolks == 0) // Werewolves win
            {
                if (allPlayers[0].role == Role.Werewolf)
                {
                    Console.WriteLine("Congratulation !\n");
                    Console.WriteLine("The werewolves have slayed all the villagers.\n");
                    Console.ForegroundColor= ConsoleColor.Cyan;
                    Console.WriteLine("Your team won !");
                    Console.ForegroundColor= ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine("Game over !\n");
                    Console.WriteLine("The werewolves have slayed all the villagers.\n");
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine("Your team lost.");
                    Console.ForegroundColor= ConsoleColor.White;
                }
            }
            else if (endGameResult.aliveTownfolks > 0 && endGameResult.aliveWerewolves == 0) // Townfolks win
            {
                if (allPlayers[0].role == Role.Werewolf)
                {
                    Console.WriteLine("Game over !\n");
                    Console.WriteLine("The villagers have slayed the last of your kind.\n");
                    Console.ForegroundColor= ConsoleColor.Red;
                    Console.WriteLine("Your team lost.");
                    Console.ForegroundColor= ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine("Congratulation !\n");
                    Console.WriteLine("The villagers have slayed all the werewolves. Millers Hollow is saved.\n");
                    Console.ForegroundColor= ConsoleColor.Cyan;
                    Console.WriteLine("Your team won !");
                    Console.ForegroundColor= ConsoleColor.White;
                }
            }
            else if (endGameResult.aliveWerewolves == 1 && endGameResult.aliveTownfolks == 1)
            {
                if (allPlayers[0].role == Role.Werewolf)
                {
                    Console.WriteLine("Congratulation !\n");
                    Console.WriteLine("With only a villager left, your kind has seized control of Millers Hollow.\n");
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("Your team won !");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    Console.WriteLine("Game over !\n");
                    Console.WriteLine("There is only a villager left to fight the werewolves. Sadly, this is a lost cause.\n");
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Your team lost.");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            #endregion
        }

        #region METHODS

        // --- get specific players ---
        private Player GetSpecialPlayer(Role role)
        {
            Player targetPlayer = null;
            foreach (Player player in allPlayers)
            {
                if (player.role == role)
                {
                    targetPlayer = player;
                }
            }
            return targetPlayer;
        }

        private List<Player> GetWerewolves()
        {
            List<Player> werewolves = [];
            foreach (Player player in allPlayers)
            {
                if (player.role == Role.Werewolf && player.isAlive) { werewolves.Add(player); }
            }
            return werewolves;
        }


        // --- special players actions ---
        private void InvokeFortuneTeller(Player fortuneTeller)
        {
            if (fortuneTeller.isHumain)
            {
                ConsoleDisplay.PrintPlayers(allPlayers);
                Console.WriteLine("Choose someone's card to see :");
            }
            else
            {
                ConsoleDisplay.Narrate("No one knows what happens at the fortune teller's house during the night,\nbut everyone agrees that there are secrets that are better left unrevealed.\n");
            }

            var result = fortuneTeller.SeeCard(allPlayers, discoveredByFT);
            Player target = allPlayers[result.index];
            discoveredByFT.Add(result.index);

            if (fortuneTeller.isHumain)
            {
                Console.Write("This person is a ");
                target.PrintRole();
                Console.WriteLine("\n");
                target.isDiscovered = true;
            }
        }

        private void InvokeThief(Player thief)
        {
            int choice;
            if (thief.isHumain)
            {
                ConsoleDisplay.PrintPlayers(allPlayers);
                Console.WriteLine("Whose card do you want to steal ?");
                choice = thief.Vote(allPlayers);
            }
            else
            {
                do
                {
                    choice = GlobalRandom.GetRandom(nbPlayer);
                } while (choice == thief.indexInPlayerList);
            }

            thief.role = allPlayers[choice].role;
            allPlayers[choice].role = Role.Thief;

            if (allPlayers[choice].isHumain)
            {
                Console.WriteLine("Your card has been stolen !\n");
                Wait();
                Console.Write("You are now a ");
                allPlayers[0].PrintRole();
                Console.WriteLine(" !\n");
            }
            ConsoleDisplay.DebugPrint($"{thief.name} have exchanged their card with {thief.role} {allPlayers[choice].name}.\n");

            if (thief.isHumain || allPlayers[choice].isHumain)
            {
                ConsoleDisplay.Next();
            }
        }

        private void Kill(Player victim) // hunter action
        {
            if (hunter != null && victim.role == Role.Hunter)
            {
                ConsoleDisplay.Next();

                int choice;
                if (hunter.isHumain)
                {
                    ConsoleDisplay.Narrate("In a twist of fate, the hunter becomes the hunted.\nNo matter how resilient you are, you will not survive this time.\n");
                    ConsoleDisplay.Narrate("In a last burst of vitality, you draw your rifle.\n");
                    ConsoleDisplay.PrintPlayers(allPlayers);
                    Console.WriteLine("Who will be the target of you vengeance ?");
                    choice = hunter.Vote(allPlayers);
                }
                else
                {
                    ConsoleDisplay.Narrate("In a twist of fate, the hunter becomes the hunted.\n");
                    ConsoleDisplay.DebugPrint("", true);
                    choice = hunter.Vote(allPlayers);
                    ConsoleDisplay.DebugPrint("", true);
                    ConsoleDisplay.Narrate($"{hunter.name} avenged themselves by shooting {allPlayers[choice].name}.");
                    Console.Write("This person was a ");
                    allPlayers[choice].PrintRole();
                }
                allPlayers[choice].isAlive = false;
                hunter.isAlive = false;

                Console.WriteLine("\n");
                ConsoleDisplay.Next();
            }
            else
            {
                victim.isAlive = false;
            }
        }


        // --- vote related stuff ---
        private List<VoteData> GetWeightedVotes(List<int> votes, List<Player> targets)
        {
            votes.Sort();
            List<VoteData> weightedVotes = [];
            foreach (int vote in votes)
            {
                if (weightedVotes.Count > 0)
                {
                    VoteData lastElement = weightedVotes.Last();
                    if (lastElement.vote == vote)
                    {
                        int i = weightedVotes.Count - 1;
                        weightedVotes.RemoveAt(i);
                        lastElement.weight++;
                        weightedVotes.Add(lastElement);
                    }
                    else { weightedVotes.Add(new VoteData(vote, 1)); }
                }
                else { weightedVotes.Add(new VoteData(vote, 1)); }
            }
            var sortedVotes = weightedVotes.OrderByDescending(item => item.weight); // sort here for printing votes in weight order
            weightedVotes = sortedVotes.ToList();
            return weightedVotes;
        }

        private int GetVictimFromVotes(List<VoteData> votes)
        {
            int victimIndex;

            foreach (VoteData vote in votes)
            {
                ConsoleDisplay.DebugPrint($"{vote}");
            }
            ConsoleDisplay.DebugPrint("", true);

            if (votes.Count == 1 || votes[0].weight != votes[1].weight)
            {
                victimIndex = votes[0].vote;
                ConsoleDisplay.DebugPrint($"the victim will be {allPlayers[victimIndex].name}\n");
            }
            else // need another vote
            {
                victimIndex = -1;
            }
            return victimIndex;
        }

        private List<int> NewVote(List<VoteData> previousVote, List<Player> voters)
        {
            List<int> NextVoteTargetsIndex = [];
            List<int> votes = [];
            foreach (VoteData vote in previousVote) // get the players who got voted
            {
                NextVoteTargetsIndex.Add(vote.vote);
            }
            foreach (Player player in voters) // vote again
            {
                if (player.isAlive)
                {
                    votes.Add(player.VoteFromIndex(NextVoteTargetsIndex));
                }
            }
            return votes;
        }


        // --- other ---
        private (int aliveWerewolves, int aliveTownFolks) CheckForEndGame()
        {
            int aliveWerewolves = 0;
            int aliveTownFolks = 0;

            foreach (Player player in allPlayers)
            {
                if (player.isAlive)
                {
                    if (player.role == Role.Werewolf)
                    {
                        aliveWerewolves++;
                    }
                    else
                    {
                        aliveTownFolks++;
                    }
                }
            }

            if (aliveTownFolks == 0 || aliveWerewolves == 0)
            {
                endGame = true;
            }
            return (aliveWerewolves, aliveTownFolks);
        }

        private void Wait(int time = 1000)   // meant to be replaced with something more elegant
        {
            Thread.Sleep(time);
        }

        #endregion
    }
}

