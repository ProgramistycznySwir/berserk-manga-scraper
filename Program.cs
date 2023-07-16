using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;



using HttpClient client = new();

string anyPageUrl = "https://readberserk.com/chapter/berserk-chapter-a0/";

var doc = new HtmlDocument();
doc.LoadHtml(await (await client.GetAsync(anyPageUrl)).Content.ReadAsStringAsync());

var chapters = doc.DocumentNode.SelectSingleNode("//select").ChildNodes.Select(node => node.GetAttributeValue("value", string.Empty));

Parallel.ForEach(chapters.OrderBy(x => x), async (chapterUrl, _) =>
{
    if (string.IsNullOrWhiteSpace(chapterUrl))
        return;

    string chapter = string.Concat(
        chapterUrl.Skip(chapterUrl.LastIndexOf("chapter-") + "chapter-".Length).TakeWhile(x => x is not '/'));
    int page = 0, successPages = 0;

    using HttpClient client = new();
    var doc = new HtmlDocument();
    doc.LoadHtml(await (await client.GetAsync(chapterUrl)).Content.ReadAsStringAsync());
    var pages = doc.DocumentNode.SelectNodes("//img[@class='pages__img']").Select(node => node.GetAttributeValue("src", string.Empty));
    foreach (var pageUrl in pages)
    {
        page++;
        if (await DownloadPage(pageUrl, chapter, page) is false)
            continue;
        successPages++;
    }
    await Console.Out.WriteLineAsync(
        $"{(successPages == pages.Count() ? "(OK)" : "[ERR]")} Chapter {chapter} downloaded {successPages}/{pages.Count()} pages!");
});



async Task<bool> DownloadPage(string url, string chapter, int page)
{
    string dirPath = $"Berserk/Berserk_{chapter}";
    string fileName = $"Berserk_{chapter}_{page:D3}.jpg";
    string path = $"{dirPath}/{fileName}";

    EnsurePath(dirPath);
    if (File.Exists(path))
        return true;

    HttpResponseMessage response;
    try
    {
        using HttpClient client = new();
        response = await client.GetAsync(url);
    }
    catch (Exception ex)
    {
        await Err(chapter, page, $"url: {url} Exception:\n{ex.Message}");
        return false;
    }

    if (response.IsSuccessStatusCode is false)
    {
        if (response.StatusCode is HttpStatusCode.RequestTimeout)
            await Err(chapter, page, "HTTP_408: Timeout (just run scraper again, it's no big deal)");
        else
            await Err(chapter, page, $"HTTP_{(int)response.StatusCode}: {response.ReasonPhrase} url: {url}");
        return false;
    }

    using var stream = await response.Content.ReadAsStreamAsync();
    await SaveToFile(path, stream);

    return true;
}

async Task Log(string chapter, int page, string log)
    => await Console.Out.WriteLineAsync($"{chapter}_{page:D3}: {log}");

async Task Err(string chapter, int page, string? log = default)
    => await Console.Error.WriteLineAsync($"{chapter}_{page:D3} ERR: {log}");

void EnsurePath(string path)
{
    if (Directory.Exists(path))
        return;
    Directory.CreateDirectory(path);
}

async Task SaveToFile(string path, Stream stream)
{
    if (File.Exists(path))
        return;

    using var file = File.Create(path);
    stream.Seek(0, SeekOrigin.Begin);
    await stream.CopyToAsync(file);
    file.Close();
}