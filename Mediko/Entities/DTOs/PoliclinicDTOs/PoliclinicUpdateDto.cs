namespace Mediko.Entities.DTOs.PoliclinicDTOs
{
    public class PoliclinicUpdateDto
    {
        public int Id { get; set; }  
        public string Name { get; set; } = string.Empty;
        public int DepartmentId { get; set; }
        public bool IsAvailable { get; set; } = true;

    }
}
