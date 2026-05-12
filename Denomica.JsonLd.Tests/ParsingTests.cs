using Denomica.JsonLd;
using Denomica.Text.Json;
using System.Text.Json;

namespace Denomica.JsonLd.Tests
{
    [TestClass]
    public sealed class ParsingTests
    {
        [TestMethod]
        public async Task Parse01()
        {
            var doc = new HtmlDocumentContainer(Properties.Resources.HTMLPage001);
            int count = 0;
            await foreach (var ld in doc.GetJsonLDElementsAsync())
            {
                count++;
            }

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public async Task Parse02()
        {
            var objects = await new HtmlDocumentContainer(Properties.Resources.HTMLPage001).GetJsonLDObjectsAsync("Product").ToListAsync();
            Assert.AreEqual(2, objects.Count);
        }

        [TestMethod]
        public async Task Parse03()
        {
            var objects = await new HtmlDocumentContainer(Properties.Resources.HTMLPage001).GetJsonLDObjectsAsync("Organization").ToListAsync();
            Assert.AreEqual(1, objects.Count);
        }

        [TestMethod]
        public async Task Parse04()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD004).RootElement;
            var objects = await jsonElem.GetJsonLDObjectsAsync().ToListAsync();
            Assert.AreEqual(4, objects.Count);
        }

        [TestMethod]
        public async Task Parse05()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD005).RootElement;
            var products = await jsonElem.GetJsonLDObjectsAsync("Product").ToListAsync();
            Assert.AreEqual(0, products.Count);

            var gprs = await jsonElem.GetJsonLDObjectsAsync("ProductGroup").ToListAsync();
            Assert.AreEqual(1, gprs.Count);
        }

        [TestMethod]
        public async Task Parse06()
        {
            var htmlDoc = new HtmlDocumentContainer(Properties.Resources.HTMLPage002);
            var objects = await htmlDoc.GetJsonLDObjectsAsync().ToListAsync();

            Assert.AreNotEqual(0, objects.Count);

            foreach(var obj in objects)
            {
                var d = obj.ToJsonDictionary();
                d.TryGetValue("@context", out var context);
                Assert.AreEqual("https://schema.org", context);
            }
        }

        [TestMethod]
        public async Task Parse07()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD006).RootElement;
            var persons = await jsonElem.GetJsonLDObjectsAsync("Person").ToListAsync();
            var orgs = await jsonElem.GetJsonLDObjectsAsync("Organization").ToListAsync();

            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(persons.Count, orgs.Count);
        }

        [TestMethod]
        public async Task Parse08()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD007).RootElement;
            Assert.IsTrue(jsonElem.IsSchemaOrgObjectType("Person"));
            Assert.IsTrue(jsonElem.IsSchemaOrgObjectType("Organization"));
        }

        [TestMethod]
        public async Task Parse09()
        {
            var htmlDoc = new HtmlDocumentContainer(Properties.Resources.HTMLPage003);
            var objects = await htmlDoc.GetJsonLDObjectsAsync().ToListAsync();
            Assert.AreEqual(1, objects.Count);

            var obj = objects.First();
            var d = obj.ToJsonDictionary();
            d.TryGetValue("@context", out var context);
            Assert.AreEqual("https://schema.org", context);
        }

        [TestMethod]
        public void Parse10()
        {
            var doc = new HtmlDocumentContainer(Properties.Resources.HTMLPage001);
            int count = 0;
            foreach (var ld in doc.GetJsonLDElements())
            {
                count++;
            }

            Assert.AreEqual(3, count);
        }

        [TestMethod]
        public void Parse11()
        {
            var objects = new HtmlDocumentContainer(Properties.Resources.HTMLPage001).GetJsonLDObjects("Product").ToList();
            Assert.AreEqual(2, objects.Count);
        }

        [TestMethod]
        public void Parse12()
        {
            var objects = new HtmlDocumentContainer(Properties.Resources.HTMLPage001).GetJsonLDObjects("Organization").ToList();
            Assert.AreEqual(1, objects.Count);
        }

        [TestMethod]
        public void Parse13()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD004).RootElement;
            var objects = jsonElem.GetJsonLDObjects().ToList();
            Assert.AreEqual(4, objects.Count);
        }

        [TestMethod]
        public void Parse14()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD005).RootElement;
            var products = jsonElem.GetJsonLDObjects("Product").ToList();
            Assert.AreEqual(0, products.Count);

            var gprs = jsonElem.GetJsonLDObjects("ProductGroup").ToList();
            Assert.AreEqual(1, gprs.Count);
        }

        [TestMethod]
        public void Parse15()
        {
            var htmlDoc = new HtmlDocumentContainer(Properties.Resources.HTMLPage002);
            var objects = htmlDoc.GetJsonLDObjects().ToList();

            Assert.AreNotEqual(0, objects.Count);

            foreach (var obj in objects)
            {
                var d = obj.ToJsonDictionary();
                d.TryGetValue("@context", out var context);
                Assert.AreEqual("https://schema.org", context);
            }
        }

        [TestMethod]
        public void Parse16()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD006).RootElement;
            var persons = jsonElem.GetJsonLDObjects("Person").ToList();
            var orgs = jsonElem.GetJsonLDObjects("Organization").ToList();

            Assert.AreEqual(1, persons.Count);
            Assert.AreEqual(persons.Count, orgs.Count);
        }

        [TestMethod]
        public void Parse17()
        {
            var jsonElem = JsonDocument.Parse(Properties.Resources.JSONLD007).RootElement;
            Assert.IsTrue(jsonElem.IsSchemaOrgObjectType("Person"));
            Assert.IsTrue(jsonElem.IsSchemaOrgObjectType("Organization"));
        }

        [TestMethod]
        public void Parse18()
        {
            var htmlDoc = new HtmlDocumentContainer(Properties.Resources.HTMLPage003);
            var objects = htmlDoc.GetJsonLDObjects().ToList();
            Assert.AreEqual(1, objects.Count);

            var obj = objects.First();
            var d = obj.ToJsonDictionary();
            d.TryGetValue("@context", out var context);
            Assert.AreEqual("https://schema.org", context);
        }
    }
}
