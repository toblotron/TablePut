using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeMachine.ViewModels;
using TablePutter;

namespace TablePutter.Models
{
    class Booking : MVVMBase
    {
        public int Id { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public int Guests { get; set; }

        public int Top { get { return Id * Constants.TableRowHeight; } }
        public int Left { get { return Start * Constants.TimeUnitPixelWidth + Constants.tablesXStart; } }
        public int Width { get { return (End - Start) * Constants.TimeUnitPixelWidth; } }
        public string GetPresentationName { get { return "#" + Id + " (" + Guests + ")"; } }
        public System.Windows.Media.Brush GetColor
        { get
            {
                Problem problem = ModelRoot.Instance.Problem;
                if(problem.GoalBookings == null)
                    return System.Windows.Media.Brushes.Yellow;
                if (problem.GoalBookings.Contains(this))
                {
                    return System.Windows.Media.Brushes.Orange;
                } else
                {
                    return System.Windows.Media.Brushes.LightBlue;
                }
            }
        }
        public bool Intersects(Booking booking)
        {
            return !(booking.Start >= End || booking.End <= Start);
        }
    }
}
