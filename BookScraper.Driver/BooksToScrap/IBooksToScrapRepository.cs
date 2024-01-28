namespace BookScraper.Driver.BooksToScrap;

public interface IBooksToScrapRepository
{
    public Task GetAllPagesAsync();
}
