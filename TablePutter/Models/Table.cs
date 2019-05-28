using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeMachine.ViewModels;

namespace TablePutter.Models
{
    class Table : MVVMBase
    {
        public int Id { get; set; }
        public int Size {get; set;}
        //public ObservableCollection<Booking> Bookings { get; set; } 

        public int Top { get { return Id * Constants.TableRowHeight; } }
        public string GetPresentationID { get { return "table " + (Id + 1) + " (" + Size + ")"; } }

        public Table()
        {
           // Bookings = new ObservableCollection<Booking>();
        }

        public bool CanFitBooking(Booking booking, Problem problem, List<BookingAssignment> AssignedBookings)
        {
            return !AssignedBookings.Any(ass => problem.Bookings[ass.BookingId].Intersects(booking));
        }
    }
}
