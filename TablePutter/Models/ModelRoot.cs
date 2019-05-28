using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeMachine.ViewModels;

namespace TablePutter.Models
{
    class ModelRoot : MVVMBase
    {
        public static ModelRoot Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ModelRoot();
                return _Instance;
            }
            set
            {
                _Instance = value;
            }
        }

        private static ModelRoot _Instance = null;

        public Problem Problem {get;set;}

    }
}
