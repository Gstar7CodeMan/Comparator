using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Rainbow.MergeEngine;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace WebApplication2.Pages
{
    public class IndexModel : PageModel
    {
        public string Link1 { get; set; }
        public string Link2 { get; set; }
        public string HighlightedDifference { get; set; }
        public List<LinkStatus> LinksStatus1 { get; set; } = new List<LinkStatus>();
        public List<LinkStatus> LinksStatus2 { get; set; } = new List<LinkStatus>();
        public List<byte[]> DifferentImages { get; set; } = new List<byte[]>();

        public class LinkStatus
        {
            public string Url { get; set; }
            public bool IsBroken { get; set; }
            public int StatusCode { get; set; }
        }

        public async Task OnGetAsync(string link1, string link2)
        {
            Link1 = link1;
            Link2 = link2;

            using var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(Link1) && !string.IsNullOrEmpty(Link2))
            {
                var html1 = await httpClient.GetStringAsync(Link1);
                var html2 = await httpClient.GetStringAsync(Link2);

                // Use the Merger from Rainbow.MergeEngine
                var merger = new Merger(html1, html2);
                HighlightedDifference = merger.merge();

                var doc1 = new HtmlDocument();
                doc1.LoadHtml(html1);
                LinksStatus1 = await GetLinksStatus(doc1, httpClient);

                var doc2 = new HtmlDocument();
                doc2.LoadHtml(html2);
                LinksStatus2 = await GetLinksStatus(doc2, httpClient);

                // Images comparison
                var imagesFromLink1 = await GetImagesFromDoc(doc1, Link1, httpClient);
                var imagesFromLink2 = await GetImagesFromDoc(doc2, Link2, httpClient);

                DifferentImages = imagesFromLink2
                    .Where(img2 => !imagesFromLink1.Any(img1 => GenerateSha256Hash(img1) == GenerateSha256Hash(img2)))
                    .ToList();
            }
        }

        private async Task<List<LinkStatus>> GetLinksStatus(HtmlDocument doc, HttpClient httpClient)
        {
            var linksStatus = new List<LinkStatus>();
            if (doc.DocumentNode.SelectNodes("//a") != null)
            {
                foreach (var linkNode in doc.DocumentNode.SelectNodes("//a"))
                {
                    var linkUrl = linkNode.GetAttributeValue("href", "");
                    var linkStatus = new LinkStatus { Url = linkUrl };

                    try
                    {
                        var response = await httpClient.GetAsync(linkUrl);
                        linkStatus.IsBroken = !response.IsSuccessStatusCode;
                        linkStatus.StatusCode = (int)response.StatusCode;
                    }
                    catch
                    {
                        linkStatus.IsBroken = true;
                        linkStatus.StatusCode = 404; // Not found status code
                    }
                    linksStatus.Add(linkStatus);
                }
            }
            return linksStatus;
        }

        private async Task<List<byte[]>> GetImagesFromDoc(HtmlDocument doc, string baseUrl, HttpClient httpClient)
        {
            var images = new List<byte[]>();
            if (doc.DocumentNode.SelectNodes("//img") != null)
            {
                foreach (var img in doc.DocumentNode.SelectNodes("//img"))
                {
                    var imageUrl = img.GetAttributeValue("src", "");
                    if (!Uri.IsWellFormedUriString(imageUrl, UriKind.Absolute))
                    {
                        imageUrl = new Uri(new Uri(baseUrl), imageUrl).ToString();
                    }
                    var imageData = await httpClient.GetByteArrayAsync(imageUrl);
                    images.Add(imageData);
                }
            }
            return images;
        }

        private string GenerateSha256Hash(byte[] inputData)
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(inputData);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}