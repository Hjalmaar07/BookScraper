using BookScraper.Driver.BooksToScrap.Model;

namespace BookScraper.Driver.BooksToScrap;

public interface IBooksToScrapRepository
{
    public Task<List<Page>> GetAllPagesAsync(string homeUrl);
    public Task<List<Thumbnail>> GetAllThumbnailsAsync(string pageUrlm, string name);
}
