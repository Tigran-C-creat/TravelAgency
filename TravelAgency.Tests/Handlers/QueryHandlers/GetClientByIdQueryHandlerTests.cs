using AutoMapper;
using FluentAssertions;
using Moq;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Application.Queries.GetClient;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;

namespace TravelAgency.Tests.Handlers.QueryHandlers
{
    public class GetClientByIdQueryHandlerTests
    {
        private readonly Mock<IRedisCacheService> _cacheMock;
        private readonly Mock<IRepository> _repositoryMock;
        private readonly Mock<IMapper> _mapperMock;

        private readonly GetClientByIdQueryHandler _handler;
        private static readonly TimeSpan ExpectedCacheExpiry = TimeSpan.FromHours(10);

        public GetClientByIdQueryHandlerTests()
        {
            _cacheMock = new Mock<IRedisCacheService>();
            _repositoryMock = new Mock<IRepository>();
            _mapperMock = new Mock<IMapper>();

            _handler = new GetClientByIdQueryHandler(
                _repositoryMock.Object,
                _mapperMock.Object,
                _cacheMock.Object);
        }

        /// <summary>
        /// Юнит-тест  для <see cref="GetClientByIdQueryHandler"/>, проверяющий поведение при отсутствии данных в кэше.
        /// </summary>
        /// <remarks>
        /// Проверяется, что:
        /// - Репозиторий вызывается один раз.
        /// - Выполняется маппинг сущности в DTO.
        /// - Результат возвращается корректно.
        /// - Метод кэширования вызывается с правильными параметрами.
        /// </remarks>
        [Fact]
        public async Task Handle_ShouldReturnClientDto_WhenCacheMiss()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            var entity = new ClientEntity { Id = clientId, FullName = "John Doe" };
            var dto = new ClientDto(clientId, "John Doe", "+1234567", "john@mail.com", "john", null);

            _cacheMock
                .Setup(c => c.TryGetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<ClientDto>>>(),
                    It.Is<TimeSpan>(expiry => expiry == ExpectedCacheExpiry)))
                .Returns<string, Func<Task<ClientDto>>, TimeSpan>((key, factory, _) => factory());

            _repositoryMock
                .Setup(r => r.GetOrThrowAsync<ClientEntity>(clientId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ClientDto>(entity))
                .Returns(dto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(dto);

            _repositoryMock.Verify(r => r.GetOrThrowAsync<ClientEntity>(clientId, It.IsAny<CancellationToken>()), Times.Once);
            _mapperMock.Verify(m => m.Map<ClientDto>(entity), Times.Once);

            _cacheMock.Verify(c => c.TryGetOrSetAsync(
                It.IsAny<string>(),
                It.IsAny<Func<Task<ClientDto>>>(),
                ExpectedCacheExpiry), Times.Once);
        }


        /// <summary>
        /// Юнит-тест для <see cref="GetClientByIdQueryHandler"/>, проверяющий поведение при наличии данных в кэше.
        /// </summary>
        /// <remarks>
        /// Проверяется, что:
        /// - Возвращается закэшированный объект.
        /// - Репозиторий не вызывается.
        /// - Маппинг не выполняется.
        /// </remarks>
        [Fact]
        public async Task Handle_ShouldReturnCachedClient_WhenCacheHit()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);
            var cachedDto = new ClientDto(clientId, "Cached John", "+1234567", "cached@mail.com", "cached", null);

            _cacheMock
                .Setup(c => c.TryGetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<ClientDto>>>(),
                    It.Is<TimeSpan>(expiry => expiry == ExpectedCacheExpiry)))
                .ReturnsAsync(cachedDto);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().BeEquivalentTo(cachedDto);

            _repositoryMock.Verify(r => r.GetOrThrowAsync<ClientEntity>(
                It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

            _mapperMock.Verify(m => m.Map<ClientDto>(It.IsAny<ClientEntity>()), Times.Never);
        }

        /// <summary>
        /// Юнит-тест для <see cref="GetClientByIdQueryHandler"/>, проверяющий передачу <see cref="CancellationToken"/> в репозиторий.
        /// </summary>
        /// <remarks>
        /// Проверяется, что:
        /// - Токен отмены, переданный в handler, передаётся в метод репозитория.
        /// </remarks>
        [Fact]
        public async Task Handle_ShouldPassCancellationToken_ToRepository()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            using var cts = new CancellationTokenSource();
            var token = cts.Token;

            var entity = new ClientEntity { Id = clientId };
            var dto = new ClientDto(clientId, "John", "+123", "mail@mail.com", "login", null);

            _cacheMock
                .Setup(c => c.TryGetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<ClientDto>>>(),
                    It.IsAny<TimeSpan>()))
                .Returns<string, Func<Task<ClientDto>>, TimeSpan>((key, factory, _) => factory());

            _repositoryMock
                .Setup(r => r.GetOrThrowAsync<ClientEntity>(clientId, token))
                .ReturnsAsync(entity);

            _mapperMock
                .Setup(m => m.Map<ClientDto>(entity))
                .Returns(dto);

            // Act
            await _handler.Handle(query, token);

            // Assert
            _repositoryMock.Verify(r => r.GetOrThrowAsync<ClientEntity>(clientId, token), Times.Once);
        }

        /// <summary>
        /// Юнит-тест для <see cref="GetClientByIdQueryHandler"/>, проверяющий поведение при отсутствии клиента в базе данных.
        /// </summary>
        /// <remarks>
        /// Проверяется, что:
        /// - Репозиторий выбрасывает исключение.
        /// - Исключение пробрасывается хендлером наружу.
        /// </remarks>
        [Fact]
        public async Task Handle_ShouldThrowException_WhenClientNotFound()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var query = new GetClientByIdQuery(clientId);

            _cacheMock
                .Setup(c => c.TryGetOrSetAsync(
                    It.IsAny<string>(),
                    It.IsAny<Func<Task<ClientDto>>>(),
                    It.IsAny<TimeSpan>()))
                .Returns<string, Func<Task<ClientDto>>, TimeSpan>((key, factory, _) => factory());

            _repositoryMock
                .Setup(r => r.GetOrThrowAsync<ClientEntity>(clientId, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Client not found"));

            // Act
            var act = () => _handler.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Client not found");
        }
   
    }
}
