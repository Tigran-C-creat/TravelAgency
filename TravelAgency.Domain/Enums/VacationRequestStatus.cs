using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TravelAgency.Domain.Enums
{
    public enum VacationRequestStatus
    {
        New = 0,         // Новая
        InProgress = 1,  // В работе
        Approved = 2,    // Согласована
        Paid = 3         // Оплачена
    }
}
