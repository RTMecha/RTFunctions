using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RTFunctions.Functions.Optimization
{
    public class Exists
    {
        public static implicit operator bool(Exists exists) => exists != null;
    }
}
