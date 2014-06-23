﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BookmarksManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BookmarksManagerTests
{
    [TestClass]
    public class CombinedReaderWriterTests
    {

        private StringBuilder _stringBuilder = new StringBuilder();


        private string Write(BookmarkFolder bookmarks, Encoding encoding = null)
        {
            var sb = new StringBuilder();
            var writter = NetscapeBookmarksWritter.Create(sb, encoding);
            writter.Write(bookmarks);
            return sb.ToString();
        }


        [TestMethod]
        public void EmptyContainer()
        {
            _stringBuilder.Clear();
            var emptyContainer = new BookmarkFolder();
            var writter = NetscapeBookmarksWritter.Create(_stringBuilder);
            var reader = new NetscapeBookmarksReader();
            writter.Write(emptyContainer);
            var readed = reader.Read(_stringBuilder.ToString());
            writter = NetscapeBookmarksWritter.Create();
            var write2 = writter.ToString();
            Assert.AreEqual(write1, write2, true);
        }

        [TestMethod]
        public void CustomAttributes()
        {
            var container = new BookmarkFolder
            {
                new BookmarkLink("a", "b") {Attributes = new Dictionary<string, string> {{"custom", "1"}}},
                new BookmarkFolder("folder") {Attributes = new Dictionary<string, string> {{"custom", "2"}, {"add_date", "ę"}}}
            };
            var writter = new NetscapeBookmarksWritter(container);
            var reader = new NetscapeBookmarksReader();
            var write1 = writter.ToString();
            var readed = reader.Read(write1);
            Assert.AreEqual("1", readed.AllLinks.First().Attributes["custom"]);
            Assert.AreEqual("2", readed.GetAllItems<BookmarkFolder>().First().Attributes["custom"]);
            Assert.IsFalse(readed.GetAllItems<BookmarkFolder>().First().Attributes.ContainsKey("add_date"), "add_date is ignored attribute, it must not be written");
        }

        [TestMethod]
        public void SimpleStructure()
        {
            var container = Helpers.GetSimpleStructure();
            container.Add(new BookmarkLink("test", "test123") {Description = "<br>"});
            var writter = new NetscapeBookmarksWritter(container);
            var reader = new NetscapeBookmarksReader();
            var write1 = writter.ToString();
            var readed = reader.Read(write1);
            writter = new NetscapeBookmarksWritter(readed);
            var write2 = writter.ToString();
            readed = reader.Read(write2);
            Assert.AreEqual(write1, write2, true);
            Assert.IsNotNull(readed.AllLinks.FirstOrDefault(l => l.Title == "test123" && l.Description == "<br>"), "Description must be preserved between reads and writes");
        }


        [TestMethod]
        public void StreamUnicode()
        {
            var container = Helpers.GetSimpleStructure();
            container.Add(new BookmarkLink("test", "ƒ"));
            var ms = new MemoryStream();
            var writter = new NetscapeBookmarksWritter(container) {OutputEncoding = Encoding.Unicode};
            writter.Write(ms);
            ms = new MemoryStream(ms.GetBuffer());
            var reader = new NetscapeBookmarksReader {AutoDetectEncoding = true};
            var readed = reader.Read(ms);
            Assert.AreEqual(container.AllItems.Last().Title, readed.AllItems.Last().Title);
        }
    }
}
