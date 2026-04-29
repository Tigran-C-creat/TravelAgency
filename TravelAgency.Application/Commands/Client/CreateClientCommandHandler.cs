using AutoMapper;
using FluentValidation;
using MediatR;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;

namespace TravelAgency.Application.Commands.Client
{
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, Guid>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        //private readonly IAccessService _access;

        public CreateClientCommandHandler(
            IRepository repository,
            IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<Guid> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            // 1. Проверка роли — только сотрудник может добавлять клиента
            //_access.EnsureEmployee();

            // 2. Валидация
            CreateClientCommandValidator.Default.ValidateAndThrow(request);

            // 3. Маппинг
            var entity = _mapper.Map<ClientEntity>(request);

            if (entity.Id == Guid.Empty)
                entity.Id = Guid.NewGuid();

            // 4. Сохранение
            await _repository.AddAsync(entity, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return entity.Id;
        }
    }

    public class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
    {
        public static CreateClientCommandValidator Default { get; } = new();

        public CreateClientCommandValidator()
        {
            // FullName
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name is too long");

            //PassportSeries
            RuleFor(x => x.PassportSeries)
               .NotEmpty().WithMessage("Passport series is required")
               .Length(4).WithMessage("Passport series must contain exactly 4 characters")
               .Matches("^[0-9]{4}$").WithMessage("Passport series must contain only digits");

            //PassportNumber
            RuleFor(x => x.PassportNumber)
               .NotEmpty().WithMessage("Passport number is required")
               .Length(6).WithMessage("Passport number must contain exactly 6 characters")
               .Matches("^[0-9]{6}$").WithMessage("Passport number must contain only digits");

            //Phone
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^\+?[0-9]{7,15}$")
                .WithMessage("Phone number must contain 7–15 digits and may start with +");

            //Email
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email format is invalid");

            //Login
            RuleFor(x => x.Login)
                .NotEmpty().WithMessage("Login is required")
                .MinimumLength(3).WithMessage("Login must be at least 3 characters")
                .MaximumLength(50).WithMessage("Login is too long");

            //Password
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password is too long");
        }
    }
}
