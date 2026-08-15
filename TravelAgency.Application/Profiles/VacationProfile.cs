using AutoMapper;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Application.Profiles
{
    public class VacationProfile : Profile
    {
        public VacationProfile()
        {
            CreateMap<VacationEntity, VacationInfoDto>();
        }
    }
}
