using LeshaGay.Data;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeshaGay
{
    public class GeneticAlgorithm
    {
        public Population CurrentPopulation { get; set; }
        public int Generation { get; set; }
        public double MutationRate { get; set; }

        private Dictionary<Group, List<WorkloadTeachers>> workloadData;
        private List<Classroom> classrooms;
        private List<int> weeks;
        private List<int> daysOfWeek;
        private List<int> numParas;

        public GeneticAlgorithm(int populationSize, Dictionary<Group, List<WorkloadTeachers>> workloadData, List<Classroom> classrooms, List<int> weeks, List<int> daysOfWeek, List<int> numParas, double mutationRate)
        {
            this.workloadData = workloadData;
            this.classrooms = classrooms;
            this.weeks = weeks;
            this.daysOfWeek = daysOfWeek;
            this.numParas = numParas;

            CurrentPopulation = new Population(populationSize, workloadData, classrooms, weeks, daysOfWeek, numParas);
            MutationRate = mutationRate;
        }

        public void Evolve()
        {
            var newPopulation = new Population(0, workloadData, classrooms, weeks, daysOfWeek, numParas);
            var selectedSchedules = Selection.TournamentSelection(CurrentPopulation, 3);
            var random = new Random();

            for (int i = 0; i < selectedSchedules.Count / 2; i++)
            {
                var parent1 = selectedSchedules[i];
                var parent2 = selectedSchedules[selectedSchedules.Count - 1 - i];
                var child = GeneticOperators.Crossover(parent1, parent2);
                GeneticOperators.Mutate(child, MutationRate, workloadData.SelectMany(kv => kv.Value.Select(w => w.Subject)).ToList(), workloadData.SelectMany(kv => kv.Value.SelectMany(w => w.Teachers)).ToList(), classrooms);
                newPopulation.Schedules.Add(child);
            }

            CurrentPopulation = newPopulation;
            Generation++;
        }
    }
}
