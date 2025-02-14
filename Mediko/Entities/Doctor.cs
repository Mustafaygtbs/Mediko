using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mediko.Entities
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public  string Name { get; set; } = string.Empty;  // Doktorun adı

        [Required]
        public int PoliclinicId { get; set; }  // Poliklinik ile ilişki

        [ForeignKey("PoliclinicId")]
        public Policlinic Policlinic { get; set; } // İlişki

        public bool IsAvailable { get; set; } = true;  // Randevu alınıp alınamayacağını belirler
    }
}
