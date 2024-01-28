using HtmlAgilityPack;
using System.Net;
using System.Text.RegularExpressions;

namespace BookScraper.Driver.BooksToScrap;

public class BooksToScrapRepository : IBooksToScrapRepository
{
    private const string PageUrl = "https://books.toscrape.com/";
    private HtmlDocument _document = new HtmlDocument();
    private string LocalFolderPath;

    public async Task GetAllPagesAsync()
    {
        LogProgress("Getting list of pages...");
        BuildLocalFolderPath();
        await CreateHtmlDocument(PageUrl);
        await DownloadPages();
    }

    public async Task GetAllThumbnailsUrlsAsync(string pageUrl, string name)
    {
        LogProgress($"Getting list of thumbnails for category: {name}...");
        await CreateHtmlDocument(pageUrl);
    }

    private void CreateDirectory(string folder)
    {
        DirectoryInfo di = Directory.CreateDirectory($"{LocalFolderPath}{folder}");
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

    private async Task DownloadPages()
    {
        using (WebClient client = new WebClient())
        {
            client.DownloadFile(PageUrl, $"{LocalFolderPath}index.html");
            var pagesToTraverse = GetNodesForContains("a", "href", "/books/");
            

            var links = GetNodesForAny("a");

            links.AddRange(GetNodesForAny("link"));

            foreach (var page in pagesToTraverse)
            {
                await DownloadThumbnails(page);
            }

            foreach (var link in links)
            {
                try
                {
                    CreateDirectory($"{GetFolderPathFromLink(link.GetAttributeValue("href", ""))}");
                }
                catch { }
                var uri = new Uri($"{PageUrl}{link.GetAttributeValue("href", "")}");
                client.DownloadFile($"{PageUrl}{uri.LocalPath}", $"{LocalFolderPath}{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
            }
        }
    }

    private async Task DownloadThumbnails(HtmlNode page)
    {
        using (WebClient client = new WebClient())
        {
            await CreateHtmlDocument(PageUrl + page.GetAttributeValue("href", ""));
            var links = GetNodesForContains("img", "class", "thumbnail");

            foreach (var link in links)
            {
                try
                {
                    CreateDirectory($"{GetFolderPathFromLink(RemoveChildrenFromPath(link.GetAttributeValue("src", "")))}");
                }
                catch { }
                var uri = new Uri($"{PageUrl}{link.GetAttributeValue("src", "")}");
                client.DownloadFile($"{PageUrl}{uri.LocalPath}", $"{LocalFolderPath}{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
            }
        }
    }

    private List<HtmlNode> GetNodesForAny(string descendantsTag)
    {
        return _document.DocumentNode.Descendants(descendantsTag)
            .Where(node => node.GetAttributeValue("href", "").Any())
            .ToList();
    }

    private List<HtmlNode> GetNodesForContains(string descendantsTag, string attributeValue, string contains)
    {
        return _document.DocumentNode.Descendants(descendantsTag)
                .Where(node => node.GetAttributeValue(attributeValue, "").Contains(contains))
                .ToList();
    }

    private string RemoveChildrenFromPath(string relativePath)
    {
        var parts = relativePath.Split('/');
        var cleanParts = new string[parts.Length];
        var cleanIndex = 0;
        foreach (string part in parts)
        {
            if (part != "..")
            {
                cleanParts[cleanIndex] = part;
                cleanIndex++;
            }
        }

        return string.Join("/", cleanParts, 0, cleanIndex);
    }

    private void LogProgress(string log)
    {
        Console.WriteLine(log);
    }

    private void BuildLocalFolderPath()
    {
        LocalFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Books\\";
    }
}
