namespace Mediko.Entities.DTOs.PoliclinicDTOs
{
    public class PoliclinicCreateWithNameDto
    {
        public string Name { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public bool IsAvailable { get; set; } = true;
    }
}
