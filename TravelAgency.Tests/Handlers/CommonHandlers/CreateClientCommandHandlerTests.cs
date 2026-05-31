using AutoMapper;
using FluentAssertions;
using Moq;
using TravelAgency.Application.Commands.Client;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;

namespace TravelAgency.Tests.Handlers.CommonHandlers;

public class CreateClientCommandHandlerTests
{
    private readonly Mock<IRepository> _repositoryMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    private CreateClientCommandHandler Handler =>
           new(_repositoryMock.Object, _mapperMock.Object);

    /// <summary>
    /// Юнит-тест для <see cref="CreateClientCommandHandler"/>, проверяющий успешное создание клиента.
    /// </summary>
    /// <remarks>
    /// Проверяется, что:
    /// - Команда проходит валидацию.
    /// - Выполняется маппинг в сущность.
    /// - Генерируется новый идентификатор.
    /// - Сущность сохраняется в репозиторий.
    /// </remarks>

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateClientAndReturnId()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        ClientEntity? savedEntity = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ClientEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ClientEntity, CancellationToken>((entity, _) => savedEntity = entity)
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock
                .Setup(m => m.Map<ClientEntity>(It.IsAny<CreateClientCommand>()))
                .Returns((CreateClientCommand cmd) => new ClientEntity
                {
                    FullName = cmd.FullName,
                    PassportSeries = cmd.PassportSeries,
                    PassportNumber = cmd.PassportNumber,
                    Phone = cmd.Phone,
                    Email = cmd.Email,
                    Login = cmd.Login,
                    Password = cmd.Password
                });

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        savedEntity.Should().NotBeNull();
        result.Should().Be(savedEntity!.Id);
        savedEntity.Id.Should().NotBe(Guid.Empty);

        // Проверка что клиент создан с правильными данными
        savedEntity!.FullName.Should().Be(command.FullName);
        savedEntity.PassportSeries.Should().Be(command.PassportSeries);
        savedEntity.PassportNumber.Should().Be(command.PassportNumber);
        savedEntity.Phone.Should().Be(command.Phone);
        savedEntity.Email.Should().Be(command.Email);
        savedEntity.Login.Should().Be(command.Login);
        savedEntity.Password.Should().Be(command.Password);


