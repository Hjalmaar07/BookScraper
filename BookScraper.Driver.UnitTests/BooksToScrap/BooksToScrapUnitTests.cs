using BookScraper.Driver.BooksToScrap;
using NUnit.Framework;

namespace BookScraper.Driver.UnitTests.BooksToScrap;

public class BooksToScrapUnitTests
{
    private static IBooksToScrapRepository _repository;

    [SetUp]
    public void SetUp()
    {
        _repository = new BooksToScrapRepository();
    }

    [Test]
    public async Task WhenGetAllPagesAsync_ThenListOfPagesIsReturned()
    {
        await _repository.GetAllPagesAsync();
    }
}
