using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using System.Collections.ObjectModel;
using System.Diagnostics;
using HttpWarmpUp.Properties;
using System.Text.RegularExpressions;

namespace HttpWarmpUp
{
    public class HttpClient
    {
        public static CredentialCache CurrentCredentialCache { get; set; }

        public static void ScanUrl(Uri uri, int depth, Collection<Uri> visitedUris, bool skipExternals, bool makeTrackingRequest)
        {
            string pageContent = RequestUriToScan(null, uri, 0, makeTrackingRequest, visitedUris);
            PreCrawlUrl(uri, pageContent, 1, depth, visitedUris, skipExternals,  makeTrackingRequest);
        }


        public static void PreCrawlUrl(Uri pageUri, String pageContent, int currentLevel, int depth, Collection<Uri> visitedUris, bool skipExternals, bool makeTrackingRequest)
        {
            if (currentLevel <= depth)
            {
                Collection<Uri> relatedLinks = HtmlParser.ExtracLinks(pageContent, pageUri, skipExternals, visitedUris);
                if (relatedLinks.Count > 0)
                {
                    Log.WriteTabbedInfo(currentLevel, "[*] Current Level: {0}.  Scaning {1} links in {2}", currentLevel, relatedLinks.Count, pageUri.ToString());
                    
                    Dictionary<Uri, String> pagesToScan = new Dictionary<Uri, String>();
                    foreach (Uri target in relatedLinks)
                    {
                        string html = RequestUriToScan(pageUri, target, currentLevel, makeTrackingRequest, visitedUris);
                        if (!pagesToScan.Keys.Contains(target) && currentLevel < depth)
                        {
                            pagesToScan.Add(target, html);
                        }
                    }
                    foreach (Uri target in pagesToScan.Keys)
                    {
                        PreCrawlUrl(target, pagesToScan[target], currentLevel + 1, depth, visitedUris, skipExternals, makeTrackingRequest);
                    }
                }
            }
        }

        public static void CrawlUrl(Uri referrer, Uri uriToScan, int currentLevel, int depth, Collection<Uri> visitedUris, bool skipExternals, bool makeTrackingRequest)
        {
            string html = RequestUriToScan(referrer, uriToScan, currentLevel, makeTrackingRequest, visitedUris);

            if (currentLevel < depth)
            {
                Collection<Uri> uris = HtmlParser.ExtracLinks(html, uriToScan, skipExternals, visitedUris);
                if (uris.Count > 0)
                {
                    Log.WriteTabbedInfo(currentLevel, "Current Level: {0}, Dependant Links: {1}", currentLevel + 1, uris.Count);
                }
                foreach (Uri candidateUri in uris)
                {
                    CrawlUrl(uriToScan, candidateUri, currentLevel + 1, depth, visitedUris, makeTrackingRequest, skipExternals);
                }
            }
        }

        private static string RequestUriToScan(Uri referrer, Uri uriToScan, int currentLevel, bool makeTrackingRequest, Collection<Uri> visitedUris)
        {
            string html = string.Empty;
            if (NotIsVisitedUri(uriToScan, visitedUris))
            {

                html = HttpClient.MakeRequest((referrer!=null)?referrer.AbsoluteUri:String.Empty, uriToScan.AbsoluteUri, currentLevel, visitedUris);
                if (makeTrackingRequest && IsArticle(uriToScan))
                {
                    HttpClient.MakeRequest(uriToScan.AbsoluteUri, GetTrackingUrl(uriToScan), currentLevel, visitedUris);
                }
                RegisterVisitedUri(visitedUris, uriToScan);
            }
            return html;
        }

        private static string GetTrackingUrl(Uri uriToScan)
        {
            return uriToScan.Scheme + "://tr." + uriToScan.DnsSafeHost + "/lomascount.gif";
        }

