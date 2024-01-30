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
        LogProgress("Getting list of pages...");
        BuildLocalFolderPath();
        await CreateHtmlDocument(HomeUrl);
        await DownloadExternalLinks();
        await DownloadCategoryPages();
        await TraverseCategories(GetNodesForContains("a", "href", "/books/"));
    }

    private void CreateDirectory(string folder)
    {
        Directory.CreateDirectory($"{LocalFolderPath}{folder}");
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
        return Path.GetDirectoryName(link);
    }

    private async Task DownloadExternalLinks()
    {
        var pages = GetNodesForAny("link");
        await DownloadFiles(pages, "href", false);
    }

    private async Task DownloadCategoryPages()
    {
        var pages = GetNodesForAny("a");
        await DownloadFiles(pages, "href", false);
    }

    private async Task TraverseCategories(List<HtmlNode> pages)
    {
        foreach (var page in pages)
        {
            await CreateHtmlDocument(HomeUrl + page.GetAttributeValue("href", ""));
            await DownloadThumbnails();
            var productsList = GetProductPages();
            foreach (var product in productsList)
            {
                await CreateHtmlDocument(HomeUrl + "catalogue/" + RemoveChildrenFromPath(product.GetAttributeValue("href", "")));
                await DownloadProductPage(product);
                await DownloadProductPictures();
            }
        }
    }

    private async Task DownloadThumbnails()
    {
        var pages = GetNodesForContains("img", "class", "thumbnail");
        await DownloadFiles(pages, "src", false);

    }

    private List<HtmlNode> GetProductPages()
    {
        var div = _document.DocumentNode.SelectNodes("//div[@class='image_container']");
        return div.Descendants("a")
        .Where(node => node.GetAttributeValue("href", "").Any())
        .ToList();
    }

    private async Task DownloadProductPage(HtmlNode page)
    {
        DownloadFile(page, "href", true);
    }

    private async Task DownloadProductPictures()
    {
        var div = _document.DocumentNode.SelectNodes("//div[@class='item active']");
        var pages = div.Descendants("img")
        .Where(node => node.GetAttributeValue("src", "").Any())
        .ToList();
        await DownloadFiles(pages, "src", false);
    }

    private async Task DownloadFiles(List<HtmlNode> pages, string attribute, bool categoryPage)
    {
        foreach(var page in pages)
        {
            await DownloadFile(page, attribute, categoryPage);
        }
    }

    private async Task DownloadFile(HtmlNode page, string attribute, bool categoryPage)
    {
        try
        {
            var uri = new Uri($"{HomeUrl}{page.GetAttributeValue(attribute, "")}");
            if(categoryPage)
            {
                CreateDirectory($"catalogue\\{GetFolderPathFromLink(RemoveChildrenFromPath(page.GetAttributeValue(attribute, "")))}");
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile($"{HomeUrl}catalogue/{uri.LocalPath}", $"{LocalFolderPath}\\catalogue\\{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
                }
            }
            else
            {
                CreateDirectory($"{GetFolderPathFromLink(RemoveChildrenFromPath(page.GetAttributeValue(attribute, "")))}");
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile($"{HomeUrl}{uri.LocalPath}", $"{LocalFolderPath}\\{GetFolderPathFromLink(uri.LocalPath)}\\{Path.GetFileName(uri.LocalPath)}");
                }
            }
        }
        catch (Exception)
        {
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

    private void LogProgress(string log)
    {
        Console.WriteLine(log);
    }

    private void BuildLocalFolderPath()
    {
        LocalFolderPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Books\\";
    }
}
