using Mediko.Entities.DTOs.PoliclinicDTOs;

namespace Mediko.Entities.DTOs.DepartmentDTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
       
        public List<PoliclinicDto> Policlinics { get; set; } = new List<PoliclinicDto>();
    }
}
