using System.ComponentModel.DataAnnotations;

namespace PeriodLib
{
    public class DeviceRegistration(string userId, string fcm)
    {
        [Key]
        public string UserId { get; set; } = userId;
        public string Fcm { get; set; } = fcm;
    }
}
