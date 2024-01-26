using BookScraper.Driver.BooksToScrap.Model;
using HtmlAgilityPack;

namespace BookScraper.Driver.BooksToScrap;

public class BooksToScrapRepository : IBooksToScrapRepository
{
    private const string PageUrl = "https://books.toscrape.com/";
    private HtmlDocument _document = new HtmlDocument();

    public async Task<IEnumerable<Page>> GetAllPages(string homeUrl)
    {
        var htmlResult = await CallUrlAsync(homeUrl);
        CreateHtmlDocument(htmlResult);
        return ParsePagesUrls();
    }

    public async Task<IEnumerable<Thumbnail>> GetAllThumbnails(string pageUrl)
    {
        var htmlResult = await CallUrlAsync(pageUrl);
        CreateHtmlDocument(htmlResult);
        return ParseThumbnailsUrls();
    }

    private static async Task<string> CallUrlAsync(string url)
    {
        try
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStringAsync(url);
            return response;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    private void CreateHtmlDocument(string htmlResult)
    {
        _document.LoadHtml(htmlResult);
    }

    private IEnumerable<Page> ParsePagesUrls()
    {
        List<Page> pages = new List<Page>();
        var links = _document.DocumentNode.Descendants("a")
            .Where(node => node.GetAttributeValue("href", "").Contains("/books/"))
            .ToList();
        
        foreach (var link in links)
        {
            pages.Add(new Page 
            { 
                Url = PageUrl + link.GetAttributeValue("href", ""), 
                Name = link.GetDirectInnerText().Trim()
            });
        }

        return pages;
    }

    private IEnumerable<Thumbnail> ParseThumbnailsUrls()
    {
        List<Thumbnail> thumbnails = new List<Thumbnail>();
        var links = _document.DocumentNode.Descendants("img")
            .Where(node => node.GetAttributeValue("class", "").Contains("thumbnail"))
            .ToList();

        foreach (var link in links)
        {
            thumbnails.Add(new Thumbnail 
            { 
                Url = PageUrl + link.GetAttributeValue("src", "") 
            });
        }

        return thumbnails;
    }
}
