using BookScraper.Driver.BooksToScrap.Model;
using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace BookScraper.Driver.BooksToScrap;

public class BooksToScrapRepository : IBooksToScrapRepository
{
    private const string PageUrl = "https://books.toscrape.com/";
    private HtmlDocument _document = new HtmlDocument();

    public async Task<List<Page>> GetAllPagesAsync()
    {
        LogProgress("Getting list of pages...");
        await CreateHtmlDocument(PageUrl);
        return await DownloadPages();
    }

    public async Task<List<Thumbnail>> GetAllThumbnailsUrlsAsync(string pageUrl, string name)
    {
        LogProgress($"Getting list of thumbnails for category: {name}...");
        await CreateHtmlDocument(pageUrl);
        return null;
    }

    public async Task DownloadAllThumbnails(List<Thumbnail> thumbnails)
    {
        
        foreach (var thumbnail in thumbnails) 
        {
            try
            {
                LogProgress($"Downloading - {thumbnail.Genre} - {thumbnail.Title}");
                CreateDirectory(thumbnail.Genre);
                await DownloadThumbnail(thumbnail);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private async Task DownloadThumbnail(Thumbnail thumbnail)
    {
        HttpClient client = new HttpClient();
        var stream = await client.GetStreamAsync(new Uri(thumbnail.Url));
        using (FileStream outputFileStream = new FileStream(CreateThumbnailPath(thumbnail.Genre, thumbnail.Title), FileMode.Create))
        {
            await stream.CopyToAsync(outputFileStream);
        }
    }

    private string CreateThumbnailPath(string genre, string title)
    {
        return $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Books\\{genre}\\{Regex.Replace(title, @"[^\w]", "_")}.jpg";
    }

    private void CreateDirectory(string folder)
    {
        DirectoryInfo di = Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Books\\{folder}");
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

    private async Task CreateHtmlDocument(string pageUrl)
    {
        var htmlResult = await CallUrlAsync(pageUrl);
        _document.LoadHtml(htmlResult);
    }

    static string GetFolderPathFromLink(string link)
    {
        if (link.StartsWith("/"))
        {
            link = link.Substring(1);
        }
        string folderPath = Path.GetDirectoryName(link);

        return folderPath;
    }

    private async Task<List<Page>> DownloadPages()
    {
        using (WebClient client = new WebClient())
        {
            client.DownloadFile(PageUrl, Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\books\\" + "index" + ".html");
            var pagesToTraverse = _document.DocumentNode.Descendants("a")
                .Where(node => node.GetAttributeValue("href", "").Contains("/books/"))
                .ToList();

            var links = _document.DocumentNode.Descendants("a")
            .Where(node => node.GetAttributeValue("href", "").Any())
            .ToList();

            links.AddRange(_document.DocumentNode.Descendants("link")
            .Where(node => node.GetAttributeValue("href", "").Any())
            .ToList());

            foreach (var page in pagesToTraverse)
            {
                await ParseThumbnailsUrls(page);
            }

            foreach (var link in links)
            {
                try
                {
                    CreateDirectory($"{GetFolderPathFromLink(link.GetAttributeValue("href", ""))}");
                }
                catch { }
                var uri = new Uri($"{PageUrl}{link.GetAttributeValue("href", "")}");
                client.DownloadFile($"{PageUrl}{uri.LocalPath}", $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\books\\{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
            }
        }
        return null;
    }

    private async Task<List<Thumbnail>> ParseThumbnailsUrls(HtmlNode page)
    {
        using (WebClient client = new WebClient())
        {
            await CreateHtmlDocument(PageUrl + page.GetAttributeValue("href", ""));
            var links = _document.DocumentNode.Descendants("img")
                .Where(node => node.GetAttributeValue("class", "").Equals("thumbnail"))
                .ToList();

            foreach (var link in links)
            {
                try
                {
                    CreateDirectory($"{GetFolderPathFromLink(RemoveChildrenFromPath(link.GetAttributeValue("src", "")))}");
                }
                catch { }
                var uri = new Uri($"{PageUrl}{link.GetAttributeValue("src", "")}");
                client.DownloadFile($"{PageUrl}{uri.LocalPath}", $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\books\\{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
            }
        }

        return null;
    }

    private static string RemoveHtmlTags(string strHtml)
    {
        string strText = Regex.Replace(strHtml, "<(.|\n)*?>", String.Empty);
        strText = HttpUtility.HtmlDecode(strText);
        strText = Regex.Replace(strText, @"\s+", " ");
        return strText;
    }

    private string RemoveChildrenFromPath(string relativePath)
    {
        string[] parts = relativePath.Split('/');
        string[] cleanParts = new string[parts.Length];
        int cleanIndex = 0;
        foreach (string part in parts)
        {
            if (part != "..")
            {
                cleanParts[cleanIndex] = part;
                cleanIndex++;
            }
        }
        string fullPath = string.Join("/", cleanParts, 0, cleanIndex);

        return fullPath;
    }

    private void LogProgress(string log)
    {
        Console.WriteLine(log);
    }
}
