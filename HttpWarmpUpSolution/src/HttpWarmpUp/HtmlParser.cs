using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace HttpWarmpUp
{
    public static class HtmlParser
    {
        public static Collection<Uri> ExtracLinks(string htmlContent, Uri baseUri, bool skipExternals, Collection<Uri> visited)
        {
            Collection<Uri> result = new Collection<Uri>();
            if (!String.IsNullOrEmpty(htmlContent))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(htmlContent);

                AddHrefLinks(baseUri, result, doc, skipExternals, visited);

                AddScriptLinks(baseUri, result, doc, skipExternals, visited);

                AddImgLinks(baseUri, result, doc, skipExternals, visited);

            }
            return result;
        }




        private static void AddImgLinks(Uri baseUri, Collection<Uri> result, HtmlDocument doc, bool skipExternals, Collection<Uri> visited)
        {
            HtmlNodeCollection images = doc.DocumentNode.SelectNodes("//img[@src]");
            if (images != null)
            {
                foreach (HtmlNode img in images)
                {
                    string href = FilterUrl(img.Attributes["src"].Value.Trim(), baseUri, skipExternals, visited);
                    if (href.Length > 0)
                    {
                        Uri foundUri = new Uri(href);
                        if (!result.Contains(foundUri))
                        {
                            result.Add(foundUri);
                        }
                    }
                }
            }
        }

        private static void AddHrefLinks(Uri baseUri, Collection<Uri> result, HtmlDocument doc, bool skipExternals, Collection<Uri> visited)
        {
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (HtmlNode link in links)
                {
                    string href = FilterUrl(link.Attributes["href"].Value.Trim(), baseUri, skipExternals, visited);

                    if (href.Length > 0)
                    {
                        Uri foundUri = new Uri(href);
                        if (!result.Contains(foundUri))
                        {
                            result.Add(foundUri);
                        }
                    }
                }
            }
        }

        private static void AddScriptLinks(Uri baseUri, Collection<Uri> result, HtmlDocument doc, bool skipExternals, Collection<Uri> visited)
        {
            HtmlNodeCollection links = doc.DocumentNode.SelectNodes("//script[@src]");
            if (links != null)
            {
                foreach (HtmlNode link in links)
                {
                    string href = FilterUrl(link.Attributes["src"].Value.Trim(), baseUri, skipExternals,  visited);

                    if (href.Length > 0)
                    {
                        Uri foundUri = new Uri(href);
                        if (!result.Contains(foundUri))
                        {
                            result.Add(foundUri);
                        }
                    }
                }
            }
        }


        static private string FilterUrl(string u, Uri baseUri, bool skipExternals, Collection<Uri> visited)
        {
            string result = string.Empty;
            if (!u.ToLower().Contains("javascript:") && Uri.IsWellFormedUriString(u, UriKind.RelativeOrAbsolute))
            {
                Uri uri = new Uri(u, UriKind.RelativeOrAbsolute);
                if (!visited.Contains(uri))
                {
                    if (uri.IsAbsoluteUri && uri != baseUri)
                    {
                        if (!skipExternals || 
                            uri.DnsSafeHost.ToLower() == baseUri.DnsSafeHost.ToLower())
                        {
                            result = u;
                        }
                    }
                    else
                    {
                        result = new Uri(baseUri, u).ToString();

                    }
                }
            }
            return result;
        }
    }
}
