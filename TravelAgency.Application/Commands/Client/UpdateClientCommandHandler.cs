using FluentValidation;
using MediatR;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Shared.Constants;
using TravelAgency.Shared.Exeptions;

namespace TravelAgency.Application.Commands.Client
{
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand>
    {
        private readonly IRepository _repository;
        private readonly IRedisCacheService _cache;

        public UpdateClientCommandHandler(IRepository repository, IRedisCacheService cache)
        {
            _repository = repository;
            _cache = cache;
        }

        public async Task Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            // При добавлении async - правил заменить на ValidateAndThrowAsync
            // 1. Валидация
            UpdateClientCommandValidator.Default.ValidateAndThrow(request);

            // 2. Получаем клиента или бросаем исключение
            var client = await _repository.GetOrThrowAsync<ClientEntity>(request.Id, cancellationToken);

            // 3. Обновляем поля
            Apply(request.FullName, v => client.FullName = v);
            Apply(request.PassportSeries, v => client.PassportSeries = v);
            Apply(request.PassportNumber, v => client.PassportNumber = v);
            Apply(request.Phone, v => client.Phone = v);
            Apply(request.Email, v => client.Email = v);
            Apply(request.Login, v => client.Login = v);
            Apply(request.Password, v => client.Password = v);

            if (request.CompanyId.HasValue)
            {
                await _repository.GetOrThrowAsync<CompanyEntity>(
                    request.CompanyId.Value,
                    cancellationToken);

                client.CompanyId = request.CompanyId.Value;
            }

            await _repository.SaveChangesAsync(cancellationToken);

            // 5. Инвалидация кэша
            await _cache.RemoveAsync(CacheKeys.Client(request.Id));
        }


        private static void Apply<T>(T? value, Action<T> setter)
        {
            if (value is not null)
                setter(value);
        }
    }

    public class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
    {
        public static UpdateClientCommandValidator Default { get; } = new();

        public UpdateClientCommandValidator()
        {
            // FullName
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name is too long")
                .When(x => x.FullName is not null);

            //PassportSeries
            RuleFor(x => x.PassportSeries)
               .Matches("^[0-9]{4}$").WithMessage("Passport series must be exactly 4 digits")
               .When(x => x.PassportSeries is not null);

            //PassportNumber
            RuleFor(x => x.PassportNumber)
               .Matches("^[0-9]{6}$").WithMessage("Passport number must be exactly 6 digits")
               .When(x => x.PassportNumber is not null);

            //Phone
            RuleFor(x => x.Phone)
                .Matches(@"^\+?[0-9]{7,15}$")
                .WithMessage("Phone number must contain 7–15 digits and may start with +")
                .When(x => x.Phone is not null);

            //Email
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email format is invalid")
                .When(x => x.Email is not null);

            //Login
            RuleFor(x => x.Login)
                .MinimumLength(3).WithMessage("Login must be at least 3 characters")
                .MaximumLength(50).WithMessage("Login is too long")
                .When(x => x.Login is not null);

            //Password
            RuleFor(x => x.Password)
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password is too long")
                .When(x => x.Password is not null);
        }
    }
}
