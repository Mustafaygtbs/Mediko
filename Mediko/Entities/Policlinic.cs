using System.ComponentModel.DataAnnotations;

namespace Mediko.Entities
{
    public class Policlinic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public  string Name { get; set; }  

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>(); 
    }
}
