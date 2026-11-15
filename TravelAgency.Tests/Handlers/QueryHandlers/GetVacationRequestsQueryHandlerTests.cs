using AutoMapper;
using FluentAssertions;
using Moq;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Application.Queries.GetVacationRequest;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Shared.Constants;

namespace TravelAgency.Tests.Handlers.QueryHandlers;

/// <summary>
/// Набор юнит-тестов для <see cref="GetVacationRequestsQueryHandler"/>.
/// </summary>
public class GetVacationRequestsQueryHandlerTests
{
    private static readonly TimeSpan ExpectedCacheExpiry = TimeSpan.FromHours(10);

    private readonly Mock<IRedisCacheService> _cacheMock;
    private readonly Mock<IRepository> _repositoryMock;
    private readonly Mock<IMapper> _mapperMock;

    private readonly GetVacationRequestsQueryHandler _handler;

    public GetVacationRequestsQueryHandlerTests()
    {
        _cacheMock = new Mock<IRedisCacheService>();
        _repositoryMock = new Mock<IRepository>();
        _mapperMock = new Mock<IMapper>();

        _handler = new GetVacationRequestsQueryHandler(
            _repositoryMock.Object,
            _mapperMock.Object,
            _cacheMock.Object);
    }

    /// <summary>
    /// Юнит-тест для <see cref="GetVacationRequestsQueryHandler"/>, проверяющий поведение при отсутствии данных в кэше.
    /// </summary>
    /// <remarks>
    /// Проверяется, что обработчик:
    /// - запрашивает заявки у репозитория с переданным токеном отмены;
    /// - преобразует сущности в DTO;
    /// - возвращает полученный список;
    /// - использует ожидаемые ключ кэша и срок хранения.
    /// </remarks>
    /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
    [Fact]
    public async Task Handle_ShouldLoadAndMapVacationRequests_WhenCacheMiss()
    {
        // Arrange
        var query = new GetVacationRequestsQuery();
        using var cts = new CancellationTokenSource();
        var requestEntities = new List<VacationRequestEntity>
        {
            new() { Id = Guid.NewGuid(), ClientId = Guid.NewGuid(), VacationId = Guid.NewGuid() },
            new() { Id = Guid.NewGuid(), ClientId = Guid.NewGuid(), VacationId = Guid.NewGuid() }
        };
        IReadOnlyList<VacationRequestDto> expectedDtos = new[]
        {
            new VacationRequestDto(requestEntities[0].Id, requestEntities[0].ClientId, requestEntities[0].VacationId, requestEntities[0].Status, requestEntities[0].Message),
            new VacationRequestDto(requestEntities[1].Id, requestEntities[1].ClientId, requestEntities[1].VacationId, requestEntities[1].Status, requestEntities[1].Message)
        };

        _cacheMock
            .Setup(c => c.TryGetOrSetAsync(
                CacheKeys.AllVacationRequests,
                It.IsAny<Func<Task<IReadOnlyList<VacationRequestDto>>>>(),
                ExpectedCacheExpiry))
            .Returns<string, Func<Task<IReadOnlyList<VacationRequestDto>>>, TimeSpan>(
                (_, factory, _) => factory());

        _repositoryMock
            .Setup(r => r.GetAllAsync<VacationRequestEntity>(cts.Token))
            .ReturnsAsync(requestEntities);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<VacationRequestDto>>(requestEntities))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, cts.Token);

        // Assert
        result.Should().BeEquivalentTo(expectedDtos);

        _cacheMock.Verify(c => c.TryGetOrSetAsync(
            CacheKeys.AllVacationRequests,
            It.IsAny<Func<Task<IReadOnlyList<VacationRequestDto>>>>(),
            ExpectedCacheExpiry), Times.Once);
        _repositoryMock.Verify(r => r.GetAllAsync<VacationRequestEntity>(cts.Token), Times.Once);
        _mapperMock.Verify(m => m.Map<IReadOnlyList<VacationRequestDto>>(requestEntities), Times.Once);
    }

    /// <summary>
    /// Юнит-тест для <see cref="GetVacationRequestsQueryHandler"/>, проверяющий поведение при наличии данных в кэше.
    /// </summary>
    /// <remarks>
    /// Проверяется, что обработчик возвращает закэшированный список и не обращается к репозиторию и мапперу.
    /// </remarks>
    /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
    [Fact]
    public async Task Handle_ShouldReturnCachedVacationRequests_WithoutLoadingFromRepository()
    {
        // Arrange
        var query = new GetVacationRequestsQuery();
        IReadOnlyList<VacationRequestDto> cachedDtos = new[]
        {
            new VacationRequestDto(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
                TravelAgency.Domain.Enums.VacationRequestStatus.New, "Cached request")
        };

        _cacheMock
            .Setup(c => c.TryGetOrSetAsync(
                CacheKeys.AllVacationRequests,
                It.IsAny<Func<Task<IReadOnlyList<VacationRequestDto>>>>(),
                ExpectedCacheExpiry))
            .ReturnsAsync(cachedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(cachedDtos);
        
        _repositoryMock.Verify(r => r.GetAllAsync<VacationRequestEntity>(It.IsAny<CancellationToken>()), Times.Never);
        _mapperMock.Verify(m => m.Map<IReadOnlyList<VacationRequestDto>>(It.IsAny<object>()), Times.Never);
    }

    /// <summary>
    /// Юнит-тест для <see cref="GetVacationRequestsQueryHandler"/>, проверяющий передачу
    /// <see cref="CancellationToken"/> в репозиторий.
    /// </summary>
    /// <remarks>
    /// Тест имитирует отсутствие данных в кэше и проверяет, что токен отмены,
    /// переданный обработчику, передаётся в метод <c>GetAllAsync</c> репозитория.
    /// </remarks>
    /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
    [Fact]
    public async Task Handle_ShouldPassCancellationToken_ToRepository()
    {
        // Arrange
        var query = new GetVacationRequestsQuery();
        using var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var requestEntities = new List<VacationRequestEntity>();
        IReadOnlyList<VacationRequestDto> mappedRequests = Array.Empty<VacationRequestDto>();

        _cacheMock
            .Setup(c => c.TryGetOrSetAsync(
                CacheKeys.AllVacationRequests,
                It.IsAny<Func<Task<IReadOnlyList<VacationRequestDto>>>>(),
                ExpectedCacheExpiry))
            .Returns<string, Func<Task<IReadOnlyList<VacationRequestDto>>>, TimeSpan>(
                (_, factory, _) => factory());

        _repositoryMock
            .Setup(r => r.GetAllAsync<VacationRequestEntity>(cancellationToken))
            .ReturnsAsync(requestEntities);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<VacationRequestDto>>(requestEntities))
            .Returns(mappedRequests);

        // Act
        await _handler.Handle(query, cancellationToken);

        // Assert
        _repositoryMock.Verify(
            r => r.GetAllAsync<VacationRequestEntity>(cancellationToken),
            Times.Once);
    }

    /// <summary>
    /// Юнит-тест для <see cref="GetVacationRequestsQueryHandler"/>, проверяющий проброс исключения репозитория.
    /// </summary>
    /// <remarks>
    /// Тест имитирует отсутствие данных в кэше и проверяет, что исключение,
    /// выброшенное методом <c>GetAllAsync</c>, передаётся вызывающему коду.
    /// </remarks>
    /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
    [Fact]
    public async Task Handle_ShouldPropagateException_WhenRepositoryFails()
    {
        // Arrange
        var query = new GetVacationRequestsQuery();
        var exception = new InvalidOperationException("Unable to load vacation requests");

        _cacheMock
            .Setup(c => c.TryGetOrSetAsync(
                CacheKeys.AllVacationRequests,
                It.IsAny<Func<Task<IReadOnlyList<VacationRequestDto>>>>(),
                ExpectedCacheExpiry))
            .Returns<string, Func<Task<IReadOnlyList<VacationRequestDto>>>, TimeSpan>(
                (_, factory, _) => factory());

        _repositoryMock
            .Setup(r => r.GetAllAsync<VacationRequestEntity>(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Unable to load vacation requests");

        _mapperMock.Verify(
            m => m.Map<IReadOnlyList<VacationRequestDto>>(It.IsAny<object>()),
            Times.Never);
    }

    /// <summary>
    /// Юнит-тест для <see cref="GetVacationRequestsQueryHandler"/>, проверяющий обработку пустого результата репозитория.
    /// </summary>
    /// <remarks>
    /// Проверяется, что пустой список сущностей передаётся мапперу, а обработчик возвращает пустой список DTO.
    /// </remarks>
    /// <returns>Асинхронная задача, представляющая выполнение теста.</returns>
    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenRepositoryReturnsNoVacationRequests()
    {
        // Arrange
        var query = new GetVacationRequestsQuery();
        var requestEntities = new List<VacationRequestEntity>();
        IReadOnlyList<VacationRequestDto> expectedDtos = Array.Empty<VacationRequestDto>();

        _cacheMock
            .Setup(c => c.TryGetOrSetAsync(
                CacheKeys.AllVacationRequests,
                It.IsAny<Func<Task<IReadOnlyList<VacationRequestDto>>>>(),
                ExpectedCacheExpiry))
            .Returns<string, Func<Task<IReadOnlyList<VacationRequestDto>>>, TimeSpan>(
                (_, factory, _) => factory());

        _repositoryMock
            .Setup(r => r.GetAllAsync<VacationRequestEntity>(It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestEntities);

        _mapperMock
            .Setup(m => m.Map<IReadOnlyList<VacationRequestDto>>(requestEntities))
            .Returns(expectedDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _repositoryMock.Verify(r => r.GetAllAsync<VacationRequestEntity>(It.IsAny<CancellationToken>()), Times.Once);
        _mapperMock.Verify(m => m.Map<IReadOnlyList<VacationRequestDto>>(requestEntities), Times.Once);
    }
}
