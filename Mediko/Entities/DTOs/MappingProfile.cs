using AutoMapper;
using Mediko.API.DTOs;
using Mediko.Entities;

namespace Mediko.API.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Department → DepartmentDto mapping
            CreateMap<Department, DepartmentDto>()
                // Eğer Policlinics koleksiyonunu da DTO'ya çevirmek istersen:
                .ForMember(dest => dest.Policlinics, opt => opt.MapFrom(src => src.Policlinics));

            // Policlinic → PoliclinicDto mapping
            CreateMap<Policlinic, PoliclinicDto>();

            // Tersi mappingleri (varsa) de ekleyebilirsin:
            CreateMap<DepartmentDto, Department>();
            CreateMap<PoliclinicDto, Policlinic>();
        }
    }
}
