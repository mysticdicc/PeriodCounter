using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace PeriodLib
{
    public class PeriodStartTime
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }
        public int PainRating { get; set; }

        [JsonConstructor]
        public PeriodStartTime(Guid Id, string UserId, DateTime StartTime, int PainRating)
        {
            this.Id = Id;
            this.UserId = UserId;
            this.StartTime = StartTime;
            this.PainRating = PainRating;
        }

        public PeriodStartTime(string userId, int painRating)
        {
            Id = Guid.NewGuid();
            StartTime = DateTime.Now;
            UserId = userId;
            PainRating = painRating;
        }

        public PeriodStartTime(string userId)
        {
            Id = Guid.NewGuid();
            StartTime = DateTime.Now;
            UserId = userId;
            PainRating = 0;
        }
    }
}
