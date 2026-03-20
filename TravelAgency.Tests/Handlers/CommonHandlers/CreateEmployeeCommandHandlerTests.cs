using FluentAssertions;
using Moq;
using TravelAgency.Application.Commands.Employee;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;

namespace TravelAgency.Tests.Handlers.CommonHandlers
{
    public class CreateEmployeeCommandHandlerTests
    {
        private readonly Mock<IRedisCacheService> _cacheMock;
        private readonly Mock<IRepository> _repositoryMock;
        private readonly CreateEmployeeCommandHandler _handler;


        public CreateEmployeeCommandHandlerTests()
        {
            _cacheMock = new Mock<IRedisCacheService>();
            _repositoryMock = new Mock<IRepository>();
            _handler = new CreateEmployeeCommandHandler(_repositoryMock.Object, _cacheMock.Object);
        }

        /// <summary>
        /// Тест проверяет успешное создание сотрудника при корректных данных.
        /// </summary>
        /// <remarks>
        /// Проверяет:
        /// - Сотрудник успешно создаётся и сохраняется в БД
        /// - Возвращается корректный идентификатор
        /// - Все необходимые методы репозитория вызываются
        /// - Данные сотрудника соответствуют переданным в команде
        /// </remarks>
        [Fact]
        public async Task Handle_WithValidCommand_ShouldCreateEmployeeAndReturnId()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = "johndoe.com",
                Password = "password123",
                Status = "Active"
            };