        // Проверка вызовов репозитория
        _repositoryMock.Verify(r => r.AddAsync(savedEntity, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Тест проверяет, что при создании клиента генерируется новый уникальный идентификатор.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldGenerateNewGuid_WhenCreatingClient()
    {
        var command = new CreateClientCommand
           (
               "Fernando Alonso",
               "1234",
               "567890",
               "+1234567",
               "fernando@mail.com",
               "fernando",
               "password"
           );

        Guid? generatedId = null;

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ClientEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ClientEntity, CancellationToken>((entity, _) => generatedId = entity.Id)
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock
                .Setup(m => m.Map<ClientEntity>(It.IsAny<CreateClientCommand>()))
                .Returns((CreateClientCommand cmd) => new ClientEntity
                {
                    FullName = cmd.FullName,
                    PassportSeries = cmd.PassportSeries,
                    PassportNumber = cmd.PassportNumber,
                    Phone = cmd.Phone,
                    Email = cmd.Email,
                    Login = cmd.Login,
                    Password = cmd.Password
                });

        // Act
        var result = await Handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().NotBe(Guid.Empty);

        generatedId.Should().NotBeNull();
        result.Should().Be(generatedId.Value);
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение, если FullName пустой.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyFullName_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
          (
              "",
              "1234",
              "567890",
              "+1234567",
              "fernando@mail.com",
              "fernando",
              "password"
          );

        // Act & Assert
        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Full name is required*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение, если FullName превышает максимальную длину.
    /// </summary>
    [Fact]
    public async Task Handle_WithFullNameExceedingMaxLength_ShouldThrowValidationException()
    {
        // Arrange
        var command = new CreateClientCommand
            (
                new string('A', 101), // 101 символ (максимум 100)
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        // Act & Assert
        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Full name is too long*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение, если PassportSeries пустой.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyPassportSeries_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        // Act 
        var act = () => Handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Passport series is required*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если PassportSeries содержит больше чем 4 символа.
    /// </summary>
    [Fact]
    public async Task Handle_WithPassportSeriesLongerThan4Characters_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "12345",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        // Act 
        var act = () => Handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Passport series must contain exactly 4 characters*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если PassportSeries содержит меньше чем 4 символа.
    /// </summary>
    [Fact]
    public async Task Handle_WithPassportSeriesShorterThan4Characters_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "123",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        // Act 
        var act = () => Handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Passport series must contain exactly 4 characters*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если PassportSeries содержит недопустимые символы (не цифры).
    /// </summary>
    [Fact]
    public async Task Handle_WithPassportSeriesContainingNonDigits_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "12A4", // содержит букву
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        // Act
        var act = () => Handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Passport series must contain only digits*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение, если PassportNumber пустой.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyPassportNumber_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        // Act 
        var act = () => Handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Passport number is required*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если PassportNumber содержит больше чем 6 символов.
    /// </summary>
    [Fact]
    public async Task Handle_WithPassportNumberLongerThan6Characters_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "5678901", // 7 символов
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Passport number must contain exactly 6 characters*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если PassportNumber содержит меньше чем 6 символов.
    /// </summary>
    [Fact]
    public async Task Handle_WithPassportNumberShorterThan6Characters_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "56789", // 5 символов
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Passport number must contain exactly 6 characters*");
    }


    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если PassportNumber содержит недопустимые символы (не цифры).
    /// </summary>
    [Fact]
    public async Task Handle_WithPassportNumberContainingNonDigits_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "56A890", // буква A
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Passport number must contain only digits*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Phone является пустым.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyPhone_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Phone number is required*");

    }


    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Phone содержит меньше чем 7 цифр.
    /// </summary>
    [Fact]
    public async Task Handle_WithPhoneShorterThan7Digits_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+123456", // 6 цифр
                "fernando@mail.com",
                "fernando",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Phone number must contain 7–15 digits and may start with +*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Phone содержит недопустимые символы (не цифры или +).
    /// </summary>
    [Fact]
    public async Task Handle_WithPhoneContainingNonDigitCharacters_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+12345A789", // буква A
                "fernando@mail.com",
                "fernando",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Phone number must contain 7–15 digits and may start with +*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Email является пустым.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyEmail_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "",
                "fernando",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Email is required*");
    }


    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Email имеет неверный формат.
    /// </summary>
    [Fact]
    public async Task Handle_WithInvalidEmailFormat_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "invalid-email-format",
                "fernando",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Email format is invalid*");
    }


    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Login является пустым.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyLogin_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "",
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Login is required*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Login содержит меньше чем 3 символа.
    /// </summary>
    [Fact]
    public async Task Handle_WithLoginShorterThan3Characters_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "ab", // 2 символа
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Login must be at least 3 characters*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Login превышает максимальную длину в 50 символов.
    /// </summary>
    [Fact]
    public async Task Handle_WithLoginLongerThan50Characters_ShouldThrowValidationException()
    {
        var longLogin = new string('a', 51);

        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                longLogin,
                "password"
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Login is too long*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Password является пустым.
    /// </summary>
    [Fact]
    public async Task Handle_WithEmptyPassword_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                ""
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Password is required*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Password содержит меньше чем 6 символов.
    /// </summary>
    [Fact]
    public async Task Handle_WithPasswordShorterThan6Characters_ShouldThrowValidationException()
    {
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "12345" // 5 символов
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Password must be at least 6 characters*");
    }

    /// <summary>
    /// Тест проверяет, что при валидации выбрасывается исключение,
    /// если Password превышает максимальную длину в 100 символов.
    /// </summary>
    [Fact]
    public async Task Handle_WithPasswordLongerThan100Characters_ShouldThrowValidationException()
    {
        var longPassword = new string('a', 101);

        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                longPassword
            );

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Password is too long*");
    }

    /// <summary>
    /// Тест проверяет, что CancellationToken передаётся в репозиторий.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<ClientEntity>(), cancellationToken))
                .Returns(Task.CompletedTask);

        _repositoryMock
               .Setup(r => r.SaveChangesAsync(cancellationToken))
               .Returns(Task.CompletedTask);

        _mapperMock
                .Setup(m => m.Map<ClientEntity>(It.IsAny<CreateClientCommand>()))
                .Returns((CreateClientCommand cmd) => new ClientEntity
                {
                    FullName = cmd.FullName,
                    PassportSeries = cmd.PassportSeries,
                    PassportNumber = cmd.PassportNumber,
                    Phone = cmd.Phone,
                    Email = cmd.Email,
                    Login = cmd.Login,
                    Password = cmd.Password
                });

        // Act
        await Handler.Handle(command, cancellationToken);

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<ClientEntity>(), cancellationToken), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(cancellationToken), Times.Once);
    }

    /// <summary>
    /// Тест проверяет, что при возникновении ошибки в репозитории (например, при сохранении) 
    /// исключение пробрасывается наверх, и не происходит некорректной обработки.
    /// </summary>
    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateClientCommand
            (
                "Fernando Alonso",
                "1234",
                "567890",
                "+1234567",
                "fernando@mail.com",
                "fernando",
                "password"
            );

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ClientEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        _mapperMock
            .Setup(m => m.Map<ClientEntity>(It.IsAny<CreateClientCommand>()))
            .Returns((CreateClientCommand cmd) => new ClientEntity
            {
                FullName = cmd.FullName,
                PassportSeries = cmd.PassportSeries,
                PassportNumber = cmd.PassportNumber,
                Phone = cmd.Phone,
                Email = cmd.Email,
                Login = cmd.Login,
                Password = cmd.Password
            });

        // Act
        var act = () => Handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database connection failed");
    }

    /// <summary>
    /// Тест проверяет, что при создании нескольких клиентов подряд,
    /// каждый получает уникальный идентификатор.
    /// </summary>
    [Fact]
    public async Task Handle_WhenCreatingMultipleClients_ShouldGenerateUniqueIds()
    {
        // Arrange
        var commands = new List<CreateClientCommand>
    {
        new("Client 1", "1234", "567890", "+1234567", "c1@mail.com", "client1", "pass11"),
        new("Client 2", "1234", "567890", "+1234567", "c2@mail.com", "client2", "pass22"),
        new("Client 3", "1234", "567890", "+1234567", "c3@mail.com", "client3", "pass33")
    };

        var createdIds = new List<Guid>();

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<ClientEntity>(), It.IsAny<CancellationToken>()))
            .Callback<ClientEntity, CancellationToken>((entity, _) => createdIds.Add(entity.Id))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mapperMock
            .Setup(m => m.Map<ClientEntity>(It.IsAny<CreateClientCommand>()))
            .Returns((CreateClientCommand cmd) => new ClientEntity
            {
                FullName = cmd.FullName,
                PassportSeries = cmd.PassportSeries,
                PassportNumber = cmd.PassportNumber,
                Phone = cmd.Phone,
                Email = cmd.Email,
                Login = cmd.Login,
                Password = cmd.Password
            });

        // Act
        foreach (var command in commands)
        {
            await Handler.Handle(command, CancellationToken.None);
        }

        // Assert
        createdIds.Should().HaveCount(3);
        createdIds.Should().OnlyHaveUniqueItems();
        createdIds.Should().AllSatisfy(id => id.Should().NotBe(Guid.Empty));
    }

}