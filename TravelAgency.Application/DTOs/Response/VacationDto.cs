namespace TravelAgency.Application.DTOs.Response
{
    public record VacationDto(
        Guid Id,
        string Name,
        DateTime StartDate,
        DateTime EndDate,
        decimal TotalCost,
        EmployeeDto Employee,             
        List<ClientDto> Clients       
    );
}
