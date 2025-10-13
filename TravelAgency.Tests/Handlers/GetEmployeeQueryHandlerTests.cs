using AutoMapper;
using Moq;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Application.Queries.GetEmployee;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;
using FluentAssertions;

namespace TravelAgency.Tests.Handlers
{
    public class GetEmployeeQueryHandlerTests
    {
        private readonly Mock<IRedisCacheService> _cacheMock;
        private readonly Mock<IRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly GetEmployeeQueryHandler _handler;
        private static readonly TimeSpan ExpectedCacheExpiry = TimeSpan.FromMinutes(10);

        public GetEmployeeQueryHandlerTests()
        {
            _cacheMock = new Mock<IRedisCacheService>();
            _repositoryMock = new Mock<IRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetEmployeeQueryHandler(_repositoryMock.Object, _mapperMock.Object, _cacheMock.Object);
        }

        /// <summary>
        /// Юнит-тест для <see cref="GetEmployeeQueryHandler"/>, проверяющий поведение при отсутствии данных в кэше.
        /// </summary>
        /// <remarks>
        /// Тест имитирует ситуацию, когда кэш не содержит нужного значения, и вызывается делегат factory.
        /// Проверяется, что:
        /// - Репозиторий запрашивает сущность сотрудника.
        /// - Сущность корректно маппится в DTO.
        /// - Возвращаемый результат соответствует ожидаемому DTO.
        /// - Метод кэширования вызывается один раз.
        /// </remarks>
        /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
        [Fact]
        public async Task Handle_ShouldReturnEmployeeDto_WhenCacheMiss()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var query = new GetEmployeeQuery(employeeId);
            var entity = new EmployeeEntity { Id = employeeId, FullName = "John Doe" };
            var dto = new EmployeeDto { Id = employeeId, FullName = "John Doe" };
            var expectedExpiry = TimeSpan.FromMinutes(10);

            _cacheMock
                .Setup(c => c.TryGetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<EmployeeDto>>>(),
                    It.Is<TimeSpan>(expiry => expiry == ExpectedCacheExpiry)))
                .Returns<string, Func<Task<EmployeeDto>>, TimeSpan>((key, factory, _) => factory());

            _repositoryMock
                .Setup(r => r.GetOrThrowAsync<EmployeeEntity>(employeeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<EmployeeDto>(entity))
                .Returns(dto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(dto);

            // Дополнительные проверки:
            result.Id.Should().Be(employeeId);
            result.FullName.Should().Be("John Doe");

            _repositoryMock.Verify(r => r.GetOrThrowAsync<EmployeeEntity>(employeeId, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<EmployeeDto>(entity), Times.Once);

            // Проверка что кэш был вызван с правильными параметрами
            _cacheMock.Verify(c => c.TryGetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<EmployeeDto>>>(),
                It.Is<TimeSpan>(expiry => expiry == ExpectedCacheExpiry)), Times.Once);
        }

        /// <summary>
        /// Юнит-тест для <see cref="GetEmployeeQueryHandler"/>, проверяющий поведение при наличии данных в кэше.
        /// </summary>
        /// <remarks>
        /// Тест имитирует ситуацию, когда кэш уже содержит нужный DTO.
        /// Проверяется, что:
        /// - Возвращается закэшированный объект без обращения к репозиторию.
        /// - Маппинг не выполняется, так как данные уже готовы.
        /// - Метод кэширования вызывается один раз и возвращает ожидаемый результат.
        /// </remarks>
        /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
        [Fact]
        public async Task Handle_ShouldReturnCachedEmployee_WhenCacheHit()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var query = new GetEmployeeQuery(employeeId);
            var cachedDto = new EmployeeDto { Id = employeeId, FullName = "Cached John" };
            var expectedExpiry = ExpectedCacheExpiry;

            _cacheMock
                .Setup(c => c.TryGetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<EmployeeDto>>>(),
                    It.Is<TimeSpan>(expiry => expiry == ExpectedCacheExpiry)))
                .ReturnsAsync(cachedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(cachedDto);

            // Репозиторий и маппер не должны вызываться при кэш-хите
            _repositoryMock.Verify(r => r.GetOrThrowAsync<EmployeeEntity>(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
            _mapperMock.Verify(m => m.Map<EmployeeDto>(It.IsAny<EmployeeEntity>()), Times.Never);
        }

        /// <summary>
        /// Юнит-тест для <see cref="GetEmployeeQueryHandler"/>, проверяющий корректную передачу <see cref="CancellationToken"/> в репозиторий.
        /// </summary>
        /// <remarks>
        /// Тест имитирует кэш-мисс и проверяет, что токен отмены, переданный в handler,
        /// передаётся дальше в метод репозитория <c>GetOrThrowAsync</c>.
        /// Это важно для поддержки отмены долгих операций, особенно при работе с БД или внешними сервисами.
        /// </remarks>
        /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
        [Fact]
        public async Task Handle_ShouldPassCancellationToken_ToRepository()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var query = new GetEmployeeQuery(employeeId);
            using var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token; // "Живой" токен

            var entity = new EmployeeEntity { Id = employeeId };
            var dto = new EmployeeDto { Id = employeeId };

            _cacheMock
                .Setup(c => c.TryGetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<EmployeeDto>>>(),
                    It.IsAny<TimeSpan>()))
                .Returns<string, Func<Task<EmployeeDto>>, TimeSpan>((key, factory, _) => factory());

            _repositoryMock
                .Setup(r => r.GetOrThrowAsync<EmployeeEntity>(employeeId, cancellationToken))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<EmployeeDto>(entity))
                .Returns(dto);

            // Act
            await _handler.Handle(query, cancellationToken);

            // Assert
            _repositoryMock.Verify(r => r.GetOrThrowAsync<EmployeeEntity>(
                employeeId, cancellationToken), Times.Once);
        }

        /// <summary>
        /// Юнит-тест для <see cref="GetEmployeeQueryHandler"/>, проверяющий поведение при отсутствии сотрудника в базе данных.
        /// </summary>
        /// <remarks>
        /// Тест имитирует ситуацию, когда кэш не содержит данных, и репозиторий выбрасывает исключение <see cref="KeyNotFoundException"/>.
        /// Проверяется, что:
        /// - Исключение действительно выбрасывается при попытке получить несуществующего сотрудника.
        /// - Сообщение исключения соответствует ожидаемому.
        /// </remarks>
        /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenEmployeeNotFound()
        {
            // Arrange
            var employeeId = Guid.NewGuid();
            var query = new GetEmployeeQuery(employeeId);

            _cacheMock
                .Setup(c => c.TryGetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<EmployeeDto>>>(),
                    It.IsAny<TimeSpan>()))
                .Returns<string, Func<Task<EmployeeDto>>, TimeSpan>((key, factory, _) => factory());

            _repositoryMock
                .Setup(r => r.GetOrThrowAsync<EmployeeEntity>(employeeId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Employee not found"));

            // Act & Assert
            var act = () => _handler.Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Employee not found");
        }
    }
}
