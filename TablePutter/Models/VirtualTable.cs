using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TablePutter.Models
{
    class VirtualTable
    {
        public VirtualTable(ObservableCollection<Table> tables, int id, int smallest)
        {
            ID = id;
            
            TableIds = new ObservableCollection<int>(tables.Select(t => t.Id));
            Capacity = tables.Sum(t => t.Size);
            Smallest = Capacity - smallest + 1;
        }
                            // unique ID, and also index in the main storage
        public int ID = 0;
                            // unique IDs of Tables
        public ObservableCollection<int> TableIds { get; set; }
                            // max number of guests
        public int Capacity { get; set; }
                            // minimum reasonable number of guests (if there is one more guest than this, there is one table that is not needed, and could be removed
        public int Smallest { get; set; }

    }
}
