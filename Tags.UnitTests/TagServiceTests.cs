using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;
using System.Security.Cryptography.X509Certificates;
using Tags.Context;
using Tags.Models;
using Tags.Options;
using Tags.Services.Implementations;
using Tags.Services.Interfaces;

namespace Tags.UnitTests;

public class TagServiceTests
{
    private readonly TagService _sut;

    private readonly TagServiceOptions _options;

    private readonly Mock<IOptions<TagServiceOptions>> _optionsMock;
    private readonly Mock<ITagSortFactory> _sortFactoryMock;
    private readonly Mock<DbSet<TagModel>> _tagsDbSetMock;
    private readonly Mock<TagsContext> _contextMock;

    private readonly List<TagModel> _tagsData;

    public TagServiceTests()
    {
        var loggerMock = new Mock<ILogger<TagService>>();

        _tagsData =
        [
            new()
            {
                Id = new Guid("00000000000000000000000000000000"),
                Name = "B",
                CountPercent = 50.0f
            },
            new()
            {
                Id = new Guid("11111111111111111111111111111111"),
                Name = "A",
                CountPercent = 10.0f
            },
            new()
            {
                Id = new Guid("22222222222222222222222222222222"),
                Name = "C",
                CountPercent = 90.0f
            }
        ];

        _options = new TagServiceOptions
        {
            DefaultGetPageSize = 1,
            MaxGetPageSize = 10,
            DefaultDescendingOrderValue = true
        };

        _optionsMock = new Mock<IOptions<TagServiceOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(_options);

        _sortFactoryMock = new Mock<ITagSortFactory>();

        _tagsDbSetMock = _tagsData.BuildMockDbSet();

        _contextMock = new Mock<TagsContext>();
        _contextMock.Setup(x => x.Tags).Returns(_tagsDbSetMock.Object);

        _sut = new TagService(
            _sortFactoryMock.Object,
            _contextMock.Object,
            _optionsMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async void GetTagsAsync_NullArguments_GetsDefaultsFromOptionsAndSortFactory_ReturnsTags()
    {
        //Arrange
        var sortMock = MockSortService("id");
        sortMock
            .Setup(x => x.Sort(_tagsDbSetMock.Object, _options.DefaultDescendingOrderValue))
            .Returns(_tagsDbSetMock.Object);

        //Act
        var result = await _sut.GetTagsAsync(null, null, null, null);

        //Assert
        result.Should().ContainSingle();
        result.Should().Contain(_tagsData[0]);
    }

    [Fact]
    public async void GetTagsAsync_SpecifiedPage_ReturnsTagsFromThatPage()
    {
        //Arrange
        var sortMock = MockSortService("id");
        sortMock
            .Setup(x => x.Sort(_tagsDbSetMock.Object, _options.DefaultDescendingOrderValue))
            .Returns(_tagsDbSetMock.Object);

        //Act
        var result = await _sut.GetTagsAsync(2, null, null, null);

        //Assert
        result.Should().ContainSingle();
        result.Should().Contain(_tagsData[1]);
    }

    [Fact]
    public async void GetTagsAsync_SpecifiedPageSize_ReturnsRequestedTagsCount()
    {
        //Arrange
        var sortMock = MockSortService("id");
        sortMock
            .Setup(x => x.Sort(_tagsDbSetMock.Object, _options.DefaultDescendingOrderValue))
            .Returns(_tagsDbSetMock.Object);

        //Act
        var result = await _sut.GetTagsAsync(null, 2, null, null);

        //Assert
        result.Should().HaveCount(2);
        result.Should().NotContain(_tagsData[2]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async void GetTagsAsync_SpecifiedSortOrder_CallsSortWithThatOrder(bool sortOrder)
    {
        //Arrange
        var sortMock = MockSortService("id");
        sortMock
            .Setup(x => x.Sort(_tagsDbSetMock.Object, sortOrder))
            .Returns(_tagsDbSetMock.Object);

        //Act
        var result = await _sut.GetTagsAsync(null, null, null, sortOrder);

        //Assert
        sortMock.Verify(x => x.Sort(_tagsDbSetMock.Object, sortOrder), Times.Once());
    }

    [Fact]
    public async void GetTagsAsync_SpecifiedSortType_RequestSpecifedSortServiceFromFactory()
    {
        //Arrange
        var defaultSortName = "id";

        var sortNames = new List<string>
        {
            defaultSortName,
            "Name"
        };

        var firstSortServiceMock = new Mock<ITagSortService>();
        firstSortServiceMock
            .Setup(x => x.Sort(_tagsDbSetMock.Object, _options.DefaultDescendingOrderValue))
            .Returns(_tagsDbSetMock.Object);

        var secondSortServiceMock = new Mock<ITagSortService>();
        secondSortServiceMock
            .Setup(x => x.Sort(_tagsDbSetMock.Object, _options.DefaultDescendingOrderValue))
            .Returns(_tagsDbSetMock.Object);

        _sortFactoryMock.Setup(x => x.DefaultSortName).Returns(defaultSortName);
        _sortFactoryMock.Setup(x => x.SortNames).Returns(sortNames);
        _sortFactoryMock
            .Setup(x => x.GetTagSortService(sortNames[0]))
            .Returns(firstSortServiceMock.Object);

        _sortFactoryMock
            .Setup(x => x.GetTagSortService(sortNames[1]))
            .Returns(firstSortServiceMock.Object);

        //Act
        var result = await _sut.GetTagsAsync(null, null, sortNames[1], null);

        //Assert
        _sortFactoryMock.Verify(x => x.GetTagSortService(sortNames[1]), Times.Once);

        firstSortServiceMock
            .Verify(
            x => x.Sort(_tagsDbSetMock.Object, _options.DefaultDescendingOrderValue),
            Times.Once);

        secondSortServiceMock
            .Verify(
            x => x.Sort(_tagsDbSetMock.Object, _options.DefaultDescendingOrderValue),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-123)]
    public async void GetTagsAsync_PageIsEqualOrLessThanZero_Throws(int page)
    {
        //Arrange

        //Act
        var action = () => _sut.GetTagsAsync(page, null, null, null);

        //Assert
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(11)]
    [InlineData(-1)]
    [InlineData(-123)]
    public async void GetTagsAsync_PageSizeIsEqualOrLessThanZero_Throws(int pagesize)
    {
        //Arrange

        //Act
        var action = () => _sut.GetTagsAsync(null, pagesize, null, null);

        //Assert
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(11)]
    [InlineData(123)]
    [InlineData(223)]
    public async void GetTagsAsync_PageSizeIsGreaterThanMaxPageSize_Throws(int pagesize)
    {
        //Arrange

        //Act
        var action = () => _sut.GetTagsAsync(null, pagesize, null, null);

        //Assert
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async void GetTagsAsync_UnsupportedSortType_Throws()
    {
        //Arrange
        _ = MockSortService("id");

        //Act
        var action = () => _sut.GetTagsAsync(null, null, "test", null);

        //Assert
        await action.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    private Mock<ITagSortService> MockSortService(string sortName)
    {
        var sortServiceMock = new Mock<ITagSortService>();

        _sortFactoryMock.Setup(x => x.DefaultSortName).Returns(sortName);
        _sortFactoryMock.Setup(x => x.SortNames).Returns([sortName]);

        _sortFactoryMock
            .Setup(x => x.GetTagSortService(sortName))
            .Returns(sortServiceMock.Object);

        return sortServiceMock;
    }
}
