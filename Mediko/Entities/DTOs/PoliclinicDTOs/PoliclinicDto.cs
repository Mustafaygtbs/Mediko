using Mediko.Entities.DTOs.DepartmentDTOs;

namespace Mediko.Entities.DTOs.PoliclinicDTOs
{
    public class PoliclinicDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;

        public int DepartmentId { get; set; }
        public DepartmentSimplifiedDto? Department { get; set; }
    }
}
