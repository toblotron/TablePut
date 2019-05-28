using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TablePutter.Workers;
using TimeMachine.ViewModels;
/*
* Visa grundplacering, för att visa problemet
*   - visa overflow i en tabell nedan, från default-lösning (bara placera ut bäst det går så länge det finns bord)
*/
namespace TablePutter.Models
{
    class Problem : MVVMBase
    {
        public ObservableCollection<Booking> Bookings { get; set; }
        public List<Booking> GoalBookings;

        public int NextUnplacedBookingId { get; set; }
        public int MutationRate { get; set; }
        public Restaurant Restaurant { get; set; }
        public ObservableCollection<BookingAssignment> Assignments { get; set; }
        public ObservableCollection<int> SkippedBookingsIds { get; set; }
        public Solver solver;
        public Random random = new Random();
        public int GetTablesHeight { get { return Restaurant.NrTables * Constants.TableRowHeight;  } }
        public int GetBookingsHeight { get { return Bookings.Count() * Constants.TableRowHeight; } }

        public Problem(int bookings, Restaurant restaurant) {
            var random = new Random();
            Restaurant = restaurant;
            MutationRate = 15;
            Bookings = new ObservableCollection<Booking>();
            Assignments = new ObservableCollection<BookingAssignment>();
            SkippedBookingsIds = new ObservableCollection<int>();

            for (int i=0; i < bookings; i++)
            {
                int length = random.Next(6) + 2;
                var start = random.Next(restaurant.MaxTime - length);

                Bookings.Add(new Booking { Id = i, Guests = random.Next(7) + 1, Start = start, End = start + length });
            }
            solver = new Solver();
            //solver.Reset(Constants.populationSize, this);
        }

        // calculate which bookings we want to place
        public void UpdateGoalBookings()
        {
            // pick all placed + the first non-placed - skipped
            List<Booking> goalBookings = Bookings.Where(b => !SkippedBookingsIds.Contains(b.Id) && Assignments.Where(a => a.BookingId == b.Id).Count() != 0).ToList();
            int nextUnplacedBookingId = GetUnplacedBookings().First(x => !SkippedBookingsIds.Contains(x.Id)).Id; //goalBookings.Last().Id + 1;
            goalBookings.Add(Bookings[nextUnplacedBookingId]);
            GoalBookings = goalBookings;
        }

        public bool PlaceNextFromQueue()
        {
            List<Booking> unplacedBookings = GetUnplacedBookings();
            int VirtualTableId = GetFirstTableWithFreeSlot(unplacedBookings.First()); //AssignFirstFittingVirtualTable(unplacedBookings.First()); // GetFirstTableWithFreeSlot(unplacedBookings.First());
            if (VirtualTableId > -1)
            {
                unplacedBookings = GetUnplacedBookings();
                NextUnplacedBookingId = unplacedBookings.First().Id;
                this.OnPropertyChanged("NextUnplacedBookingId");
                this.OnPropertyChanged("Bookings");
            }
            return VirtualTableId > -1;
        }

        public List<Booking> GetUnplacedBookings()
        {
            return Bookings.Where(b => !SkippedBookingsIds.Contains(b.Id) && Assignments.Where(a => a.BookingId == b.Id).Count() == 0).ToList();
        }

        public void SkipNextBooking()
        {
            List<Booking> unplacedBookings = GetUnplacedBookings();
            NextUnplacedBookingId = unplacedBookings.First().Id;

            SkippedBookingsIds.Add(NextUnplacedBookingId);
            //Bookings.RemoveAt(NextUnplacedBookingId);

            NextUnplacedBookingId++;
            this.OnPropertyChanged("NextUnplacedBookingId");
            this.OnPropertyChanged("Bookings");
        }


        public void EvaluateLeader()
        {
            Chromosome winner = solver.population.First();
            solver.Evaluate(this, winner, Bookings.ToList());
        }


        public int GetReasonableVtId(int bookingNr)
        {
            Booking booking = GoalBookings[bookingNr];

            int seatsRequired = booking.Guests;
            while (!Restaurant.vtDict.ContainsKey(seatsRequired))
                seatsRequired++;
            
            int count = Restaurant.vtDict[seatsRequired].Count;
            return Restaurant.vtDict[seatsRequired][random.Next(count - 1)].ID;
        }

