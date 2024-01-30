using HtmlAgilityPack;
using System.Net;

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

        var pagesToTraverse = GetNodesForContains("a", "href", "/books/");
        await TraverseCategories(pagesToTraverse);
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
            var links = GetNodesForAny("a");
            links.AddRange(GetNodesForAny("link"));

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

    private async Task TraverseCategories(List<HtmlNode> pages)
    {
        foreach (var page in pages)
        {
            await CreateHtmlDocument(PageUrl + page.GetAttributeValue("href", ""));
            await DownloadThumbnails();
            var productsList = await GetProductPages();
            foreach (var product in productsList)
            {
                await CreateHtmlDocument(PageUrl + "catalogue/" + RemoveChildrenFromPath(product.GetAttributeValue("href", "")));
                await DownloadProductPage(product);
                await DownloadProductPictures();
            }
        }
    }

    private async Task DownloadThumbnails()
    {
        using (WebClient client = new WebClient())
        {
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

    private async Task<List<HtmlNode>> GetProductPages()
    {
        var div = _document.DocumentNode.SelectNodes("//div[@class='image_container']");
        return div.Descendants("a")
        .Where(node => node.GetAttributeValue("href", "").Any())
        .ToList();
    }

    private async Task DownloadProductPage(HtmlNode product)
    {
        using (WebClient client = new WebClient())
        {
                try
                {
                    CreateDirectory($"catalogue\\{GetFolderPathFromLink(RemoveChildrenFromPath(product.GetAttributeValue("href", "")))}");
                }
                catch { }
                var uri = new Uri($"{PageUrl}{product.GetAttributeValue("href", "")}");
                client.DownloadFile($"{PageUrl}catalogue/{uri.LocalPath}", $"{LocalFolderPath}\\catalogue\\{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");

        }
    }

    private async Task DownloadProductPictures()
    {
        using (WebClient client = new WebClient())
        {
            var div = _document.DocumentNode.SelectNodes("//div[@class='item active']");
            var links = div.Descendants("img")
            .Where(node => node.GetAttributeValue("src", "").Any())
            .ToList();

            foreach (var link in links)
            {
                try
                {
                    CreateDirectory($"{GetFolderPathFromLink(RemoveChildrenFromPath(link.GetAttributeValue("src", "")))}");
                }
                catch { }
                var uri = new Uri($"{PageUrl}{link.GetAttributeValue("src", "")}");
                client.DownloadFile($"{PageUrl}{uri.LocalPath}", $"{LocalFolderPath}\\{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
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
