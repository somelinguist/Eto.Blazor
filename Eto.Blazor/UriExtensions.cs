using System;
using System.IO;

namespace Eto.Blazor
{
    public static class UriExtensions
    {
        public static bool IsBaseOfPage(this Uri baseUri, string? uriString)
        {
            if (Path.HasExtension(uriString))
            {
                // If the path ends in a file extension, it's not referring to a page.
                return false;
            }

            var uri = new Uri(uriString!);
            return baseUri.IsBaseOf(uri);
        }
    }
}