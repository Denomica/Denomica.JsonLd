using System;
using System.Collections.Generic;
using System.Text;

namespace Denomica.JsonLd
{
    /// <summary>
    /// A container class that contains an HTML document.
    /// </summary>
    public class HtmlDocumentContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlDocumentContainer"/> class with the specified HTML content.
        /// </summary>
        /// <param name="html">The HTML content to be stored in the container. Cannot be <see langword="null"/>.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="html"/> is <see langword="null"/>.</exception>
        public HtmlDocumentContainer(string html)
        {
            this.Html = html ?? throw new ArgumentNullException(nameof(html));
        }

        /// <summary>
        /// Gets the HTML content as a string.
        /// </summary>
        public string Html { get; private set; }
    }
}
