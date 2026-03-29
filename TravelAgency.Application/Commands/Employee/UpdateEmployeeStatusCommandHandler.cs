using FluentValidation;
using MediatR;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;

namespace TravelAgency.Application.Commands.Employee;

public class UpdateEmployeeStatusCommandHandler : IRequestHandler<UpdateEmployeeStatusCommand>
{
    private readonly IRepository _repository;
    private readonly IRedisCacheService _cache;

    public UpdateEmployeeStatusCommandHandler(IRepository repository, IRedisCacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task Handle(UpdateEmployeeStatusCommand request, CancellationToken cancellationToken)
    {
        UpdateEmployeeStatusCommandValidator.Default.ValidateAndThrow(request);

        var employee = await _repository.GetOrThrowAsync<EmployeeEntity>(request.Id, cancellationToken);

        employee.Status = request.Status;

        await _repository.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"employee:{request.Id}");
    }
}

public class UpdateEmployeeStatusCommandValidator : AbstractValidator<UpdateEmployeeStatusCommand>
{
    public static UpdateEmployeeStatusCommandValidator Default { get; } = new();
    public UpdateEmployeeStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("The specified employee status does not exist");
    }
}
