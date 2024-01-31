using HtmlAgilityPack;
using System.Net;

namespace BookScraper.Driver.BooksToScrap;

public class BooksToScrapRepository : IBooksToScrapRepository
{
    private const string HomeUrl = "https://books.toscrape.com/";
    private HtmlDocument _document = new HtmlDocument();
    private string LocalFolderPath;

    public async Task GetAllPagesAsync()
    {
        BuildLocalFolderPath();
        await CreateHtmlDocumentAsync(HomeUrl);
        DownloadExternalLinks();
        DownloadCategoryPages();
        await DownloadCategoriesContentAsync(GetNodesForContains("a", "href", "/books/"));
        LogProgress("\n\n\n\nAll files have been successfully downloaded. You may now exit the application by pressing enter...");
    }

    private async Task CreateHtmlDocumentAsync(string pageUrl)
    {
        var htmlResult = await CallUrlAsync(pageUrl);
        _document.LoadHtml(htmlResult);
    }

    private static async Task<string> CallUrlAsync(string url)
    {
        try
        {
            HttpClient client = new HttpClient();
            return await client.GetStringAsync(url);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw new Exception(e.Message);
        }
    }

    static string GetFolderPathFromLink(string link)
    {
        if (link.StartsWith("/"))
        {
            link = link.Substring(1);
        }
        return Path.GetDirectoryName(link);
    }

    private void DownloadExternalLinks()
    {
        var pages = GetNodesForAny("link");

        DownloadFilesAsync(pages, "href", false);
    }

    private void DownloadCategoryPages()
    {
        var pages = _document.DocumentNode.Descendants("a")
            .Where(node =>
                node.GetAttributeValue("href", null) != null &&
                !node.Ancestors("article").Any(a => a.Attributes["class"]?.Value == "product_pod"))
            .ToList();

        DownloadFilesAsync(pages, "href", false);
    }

    private async Task DownloadCategoriesContentAsync(List<HtmlNode> pages)
    {
        foreach (var page in pages)
        {
            await CreateHtmlDocumentAsync(HomeUrl + page.GetAttributeValue("href", ""));
            DownloadThumbnails();
            DownloadProductPages();
            var productPictures = new List<HtmlNode>();
            foreach (var product in GetProductPages())
            {
                await CreateHtmlDocumentAsync($"{HomeUrl}catalogue/{RemoveChildrenFromPath(product.GetAttributeValue("href", ""))}");
                productPictures.AddRange(GetProductPictures());
            }
            DownloadProductPictures(productPictures);
        }
    }

    private void DownloadThumbnails()
    {
        var pages = GetNodesForContains("img", "class", "thumbnail");

        DownloadFilesAsync(pages, "src", false);
    }

    private void DownloadProductPages()
    {
        DownloadFilesAsync(GetProductPages(), "href", true);
    }

    private List<HtmlNode> GetProductPages()
    {
        var div = _document.DocumentNode.SelectNodes("//div[@class='image_container']");
        return div.Descendants("a")
        .Where(node => node.GetAttributeValue("href", "").Any())
        .ToList();
    }

    private void DownloadProductPictures(List<HtmlNode> productPictures)
    {
        DownloadFilesAsync(productPictures, "src", false);
    }

    private List<HtmlNode> GetProductPictures()
    {
        var div = _document.DocumentNode.SelectNodes("//div[@class='item active']");
        return div.Descendants("img")
        .Where(node => node.GetAttributeValue("src", "").Any())
        .ToList();
    }

    private async Task DownloadFilesAsync(List<HtmlNode> pages, string attribute, bool categoryPage)
    {
        List<Task> downloadTasks = new List<Task>();
        foreach(var page in pages)
        {
            downloadTasks.Add(DownloadFileAsync(page, attribute, categoryPage));
        }
        Task.WhenAll(downloadTasks);
    }

    private async Task DownloadFileAsync(HtmlNode page, string attribute, bool categoryPage)
    {
        try
        {
            LogProgress($"Downloading file: { page.GetAttributeValue(attribute, "" )}");
            var uri = new Uri($"{HomeUrl}{page.GetAttributeValue(attribute, "")}");
            if(categoryPage)
            {
                CreateDirectory($"catalogue\\{GetFolderPathFromLink(RemoveChildrenFromPath(page.GetAttributeValue(attribute, "")))}");
                using (WebClient client = new WebClient())
                {
                    client.DownloadFileAsync(new Uri($"{HomeUrl}catalogue/{uri.LocalPath}"), $"{LocalFolderPath}\\catalogue\\{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
                }
            }
            else
            {
                CreateDirectory($"{GetFolderPathFromLink(RemoveChildrenFromPath(page.GetAttributeValue(attribute, "")))}");
                using (WebClient client = new WebClient())
                {
                    client.DownloadFileAsync(new Uri($"{HomeUrl}{uri.LocalPath}"), $"{LocalFolderPath}\\{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            throw;
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

    private void CreateDirectory(string folder)
    {
        Directory.CreateDirectory($"{LocalFolderPath}{folder}");
    }

    private void LogProgress(string log)
    {
        Console.WriteLine(log);
    }

    private void BuildLocalFolderPath()
    {
        LocalFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\ScrapedBooks\\";
    }
}
