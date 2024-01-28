using BookScraper.Domain.Book.UseCase;
using BookScraper.Driver.BooksToScrap;
using Moq;
using NUnit.Framework;

namespace BookScraper.Domain.UnitTests.Book.UseCase;

public class GetBooksUseCaseUnitTests
{
    private static IGetBooksUseCase _getBooksUseCase;
    private static readonly Mock<IBooksToScrapRepository> _booksToScrapRepository;

    [SetUp]
    public void SetUp()
    {
        _getBooksUseCase = new GetBooksUseCase(_booksToScrapRepository.Object);
    }

    [Test]
    public void WhenRun_Then()
    {
        _getBooksUseCase.Run();
    }
}
