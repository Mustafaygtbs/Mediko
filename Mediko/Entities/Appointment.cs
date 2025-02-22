using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography.X509Certificates;

namespace Mediko.Entities
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }


        [Required]
        public int PoliclinicTimeslotId { get; set; }
        [ForeignKey("PoliclinicTimeslotId")]
        public PoliclinicTimeslot PoliclinicTimeslot { get; set; }

   
        [Required]
        public int PoliclinicId { get; set; }
        [ForeignKey("PoliclinicId")]
        public Policlinic Policlinic { get; set; }

        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public DateOnly AppointmentDate { get; set; }

        [Required]
        public TimeOnly AppointmentTime { get; set; }

        [Required]
        public DateTime FullAppointmentDateTime { get; set; }

         public bool IsConfirmed { get; set; } = false;
       // public string IsCOnfirmed { get; set; }

        public Appointment()
        {
        }

        public Appointment(DateOnly date, TimeOnly time)
        {
            AppointmentDate = date;
            AppointmentTime = time;
            FullAppointmentDateTime = new DateTime(
                date.Year, date.Month, date.Day, time.Hour, time.Minute, 0
            );
        }
    }
}
