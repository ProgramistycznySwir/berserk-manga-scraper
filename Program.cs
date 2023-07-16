using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

// async Task Test()
// {
//     string chapterUrl = "https://readberserk.com/chapter/berserk-chapter-364-5/";
//     using HttpClient client = new();
//     var doc = new HtmlDocument();
//     doc.LoadHtml(await (await client.GetAsync(chapterUrl)).Content.ReadAsStringAsync());
//     var pages = doc.DocumentNode.SelectNodes("//img[@class='pages__img']").Select(node => node.GetAttributeValue("src", string.Empty));
//     foreach (var (pageUrl, idx) in pages.Zip(Enumerable.Range(1, pages.Count())))
//         await DownloadPage(pageUrl, "364-5", idx);
// }
// await Test();
// return;

using HttpClient client = new();

string anyPageUrl = "https://readberserk.com/chapter/berserk-chapter-a0/";

var doc = new HtmlDocument();
doc.LoadHtml(await (await client.GetAsync(anyPageUrl)).Content.ReadAsStringAsync());

var chapters = doc.DocumentNode.SelectSingleNode("//select").ChildNodes.Select(node => node.GetAttributeValue("value", string.Empty));

// IEnumerable<ProgressTracker>

await Parallel.ForEachAsync(chapters.OrderBy(x => x), async (chapterUrl, _) => {
    if (string.IsNullOrWhiteSpace(chapterUrl))
        return;

    string chapter = string.Concat(
        chapterUrl.Skip(chapterUrl.LastIndexOf("chapter-") + "chapter-".Length).TakeWhile(x => x is not '/'));
    Console.WriteLine($"\nCHAPTER {chapter}");
    int page = 0;

    using HttpClient client = new();
    var doc = new HtmlDocument();
    doc.LoadHtml(await (await client.GetAsync(chapterUrl)).Content.ReadAsStringAsync());
    var pages = doc.DocumentNode.SelectNodes("//img[@class='pages__img']").Select(node => node.GetAttributeValue("src", string.Empty));
    foreach (var pageUrl in pages)
    {
        page++;

        Log(chapter, page, "...");
        if (await DownloadPage(pageUrl, chapter, page) is false)
            break;
        Log(chapter, page, "OK");
    }
});

async Task<bool> DownloadPage(string url, string chapter, int page)
{
    string dirPath = $"Berserk/Berserk_{chapter}";
    string fileName = $"Berserk_{chapter}_{page:D3}.jpg";
    string path = $"{dirPath}/{fileName}";

    EnsurePath(dirPath);
    if (File.Exists(path))
    {
        Log(chapter, page, "skipped...");
        return true;
    }

    using HttpClient client = new();
    using var response = await client.GetAsync(url);

    if (response.IsSuccessStatusCode is false)
    {
        Log(chapter, page, "ERR");
        return false;
    }

    using var stream = await response.Content.ReadAsStreamAsync();
    await SaveToFile(path, stream);

    return true;
}

void Log(string chapter, int page, string log)
    => Console.WriteLine($"{chapter}_{page:D3}: {log}");

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