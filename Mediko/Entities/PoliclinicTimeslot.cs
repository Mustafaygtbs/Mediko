using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mediko.Entities
{
    public class PoliclinicTimeslot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PoliclinicId { get; set; }

        [ForeignKey("PoliclinicId")]
        public Policlinic Policlinic { get; set; }

        [Required]
        public DateOnly Date { get; set; }    

        [Required]
        public TimeOnly StartTime { get; set; } 

        public bool IsOpen { get; set; } = true;   
        public bool IsBooked { get; set; } = false; 
    }
}
