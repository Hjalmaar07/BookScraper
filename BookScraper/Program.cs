using BookScraper.Domain.Book.Service;
using BookScraper.Domain.Book.UseCase;
using BookScraper.Driver.BooksToScrap;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

IHost _host = Host.CreateDefaultBuilder().ConfigureServices(
    services =>
    {
        services.AddSingleton<IBookService, BookService>();
        services.AddSingleton<IGetBooksUseCase, GetBooksUseCase>();
        services.AddSingleton<IBooksToScrapRepository, BooksToScrapRepository>();
    })
    .Build();
var app = _host.Services.GetService<IBookService>();
app.GetBooks();

Console.WriteLine("Press Enter to exit...");
ConsoleKeyInfo keyInfo = Console.ReadKey(true);
if (keyInfo.Key == ConsoleKey.Enter)
{
    Environment.Exit(0);
}