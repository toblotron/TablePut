using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TablePutter.Models
{
    class Chromosome
    {
        public List<int> VirtualTableIds { get; set; }
        public int fitness { get; set; }
        public static Random random = new Random();
        public static int mutationPercentage = 2;
        public static Problem problem = null;

        public Chromosome()
        {
            VirtualTableIds = new List<int>();
            fitness = 10000;
        }

        //  create with randomize assignments
        public Chromosome(Problem problem)
        {
            VirtualTableIds = new List<int>();
            Chromosome.problem = problem;
            int max = problem.Restaurant.VirtualTables.Count();
            // generate possible solutions for GoalBookings
            foreach (Booking b in problem.GoalBookings)
            {
                VirtualTableIds.Add(random.Next(max)) ;
            }
        }

        // crossbreed
        public Chromosome(Chromosome a, Chromosome b, List<bool> BitMask, int maxVal, int MutationRate = 20)
        {
            VirtualTableIds = new List<int>();
            int nextMutationIn = random.Next(1000/ MutationRate);
            for (int i=0; i<BitMask.Count(); i++)
            {
                if (nextMutationIn == 0)
                {
                    //List<VirtualTable> largeEnoughVTs = problem.Restaurant.VirtualTables.Where(vt => vt.Capacity >= booking.Guests).ToList();
                    VirtualTableIds.Add(random.Next(maxVal));
                    nextMutationIn = random.Next(100);
                }
                else
                {
                    VirtualTableIds.Add(BitMask[i] ? a.VirtualTableIds[i] : b.VirtualTableIds[i]);
                    nextMutationIn--;
                }
            }
        }

        public Chromosome(Problem problem, Chromosome a, Chromosome b, List<bool> BitMask, int maxVal)
        {
            VirtualTableIds = new List<int>();
            int nextMutationIn = random.Next(100);
            for (int i = 0; i < BitMask.Count(); i++)
            {
                if (nextMutationIn == 0)
                {
                    int vtID = problem.GetReasonableVtId(i);
                    VirtualTableIds.Add(vtID);
                    nextMutationIn = random.Next(100);
                }
                else
                {
                    VirtualTableIds.Add(BitMask[i] ? a.VirtualTableIds[i] : b.VirtualTableIds[i]);
                    nextMutationIn--;
                }
            }
        }

        // mutate
        public Chromosome(Chromosome a, int mutations, int maxVal)
        {
            VirtualTableIds = a.VirtualTableIds.Select(id => id).ToList();
            for(int i = 0; i < maxVal; i++)
            {
                VirtualTableIds[random.Next(VirtualTableIds.Count())] = random.Next(maxVal); 
            }
        }

    }
}
