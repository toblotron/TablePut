using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeMachine.ViewModels;

namespace TablePutter.Models
{
    class Restaurant : MVVMBase
    {
        public int NrTables { get; set; }
        public ObservableCollection<Table> Tables { get; set; }
        public int MaxTime { get; set; }
        public ObservableCollection<VirtualTable> VirtualTables { get; set; }
        public Dictionary<int, ObservableCollection<VirtualTable>> vtDict = new Dictionary<int, ObservableCollection<VirtualTable>>();

        public Restaurant(int nrTables, int maxTime)
        {
            NrTables = nrTables;
            MaxTime = maxTime;

            var random = new Random();
            Tables = new ObservableCollection<Table>();
            for (int i=0; i< nrTables; i++)
            {
                Tables.Add(new Table { Id = i, Size = random.Next(4) + 2 });
            }

            // generate virtual tables - allow up to 5 adjecent tables to be a virtual table
            VirtualTables = new ObservableCollection<VirtualTable>();

            int vtID = 0;
            for(int t = 0; t < Tables.Count(); t++)
            {
                int smallest = 100;
                for(int s=0; s < 5 && s + t < Tables.Count(); s++ )
                {
                    ObservableCollection<Table> tables = new ObservableCollection<Table>();
                    for (int tnr = t; tnr < t + s +1; tnr++) {
                        if (Tables[tnr].Size < smallest)
                            smallest = Tables[tnr].Size;

                        tables.Add(Tables[tnr]);
                    }
                    VirtualTable vt = new VirtualTable(tables, vtID, smallest);
                    vtID++;
                    VirtualTables.Add(vt);

                    // build vtDict
                    // - reasonable (non wasteful) vt's for each {nr of guests}
                    for (int dictKey = vt.Smallest; dictKey <= vt.Capacity; dictKey++)
                    {
                        // add this vt to the dicts where it fits
                        ObservableCollection<VirtualTable> tablesOfCapacity = null;
                        if (vtDict.Keys.Contains(vt.Capacity))
                            tablesOfCapacity = vtDict[vt.Capacity];
                        else
                        {
                            tablesOfCapacity = new ObservableCollection<VirtualTable>();
                            vtDict.Add(vt.Capacity, tablesOfCapacity);
                        }
                        tablesOfCapacity.Add(vt);
                    }
                }
            }

        }
    }
}
