using System.Collections.Generic;
using Opti.Models;

namespace Opti.Models
{
    public class WorkerDashboardViewModel
    {
        // Statistics for the dashboard
        public int MachinesOperational { get; set; }
        public int MachinesUnderMaintenance { get; set; }
        public int ProductionOrdersCompleted { get; set; }
        public int ProductionOrdersInProgress { get; set; }
        // Lists for tables
        public List<Machine> Machine { get; set; }
        public List<ProductionOrder> ProductionOrders { get; set; }

    }
}