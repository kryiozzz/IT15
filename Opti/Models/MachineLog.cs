namespace Opti.Models
{
    public class MachineLog
    {
        public int LogId { get; set; }
        public int MachineId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public Machine Machine { get; set; }
        public User User { get; set; }
    }
}
