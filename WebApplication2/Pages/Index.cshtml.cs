using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Rainbow.MergeEngine;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using LogLevel = OpenQA.Selenium.LogLevel;
using OpenQA.Selenium.Remote;
using Newtonsoft.Json.Linq;

namespace WebApplication2.Pages
{
    public class IndexModel : PageModel
    {
        public string Link1 { get; set; } = string.Empty;
        public string Link2 { get; set; } = string.Empty;
        public string HighlightedDifference { get; set; } = string.Empty;
        public List<LinkStatus> LinksStatus1 { get; set; } = new List<LinkStatus>();
        public List<LinkStatus> LinksStatus2 { get; set; } = new List<LinkStatus>();
        public List<byte[]> DifferentImages { get; set; } = new List<byte[]>();
        public List<string> StyleDifferences { get; set; } = new List<string>();

        public bool IsMobileResponsive1 { get; set; } = false; // If Link1 is mobile responsive
        public bool IsMobileResponsive2 { get; set; } = false; // If Link2 is mobile responsive
        public int ExternalScriptsCount1 { get; set; } = 0; // Number of external scripts in Link1
        public int ExternalScriptsCount2 { get; set; } = 0; // Number of external scripts in Link2
        public int ExternalStylesheetsCount1 { get; set; } = 0; // Number of external stylesheets in Link1
        public int ExternalStylesheetsCount2 { get; set; } = 0; // Number of external stylesheets in Link2
        public Dictionary<string, double?> PerformanceMetrics1 { get; set; } = new Dictionary<string, double?>();
        public Dictionary<string, double?> PerformanceMetrics2 { get; set; } = new Dictionary<string, double?>();


        public class LinkStatus
        {
            public string Url { get; set; } = string.Empty;
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
                // Set up Selenium WebDriver
                var options = new ChromeOptions();
                options.AddArgument("--headless"); // This runs Chrome in the background
                options.AddArgument("--disable-gpu");
                options.SetLoggingPreference("performance", OpenQA.Selenium.LogLevel.All);

                using var driver = new ChromeDriver(options);
                try
                {
                // Load the first link and get its source
                driver.Navigate().GoToUrl(Link1);
                var html1 = driver.PageSource;
                PerformanceMetrics1 =  GetPerformanceMetrics(driver, Link1);


                    // Load the second link and get its source
                    driver.Navigate().GoToUrl(Link2);
                var html2 = driver.PageSource;
                PerformanceMetrics2 =  GetPerformanceMetrics(driver, Link2);

                // Check for mobile responsiveness
                IsMobileResponsive1 = html1.Contains("<meta name=\"viewport\"");
                IsMobileResponsive2 = html2.Contains("<meta name=\"viewport\"");
              

                var doc1 = new HtmlDocument();
                doc1.LoadHtml(html1);
                var classAttributes1 = doc1.DocumentNode.SelectNodes("//div[@class] | //a[@class]").Select(n => n.Attributes["class"].Value).ToList();
                LinksStatus1 = await GetLinksStatus(doc1, httpClient, Link1);
                ExternalScriptsCount1 = doc1.DocumentNode.SelectNodes("//script[@src]")?.Count ?? 0;
                ExternalStylesheetsCount1 = doc1.DocumentNode.SelectNodes("//link[@rel='stylesheet']")?.Count ?? 0;


                var doc2 = new HtmlDocument();
                doc2.LoadHtml(html2);
                var classAttributes2 = doc2.DocumentNode.SelectNodes("//div[@class] | //a[@class]").Select(n => n.Attributes["class"].Value).ToList();
                LinksStatus2 = await GetLinksStatus(doc2, httpClient, Link2);
                ExternalScriptsCount2 = doc2.DocumentNode.SelectNodes("//script[@src]")?.Count ?? 0;
                ExternalStylesheetsCount2 = doc2.DocumentNode.SelectNodes("//link[@rel='stylesheet']")?.Count ?? 0;

                // Use the Merger from Rainbow.MergeEngine
                var merger = new Merger(html1, html2);
                HighlightedDifference = merger.merge();

                // Image comparison
                var imagesFromLink1 = await GetImagesFromDoc(doc1, Link1, httpClient);
                var imagesFromLink2 = await GetImagesFromDoc(doc2, Link2, httpClient);

                DifferentImages = imagesFromLink2
                    .Where(img2 => !imagesFromLink1.Any(img1 => GenerateSha256Hash(img1) == GenerateSha256Hash(img2)))
                    .ToList();

                }
                finally
                {
                    driver.Quit();
                }

            }
        }


