using AutoMapper;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Domain.Entities;


namespace TravelAgency.Application.Profiles
{
    public class EmployeeProfile : Profile
    {
        public EmployeeProfile()
        {
            CreateMap<EmployeeEntity, EmployeeDto>();
        }
    }
}
