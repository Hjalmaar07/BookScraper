using BookScraper.Driver.BooksToScrap.Model;

namespace BookScraper.Driver.BooksToScrap
{
    public interface IBooksToScrapRepository
    {
        public Task<IEnumerable<Page>> GetAllPages(string homeUrl);
        public Task<IEnumerable<Thumbnail>> GetAllThumbnails(string pageUrl);
    }
}
