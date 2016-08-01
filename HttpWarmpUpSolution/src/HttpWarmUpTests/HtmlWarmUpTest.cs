using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using HttpWarmpUp;
using System.IO;

namespace HttpWarmUpTests
{
    [TestClass]
    public class HtmlWarmUpTest
    {

        const string HREFs = "<html><body><a  href   =    \"Level2_AA.htm \"      >Level 2 Link AA</a><img width=\"107\" height=\"18\" border=\"0\" align=\"absmiddle\" alt=\"Iniciar Sesión\" src=\"/nav/img_cabec/iniciarsesion.gif\" /><a href='Level2_ZZ.htm'>Level 2 Link ZZ</a><a href=Level2_MM.htm >Level 2 Link MM</a><a  href   =    \"Level2_JJ.htm \"      >Level 2 Link JJ</a><a href=Level2_PP.htm>Level 2 Link PP</a></body></html>";
        const bool SHOW_SKIPPED = false;

        Uri baseSite = new Uri("file://" + Path.Combine(System.Environment.CurrentDirectory, "HtmlLinkedFiles"));

        [TestMethod]
        public void ExtractLinks()
        {
            Collection<Uri> urisToRequest = HtmlParser.ExtracLinks(HREFs, baseSite, true, new Collection<Uri>());
            Assert.AreEqual(6, urisToRequest.Count, "El numero de URLs encontradas no es el esperado");
        }

        [TestMethod]
        public void ExtractLinksAndImages()
        {
            Collection<Uri> urisToRequest = HtmlParser.ExtracLinks(HREFs, baseSite, true, new Collection<Uri>());
            Assert.AreEqual(6, urisToRequest.Count, "El numero de URLs encontradas no es el esperado");
        }

        [TestMethod]
        public void CrawlDepth_0()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.CrawlUrl(null, new Uri(baseSite, "Root.htm"), 0, 0, visited, true, false);

            Assert.AreEqual(1, visited.Count);
        }

        [TestMethod]
        public void ScanUrlDepth_0()
        {
            Collection<Uri> visited = new Collection<Uri>();

            HttpClient.ScanUrl(new Uri(baseSite, "Root.htm"), 0, visited, true, false);

            Assert.AreEqual(1, visited.Count);
        }

        [TestMethod]
        public void CrawlDepth_1()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.CrawlUrl(null, new Uri(baseSite, "Root.htm"), 0, 1, visited, true, false);

            Assert.AreEqual(6, visited.Count);
        }

        [TestMethod]
        public void ScanUrlDepth_1()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.ScanUrl(new Uri(baseSite, "Root.htm"), 1, visited, true, false);

            Assert.AreEqual(6, visited.Count);
        }


        [TestMethod]
        public void CrawlDepth_2()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.CrawlUrl(null, new Uri(baseSite, "Root.htm"), 0, 2, visited, true, false);

            Assert.AreEqual(18, visited.Count);
        }

        [TestMethod]
        public void ScanUrlDepth_2()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.ScanUrl(new Uri(baseSite, "Root.htm"), 2, visited, true, false);

            Assert.AreEqual(18, visited.Count);
        }

        [TestMethod]
        public void CrawlDepth_3()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.CrawlUrl(null, new Uri(baseSite, "Root.htm"), 0, 3, visited, true, false);

            Assert.AreEqual(37, visited.Count);
        }

        [TestMethod]
        public void ScanUrlDepth_3()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.ScanUrl(new Uri(baseSite, "Root.htm"), 3, visited, true, false);

            Assert.AreEqual(37, visited.Count);
        }

        [TestMethod]
        public void CrawlDepth_100()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.CrawlUrl(null, new Uri(baseSite, "Root.htm"), 0, 100, visited, true, false);

            Assert.AreEqual(41, visited.Count);
        }

        [TestMethod]
        public void ScanUrlDepth_100()
        {
            Collection<Uri> visited = new Collection<Uri>();
            HttpClient.ScanUrl(new Uri(baseSite, "Root.htm"), 100, visited, true, false);
            
            Assert.AreEqual(41, visited.Count);
        }

    }
}
