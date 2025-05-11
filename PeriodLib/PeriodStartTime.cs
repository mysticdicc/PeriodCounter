using System.ComponentModel.DataAnnotations;

namespace PeriodLib
{
    public class PeriodStartTime
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }

        public PeriodStartTime(string userId)
        {
            Id = Guid.NewGuid();
            StartTime = DateTime.Now;
            UserId = userId;
        }
    }
}
