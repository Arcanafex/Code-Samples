using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Xml;
using System.Net;

namespace Spaces.Core
{
    public class FeedWidget : Widget
    {
        public enum FeedType
        {
            ImageGallery,
            MoviePlaylist,
            RSSFeed,
            MRSSFeed
        }

        public string m_feedUrl;
        public string m_feedUser;
        public string m_feedPassword;

        //public FeedType feedType;
        private MediaFeed mediaFeed;
        public List<MediaFeedItem> items;

        private int currentFeedIndex;
        public MediaFeedItem CurrentFeedItem
        {
            get
            {
                if (currentFeedIndex > -1 && currentFeedIndex < items.Count)
                    return items[currentFeedIndex];
                else
                    return null;
            }

        }

        public bool IsOpen
        {
            get { return mediaFeed.IsOpen; }
        }

        public UnityEvent OnFeedOpened;
        public UnityEvent OnFeedClosed;

        void Start()
        {
            currentFeedIndex = -1;
            InitializeFeed();
        }

        void OnEnable()
        {
            if (mediaFeed != null)
            {
                mediaFeed.OnOpened += MediaFeed_OnOpened;
                mediaFeed.OnClosed += MediaFeed_OnClosed;
            }
        }

        void OnDisable()
        {
            if (mediaFeed != null)
            {
                mediaFeed.OnOpened -= MediaFeed_OnOpened;
                mediaFeed.OnClosed -= MediaFeed_OnClosed;
            }
        }

        void OnDestroy()
        {
            if (mediaFeed != null)
            {
                mediaFeed.Close();

                mediaFeed.OnOpened -= MediaFeed_OnOpened;
                mediaFeed.OnClosed -= MediaFeed_OnClosed;
            }
        }

        private void InitializeFeed()
        {
            if (!string.IsNullOrEmpty(m_feedUrl))
            {
                mediaFeed = new MediaFeed();
                mediaFeed.OnOpened += MediaFeed_OnOpened;
                mediaFeed.OnClosed += MediaFeed_OnClosed;
                mediaFeed.Open(m_feedUrl, m_feedUser, m_feedPassword);
            }
        }

        private void MediaFeed_OnClosed()
        {
            OnFeedClosed.Invoke();
        }

        private void MediaFeed_OnOpened()
        {
            OnFeedOpened.Invoke();
        }

        public void Next()
        {

            if (currentFeedIndex < 0 || currentFeedIndex + 1 == items.Count)
            {
                var nextItem = mediaFeed.NextFeedItem;

                if (nextItem != null)
                {
                    items.Add(nextItem);
                }
                else
                    return;
            }

            currentFeedIndex++;
        }

        public MediaFeedItem GetNextFeedItem()
        {
            Next();
            return CurrentFeedItem; 
        }

        public void Previous()
        {
            if (currentFeedIndex > 0)
                currentFeedIndex--;
        }

        public void First()
        {
            currentFeedIndex = 0;
        }

        public void Last()
        {
            currentFeedIndex = items.Count - 1;
        }

        public void Select(int index)
        {
            // Currently you cannot select an item explicitly if you have not progressed to it with Next().

            if (index > -1 && index < items.Count)
            {
                currentFeedIndex = index;
            }
        }


        [System.Serializable]
        public class MediaFeedItem
        {
            //public Dictionary<string, string> Parameters;

            public string Title;
            public string Link;
            public string Description;
            public string ContentType;
            public string ContentURL;
            public string ThumbnailURL;
            public string Keywords;
        }

        [System.Serializable]
        public class MediaFeed
        {
            private XmlReader m_reader;
            private System.IO.Stream stream;

            public delegate void Opened();
            public event Opened OnOpened;

            public delegate void Closed();
            public event Closed OnClosed;

            public bool IsOpen
            {
                get { return m_reader != null; }
            }

            public MediaFeedItem NextFeedItem
            {
                get
                {
                    foreach (var item in GetMediaFeedItems())
                    {
                        return item;
                    }

                    return null;
                }
            }

            private IEnumerable<MediaFeedItem> GetMediaFeedItems()
            {
                while (m_reader != null && m_reader.ReadToFollowing("item"))
                {
                    var item = new MediaFeedItem();

                    if (m_reader.ReadToFollowing("title"))
                        item.Title = m_reader.ReadElementContentAsString();

                    if (m_reader.ReadToFollowing("description"))
                        item.Description = m_reader.ReadElementContentAsString();

                    if (m_reader.ReadToFollowing("media:content"))
                    {
                        m_reader.MoveToFirstAttribute();
                        item.ContentType = m_reader.Value;

                        m_reader.MoveToNextAttribute();
                        item.ContentURL = m_reader.Value;

                        m_reader.MoveToContent();
                        if (m_reader.ReadToDescendant("media:keywords"))
                            item.Keywords = m_reader.ReadElementContentAsString();

                        if (m_reader.ReadToNextSibling("media:thumbnail"))
                            item.ThumbnailURL = m_reader.ReadElementContentAsString();
                    }

                    yield return item;
                }

                Close();
            }

            public void Open(string url, string user, string password)
            {
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

                // For development, Trust all signing authorities for https.
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };

                Debug.Log("MediaFeed Reader [Opening]");

                m_reader = XmlReader.Create(url, settings, context);
                
                if (OnOpened != null)
                    OnOpened();

                Debug.Log("MediaFeed Reader [Opened]");
            }

            public void Close()
            {
                if (m_reader != null)
                {
                    m_reader.Close();
                    m_reader = null;
                }

                Debug.Log("MediaFeed Reader [Closed]");
            }
        }
    }
}