using AutoMapper;
using TravelAgency.Application.Commands.Client;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Application.Profiles
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<CreateClientCommand, ClientEntity>()
                /*.ForMember(x => x.Id, opt => opt.Ignore());*/ ;
        }
    }
}
