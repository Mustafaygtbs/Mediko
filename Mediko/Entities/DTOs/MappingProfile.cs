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
            CreateMap<Department, DepartmentDto>()

                .ForMember(dest => dest.Policlinics, opt => opt.MapFrom(src => src.Policlinics));


            CreateMap<Policlinic, PoliclinicDto>();


            CreateMap<DepartmentDto, Department>();
            CreateMap<PoliclinicDto, Policlinic>();


            CreateMap<DepartmentCreateDto, Department>();
            CreateMap<DepartmentUpdateDto, Department>();


            CreateMap<Policlinic, PoliclinicDto>()
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Department));



            CreateMap<PoliclinicCreateDto, Policlinic>();
            CreateMap<PoliclinicUpdateDto, Policlinic>();
            CreateMap<PoliclinicCreateWithNameDto, Policlinic>();

            CreateMap<Department, DepartmentCreateDto>();


        }
    }
}
