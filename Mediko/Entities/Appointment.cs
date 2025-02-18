using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mediko.Entities
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DoctorId { get; set; }

        [ForeignKey("DoctorId")]
        public Doctor Doctor { get; set; }

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

        [Required]
        public bool IsConfirmed { get; set; } = false;


        public Appointment()
        {
        }

        public Appointment(DateOnly date, TimeOnly time)
        {
            AppointmentDate = date;
            AppointmentTime = time;
            FullAppointmentDateTime = new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, 0);
        }
    }
}
