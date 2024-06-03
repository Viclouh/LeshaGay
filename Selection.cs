using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay
{
    public class Selection
    {
        public static List<Schedule> TournamentSelection(Population population, int tournamentSize)
        {
            var selected = new List<Schedule>();
            for (int i = 0; i < population.Schedules.Count; i++)
            {
                var tournament = new List<Schedule>();
                for (int j = 0; j < tournamentSize; j++)
                {
                    var randomIndex = new Random().Next(population.Schedules.Count);
                    tournament.Add(population.Schedules[randomIndex]);
                }
                tournament.Sort((s1, s2) => FitnessEvaluator.Evaluate(s1).CompareTo(FitnessEvaluator.Evaluate(s2)));
                selected.Add(tournament[0]);
            }
            return selected;
        }
    }
}
