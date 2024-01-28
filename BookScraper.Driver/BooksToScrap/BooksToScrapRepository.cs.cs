using BookScraper.Driver.BooksToScrap.Model;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.Web;

namespace BookScraper.Driver.BooksToScrap;

public class BooksToScrapRepository : IBooksToScrapRepository
{
    private const string PageUrl = "https://books.toscrape.com/";
    private HtmlDocument _document = new HtmlDocument();

    public async Task<List<Page>> GetAllPagesAsync(string homeUrl)
    {
        LogProgress("Getting list of pages...");
        var htmlResult = await CallUrlAsync(homeUrl);
        CreateHtmlDocument(htmlResult);
        return ParsePagesUrls();
    }

    public async Task<List<Thumbnail>> GetAllThumbnailsUrlsAsync(string pageUrl, string name)
    {
        LogProgress("Getting list of thumbnails...");
        var htmlResult = await CallUrlAsync(pageUrl);
        CreateHtmlDocument(htmlResult);
        return ParseThumbnailsUrls(name);
    }

    public async Task DownloadAllThumbnails(List<Thumbnail> thumbnails)
    {
        HttpClient client = new HttpClient();
        foreach (var thumbnail in thumbnails) 
        {
            try
            {
                LogProgress($"Downloading - {thumbnail.Genre} - {thumbnail.Title}");
                CreateDirectory(thumbnail.Genre);
                var stream = await client.GetStreamAsync(new Uri(thumbnail.Url));
                using (FileStream outputFileStream = new FileStream(CreateThumbnailPath(thumbnail.Genre, thumbnail.Title), FileMode.Create))
                {
                    await stream.CopyToAsync(outputFileStream);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private string CreateThumbnailPath(string genre, string title)
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Books\\{genre}\\{Regex.Replace(title, @"[^\w]", "_")}.jpg";
    }

    private void CreateDirectory(string genre)
    {
        DirectoryInfo di = Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Books\\{genre}");
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

    private List<Page> ParsePagesUrls()
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
                Genre = link.GetDirectInnerText().Trim()
            });
        }

        return pages;
    }

    private List<Thumbnail> ParseThumbnailsUrls(string name)
    {
        List<Thumbnail> thumbnails = new List<Thumbnail>();
        var links = _document.DocumentNode.Descendants("img")
            .Where(node => node.GetAttributeValue("class", "").Equals("thumbnail"))
            .ToList();

        var titles = _document.DocumentNode.Descendants("h3")
            .Where(node => node.ParentNode.GetAttributeValue("class", "").Equals("product_pod"))
            .ToList();

        for (int i = 0; i < links.Count(); i++)
        {
            thumbnails.Add(new Thumbnail 
            { 
                Url = PageUrl + links[i].GetAttributeValue("src", ""),
                Genre = name,
                Title = RemoveHtmlTags(titles[i].FirstChild.GetAttributeValue("title", "")).Trim()
            });
        }

        return thumbnails;
    }

    private static string RemoveHtmlTags(string strHtml)
    {
        string strText = Regex.Replace(strHtml, "<(.|\n)*?>", String.Empty);
        strText = HttpUtility.HtmlDecode(strText);
        strText = Regex.Replace(strText, @"\s+", " ");
        return strText;
    }

    private void LogProgress(string log)
    {
        Console.WriteLine(log);
    }
}
