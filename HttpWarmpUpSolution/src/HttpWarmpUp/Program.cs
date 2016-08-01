using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using HttpWarmpUp.Properties;

namespace HttpWarmpUp
{
    class Program
    {
        

        static void Main(string[] args)
        {
            new Program().Run(args);
        }

        void Run(string[] args)
        {
            string urlToScan = string.Empty;
            int depthLevel = 2;
            int repeatCount = 1;
            bool makeTracking = true;
            bool skipExternal = true;
            string skipUrlsContaining = String.Empty;

            try
            {
                if (args.Length == 1)
                {
                    urlToScan = args[0];
                }else if (args.Length == 5 || args.Length == 6)
                {
                    urlToScan = args[0];
                    depthLevel = Convert.ToInt32(args[1]);
                    repeatCount = Int32.Parse(args[2]);
                    skipExternal = Boolean.Parse(args[3]);
                    makeTracking = Boolean.Parse(args[4]);
                    if (args.Length == 6)
                    {
                        skipUrlsContaining = args[5];
                    }
                }
                else if (args.Length < 5)
                {
                    ShowUsage();
                    return;
                }


                Log.WriteTabbedInfo(0, "Repeat Count:{1}. Depth:{2}. Skip External Links:{3}. Make Tracking Requests:{4}", urlToScan, repeatCount, depthLevel, skipExternal.ToString(), makeTracking.ToString());


                for (int i = 0; i < repeatCount; i++)
                {
                    Stopwatch counter = Stopwatch.StartNew();

                    Log.WriteTabbedInfo(0, ">>>>>>>> Round {0}", i + 1);
                    Collection<Uri> visitedUris = new Collection<Uri>();

                    HttpClient.CurrentCredentialCache = GetCredentialCache(urlToScan); ;

                    //HttpClient.CrawlUrl(null, new Uri(urlToScan), 0, depthLevel, visitedUris, skipExternal);
                    HttpClient.ScanUrl(new Uri(urlToScan), depthLevel, visitedUris, skipExternal, makeTracking);
                    counter.Stop();
                    Log.WriteTabbedInfo(0, ">>>>>>>> Round {0} Total Time:{1} Visited Links:{2}", i + 1, counter.Elapsed, visitedUris.Count);
                    Log.WriteTabbedInfo(0, "");
                }
            }
            catch (Exception ex)
            {
                Log.WriteTabbedError(0, "Error: {0}", ex.ToString());
                Log.WriteTabbedInfo(0, "");
                ShowUsage();
            }
            finally
            {
                Log.FlushInfoToEventLog();
                Log.FlushWarningsToEventLog();
                Log.FlushErrorsToEventLog();
            }
        }

        private static CredentialCache GetCredentialCache(string urlToScan)
        {
            NetworkCredential credentials = null;
            CredentialCache credCache = null;
            if (Settings.Default.UserName.Length != 0)
            {
                credentials = new NetworkCredential(Settings.Default.UserName, Settings.Default.Password, Settings.Default.Domain);
                credCache = new CredentialCache();
                Uri requestedUri = new Uri(urlToScan);
                Uri credCacheUri = new Uri(requestedUri.Scheme + "://" + requestedUri.Authority);

                credCache.Add(credCacheUri, "NTLM", credentials);
            }
            return credCache;
        }
        private void ShowUsage()
        {
            Log.WriteTabbedText(0, "Usage: HttpWarmp <URL> [<DEPTH_LEVEL> <REPEAT_COUNT> <SKIP_EXTERNAL_LINKS> <MAKE_TRACKING_REQUESTS>]\r\n");
        }
    }
}
