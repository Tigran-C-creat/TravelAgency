using MediatR;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;
using FluentValidation;

namespace TravelAgency.Application.Commands.Employee;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Guid>
{
    private readonly IRepository _repository;
    private readonly IRedisCacheService _cache;

    public CreateEmployeeCommandHandler(
        IRepository repository,
        IRedisCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Guid> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {

        CreateEmployeeCommandValidator.Default.ValidateAndThrow(request);

        // 1. Создаём сущность из команды
        var employee = new EmployeeEntity
        {
            Id =  Guid.NewGuid(),
            FullName = request.FullName,
            Login = request.Login,
            Password = request.Password,
            Status = request.Status,
        };

        // 2. Сохраняем в БД
        await _repository.AddAsync(employee, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        // 3. Возвращаем созданного сотрудника
        return employee.Id;

    }
}

public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public static CreateEmployeeCommandValidator Default { get; } = new();
    public CreateEmployeeCommandValidator()
    {
        // FullName
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required")
            .MaximumLength(100).WithMessage("Full name is too long");

        // Login
        RuleFor(x => x.Login)
            .NotEmpty().WithMessage("Login is required")
            .MinimumLength(3).WithMessage("Login must be at least 3 characters")
            .MaximumLength(50).WithMessage("Login is too long");

        // Password
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");

        // Status
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required");
    }
}