        public Dictionary<string, double?> GetPerformanceMetrics(ChromeDriver driver, string url)
        {
            driver.Navigate().GoToUrl(url);

            var performanceTiming = (Dictionary<string, object>)driver.ExecuteScript(
                "var performance = window.performance || window.webkitPerformance || window.mozPerformance || window.msPerformance || {}; var timings = performance.timing || {}; return timings;"
            );

            double? navigationStart = GetDoubleValue(performanceTiming, "navigationStart");
            double? loadEventEnd = GetDoubleValue(performanceTiming, "loadEventEnd");

            double? ttfbMs = CalculateTimingDifference(performanceTiming, "responseStart", "requestStart");
            double? domContentLoadedMs = CalculateTimingDifference(performanceTiming, "domContentLoadedEventEnd", "navigationStart");
            double? totalResponseTimeMs = CalculateTimingDifference(performanceTiming, "responseEnd", "requestStart");
            double? loadTimeS = CalculateTimingDifference(navigationStart, loadEventEnd) / 1000.0; // Convert to seconds

            var performanceMetrics = new Dictionary<string, double?>
    {
        { "TotalLoadTimeS", loadTimeS },
        { "TTFBMs", ttfbMs },
        { "DOMContentLoadedMs", domContentLoadedMs },
        { "TotalResponseTimeMs", totalResponseTimeMs }
    };

            return performanceMetrics;
        }

        private double? GetDoubleValue(Dictionary<string, object> timingData, string key)
        {
            if (timingData.ContainsKey(key) && timingData[key] is long)
            {
                return Convert.ToDouble((long)timingData[key]);
            }
            return null;
        }

        private double? CalculateTimingDifference(Dictionary<string, object> timingData, string endKey, string startKey)
        {
            double? endTime = GetDoubleValue(timingData, endKey);
            double? startTime = GetDoubleValue(timingData, startKey);

            if (endTime != null && startTime != null)
            {
                return endTime - startTime;
            }

            return null;
        }

        private double? CalculateTimingDifference(double? startTime, double? endTime)
        {
            if (startTime != null && endTime != null)
            {
                return endTime - startTime;
            }

            return null;
        }


        private async Task<List<LinkStatus>> GetLinksStatus(HtmlDocument doc, HttpClient httpClient, string baseUrl)
        {
            var linksStatus = new List<LinkStatus>();
            if (doc.DocumentNode.SelectNodes("//a") != null)
            {
                foreach (var linkNode in doc.DocumentNode.SelectNodes("//a"))
                {
                    var linkUrl = linkNode.GetAttributeValue("href", "");

                    // Check if the URL is relative and combine with the base URL
                    if (!linkUrl.StartsWith("http://") && !linkUrl.StartsWith("https://"))
                    {
                        linkUrl = new Uri(new Uri(baseUrl), linkUrl).ToString();
                    }

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
            var web = new HtmlWeb();

            // Fetching regular images
            if (doc.DocumentNode.SelectNodes("//img") != null)
            {
                foreach (var img in doc.DocumentNode.SelectNodes("//img"))
                {
                    var imageUrl = img.GetAttributeValue("src", "");
                    await AddImageFromUrl(imageUrl, baseUrl, httpClient, images);
                }
            }

            // Fetching icons
            if (doc.DocumentNode.SelectNodes("//link[@rel='icon']") != null)
            {
                foreach (var link in doc.DocumentNode.SelectNodes("//link[@rel='icon']"))
                {
                    var iconUrl = link.GetAttributeValue("href", "");
                    await AddImageFromUrl(iconUrl, baseUrl, httpClient, images);
                }
            }

            return images;
        }

        private async Task AddImageFromUrl(string url, string baseUrl, HttpClient httpClient, List<byte[]> imageList)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                url = new Uri(new Uri(baseUrl), url).ToString();
            }

            try
            {
                var imageData = await httpClient.GetByteArrayAsync(url);
                imageList.Add(imageData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching image from URL: {url}. Error message: {ex.Message}");
            }
        }


        private string GenerateSha256Hash(byte[] inputData)
        {
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(inputData);
            return BitConverter.ToString(hashBytes).Replace("-", "");
        }
    }
}
