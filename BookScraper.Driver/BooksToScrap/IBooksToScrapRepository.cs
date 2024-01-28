using BookScraper.Driver.BooksToScrap.Model;

namespace BookScraper.Driver.BooksToScrap;

public interface IBooksToScrapRepository
{
    public Task<List<Page>> GetAllPagesAsync();
    public Task<List<Thumbnail>> GetAllThumbnailsUrlsAsync(string pageUrlm, string name);
    public Task DownloadAllThumbnails(List<Thumbnail> thumbnails);
}
