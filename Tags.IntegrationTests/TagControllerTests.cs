using FluentAssertions;
using System.Net.Http.Json;
using Tags.Models;

namespace Tags.IntegrationTests;

public class TagControllerTests(
    TagsApiFactory tagsApiFactory) : IClassFixture<TagsApiFactory>
{
    private readonly TagsApiFactory _factory = tagsApiFactory;
    private readonly HttpClient _tagsApi = tagsApiFactory.CreateClient();

    [Fact]
    public async Task GetTagsAsync_NoQueryParams_AppliesDefaultsAndReturnsTags()
    {
        //Arrange

        //Act
        var response = await _tagsApi.GetAsync("/Tag/Get");

        //Assert
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<ICollection<TagModel>>();

        data.Should().NotBeNull();
        data.Should().HaveCount(10);

        data!.Select(x => x.Name)
            .Should()
            .Contain(_factory.StackExchangeMockTags
            .Select(x => x.name));
    }

    [Fact]
    public async Task GetTagsAsync_WithPageAndPageSizeAndSortType_ReturnsRequestedTags()
    {
        //Arrange
        var page = 2;
        var pageSize = 3;
        var sortType = "percent";

        //Act
        var response = await _tagsApi.GetAsync($"/Tag/Get?page={page}&pageSize={pageSize}&sortType={sortType}");

        //Assert
        response.EnsureSuccessStatusCode();

        var data = await response.Content.ReadFromJsonAsync<ICollection<TagModel>>();

        data.Should().NotBeNull();
        data.Should().HaveCount(3);

        data!.Select(x => x.Name)
            .Should()
            .ContainInOrder(_factory.StackExchangeMockTags
            .OrderByDescending(x => x.count)
            .Skip(3)
            .Take(3)
            .Select(x => x.name));
    }

    [Fact]
    public async Task GetTagsAsync_WithInvalidQuery_ReturnsBadRequest()
    {
        //Arrange
        var page = -1;

        //Act
        var response = await _tagsApi.GetAsync($"/Tag/Get?page={page}");

        //Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }
}