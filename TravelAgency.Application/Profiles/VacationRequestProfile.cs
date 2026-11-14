using AutoMapper;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Profiles;

/// <summary>
/// Настраивает маппинг данных заявки на отпуск.
/// </summary>
public class VacationRequestProfile : Profile
{
    public VacationRequestProfile()
    {
        CreateMap<VacationRequestEntity, VacationRequestDto>();
           
    }
}