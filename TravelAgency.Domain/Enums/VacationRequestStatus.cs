namespace TravelAgency.Domain.Enums
{
    /// <summary>
    /// Статус заявки на отпуск.
    /// </summary>
    public enum VacationRequestStatus
    {
        New = 0,         // Новая
        InProgress = 1,  // В работе
        Approved = 2,    // Согласована
        Paid = 3         // Оплачена
    }
}
