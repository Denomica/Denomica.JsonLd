﻿using Denomica.Text.Json;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace Denomica.JsonLd
{
    /// <summary>
    /// Provides extension methods for working with JSON-LD data in HTML documents and <see cref="JsonElement"/>
    /// objects.
    /// </summary>
    /// <remarks>This static class includes methods for extracting, filtering, and processing JSON-LD elements
    /// from HTML documents and JSON structures. It supports asynchronous enumeration for efficient handling of large
    /// datasets and provides utilities for working with Schema.org object types.</remarks>
    public static class ExtensionMethods
    {

        private const string DefaultContext = "https://schema.org";


        /// <summary>
        /// Extracts JSON-LD elements from the specified HTML document.
        /// </summary>
        /// <remarks>This method searches the provided HTML document for <c>script</c> elements with a
        /// <c>type</c> attribute of <c>application/ld+json</c>. The contents of each matching <c>script</c> element are
        /// parsed as JSON. If parsing fails for a particular element, it is skipped.  The method returns an
        /// asynchronous stream, allowing the caller to process the JSON-LD elements as they are discovered. Use
        /// <c>await foreach</c> to enumerate the results.</remarks>
        /// <param name="container">The HTML document to search for JSON-LD script elements.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> objects representing the parsed JSON-LD data. Each
        /// element corresponds to a <c>script</c> tag with a <c>type</c> attribute of <c>application/ld+json</c>.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDElementsAsync(this HtmlDocumentContainer container)
        {
            var document = container.CreateHtmlDocument();
            foreach (var htmlNode in document.QuerySelectorAll("script[type='application/ld+json']"))
            {
                JsonElement? elem = null;
                try
                {
                    using (var strm = new MemoryStream())
                    {
                        using (var writer = new StreamWriter(strm))
                        {
                            await writer.WriteAsync(htmlNode.InnerText);
                            await writer.FlushAsync();
                            strm.Position = 0;

                            var doc = await JsonDocument.ParseAsync(strm);
                            elem = doc.RootElement;
                        }
                    }
                }
                catch { }

                if (elem.HasValue)
                {
                    yield return elem.Value;
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves JSON-LD objects from the specified HTML document.
        /// </summary>
        /// <remarks>This method processes the HTML document to locate and parse JSON-LD elements,
        /// returning each parsed object as a <see cref="JsonElement"/>. The method uses asynchronous enumeration to
        /// efficiently handle large documents or multiple JSON-LD elements.</remarks>
        /// <param name="container">The HTML document to search for JSON-LD objects. Cannot be null.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> instances representing the JSON-LD objects found within
        /// the document. The stream will be empty if no JSON-LD objects are present.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDObjectsAsync(this HtmlDocumentContainer container)
        {
            await foreach (var elem in container.GetJsonLDElementsAsync())
            {
                await foreach (var obj in elem.GetJsonLDObjectsAsync())
                {
                    yield return obj;
                }
            }

            yield break;
        }

        /// <summary>
        /// Asynchronously retrieves JSON-LD objects from the specified HTML document that match the given types.
        /// </summary>
        /// <remarks>This method filters JSON-LD objects based on the specified types using the Schema.org
        /// vocabulary.  It yields each matching object as it is found, allowing for efficient processing of large
        /// documents.</remarks>
        /// <param name="container">The HTML document from which to extract JSON-LD objects.</param>
        /// <param name="types">An array of type names to filter the JSON-LD objects. Only objects matching these types will be returned.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> objects representing the filtered JSON-LD data.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDObjectsAsync(this HtmlDocumentContainer container, params string[] types)
        {
            await foreach(var obj in container.GetJsonLDObjectsAsync())
            {
                if(obj.IsSchemaOrgObjectType(types))
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Asynchronously retrieves JSON-LD objects from the specified <see cref="JsonElement"/>.
        /// </summary>
        /// <remarks>This method processes the input <see cref="JsonElement"/> to identify and yield
        /// JSON-LD objects. If the element contains a `@graph` property, the method enumerates its array and
        /// recursively retrieves JSON-LD objects from each item. If the element has a `@type` property with a string
        /// value, the element itself is yielded as a JSON-LD object.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to search for JSON-LD objects.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> instances representing JSON-LD objects.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDObjectsAsync(this JsonElement element)
        {
            if(element.IsJsonLdGraphElement())
            {
                foreach(var obj in element.EnumerateGraph())
                {
                    yield return obj;
                }
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    await foreach (var obj in item.GetJsonLDObjectsAsync())
                    {
                        yield return obj;
                    }
                }
            }
            else if(element.ValueKind == JsonValueKind.Object && element.IsSchemaOrgElement())
            {
                yield return element;
            }
        }

        /// <summary>
        /// Asynchronously retrieves JSON-LD objects of specified types from the given JSON element.
        /// </summary>
        /// <remarks>This method filters JSON-LD objects based on the provided types and yields them
        /// asynchronously.  It is useful for processing large JSON documents where only specific types of JSON-LD
        /// objects are needed.</remarks>
        /// <param name="element">The JSON element to search for JSON-LD objects.</param>
        /// <param name="types">An array of type names to filter the JSON-LD objects. Only objects matching these types are returned.</param>
        /// <returns>An asynchronous stream of <see cref="JsonElement"/> objects that match the specified types.</returns>
        public static async IAsyncEnumerable<JsonElement> GetJsonLDObjectsAsync(this JsonElement element, params string[] types)
        {
            await foreach (var obj in element.GetJsonLDObjectsAsync())
            {
                if (obj.IsSchemaOrgObjectType(types))
                {
                    yield return obj;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="JsonElement"/> is of any of the given Schema.org object types.
        /// </summary>
        /// <remarks>This method iterates over the provided types and checks if the <paramref
        /// name="element"/> matches any of them using the <c>IsSchemaOrgObjectType</c> method.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to check.</param>
        /// <param name="types">An array of Schema.org object type names to check against.</param>
        /// <returns><see langword="true"/> if the <paramref name="element"/> matches any of the specified types; otherwise, <see
        /// langword="false"/>.</returns>
        public static bool IsSchemaOrgObjectType(this JsonElement element, params string[] types)
        {
            foreach(var type in types)
            {
                if(element.IsSchemaOrgObjectType(type))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Converts an <see cref="IAsyncEnumerable{T}"/> to a <see cref="Task{TResult}"/> that represents a list of
        /// elements.
        /// </summary>
        /// <remarks>This method enumerates all elements in the <paramref name="enumerable"/>
        /// asynchronously and adds them to a list. It is useful for scenarios where you need to materialize an <see
        /// cref="IAsyncEnumerable{T}"/> into a concrete collection.</remarks>
        /// <typeparam name="T">The type of elements in the asynchronous enumerable.</typeparam>
        /// <param name="enumerable">The asynchronous enumerable to convert to a list.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of elements from the
        /// asynchronous enumerable.</returns>
        public static async Task<IList<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable)
        {
            var result = new List<T>();

            await foreach (var item in enumerable)
            {
                result.Add(item);
            }

            return result;
        }



        /// <summary>
        /// Creates a new <see cref="HtmlDocument"/> instance and loads the HTML content from the specified container.
        /// </summary>
        /// <param name="container">The <see cref="HtmlDocumentContainer"/> containing the HTML content to load.</param>
        /// <returns>A new <see cref="HtmlDocument"/> instance populated with the HTML content from the <paramref
        /// name="container"/>.</returns>
        private static HtmlDocument CreateHtmlDocument(this HtmlDocumentContainer container)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(container.Html);
            return doc;
        }

        /// <summary>
        /// Determines whether the specified <see cref="JsonElement"/> contains a property with the given name.
        /// </summary>
        /// <param name="element">The <see cref="JsonElement"/> to inspect.</param>
        /// <param name="propertyName">The name of the property to check for. This value is case-sensitive.</param>
        /// <returns><see langword="true"/> if the <see cref="JsonElement"/> contains a property with the specified name; 
        /// otherwise, <see langword="false"/>.</returns>
        private static bool HasProperty(this JsonElement element, string propertyName)
        {
            return element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out _);
        }

        /// <summary>
        /// Returns <see langword="true"/> if the current <see cref="JsonElement"/> represents a JSON-LD graph element.
        /// </summary>
        private static bool IsJsonLdGraphElement(this JsonElement element)
        {
            return 
                element.IsSchemaOrgElement()
                && element.TryGetProperty("@graph", out JsonElement graph)
                && (
                    graph.ValueKind == JsonValueKind.Array
                    || graph.ValueKind == JsonValueKind.Object
                );
        }

        /// <summary>
        /// Determines whether the specified <see cref="JsonElement"/> represents a Schema.org element.
        /// </summary>
        /// <remarks>This method checks for the presence of the "@context" property in the JSON element
        /// and verifies  that its value matches the Schema.org context URI.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to evaluate.</param>
        /// <returns><see langword="true"/> if the <paramref name="element"/> contains a property named "@context"  with the
        /// value "https://schema.org"; otherwise, <see langword="false"/>.</returns>
        private static bool IsSchemaOrgElement(this JsonElement element)
        {
            if (element.HasProperty("@context") && element.TryGetProperty("@context", out var context) && context.ValueKind == JsonValueKind.String)
            {
                var text = context.GetString()?.ToLower();
                return text == DefaultContext || text == "https://schema.org/" || text == "http://schema.org" || text == "http://schema.org/";
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="JsonElement"/> represents a Schema.org object of the given type.
        /// </summary>
        /// <remarks>This method checks if the <paramref name="element"/> contains a Schema.org type
        /// definition and compares it to the specified type. The comparison is case-insensitive.</remarks>
        /// <param name="element">The <see cref="JsonElement"/> to evaluate. Must represent a valid Schema.org element.</param>
        /// <param name="type">The type to check against, as a case-insensitive string.</param>
        /// <returns><see langword="true"/> if the <paramref name="element"/> is a Schema.org object and its type matches the
        /// specified <paramref name="type"/>; otherwise, <see langword="false"/>.</returns>
        private static bool IsSchemaOrgObjectType(this JsonElement element, string type)
        {
            if (element.IsSchemaOrgElement() && element.TryGetProperty("@type", out var typeObj))
            {
                if(typeObj.ValueKind == JsonValueKind.String)
                {
                    var text = typeObj.GetString();
                    return string.Equals(text, type, StringComparison.OrdinalIgnoreCase);
                }
                else if(typeObj.ValueKind == JsonValueKind.Array)
                {
                    var arr = typeObj.EnumerateStringArray();
                    if(arr.Contains(type, StringComparer.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Assumes that the given element contains a JSON-LD graph and enumerates its items.
        /// </summary>
        private static IEnumerable<JsonElement> EnumerateGraph(this JsonElement element)
        {
            if(element.IsSchemaOrgElement() && element.TryGetProperty("@graph", out JsonElement graph))
            {
                var contextUri = element.GetProperty("@context").GetString() ?? DefaultContext;

                if(graph.ValueKind == JsonValueKind.Array)
                {
                    foreach(var item in graph.EnumerateArray())
                    {
                        yield return item.EnsureContextAttribute(contextUri);
                    }
                }
                else if(graph.ValueKind == JsonValueKind.Object)
                {
                    yield return graph.EnsureContextAttribute(contextUri);
                }
            }

            yield break;
        }

        private static JsonElement EnsureContextAttribute(this JsonElement element, string contextValue)
        {
            if(!element.TryGetProperty("@context", out var context))
            {
                var d = element.ToJsonDictionary();
                d["@context"] = contextValue;

                return d.Deserialize<JsonElement>();
            }

            return element;
        }

        private static IEnumerable<string> EnumerateStringArray(this JsonElement element)
        {
            if(element.ValueKind == JsonValueKind.Array)
            {
                foreach(var item in element.EnumerateArray())
                {
                    if(item.ValueKind == JsonValueKind.String)
                    {
                        yield return item.GetString() ?? string.Empty;
                    }
                }
            }
        }
    }
}
