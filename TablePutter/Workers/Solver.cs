using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TablePutter.Models;

namespace TablePutter.Workers
{
    class Solver
    {
        public List<Chromosome> population = new List<Chromosome>();
        public int populationSize = 0;
        public Problem problem;
        int tables = 0;
        int maxTime = 0;

        public void Reset(int _populationSize, Problem _problem)
        {
            // create initial population
            population = new List<Chromosome>();
            populationSize = _populationSize;
            problem = _problem;
            
            tables = problem.Restaurant.Tables.Count();
            maxTime = problem.Restaurant.MaxTime;

            for (int i=0; i< populationSize; i++)
            {
                Chromosome candidate = new Chromosome(problem); // fill with random assignments
                candidate.fitness = Evaluate(problem, candidate, problem.GoalBookings);
                population.Add(candidate);
            }
        }
        
        // run next batch of generations, while reporting progress
        public void NextBatch()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Random random = new Random();
            // order = beast -> least fitness
            population = population.OrderBy(c => c.fitness).ToList();

            int maxIndex = populationSize - 1;
            int maxVal = problem.Restaurant.VirtualTables.Count();  // max mutation assignment

            List<bool> BitMask = new List<bool>();
            for (int b = 0; b < problem.GoalBookings.Count(); b++)
                BitMask.Add(random.Next(0, 1) == 1);


            int i = 0;
            bool solved = false;
            while(i < 100000 && !solved)
            {
                // remove least fit
                population.Remove(population.Last());

                // pick two for mating!
                Chromosome a = population[random.Next((int)maxIndex / 4)];
                Chromosome b = population[random.Next((int)maxIndex  )];

                // mate!
                // Chromosome child = new Chromosome(problem, a, b, BitMask, maxVal);// try selecting Reasonable vt's... won't work!
                Chromosome child = new Chromosome(a, b, BitMask, maxVal, problem.MutationRate);
                child.fitness = Evaluate(problem, child, problem.GoalBookings);   
                
                if (child.fitness == 0)
                {
                    int wi = 0;
                }

                // insert child
                population.Add(child);
                // order = beast -> least fitness
                population = population.OrderBy(c => c.fitness).ToList();
                if (child.fitness == 0)
                    solved = true;

                if(i % 10000 == 0)
                {
                    sw.Stop();
                    Console.WriteLine("top:" + population[0].fitness + " time:" + sw.Elapsed);
                    sw.Restart();
                }
                i++;
            }

            sw.Stop();
            Console.WriteLine("top:" + population[0].fitness + " time:" + sw.Elapsed);


        }

        // run next batch of generations, while reporting progress
        public void NextBatchParalell()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            Random random = new Random();
            // order = beast -> least fitness
            population = population.OrderBy(c => c.fitness).ToList();

            int maxIndex = populationSize - 1;
            int maxVal = problem.Restaurant.VirtualTables.Count();  // max mutation assignment

            List<bool> BitMask = new List<bool>();
            for (int b = 0; b < problem.GoalBookings.Count(); b++)
                BitMask.Add(random.Next(0, 1) == 1);


            //int i = 0;
            bool solved = false;
            int[] nums = Enumerable.Range(0, 1000000).ToArray();
            Parallel.For(0, nums.Length, i => {
                

                // pick two for mating!
                Chromosome a = population[random.Next((int)maxIndex / 3)];
                Chromosome b = population[random.Next((int)maxIndex)];

                // mate!
                Chromosome child = new Chromosome(a, b, BitMask, maxVal);
                child.fitness = Evaluate(problem, child, problem.GoalBookings);
                if (child.fitness == 0)
                {
                    int wi = 0;
                }

                // insert child
                population.Add(child);
                // order = beast -> least fitness
                population = population.OrderBy(c => c.fitness).ToList();
                if (child.fitness == 0)
                    solved = true;

                if (i % 10000 == 0)
                {
                    sw.Stop();
                    Console.WriteLine("top:" + population[0].fitness + " time:" + sw.Elapsed);
                    sw.Restart();
                }
                i++;
            });
            

            sw.Stop();
            Console.WriteLine("top:" + population[0].fitness + " time:" + sw.Elapsed);


        }
        // evaluation blackboard
        //private int[,] spacetime;

        public int Evaluate(Problem problem, Chromosome solution, List<Booking> bookings)
        {
            int result = 0;
            int [,] spacetime = new int[problem.Restaurant.Tables.Count(), problem.Restaurant.MaxTime];
            /*
            for (int t = 0; t < tables; t++)
                for (int m = 0; m < maxTime; m++)
                    spacetime[t, m] = 0;
                    */
            int geneNr = 0;
            foreach (Booking booking in problem.GoalBookings)
            {
                VirtualTable vt = problem.Restaurant.VirtualTables[solution.VirtualTableIds[geneNr]];

                foreach(int tableId in vt.TableIds)
                {
                    for(int time=booking.Start; time<booking.End; time++)
                    {
                        spacetime[tableId, time]++;
                    }
                }

                // punishment for every booking that has a VT with too few seats
                if(vt.Capacity < booking.Guests)
                {
                    result += booking.Guests - vt.Capacity;
                }

                geneNr++;
            }

            for (int table = 0; table < problem.Restaurant.Tables.Count(); table++)
                for (int time = 0; time < problem.Restaurant.MaxTime; time++)
                    result += spacetime[table, time] > 1 ? spacetime[table, time] - 1 : 0;

            


            return result;
        }
    }
}
