using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Spaces.Core
{
    public class FeedDisplayAdapterWidget : Widget
    {
        public enum FeedAdapterType
        {
            Title,
            Link,
            Description,
            ContentType,
            ContentURL,
            ThumbnailURL,
            Keywords
        }

        [System.Serializable]
        public class FeedAdapter
        {
            public string name;
            public FeedAdapterType key;
            public GameObject ContentDisplay;
            public bool shared = false;
        }

        public FeedWidget m_feed;
        public List<FeedAdapter> adapters;
        public FeedWidget.MediaFeedItem currentItem;

        private FeedWidget.MediaFeedItem lastItem;

        void Start()
        {
            if (!m_feed)
                m_feed = GetComponentInParent<FeedWidget>();

            m_feed.OnFeedOpened.AddListener(AdaptFeedItemContent);

            if (m_feed.IsOpen)
                AdaptFeedItemContent();
        }

        public void AdaptFeedItemContent()
        {
            
            if (m_feed)
            {
                lastItem = m_feed.CurrentFeedItem;
                currentItem = m_feed.GetNextFeedItem();

                if (currentItem != lastItem && currentItem != null)
                {
                    Debug.Log(currentItem.Title);

                    UpdateLocalAdapters();
                }
                else
                    Debug.Log("Nope");

                Debug.Log("Adapting Content for: " + currentItem.Title);
            }
        }

        public void UpdateLocalAdapters()
        {
            foreach (FeedAdapter adapter in adapters.Where(a => a.shared == false))
            {
                UpdateAdapter(adapter);
            }
        }

        public void UpdateSharedAdapters()
        {
            foreach (FeedAdapter adapter in adapters.Where(a => a.shared == true))
            {
                UpdateAdapter(adapter);
            }
        }

        public void UpdateAdapter(FeedAdapter adapter)
        {

            switch (adapter.key)
            {
                case FeedAdapterType.Title:
                    adapter.ContentDisplay.GetComponent<IDisplay<string>>().SetContent(currentItem.Title);
                    break;

                case FeedAdapterType.Link:
                    adapter.ContentDisplay.GetComponent<IDisplay<string>>().SetContent(currentItem.Link);
                    break;

                case FeedAdapterType.Description:
                    adapter.ContentDisplay.GetComponent<IDisplay<string>>().SetContent(currentItem.Description);
                    break;

                case FeedAdapterType.ContentType:
                    adapter.ContentDisplay.GetComponent<IDisplay<string>>().SetContent(currentItem.ContentType);
                    break;

                case FeedAdapterType.ContentURL:
                    adapter.ContentDisplay.GetComponent<IDisplay<string>>().SetContent(currentItem.ContentURL);
                    break;

                case FeedAdapterType.ThumbnailURL:
                    adapter.ContentDisplay.GetComponent<IDisplay<string>>().SetContent(currentItem.ThumbnailURL);
                    break;

                case FeedAdapterType.Keywords:
                    adapter.ContentDisplay.GetComponent<IDisplay<string>>().SetContent(currentItem.Keywords);
                    break;

                default:
                    break;
            }

        }
    }
}
