using AutoMapper;
using Mediko.Entities;
using Mediko.Entities.DTOs.DepartmentDTOs;
using Mediko.Entities.DTOs.PoliclinicDTOs;

namespace Mediko.API.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // DEPARTMENT → DEPARTMENTDTO
            CreateMap<Department, DepartmentDto>()
                .ForMember(dest => dest.Policlinics, opt => opt.MapFrom(src => src.Policlinics));

            // DEPARTMENT → DEPARTMENTSIMPLIFIEDDTO
            CreateMap<Department, DepartmentSimplifiedDto>();

            // POLICLINIC → POLICLINICDTO 
            CreateMap<Policlinic, PoliclinicDto>()
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department));

            // TERS EŞLEME VE CREATE/UPDATE DTO'LAR
            CreateMap<DepartmentDto, Department>();
            CreateMap<DepartmentCreateDto, Department>();
            CreateMap<DepartmentUpdateDto, Department>();

            CreateMap<PoliclinicDto, Policlinic>();
            CreateMap<PoliclinicCreateDto, Policlinic>();
            CreateMap<PoliclinicUpdateDto, Policlinic>();
            CreateMap<PoliclinicCreateWithNameDto, Policlinic>();
        }
    }
}
