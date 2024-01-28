using BookScraper.Driver.BooksToScrap;
using BookScraper.Driver.BooksToScrap.Model;
using NUnit.Framework;

namespace BookScraper.Driver.UnitTests.BooksToScrap;

public class BooksToScrapUnitTests
{
    private static IBooksToScrapRepository _repository;
    private const string PageUrl = "https://books.toscrape.com/";

    [SetUp]
    public void SetUp()
    {
        _repository = new BooksToScrapRepository();
    }

    [Test]
    public async Task GivenCorrectUrl_WhenGetAllPages_ThenListOfPagesIsReturned()
    {
        var result = await _repository.GetAllPagesAsync(PageUrl);

        Assert.IsNotNull(result);
        Assert.That(!string.IsNullOrEmpty(result.FirstOrDefault().Genre));
        Assert.That(!string.IsNullOrEmpty(result.FirstOrDefault().Url));
    }

    [Test]
    public async Task GivenWrongUrl_WhenGetAllPages_ThenExceptionIsThrown()
    {
        Assert.ThrowsAsync<Exception>(() => _repository.GetAllPagesAsync(""));
    }
    
}
