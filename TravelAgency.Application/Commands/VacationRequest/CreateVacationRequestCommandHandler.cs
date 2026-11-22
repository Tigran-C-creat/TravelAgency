using FluentValidation;
using MediatR;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Enums;
using TravelAgency.Domain.Interfaces;

namespace TravelAgency.Application.Commands.VacationRequest;

public sealed class CreateVacationRequestCommandHandler : IRequestHandler<CreateVacationRequestCommand, Guid>
{
    private readonly IRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public CreateVacationRequestCommandHandler(IRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateVacationRequestCommand request, CancellationToken cancellationToken)
    {
        CreateVacationRequestCommandValidator.Default.ValidateAndThrow(request);
        _ = await _repository.GetOrThrowAsync<VacationEntity>(request.VacationId, cancellationToken);

        var entity = new VacationRequestEntity
        {
            Id = Guid.NewGuid(),
            ClientId = _currentUser.ClientId,
            VacationId = request.VacationId,
            Message = request.Message,
            Status = VacationRequestStatus.New
        };

        await _repository.AddAsync(entity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}

public sealed class CreateVacationRequestCommandValidator : AbstractValidator<CreateVacationRequestCommand>
{
    public static CreateVacationRequestCommandValidator Default { get; } = new();

    public CreateVacationRequestCommandValidator()
    {
        RuleFor(x => x.VacationId).NotEmpty();
        RuleFor(x => x.Message).MaximumLength(2000);
    }
}
