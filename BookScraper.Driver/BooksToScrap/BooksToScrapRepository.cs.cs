namespace BookScraper.Driver.BooksToScrap;

public class BooksToScrapRepository : IBooksToScrapRepository
{
    public void GetAllBooks()
    {
        var result = CallUrl("https://books.toscrape.com/");
    }

    private static async Task<string> CallUrl(string url)
    {
        HttpClient client = new HttpClient();
        var response = await client.GetStringAsync(url);
        return response;
    }
}
