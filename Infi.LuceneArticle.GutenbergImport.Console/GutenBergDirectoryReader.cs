using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using Infi.LuceneArticle.Models;

namespace Infi.LuceneArticle.GutenbergImport.Console
{
    public class GutenBergDirectoryReader
    {
        private readonly string _path;

        public GutenBergDirectoryReader(string path) {
            _path = path;
        }

        public IEnumerable<Book> ReadAll()
        {
            var books = new List<Book>(); 

            foreach (string file in Directory.EnumerateFiles(_path, "*.zip", SearchOption.AllDirectories))
            {
                var book = ImportFromZip(file);
                if (book == null) {
                    continue;
                }

                if (books.Find(x => x.Title == book.Title) != null) {
                    continue;
                }

                if (books.Count % 100 == 0) {
                    System.Console.Out.WriteLineAsync(string.Format("{0} books imported", books.Count));
                }

                books.Add(book);
                yield return book;
            }
        }

        private Book ImportFromZip(string zipPath)
        {
            using (var s = new ZipInputStream(File.OpenRead(zipPath)))
            {
                // For simplicity assume 1 txt file in each zip
                var zipEntry = s.GetNextEntry();
                if (!zipEntry.CanDecompress){
                    return null;
                }

                using (var sw = new StringWriter(new StringBuilder())){
                    var data = new byte[2048];

                    while (true)
                    {
                        var size = s.Read(data, 0, data.Length);
                        if (size > 0)
                        {
                            sw.Write(data.Select(Convert.ToChar).ToArray());
                        }
                        else {
                            break;
                        }
                    }
                    return ReadABook(sw.ToString(),zipEntry.Name);
                }
            }
        }

        private Book ReadABook(string bookText, string GutenbergId)
        {
            var book = new Book();
            book.Title = GetFieldValue(bookText, "Title");
            book.GutenBergId = GutenbergId;
           
            book.Author = GetFieldValue(bookText, "Author");
            book.Language = GetFieldValue(bookText, "Language");
            book.ReleaseDate = GetReleaseDate(bookText);
            
            book.Contents = GetBookContents(bookText);
            if (book.Contents==null) {
                System.Console.Out.WriteLineAsync(string.Format("Content part of '{0}' could not be matched - using full text from file", book.Title));
                book.Contents = bookText;
            }
           
            return book;
        }

        // Got 99 problems but a Regexp ain't one :)
        private const string ContentPattern = @"[*]{3} START OF THIS PROJECT .+ [*]{3}(?<content>.+)[*]{3} END OF THIS PROJECT .+[*]{3}";
        
        private string GetBookContents(string bookText)
        {
            var match = Regex.Match(bookText, ContentPattern, RegexOptions.Singleline);
            if (!match.Success || match.Groups["content"] == null) {
                return null;
            }

            return match.Groups["content"].Value;
        }

        private string GetFieldValue(string text, string fieldName)
        {
            var match = Regex.Match(text, string.Format("^{0}: (?<fieldValue>.+)$",fieldName), RegexOptions.Multiline);
            return match.Groups["fieldValue"].Value.Replace("\r", string.Empty);
        }

        private DateTime? GetReleaseDate(string bookText)
        {
            var releaseRaw = GetFieldValue(bookText, "Release Date");
            var matches = Regex.Matches(releaseRaw, @"[\w]+");
            if (matches.Count < 3)
            {
                return null;
            }
            var date = string.Format("{0} {1} {2}", matches[0], matches[1], matches[2]);

            var usDtfi = new CultureInfo("en-US", false).DateTimeFormat;
            DateTime result;
            if (DateTime.TryParse(date, usDtfi, DateTimeStyles.None, out result))
            {
                return result;
            }
            return null;
        }
    }
}