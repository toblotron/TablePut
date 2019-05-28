using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeMachine.ViewModels;

namespace TablePutter.Models
{
    class BookingAssignment : MVVMBase
    {
        public int BookingId { get; set; }
        public int TableId { get; set; }

        public int Top { get { return TableId * Constants.TableRowHeight; } }
        public int Left { get { return ModelRoot.Instance.Problem.Bookings[BookingId].Start * Constants.TimeUnitPixelWidth + Constants.tablesXStart; } }
        public int Width {
            get {
                Booking booking = ModelRoot.Instance.Problem.Bookings[BookingId];
                return (booking.End - booking.Start) * Constants.TimeUnitPixelWidth -2;
            }
        }
        public string GetPresentationName {
            get {
                Booking booking = ModelRoot.Instance.Problem.Bookings[BookingId];
                return "#" + BookingId + " (" + booking.Guests +")";
            }
        }
    }
}
