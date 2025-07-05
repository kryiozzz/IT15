using Opti.Models;
using System;
using System.Collections.Generic;

namespace Opti.Models
{
    public class AdminDashboardViewModel
    {
        // Revenue Statistics
        public decimal TotalRevenue { get; set; }

        // User Statistics
        public int NewCustomers { get; set; }
        public int ActiveAccounts { get; set; }
        public double GrowthRate { get; set; } // Add this property

        // Machine Statistics
        public int OperationalMachines { get; set; }
        public int MachinesUnderMaintenance { get; set; }
        public int OfflineMachines { get; set; }

        // Order Statistics
        public int PendingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int InProgressOrders { get; set; }

        // Recent Orders
        public List<CustomerOrder> RecentOrders { get; set; } = new List<CustomerOrder>();

        // Chart Data
        public decimal[] MonthlySales { get; set; } = new decimal[7];
        public int[] MonthlyCustomerGrowth { get; set; } = new int[7];

        // Machine Type Summary
        public List<MachineTypeSummary> MachineTypeSummary { get; set; } = new List<MachineTypeSummary>();
    }

    public class MachineTypeSummary
    {
        public string MachineType { get; set; }
        public int Total { get; set; }
        public int Operational { get; set; }
        public int UnderMaintenance { get; set; }
        public int Offline { get; set; }
    }
}