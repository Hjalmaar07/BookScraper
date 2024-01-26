using BookScraper.Driver.BooksToScrap;

namespace BookScraper.Domain.Book.UseCase
{
    public class GetBooksUseCase : IGetBooksUseCase
    {
        private readonly IBooksToScrapRepository _booksToScrapRepository;

        public GetBooksUseCase(IBooksToScrapRepository booksToScrapRepository)
        {
            _booksToScrapRepository = booksToScrapRepository;
        }

        public void Run()
        {
            _booksToScrapRepository.GetAllPages();
        }
    }
}
