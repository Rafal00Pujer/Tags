using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Moq;
using Tags.Context;
using Tags.Models.StackExchange;
using Tags.Options;
using Tags.Services.Interfaces;
using Testcontainers.MsSql;

namespace Tags.IntegrationTests;

public class TagsApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public MsSqlContainer DbContainer { get; private set; }
    public ReloadTagServiceOptions ReloadTagServiceOptions { get; private set; }
    public List<StackExchangeTag> StackExchangeMockTags { get; private set; }
    public Mock<IStackExchangeApiService> StackExchangeApiMock { get; private set; }

    public TagsApiFactory()
    {
        var dbContainerBuilder = new MsSqlBuilder();

        dbContainerBuilder.WithEnvironment("ACCEPT_EULA", "y");
        dbContainerBuilder.WithImage("mcr.microsoft.com/mssql/server:2022-latest");

        DbContainer = dbContainerBuilder.Build();

        ReloadTagServiceOptions = new ReloadTagServiceOptions
        {
            RequestedPageSize = 2,
            RequestedNumOnPages = 5,
            RequestedSortType = "popular",
            RequestedDescendingOrder = true
        };

        StackExchangeMockTags = CreateStackExchangeTags();

        StackExchangeApiMock = CreateStackExchangeMock(StackExchangeMockTags);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            MockStackExchangeApi(services);
            ReplaceReloadTagOptions(services);
            ReplaceDbContextConnectionString(services);
        });
    }

    private void MockStackExchangeApi(IServiceCollection services)
    {
        services.RemoveAll(typeof(IStackExchangeApiService));

        services.AddSingleton(_ => StackExchangeApiMock.Object);
    }

    private void ReplaceReloadTagOptions(IServiceCollection services)
    {
        services.RemoveAll(typeof(IOptions<ReloadTagServiceOptions>));

        services
            .AddOptions<ReloadTagServiceOptions>()
            .Configure(optinos =>
            {
                optinos.RequestedPageSize = ReloadTagServiceOptions.RequestedPageSize;
                optinos.RequestedNumOnPages = ReloadTagServiceOptions.RequestedNumOnPages;
                optinos.RequestedSortType = ReloadTagServiceOptions.RequestedSortType;
                optinos.RequestedDescendingOrder = ReloadTagServiceOptions.RequestedDescendingOrder;
            })
            .ValidateDataAnnotations();
    }

    private void ReplaceDbContextConnectionString(IServiceCollection services)
    {
        services.RemoveAll(typeof(DbContextOptions<TagsContext>));

        services.AddDbContext<TagsContext>(options =>
        {
            var connectionString = DbContainer.GetConnectionString();

            options.UseSqlServer(connectionString);
        });
    }

    private static List<StackExchangeTag> CreateStackExchangeTags()
    {
        return
        [
            new()
            {
                count = 1,
                name = "a",
            },
            new()
            {
                count = 2,
                name = "b",
            },
            new()
            {
                count = 3,
                name = "c",
            },
            new()
            {
                count = 4,
                name = "d",
            },
            new()
            {
                count = 5,
                name = "e",
            },
            new()
            {
                count = 6,
                name = "f",
            },
            new()
            {
                count = 7,
                name = "g",
            },
            new()
            {
                count = 8,
                name = "h",
            },
            new()
            {
                count = 9,
                name = "i",
            },
            new()
            {
                count = 10,
                name = "j",
            }
        ];
    }

    private Mock<IStackExchangeApiService> CreateStackExchangeMock(List<StackExchangeTag> sampleTags)
    {
        var orderedTags = sampleTags.OrderBy(x => x.count);

        var stackExchangeApiMock = new Mock<IStackExchangeApiService>();

        for (var i = 1; i <= ReloadTagServiceOptions.RequestedNumOnPages; i++)
        {
            stackExchangeApiMock
                .Setup(x => x.GetStackOverflowTags(
                    i,
                    ReloadTagServiceOptions.RequestedPageSize,
                    ReloadTagServiceOptions.RequestedSortType,
                    ReloadTagServiceOptions.RequestedDescendingOrder))
                .Returns(Task.FromResult(new StackExchangeResponse<StackExchangeTag>
                {
                    has_more = true,
                    quota_max = 10,
                    quota_remaining = 10,
                    items = orderedTags
                        .Skip((i - 1) * ReloadTagServiceOptions.RequestedPageSize)
                        .Take(ReloadTagServiceOptions.RequestedPageSize)
                        .ToList()
                }));
        }

        return stackExchangeApiMock;
    }

    public async Task InitializeAsync() => await DbContainer.StartAsync();

    public new async Task DisposeAsync() => await DbContainer.StopAsync();
}
