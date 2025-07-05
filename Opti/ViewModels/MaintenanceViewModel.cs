using Opti.Models;
using System;
using System.Collections.Generic;

namespace Opti.ViewModels
{
    public class MaintenanceViewModel
    {
        public List<Machine> MachinesUnderMaintenance { get; set; }
        public Dictionary<int, List<MachineLog>> MaintenanceLogs { get; set; }
        public int MaintenanceCount { get; set; }
    }
}