        private static bool IsArticle(Uri uriToScan)
        {
            string regex = @".+[/](genteycultura|mundo|espana|economia|sucesos|local)[/]\d\d\d\d[/]\d\d\d\d[/]actualidad[/].+aspx|.+[/]\d\d\d\d[/](genteycultura|mundo|espana|economia|sucesos|local)[/]\d\d\d\d[/]actualidad[/].+aspx|.+[/](especiales)[/].+[/]actualidad[/].+aspx|.+[/](videos|fotos)[/]actualidad[/]ficha.+aspx\?.+";
            RegexOptions options = (RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            Regex reg = new Regex(regex, options);
            return reg.IsMatch(uriToScan.ToString());
        }

        private static bool NotIsVisitedUri(Uri uriToScan, Collection<Uri> visitedUris)
        {
            return !visitedUris.Contains(uriToScan);
        }

        internal static string MakeRequest(string referrer, string url, int urlLevel, Collection<Uri>visitedLinks)
        {
            WebResponse resp = null;
            string result = string.Empty;
            try
            {
                WebRequest req = WebRequest.Create(System.Web.HttpUtility.HtmlDecode(url));        
                req.Credentials = CurrentCredentialCache;

                req.Timeout = Settings.Default.TimeOutInSeconds * 1000;

                HttpWebRequest httpReq = req as HttpWebRequest;
                if (httpReq != null)
                {
                    httpReq.UserAgent = String.Format("Terra.HttpWarmUp/1.0 ({0}) From {1}", System.Environment.OSVersion, System.Environment.MachineName);
                    httpReq.Referer = referrer;
                }

                Stopwatch requestWatch = Stopwatch.StartNew();
                using (resp = req.GetResponse())
                {
                    RegisterVisitedUri(visitedLinks, resp.ResponseUri);
                    using (Stream s = resp.GetResponseStream())
                    {
                        using (StreamReader sr = new StreamReader(s))
                        {
                            result = sr.ReadToEnd();
                            requestWatch.Stop();
                            int statusCode = GetStatusCode(resp);
                            string frontend = GetFrontEnd(resp);
                            ReportElapsedTime(referrer, url, statusCode, requestWatch.Elapsed, urlLevel, frontend);
                        }
                    }
                }
            }
            catch (WebException wex)
            {
                ReportRequestError(referrer, url, urlLevel, wex);
            }
            catch (Exception ex)
            {
                Log.WriteTabbedError(urlLevel, "Error al preparar la petición. Url solicitada {0}. Mensaje de Error: {1}", url, ex.Message);
            }
            return result;
        }

        private static string GetFrontEnd(WebResponse resp)
        {
            string frontend = "UNK";
            if (resp != null && resp.Headers != null && resp.Headers.AllKeys.Contains<string>("f"))
            {
                frontend = resp.Headers["f"];
            }
            if (resp != null && resp.Headers != null && resp.Headers.AllKeys.Contains<string>("Server") && resp.Headers["Server"].StartsWith("NetCache"))
            {
                frontend = "NTC";
            }
            if (frontend == "UNK" && resp != null && resp.Headers != null && resp.Headers.AllKeys.Contains<string>("Server"))
            {
                frontend = resp.Headers["Server"];
            }
            return frontend;
        }

        private static void ReportRequestError(string referrer, string url, int urlLevel, WebException wex)
        {
            string statusInfo = "";
            string frontEnd = "UKN";
            HttpWebResponse httpResp = wex.Response as HttpWebResponse;
            if (httpResp != null)
            {
                statusInfo = ((int)httpResp.StatusCode).ToString();
                frontEnd = GetFrontEnd(httpResp);
            }
            else
            {
                statusInfo = wex.Status.ToString();
            }

            Log.WriteTabbedError(urlLevel, "ERROR".PadLeft(16, ' ') + "\t[{0},{1}]\t{2}\t{3}\tReferrer: {4}",
                frontEnd, statusInfo, url,
                wex.Message, referrer);
        }

        private static int GetStatusCode(WebResponse resp)
        {
            int statusCode = -1;
            HttpWebResponse httpResp = resp as HttpWebResponse;
            if (httpResp != null)
            {
                
                string statusCodeDesc = httpResp.StatusDescription;
                statusCode = (int)httpResp.StatusCode;
            }
            return statusCode;
        }

        private static void RegisterVisitedUri(Collection<Uri> visitedLinks, Uri uri)
        {
            if (visitedLinks != null)
            {
                if (!visitedLinks.Contains(uri))
                {
                    visitedLinks.Add(uri);
                }
            }
        }

        private static void ReportElapsedTime(string referrer, string url, int statusCode, TimeSpan taken, int urlLevel, string frontend)
        {
            string urlData = new Uri(url).PathAndQuery;
            if(url.Contains("lomascount.gif"))
            {
                urlData = "Tracking for URL:" + new Uri(referrer).PathAndQuery; ;
            }
            if (taken.Seconds > Settings.Default.WarnTimeTakenInSeconds)
            {
                Log.WriteTabbedWarning(urlLevel, "{0}\t[{1},{2}]\t{3}",
                taken.ToString(), frontend.PadLeft(3), statusCode,
                urlData);
            }
            else
            {
                Log.WriteTabbedInfo(urlLevel, "{0}\t[{1},{2}]\t{3}",
                taken.ToString(), frontend.PadLeft(3), statusCode,
                urlData);
            }
        }
    }
}
