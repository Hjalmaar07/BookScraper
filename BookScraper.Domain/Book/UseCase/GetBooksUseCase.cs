using BookScraper.Driver.BooksToScrap;
using BookScraper.Driver.BooksToScrap.Model;

namespace BookScraper.Domain.Book.UseCase
{
    public class GetBooksUseCase : IGetBooksUseCase
    {
        private readonly IBooksToScrapRepository _booksToScrapRepository;
        private List<Page> _pages;
        private List<Thumbnail> _thumbnails;

        public GetBooksUseCase(IBooksToScrapRepository booksToScrapRepository)
        {
            _booksToScrapRepository = booksToScrapRepository;
            _thumbnails = new List<Thumbnail>();
        }

        public async void Run()
        {
            await GetAllPagesAsync();
            await GetAllThumbnailsUrlsAsync();
            await DownloadAllThumbnails();
        }

        private async Task GetAllPagesAsync()
        {
            _pages = await _booksToScrapRepository.GetAllPagesAsync();
        }

        private async Task GetAllThumbnailsUrlsAsync()
        {
            foreach (var page in _pages)
            {
                var result = await _booksToScrapRepository.GetAllThumbnailsUrlsAsync(page.Url, page.Genre);
                _thumbnails.AddRange(result);
            }
        }

        private async Task DownloadAllThumbnails()
        {
            _booksToScrapRepository.DownloadAllThumbnails(_thumbnails);
        }
    }
}
