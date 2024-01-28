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

        public async void Run()
        {
            await GetAllPagesAsync();
        }

        private async Task GetAllPagesAsync()
        {
            await _booksToScrapRepository.GetAllPagesAsync();
        }
    }
}
