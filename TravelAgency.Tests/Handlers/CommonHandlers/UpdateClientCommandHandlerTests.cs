using FluentAssertions;
using Moq;
using TravelAgency.Application.Commands.Client;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Shared.Constants;


namespace TravelAgency.Tests.Handlers.CommonHandlers;

public class UpdateClientCommandHandlerTests
{
    private readonly Mock<IRepository> _repositoryMock = new();
    private readonly Mock<IRedisCacheService> _cacheMock = new();

    private UpdateClientCommandHandler Handler =>
        new(_repositoryMock.Object, _cacheMock.Object);

    //Ещё вариант — сделать его фабрикой с базовыми значениями и переопределять нужные поля,
    private static UpdateClientCommand CreateCommand(
        Guid? id = null,
        string? fullName = null,
        string? passportSeries = null,
        string? passportNumber = null,
        string? phone = null,
        string? email = null,
        string? login = null,
        string? password = null,
        Guid? companyId = null)
    {
        return new UpdateClientCommand(
            id ?? Guid.NewGuid(),
            fullName,
            passportSeries,
            passportNumber,
            phone,
            email,
            login,
            password,
            companyId);
    }

    /// <summary>
    /// Проверяет успешное обновление клиента.
    /// </summary>
    /// <remarks>
    /// Проверяется, что обработчик:
    /// - загружает клиента из репозитория;
    /// - обновляет только поля, переданные в команде;
    /// - сохраняет изменения;
    /// - удаляет кэш клиента.
    /// </remarks>
    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateClientSaveChangesAndInvalidateCache()
    {
        var clientId = Guid.NewGuid();
        var command = CreateCommand(
            id: clientId,
            fullName: "Fernando Alonso",
            email: "fernando@mail.com");

        var client = new ClientEntity
        {
            Id = clientId,
            FullName = "Old name",
            Email = "old@mail.com",
            Login = "fernando",
            Password = "password"
        };

        _repositoryMock
            .Setup(r => r.GetOrThrowAsync<ClientEntity>(
                clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveAsync(CacheKeys.Client(clientId)))
            .Returns(Task.CompletedTask);

        await Handler.Handle(command, CancellationToken.None);

        client.FullName.Should().Be("Fernando Alonso");
        client.Email.Should().Be("fernando@mail.com");
        client.Login.Should().Be("fernando"); // null-поля не изменяются
        client.Password.Should().Be("password");

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _cacheMock.Verify(
            c => c.RemoveAsync(CacheKeys.Client(clientId)), Times.Once);
    }

    /// <summary>
    /// Проверяет, что при обновлении клиента с идентификатором компании
    /// обработчик проверяет наличие компании и устанавливает её идентификатор клиенту.
    /// </summary>
    [Fact]
    public async Task Handle_WithCompanyId_ShouldCheckCompanyAndUpdateClientCompanyId()
    {
        var clientId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var command = CreateCommand(id: clientId, companyId: companyId);
        var client = new ClientEntity { Id = clientId };

        _repositoryMock
            .Setup(r => r.GetOrThrowAsync<ClientEntity>(
                clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _repositoryMock
            .Setup(r => r.GetOrThrowAsync<CompanyEntity>(
                companyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CompanyEntity());

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveAsync(CacheKeys.Client(clientId)))
            .Returns(Task.CompletedTask);

        await Handler.Handle(command, CancellationToken.None);

        client.CompanyId.Should().Be(companyId);

        _repositoryMock.Verify(
            r => r.GetOrThrowAsync<CompanyEntity>(
                companyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Проверяет, что если идентификатор компании не передан,
    /// обработчик не загружает компанию из репозитория.
    /// </summary>
    [Fact]
    public async Task Handle_WithoutCompanyId_ShouldNotLoadCompany()
    {
        var clientId = Guid.NewGuid();
        var command = CreateCommand(id: clientId);
        var client = new ClientEntity { Id = clientId };

        _repositoryMock
            .Setup(r => r.GetOrThrowAsync<ClientEntity>(
                clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveAsync(CacheKeys.Client(clientId)))
            .Returns(Task.CompletedTask);

        await Handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.GetOrThrowAsync<CompanyEntity>(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Проверяет, что пустое имя клиента или имя длиннее допустимого
    /// вызывает исключение валидации.
    /// </summary>
    /// <remarks>
    /// Также проверяется, что при ошибке валидации обработчик не загружает клиента из репозитория.
    /// </remarks>
    [Theory]
    [InlineData("", "Full name is required")]
    [InlineData("A name longer than the allowed limit........................................................................", "Full name is too long")]
    public async Task Handle_WithInvalidFullName_ShouldThrowValidationException(
        string fullName,
        string expectedMessage)
    {
        var command = CreateCommand(fullName: fullName);

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage($"*{expectedMessage}*");

        _repositoryMock.Verify(
            r => r.GetOrThrowAsync<ClientEntity>(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Проверяет, что серия паспорта неверного формата вызывает исключение валидации.
    /// </summary>
    /// <remarks>
    /// Проверяются значения короче и длиннее четырёх цифр, а также значение,
    /// содержащее нецифровой символ.
    /// </remarks>
    [Theory]
    [InlineData("123")]
    [InlineData("12345")]
    [InlineData("12a4")]
    public async Task Handle_WithInvalidPassportSeries_ShouldThrowValidationException(
        string passportSeries)
    {
        var command = CreateCommand(passportSeries: passportSeries);

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Passport series must be exactly 4 digits*");
    }

    /// <summary>
    /// Проверяет, что номер паспорта неверного формата вызывает исключение валидации.
    /// </summary>
    /// <remarks>
    /// Проверяются значения короче и длиннее шести цифр, а также значение,
    /// содержащее нецифровой символ.
    /// </remarks>
    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("12a456")]
    public async Task Handle_WithInvalidPassportNumber_ShouldThrowValidationException(
        string passportNumber)
    {
        var command = CreateCommand(passportNumber: passportNumber);

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Passport number must be exactly 6 digits*");
    }

    /// <summary>
    /// Проверяет, что телефон неверного формата вызывает исключение валидации.
    /// </summary>
    /// <remarks>
    /// Проверяются слишком короткий номер и номер, содержащий недопустимые символы.
    /// </remarks>
    [Theory]
    [InlineData("123")]
    [InlineData("abc1234567")]
    public async Task Handle_WithInvalidPhone_ShouldThrowValidationException(string phone)
    {
        var command = CreateCommand(phone: phone);

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Phone number must contain 7–15 digits*");
    }

    /// <summary>
    /// Проверяет, что адрес электронной почты неверного формата вызывает исключение валидации.
    /// </summary>
    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldThrowValidationException()
    {
        var command = CreateCommand(email: "not-an-email");

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Email format is invalid*");
    }

    /// <summary>
    /// Проверяет, что логин короче трёх символов вызывает исключение валидации.
    /// </summary>
    [Fact]
    public async Task Handle_WithShortLogin_ShouldThrowValidationException()
    {
        var command = CreateCommand(login: "ab");

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Login must be at least 3 characters*");
    }

    /// <summary>
    /// Проверяет, что пароль короче шести символов вызывает исключение валидации.
    /// </summary>
    [Fact]
    public async Task Handle_WithShortPassword_ShouldThrowValidationException()
    {
        var command = CreateCommand(password: "12345");

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<FluentValidation.ValidationException>()
            .WithMessage("*Password must be at least 6 characters*");
    }

    /// <summary>
    /// Проверяет, что обработчик передаёт исключение, возникшее при поиске клиента.
    /// </summary>
    /// <remarks>
    /// Также проверяется, что при ошибке изменения не сохраняются и кэш не удаляется.
    /// </remarks>
    [Fact]
    public async Task Handle_WhenClientLookupFails_ShouldPropagateException()
    {
        var command = CreateCommand();

        _repositoryMock
            .Setup(r => r.GetOrThrowAsync<ClientEntity>(
                command.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Client not found"));

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Client not found");

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _cacheMock.Verify(
            c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
    }

    /// <summary>
    /// Проверяет, что обработчик передаёт исключение, возникшее при поиске компании.
    /// </summary>
    /// <remarks>
    /// Также проверяется, что изменения клиента не сохраняются, если компания не найдена.
    /// </remarks>
    [Fact]
    public async Task Handle_WhenCompanyLookupFails_ShouldPropagateException()
    {
        var clientId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var command = CreateCommand(id: clientId, companyId: companyId);

        _repositoryMock
            .Setup(r => r.GetOrThrowAsync<ClientEntity>(
                clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ClientEntity { Id = clientId });

        _repositoryMock
            .Setup(r => r.GetOrThrowAsync<CompanyEntity>(
                companyId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Company not found"));

        var act = () => Handler.Handle(command, CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Company not found");

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Проверяет, что обработчик передаёт токен отмены в вызовы репозитория.
    /// </summary>
    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToRepository()
    {
        var clientId = Guid.NewGuid();
        var command = CreateCommand(id: clientId);
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _repositoryMock
            .Setup(r => r.GetOrThrowAsync<ClientEntity>(clientId, token))
            .ReturnsAsync(new ClientEntity { Id = clientId });

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(token))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveAsync(CacheKeys.Client(clientId)))
            .Returns(Task.CompletedTask);

        await Handler.Handle(command, token);

        _repositoryMock.Verify(
            r => r.GetOrThrowAsync<ClientEntity>(clientId, token), Times.Once);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(token), Times.Once);
    }
}