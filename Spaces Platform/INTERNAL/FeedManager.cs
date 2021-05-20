using System.Collections.Generic;
using System.Xml;

namespace Spaces.Core
{
    public class FeedManager
    {
        public enum FeedType
        {
            ImageGallery,
            MoviePlaylist,
            RSSFeed,
            MRSSFeed
        }

        public string feedUrl;
        public string feedUser;
        public string feedPassword;
        public MediaFeed feed;

        public FeedManager(string url, string user = "", string password = "")
        {
            feedUrl = url;
            feedUser = user;
            feedPassword = password;
        }

        public void LoadFeed()
        {
            feed = new MediaFeed();
            feed.Load(feedUrl, feedUser, feedPassword);
        }

        [System.Serializable]
        public class MediaFeed
        {
            [System.Serializable]
            public class MediaFeedItem
            {
                public string Title;
                public string Link;
                public string Description;
                public string ContentType;
                public string ContentURL;
                public string ThumbnailURL;
                public string Keywords;
            }

            public List<MediaFeedItem> Items;

            //Ideally this should be done outside of the main thread, or in some async way that won't hold up the game thread
            public void Load(string url, string user, string password)
            {
                Items = new List<MediaFeedItem>();

                XmlReaderSettings settings = new XmlReaderSettings { NameTable = new NameTable() };
                XmlNamespaceManager xmlns = new XmlNamespaceManager(settings.NameTable);
                xmlns.AddNamespace("media", @"http://search.yahoo.com/mrss/");
                XmlParserContext context = new XmlParserContext(null, xmlns, string.Empty, XmlSpace.Default);

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                {
                    XmlUrlResolver resolver = new XmlUrlResolver();
                    resolver.Credentials = new System.Net.NetworkCredential(user, password);
                    settings.XmlResolver = resolver;
                }

                using (XmlReader reader = XmlReader.Create(url, settings, context))
                {

                    for (int i = 0; i < 9; i++)
                    {

                        if (reader.ReadToFollowing("item"))
                        {
                            MediaFeedItem item = new MediaFeedItem();
                            Items.Add(item);

                            if (reader.ReadToFollowing("title"))
                                item.Title = reader.ReadElementContentAsString();

                            if (reader.ReadToFollowing("description"))
                                item.Description = reader.ReadElementContentAsString();

                            if (reader.ReadToFollowing("media:content"))
                            {
                                reader.MoveToFirstAttribute();
                                item.ContentType = reader.Value;

                                reader.MoveToNextAttribute();
                                item.ContentURL = reader.Value;

                                reader.MoveToContent();
                                if (reader.ReadToDescendant("media:keywords"))
                                    item.Keywords = reader.ReadElementContentAsString();

                                if (reader.ReadToNextSibling("media:thumbnail"))
                                    item.ThumbnailURL = reader.ReadElementContentAsString();
                            }

                        }
                        else
                            break;

                    }

                    reader.Close();
                }
            }
        }
    }
}