using FluentAssertions;
using Moq;
using TravelAgency.Application.Commands.Employee;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Enums;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Shared.Exeptions;

namespace TravelAgency.Tests.Handlers.CommonHandlers
{
    public class UpdateEmployeeStatusCommandHandlerTests
    {
        private readonly Mock<IRedisCacheService> _cacheMock = new();
        private readonly Mock<IRepository> _repositoryMock = new();

        private UpdateEmployeeStatusCommandHandler Handler =>
            new(_repositoryMock.Object, _cacheMock.Object);

        /// <summary>
        /// Проверяет, что статус сотрудника успешно обновляется,
        /// изменения сохраняются в БД, а соответствующий ключ кеша удаляется.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldUpdateStatus_AndClearCache()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var employee = new EmployeeEntity { Id = employeeId, Status = EmployeeStatus.Active };

            _repositoryMock
                .Setup(r => r.GetOrThrowAsync<EmployeeEntity>(employeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(employee);

            var command = new UpdateEmployeeStatusCommand
            {
                Id = employeeId,
                Status = EmployeeStatus.Blocked
            };

            // Act
            await Handler.Handle(command, CancellationToken.None);

            // Assert
            employee.Status.Should().Be(EmployeeStatus.Blocked);

            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _cacheMock.Verify(c => c.RemoveAsync($"employee:{employeeId}"), Times.Once);
        }

        /// <summary>
        /// Проверяет, что при отсутствии сотрудника хендлер выбрасывает исключение,
        /// а операции сохранения и очистки кеша не выполняются.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrow_WhenEmployeeNotFound()
        {
            // Arrange
            var employeeId = Guid.NewGuid();

            _repositoryMock
                .Setup(r => r.GetOrThrowAsync<EmployeeEntity>(employeeId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new NotFoundException());

            var command = new UpdateEmployeeStatusCommand
            {
                Id = employeeId,
                Status = EmployeeStatus.Active
            };

            // Act
            Func<Task> act = async () => await Handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();

            _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
            _repositoryMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Проверяет, что при передаче пустого идентификатора
        /// хендлер выбрасывает ValidationException и не обращается к репозиторию.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenIdIsEmpty()
        {
            // Arrange
            var command = new UpdateEmployeeStatusCommand
            {
                Id = Guid.Empty,
                Status = EmployeeStatus.Active
            };

            // Act
            Func<Task> act = async () => await Handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*Id is required*");

            _repositoryMock.Verify(r => r.GetOrThrowAsync<EmployeeEntity>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Проверяет, что при передаче некорректного значения enum
        /// хендлер выбрасывает ValidationException и не выполняет никаких операций.
        /// </summary>
        [Fact]
        public async Task Handle_ShouldThrowValidationException_WhenStatusIsInvalid()
        {
            // Arrange
            var command = new UpdateEmployeeStatusCommand
            {
                Id = Guid.NewGuid(),
                Status = (EmployeeStatus)9 // невалидный enum
            };

            // Act
            Func<Task> act = async () => await Handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<FluentValidation.ValidationException>()
                .WithMessage("*The specified employee status does not exist*");

            _repositoryMock.Verify(r => r.GetOrThrowAsync<EmployeeEntity>(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _cacheMock.Verify(c => c.RemoveAsync(It.IsAny<string>()), Times.Never);
        }
    }

}
