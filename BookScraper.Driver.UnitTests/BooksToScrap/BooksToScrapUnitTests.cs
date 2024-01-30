using BookScraper.Driver.BooksToScrap;
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
    }

    [Test]
    public async Task GivenWrongUrl_WhenGetAllPages_ThenExceptionIsThrown()
    {
    }
    
}
