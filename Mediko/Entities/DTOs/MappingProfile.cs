using AutoMapper;
using Mediko.Entities;
using Mediko.Entities.DTOs.AppointmentDTOs;
using Mediko.Entities.DTOs.AppointmentDTOs.Mediko.Entities.DTOs.AppointmentDTOs;
using Mediko.Entities.DTOs.DepartmentDTOs;
using Mediko.Entities.DTOs.PoliclinicDTOs;
using Mediko.Entities.DTOs;
using Mediko.Entities.DTOs.UserDTOs;

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

            // Ters eşleme ve Create/Update DTO'lar
            CreateMap<DepartmentDto, Department>();
            CreateMap<DepartmentCreateDto, Department>();
            CreateMap<DepartmentUpdateDto, Department>();

            CreateMap<PoliclinicDto, Policlinic>();
            CreateMap<PoliclinicCreateDto, Policlinic>();
            CreateMap<PoliclinicUpdateDto, Policlinic>();
            CreateMap<PoliclinicCreateWithNameDto, Policlinic>();

            // Kullanıcı dönüşümü için mapping
            CreateMap<User, UserDto>();

            // Randevu dönüşümü
            CreateMap<Appointment, AppointmentDto>();

            CreateMap<AppointmentCreateDto, Appointment>()
                .ForMember(dest => dest.FullAppointmentDateTime, opt =>
                    opt.MapFrom(src => new DateTime(
                        src.AppointmentDate.Year,
                        src.AppointmentDate.Month,
                        src.AppointmentDate.Day,
                        src.AppointmentTime.Hour,
                        src.AppointmentTime.Minute,
                        0
                    )));

            CreateMap<AppointmentUpdateDto, Appointment>()
               .ForMember(dest => dest.FullAppointmentDateTime, opt =>
                   opt.MapFrom(src => new DateTime(
                       src.AppointmentDate.Year,
                       src.AppointmentDate.Month,
                       src.AppointmentDate.Day,
                       src.AppointmentTime.Hour,
                       src.AppointmentTime.Minute,
                       0
                   )));
        }
    }
}