        public void RemovePreviousFromQueue()
        {
            List<Booking> placedBookings = Bookings.Where(b => SkippedBookingsIds.Contains(b.Id) || Assignments.Where(a => a.BookingId == b.Id).Count() > 0).ToList(); 
            if (placedBookings.Count() > 0) {
                Booking lastPlaced = placedBookings.OrderBy(x => x.Id).Last();

                if (SkippedBookingsIds.Contains(lastPlaced.Id))
                {
                    // just skipped? un-skip
                    SkippedBookingsIds.Remove(lastPlaced.Id);
                } else { 
                    Assignments = new ObservableCollection<BookingAssignment>(Assignments.Where(x => x.BookingId != lastPlaced.Id));
                    this.OnPropertyChanged("Assignments");
                }
                List<Booking> unplacedBookings = GetUnplacedBookings();
                NextUnplacedBookingId = unplacedBookings.First().Id;
                this.OnPropertyChanged("NextUnplacedBookingId");
                this.OnPropertyChanged("Bookings");
            }
        }

        // if there is a solution available that has 0 objections, take it and
        // replace the current BookingAssignments with those in that solution
        public bool AdaptAdequateSolution()
        {
            bool result = false;

            Chromosome winner = solver.population.First();
            if (winner.fitness == 0)
            {
                Assignments = new ObservableCollection<BookingAssignment>();
                int geneNr = 0;
                foreach(Booking booking in GoalBookings)
                {
                    VirtualTable vt = Restaurant.VirtualTables[winner.VirtualTableIds[geneNr]];
                    foreach (int tableIndex in vt.TableIds)
                    {
                        Table Table = Restaurant.Tables[tableIndex];
                        Assignments.Add(new BookingAssignment() { TableId = tableIndex, BookingId = booking.Id }); // kan bli fel! - vi kör inte med ostörd mängd bokningar!
                    }
                    geneNr++;
                }

                /*
                 * foreach(int vtId in winner.VirtualTableIds)
                {
                    VirtualTable vt = Restaurant.VirtualTables[vtId];
                    foreach (int tableIndex in vt.TableIds)
                    {
                        Table Table = Restaurant.Tables[tableIndex];
                        Assignments.Add(new BookingAssignment() { TableId = tableIndex, BookingId = bookingId }); // kan bli fel! - vi kör inte med ostörd mängd bokningar!
                    }
                    bookingOrder++;
                }*/

                this.OnPropertyChanged("Assignments");
            }

            return result;
        }
        /*
        public int AssignFirstFittingVirtualTable(Booking booking)
        {
            int result = -1;

            List<VirtualTable> largeEnoughVTs = Restaurant.VirtualTables.Where(vt => vt.Capacity >= booking.Guests).ToList();
            int vtIndex = 0;
            while (result == -1 && vtIndex < largeEnoughVTs.Count())
            {
                VirtualTable vt = largeEnoughVTs[vtIndex];
                bool canFit = true;
                foreach(int tableIndex in vt.TableIds)
                {
                    Table Table = Restaurant.Tables[tableIndex];
                    if (!Table.CanFitBooking(booking))
                        canFit = false;
                }
                if (canFit)
                    result = vtIndex;

                vtIndex += 1;
            }

            if (result > -1)
            {
                // assign this vt to booking
                Assignments.Add(new BookingAssignment() { BookingId = booking.Id, TableId = vtIndex });
            }

            return result;
        }
        */
                public int GetFirstTableWithFreeSlot(Booking booking)
        {
            int tableIndex = 0;
            int foundTable = -1;
            
            while(tableIndex < Restaurant.NrTables && foundTable == -1)
            {
                int placesLeftToFill = booking.Guests;
                Table table = Restaurant.Tables[tableIndex];
                int groupIndex = tableIndex;
                List<BookingAssignment> AssignedBookings = Assignments.Where(a => a.TableId == tableIndex).ToList();
                while (placesLeftToFill > 0 && table.CanFitBooking(booking, this, AssignedBookings) && groupIndex < Restaurant.NrTables) {
                    placesLeftToFill -= table.Size;
                    groupIndex += 1;
                    if (groupIndex < Restaurant.NrTables)
                    {
                        table = Restaurant.Tables[groupIndex];
                        AssignedBookings = Assignments.Where(a => a.TableId == table.Id).ToList();
                    }
                }

                if (placesLeftToFill < 1) {
                    // a place hsa been found for the booking
                    foundTable = tableIndex;
                    // grab the tables
                    for (int i = foundTable; i < groupIndex; i++)
                    {
                        //Restaurant.Tables[i].Bookings.Add(booking);
                        Assignments.Add(new BookingAssignment() { BookingId = booking.Id, TableId = i });
                    }
                    
                }

                tableIndex++;
            }
            return foundTable;
        }

    }
}
