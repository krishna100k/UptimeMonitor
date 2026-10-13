using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum EndpointStatus
    {
        Unknown = 0,
        Up = 1,
        Degraded = 2,
        Down = 3,
        Paused = 4
    }

}