            EmployeeEntity? savedEntity = null;

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<EmployeeEntity>(), It.IsAny<CancellationToken>()))
                .Callback<EmployeeEntity, CancellationToken>((entity, _) => savedEntity = entity)
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            // Сначала проверяем, что savedEntity не null
            savedEntity.Should().NotBeNull();

            // Теперь безопасно используем savedEntity
            result.Should().NotBeEmpty();
            result.Should().Be(savedEntity!.Id);

            // Проверка что сотрудник создан с правильными данными
            savedEntity!.FullName.Should().Be(command.FullName);
            savedEntity.Login.Should().Be(command.Login);
            savedEntity.Password.Should().Be(command.Password);
            savedEntity.Status.Should().Be(command.Status);
            savedEntity.Id.Should().NotBeEmpty();

            // Проверка вызовов репозитория
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<EmployeeEntity>(), It.IsAny<CancellationToken>()), Times.Once);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

            // Кэш не должен очищаться при создании (это будет в тестах Update/Delete)
            _cacheMock.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Тест проверяет, что при создании сотрудника генерируется новый уникальный идентификатор.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldGenerateNewGuid_WhenCreatingEmployee()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "Jane Smith",
                Login = "janesmith",
                Password = "pass456",
                Status = "Active"
            };

            Guid? generatedId = null;

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<EmployeeEntity>(), It.IsAny<CancellationToken>()))
                .Callback<EmployeeEntity, CancellationToken>((entity, _) => generatedId = entity.Id)
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

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
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "",
                Login = "johndoe",
                Password = "password123",
                Status = "Active"
            };

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

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
            var command = new CreateEmployeeCommand
            {
                FullName = new string('A', 101), // 101 символ (максимум 100)
                Login = "johndoe",
                Password = "password123",
                Status = "Active"
            };

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Full name is too long*");
        }
        
        /// <summary>
        /// Тест проверяет, что при валидации выбрасывается исключение, если Login пустой.
        /// </summary>
        [Fact]
        public async Task Handle_WithEmptyLogin_ShouldThrowValidationException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = "",
                Password = "password123",
                Status = "Active"
            };

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Login is required*");
        }

        /// <summary>
        /// Тест проверяет, что при валидации выбрасывается исключение, если Login короче минимальной длины.
        /// </summary>
        [Fact]
        public async Task Handle_WithLoginTooShort_ShouldThrowValidationException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = "ab", // 2 символа (минимум 3)
                Password = "password123",
                Status = "Active"
            };

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Login must be at least 3 characters*");
        }

        /// <summary>
        /// Тест проверяет, что при валидации выбрасывается исключение, если Login превышает максимальную длину.
        /// </summary>
        [Fact]
        public async Task Handle_WithLoginExceedingMaxLength_ShouldThrowValidationException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = new string('A', 51), // 51 символ (максимум 50)
                Password = "password123",
                Status = "Active"
            };

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Login is too long*");
        }

        /// <summary>
        /// Тест проверяет, что при валидации выбрасывается исключение, если Password пустой.
        /// </summary>
        [Fact]
        public async Task Handle_WithEmptyPassword_ShouldThrowValidationException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = "johndoe",
                Password = "",
                Status = "Active"
            };

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Password is required*");
        }
        
        /// <summary>
        /// Тест проверяет, что при валидации выбрасывается исключение, если Password короче минимальной длины.
        /// </summary>
        [Fact]
        public async Task Handle_WithPasswordTooShort_ShouldThrowValidationException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = "johndoe",
                Password = "12345", // 5 символов (минимум 6)
                Status = "Active"
            };

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Password must be at least 6 characters*");
        }
     
        /// <summary>
        /// Тест проверяет, что при валидации выбрасывается исключение, если Status пустой.
        /// </summary>
        [Fact]
        public async Task Handle_WithEmptyStatus_ShouldThrowValidationException()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = "johndoe",
                Password = "password123",
                Status = ""
            };

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Status is required*");
        }

        /// <summary>
        /// Тест проверяет, что CancellationToken передаётся в репозиторий.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldPassCancellationToken_ToRepository()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = "johndoe",
                Password = "password123",
                Status = "Active"
            };

            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<EmployeeEntity>(), cancellationToken))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(cancellationToken))
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, cancellationToken);

            // Assert
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<EmployeeEntity>(), cancellationToken), Times.Once);
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
            var command = new CreateEmployeeCommand
            {
                FullName = "John Doe",
                Login = "johndoe",
                Password = "password123",
                Status = "Active"
            };

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<EmployeeEntity>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Database connection failed"));

            // Act & Assert
            var act = () => _handler.Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Database connection failed");
        }
        
        /// <summary>
        /// Тест проверяет, что команда с валидными данными, содержащими пробелы и специальные символы, 
        /// корректно обрабатывается.
        /// </summary>
        [Fact]
        public async Task Handle_WithValidCommandContainingSpecialCharacters_ShouldCreateEmployee()
        {
            // Arrange
            var command = new CreateEmployeeCommand
            {
                FullName = "John O'Conner-Smith Jr.",
                Login = "john.o'conner-smith",
                Password = "P@ssw0rd!123",
                Status = "Active-Pending"
            };

            EmployeeEntity? savedEntity = null;

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<EmployeeEntity>(), It.IsAny<CancellationToken>()))
                .Callback<EmployeeEntity, CancellationToken>((entity, _) => savedEntity = entity)
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeEmpty();
            savedEntity.Should().NotBeNull();
            savedEntity!.FullName.Should().Be(command.FullName);
            savedEntity.Login.Should().Be(command.Login);
            savedEntity.Password.Should().Be(command.Password);
            savedEntity.Status.Should().Be(command.Status);
        }
        
        /// <summary>
        /// Тест проверяет, что при создании нескольких сотрудников подряд, 
        /// каждый получает уникальный идентификатор.
        /// </summary>
        [Fact]
        public async Task Handle_WhenCreatingMultipleEmployees_ShouldGenerateUniqueIds()
        {
            // Arrange
            var commands = new List<CreateEmployeeCommand>
            {
                new() { FullName = "Employee 1", Login = "emp1", Password = "pass11", Status = "Active" },
                new() { FullName = "Employee 2", Login = "emp2", Password = "pass22", Status = "Active" },
                new() { FullName = "Employee 3", Login = "emp3", Password = "pass33", Status = "Inactive" }
            };

            var createdIds = new List<Guid>();

            _repositoryMock
                .Setup(r => r.AddAsync(It.IsAny<EmployeeEntity>(), It.IsAny<CancellationToken>()))
                .Callback<EmployeeEntity, CancellationToken>((entity, _) => createdIds.Add(entity.Id))
                .Returns(Task.CompletedTask);

            _repositoryMock
                .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            foreach (var command in commands)
            {
                await _handler.Handle(command, CancellationToken.None);
            }

            // Assert
            createdIds.Should().HaveCount(3);
            createdIds.Should().OnlyHaveUniqueItems();
            createdIds.Should().AllSatisfy(id => id.Should().NotBe(Guid.Empty));
        }
        
    }
}


