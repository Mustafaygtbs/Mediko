using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mediko.Entities
{
    public class Policlinic
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }  

        [Required]
        public int DepartmentId { get; set; }  

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }  

        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
