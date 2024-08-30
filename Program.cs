namespace RedShelfDownloader;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

class Program
{
    static string targetDirectoryImg = "tmp";
    static string baseUrl = "";
    static int numpages = 100;
    static string pdfFile = "output.pdf";

    static async Task Main(string[] args)
    {
        Console.WriteLine("Paste the cookies: ");
        var cookies = Console.ReadLine();

        Console.WriteLine("Format: https://platform.virdocs.com/rscontent/epub/XXXXXX/XXXXXXXX/OEBPS/");
        Console.WriteLine("Enter the base URL of the image: ");
        baseUrl += Console.ReadLine();
        baseUrl += "images/page-{0}.jpg";

        Console.WriteLine("Enter how many pages in the book: ");
        numpages = int.Parse(Console.ReadLine());

        Directory.CreateDirectory(targetDirectoryImg);

        if (cookies != null)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Cookie", cookies);

                var images = new List<(int, string)>();

                for (int i = 1; i <= numpages; i++)
                {
                    string url = string.Format(baseUrl, i);
                    string fileName = Path.Combine(targetDirectoryImg, $"{i}.jpg");

                    try
                    {
                        byte[] imageBytes = await httpClient.GetByteArrayAsync(url);
                        await File.WriteAllBytesAsync(fileName, imageBytes);
                        Console.WriteLine($"Image {fileName} successfully downloaded.");
                        images.Add((i, fileName));
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine($"Could not download the {fileName}. Error: {e.Message}");
                        Console.ReadLine();
                    }
                }

                Console.WriteLine("Sorting images by page number...");
                images = images.OrderBy(x => x.Item1).ToList();

                using (var document = new Document(PageSize.A4, 30, 30, 30, 30))
                {
                    using (var writer = PdfWriter.GetInstance(document, new FileStream(pdfFile, FileMode.Create)))
                    {
                        document.Open();

                        foreach (var (_, imagePath) in images)
                        {
                            var image = Image.GetInstance(imagePath);
                            image.ScaleToFit(PageSize.A4.Width - 60, PageSize.A4.Height - 60);
                            document.Add(image);
                            document.NewPage();
                        }
                        document.Close();
                    }
                }

                Console.WriteLine($"PDF created: {pdfFile}");
                Directory.Delete(targetDirectoryImg, true);
            }
        }
        else
        {
            Console.WriteLine("Cookies error!");
        }
    }
}