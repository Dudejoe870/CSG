﻿namespace CSG.Viewer.Framework.Content
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Reflection;
    using System.Threading.Tasks;

    public interface IContentProvider
    {
        /// <summary>
        /// Opens a stream for the given content name.
        /// </summary>
        Task<Stream> Open(string name);
    }

    /// <summary>
    /// ContentProvider supports reading files, http files and embedded resources.
    /// </summary>
    /// <example>
    /// await contentProvider.Open("https://example.com/logo.png");
    /// await contentProvider.Open("embedded:Resources.png");
    /// await contentProvider.Open("C:\example.png");
    /// </example>
    public class ContentProvider : IContentProvider
    {
        private readonly Lazy<HttpClient> http;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentProvider"/> class.
        /// </summary>
        /// <param name="messageHandler"></param>
        public ContentProvider(HttpMessageHandler messageHandler = null)
        {
            this.http = new Lazy<HttpClient>(() => new HttpClient(messageHandler ?? new HttpClientHandler()));
        }

        /// <summary>
        /// Opens a stream for the given content name.
        /// </summary>
        public async Task<Stream> Open(string name)
        {
            if (name.StartsWith("embedded:", StringComparison.OrdinalIgnoreCase))
            {
                var embeddedName = name.Substring(9);
                return GetAssembly().GetManifestResourceStream(embeddedName);
            }

            if (name.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                name.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                var response = await http.Value.GetAsync(name).ConfigureAwait(false);
                var content = response.EnsureSuccessStatusCode().Content;
                return await content.ReadAsStreamAsync().ConfigureAwait(false);
            }

            if (!File.Exists(name))
                return null;

            return File.OpenRead(name);
        }

        private static Assembly GetAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }
    }
}
