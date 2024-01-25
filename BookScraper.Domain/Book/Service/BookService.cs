using BookScraper.Domain.Book.UseCase;

namespace BookScraper.Domain.Book.Service;

public class BookService : IBookService
{
    private readonly IGetBooksUseCase _getBooksUseCase;

    public BookService(IGetBooksUseCase getBooksUseCase)
    {
        _getBooksUseCase = getBooksUseCase;
    }

    public void GetBooks()
    {
        _getBooksUseCase.Run();
    }
}
