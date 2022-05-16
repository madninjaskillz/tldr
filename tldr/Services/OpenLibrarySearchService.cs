using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Web;
using tldr.Models;

namespace tldr.Services
{
    public class OpenLibrarySearchService
    {
        private OpenLibrary.SearchResults Search(string search)
        {
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            Uri uri = new Uri("http://openlibrary.org/search.json?q="+HttpUtility.UrlEncode(search));

            string json = webClient.DownloadString(uri);

           return JsonSerializer.Deserialize<OpenLibrary.SearchResults>(json);
        }

        public List<Book> SearchForBook(string search)
        {
            List<Book> results = new List<Book>();
            if (search.Length >= 3)
            {
                var searchResults = Search(search);
                

                foreach (var doc in searchResults.docs.Where(p=>p.isbn?.Length>0))
                {
                    results.Add(new Book
                    {
                        ASIN = doc.isbn?.FirstOrDefault() ?? "",
                        Authors = doc.author_name?.ToList() ?? new List<string>(),
                        Title = doc.title,
                        Id = Guid.NewGuid(),
                        ThumbnailUrl = doc.cover_i > 0 ? $"https://covers.openlibrary.org/b/id/{doc.cover_i}-L.jpg" : null,
                        Review = new List<Review>()
                    });
                }
            }

            results = results.GroupBy(x => String.Join(',', x.Authors) + x.Title).Select(g => g.FirstOrDefault())
                .ToList();

            return results;
        }
    }
}
