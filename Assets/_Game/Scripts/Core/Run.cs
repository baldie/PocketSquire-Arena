using System;
using System.Collections.Generic;
using System.Linq;

namespace PocketSquire.Arena.Core
{
    /// <summary>
    /// Represents a run through the arena
    /// </summary>
    public class Run
    {
        public List<Monster> Monsters { get; private set; }
        public int ArenaRank { get; private set; }

        public static Run StartNewRun()
        {
            var run = new Run();
            run.Monsters = GameWorld.AllMonsters.OrderBy(m => m.Rank).ToList();
            run.ArenaRank = 1;
            return run;
        }

        public Monster GetMonsterForCurrentRank()
        {
            var m = Monsters.FirstOrDefault(m => m.Rank == ArenaRank);
            Console.WriteLine($"Monster for rank {ArenaRank}: {m.ToString()}");
            return m;
        }

        public void NextRank()
        {
            ArenaRank++;
        }

        public void Reset()
        {
            if (GameState.Player != null)
            {
                if (GameState.Player.Health <= 0)
                {
                    GameState.Player.Health = GameState.Player.MaxHealth;
                }
            }
            GameWorld.ResetAllMonsters();
        }
    }
}