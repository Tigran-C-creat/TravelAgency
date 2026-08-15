namespace TravelAgency.Application.DTOs.Response
{
    public record VacationInfoDto(
        Guid Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        decimal TotalCost
    );
}
