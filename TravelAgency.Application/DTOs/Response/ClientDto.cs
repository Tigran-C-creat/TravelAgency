namespace TravelAgency.Application.DTOs.Response
{
    public record ClientDto(
      Guid Id,
      string FullName,
      string Phone,
      string Email,
      string Login,
      Guid? CompanyId
  );
}
