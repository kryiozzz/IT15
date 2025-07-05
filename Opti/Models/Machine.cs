using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Opti.Models
{
    public class Machine
    {
        [Key]
        public int MachineId { get; set; }

        [Required]
        public string MachineName { get; set; }

        [Required]
        public string MachineType { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime LastMaintenanceDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string ImagePath { get; set; }

        // Navigation properties
        public virtual ICollection<MachineLog> MachineLogs { get; set; }
        // Constructor to initialize collections
        public Machine()
        {
            MachineLogs = new HashSet<MachineLog>();
        }
    }
}