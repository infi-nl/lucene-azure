using System;

namespace Infi.LuceneArticle.Models
{
    public class Book {
        public string GutenBergId { get; set; }
        public string Title { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Language { get; set; }
        public string Contents { get; set; }
        public string Author { get; set; }
    }
}
