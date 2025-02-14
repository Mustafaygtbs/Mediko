using System.ComponentModel.DataAnnotations;

namespace Mediko.Entities
{
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }=string.Empty;

        public ICollection<Policlinic> Policlinics { get; set; } = new List<Policlinic>();
    }
}
