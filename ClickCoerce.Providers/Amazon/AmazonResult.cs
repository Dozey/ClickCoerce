using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Amazon.ECS;
using Amazon.ECS.Model;
using Amazon.ECS.Query;

namespace ClickCoerce.Providers.Amazon
{
    internal class AmazonResult<TElement> : IEnumerable<TElement>
    {
        private AmazonElementBinder<TElement> resultBinder;
        private AmazonECS ecs;
        private Item[] itemCache;
        private int totalItemCount;
        private Canon<AmazonCriteria>[] searchCanon;

        private AmazonResult(Canon<AmazonCriteria>[] searchCriteria)
        {
            ecs = new AmazonECSQuery(AmazonContext.AwsAccessKeyId, AmazonContext.AssociateTag, AmazonContext.EcsLocale);
            resultBinder = new AmazonElementBinder<TElement>(new BindingContext<TElement, AmazonCriteria>(new CriteriaResolver<AmazonCriteria>()));
            totalItemCount = -1;
            searchCanon = searchCriteria;
        }

        private int TotalItemCount
        {
            get
            {
                if (totalItemCount == -1)
                    InitializeResults();

                return totalItemCount;
            }
            set
            {
                totalItemCount = value;
            }
        }

        public static AmazonResult<TElement> Fetch(Canon<AmazonCriteria>[] searchCriteria)
        {
            return new AmazonResult<TElement>(searchCriteria);
        }

        private ItemSearchRequest CreateSearchRequest()
        {
            return CreateSearchRequest(1);
        }

        private ItemSearchRequest CreateSearchRequest(int itemPage)
        {
            if (itemPage < AmazonContext.MinimumItemsPage)
                throw new ArgumentOutOfRangeException("itemsPage");

            Type criteriaType = typeof(AmazonCriteria);
            StringConverter stringConverter = new StringConverter();
            List<string> keywords = new List<string>();

            ItemSearchRequest searchRequest = new ItemSearchRequest();

            foreach (Canon<AmazonCriteria> canon in searchCanon)
            {
                switch (canon.Criteria)
                {
                    case AmazonCriteria.Manufacturer:
                        if (stringConverter.CanConvertFrom(canon.Value.GetType()))
                        {
                            searchRequest.Manufacturer = stringConverter.ConvertToString(canon.Value);
                        }
                        break;
                    case AmazonCriteria.Model:
                        if (stringConverter.CanConvertFrom(canon.Value.GetType()))
                        {
                            keywords.Add(stringConverter.ConvertToString(canon.Value));
                        }
                        break;
                    case AmazonCriteria.Title:
                        if (stringConverter.CanConvertFrom(canon.Value.GetType()))
                        {
                            keywords.Add(stringConverter.ConvertToString(canon.Value));
                        }
                        break;
                }
            }

            searchRequest.Keywords = String.Join(" ", keywords.ToArray());
            searchRequest.SearchIndex = AmazonContext.DefaultSearchIndex;
            searchRequest.ItemPage = itemPage;

            return searchRequest;
        }

        private ItemSearchRequest[] CreateSearchRequestRange(int offset, int count)
        {
            if ((offset + 1) < AmazonContext.MinimumItemsPage || (offset + 1) > AmazonContext.MaximumItemsPage)
                throw new ArgumentOutOfRangeException("itemsRangeBegin");

            if (count < 1 || (offset + count) > AmazonContext.MaximumItemsPage)
                throw new ArgumentOutOfRangeException("itemsRangeEnd");

            ItemSearchRequest[] requestRange = new ItemSearchRequest[count];

            for (int i = 0; i < count; i++)
            {
                requestRange[i] = CreateSearchRequest(offset + i + 1);
            }

            return requestRange;
        }

        private void InitializeResults()
        {
            ItemSearchResponse response = ecs.ItemSearch(CreateSearchRequest(1));

            if (response.Items.Count != 0)
            {
                TotalItemCount = (int)response.Items[0].TotalResults;

                itemCache = new Item[TotalItemCount];

                for (int i = 0; i < response.Items.Count; i++)
                    for (int j = 0; j < response.Items[i].Item.Count; j++)
                        itemCache[(i * AmazonContext.ItemsPerPage) + j] = response.Items[i].Item[j];

            }
            else
            {
                TotalItemCount = 0;
            }
        }

        private Item FetchItem(int itemOffset)
        {
            Item[] items = FetchItemRange(itemOffset, 1);

            if (items.Length == 0)
                return null;

            return items[0];
        }

        private Item[] FetchItemRange(int itemOffset, int itemCount)
        {
            ItemSearchResponse response = null;
            return FetchItemRange(itemOffset, itemCount, out response);
        }

        private Item[] FetchItemRange(int itemOffset, int itemCount, out ItemSearchResponse response)
        {
            if (itemOffset >= TotalItemCount)
                throw new ArgumentOutOfRangeException("itemOffset");

            if (itemOffset + itemCount > TotalItemCount)
                throw new ArgumentOutOfRangeException("itemCount");

            int requestOffset = (int)Math.Ceiling((decimal)((decimal)(itemOffset) / AmazonContext.ItemsPerPage));
            int requestCount = (int)Math.Ceiling((decimal)((decimal)((itemOffset % AmazonContext.ItemsPerPage) + itemCount) / AmazonContext.ItemsPerPage));

            ItemSearchRequest[] request = CreateSearchRequestRange(requestOffset, requestCount);
            request = request.Where(r => itemCache[(int)((r.ItemPage - 1) * AmazonContext.ItemsPerPage)] == null).ToArray();

            if (request.Length != 0)
            {
                int cacheOffset = ((int)request.Min(r => r.ItemPage) - 1) * AmazonContext.ItemsPerPage;

                try
                {
                    response = ecs.ItemSearch(request);
                }
                catch (Exception e)
                {
                    throw new Exception("Failed to query Amazon", e);
                }

                for (int i = 0; i < response.Items.Count; i++)
                    for (int j = 0; j < response.Items[i].Item.Count; j++)
                        itemCache[cacheOffset + (i * AmazonContext.ItemsPerPage) + j] = response.Items[i].Item[j];
            }
            else
            {
                response = null;
            }

            if (itemCache.Length == 0)
                return itemCache;

            Item[] result = new Item[itemCount];
            Array.Copy(itemCache, itemOffset, result, 0, itemCount);

            return result;
        }


        #region IEnumerable<TElement> Members

        public IEnumerator<TElement> GetEnumerator()
        {
            return ItemFetcher.FetchAll(this).GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        delegate void FetchItemsCallback(int itemOffset, int itemCount);

        /// <summary>
        /// Self-Abstract-Factory for fetching AmazonResult<> items across multiple pages
        /// </summary>
        abstract class ItemFetcher : IEnumerable<TElement>
        {
            private Item[] items;
            private bool loaded;
            protected int step;
            protected int offset;
            protected AmazonResult<TElement> sourceResult;

            protected ItemFetcher(AmazonResult<TElement> result, int currentOffset)
            {
                loaded = false;
                step = AmazonContext.ItemPagesPerRequest * AmazonContext.ItemsPerPage;
                offset = currentOffset;
                sourceResult = result;

                FetchItems = new FetchItemsCallback(new Action<int, int>((itemOffset, itemsCount) =>
                {
                    if (result.TotalItemCount > (currentOffset + step))
                        Items = result.FetchItemRange(currentOffset, step);
                    else if(itemOffset < result.TotalItemCount)
                        Items = result.FetchItemRange(currentOffset, result.TotalItemCount - itemOffset);
                }));
            }

            public static ItemFetcher FetchAll(AmazonResult<TElement> result)
            {
                switch (AmazonContext.FetchMode)
                {
                    case ResultFetchMode.Async:
                        return new AsyncItemFetcher(result, 0, new Semaphore(AmazonContext.MaxWorkers, AmazonContext.MaxWorkers));
                    case ResultFetchMode.Sync:
                        return new SyncItemFetcher(result, 0);
                    default:
                        throw new NotSupportedException();
                }
            }

            protected FetchItemsCallback FetchItems { get; set;}

            protected virtual Item[] Items
            {
                get
                {
                    if (!loaded)
                        FetchItems(offset, step);

                    return items;
                }
                set
                {
                    items = value;
                    loaded = true;
                }
            }

            protected abstract ItemFetcher Next { get; set; }

            #region IEnumerable<TElement> Members

            public IEnumerator<TElement> GetEnumerator()
            {
                ItemFetcher currentResult = this;

                ENUMERATE:
                foreach (Item item in currentResult.Items)
                {
                    yield return sourceResult.resultBinder.Bind(Activator.CreateInstance<TElement>(), item);
                }

                if (currentResult.Next != null)
                {
                    currentResult = currentResult.Next;
                    goto ENUMERATE;
                }

                yield break;
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion
        }

        class SyncItemFetcher : ItemFetcher
        {
            private ItemFetcher nextItems;

            public SyncItemFetcher(AmazonResult<TElement> result, int currentOffset)
                : base(result, currentOffset)
            {
            }


            protected override ItemFetcher Next
            {
                get
                {
                    if (nextItems == null && (offset + step) < sourceResult.TotalItemCount)
                    {
                        nextItems = new SyncItemFetcher(sourceResult, offset + step);
                    }
                                       

                    return nextItems;
                }
                set
                {
                    nextItems = value;
                }
            }
        }

        class AsyncItemFetcher : ItemFetcher
        {
            private ManualResetEvent itemsEvent;
            private ManualResetEvent nextItemsEvent;
            private ItemFetcher nextItems;

            public AsyncItemFetcher(AmazonResult<TElement> result, int currentOffset, Semaphore fetchSemaphore)
                : base(result, currentOffset)
            {
                itemsEvent = new ManualResetEvent(false);
                nextItemsEvent = new ManualResetEvent(false);

                if (result.TotalItemCount > (currentOffset + step))
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(new Action<object>(o =>
                    {
                        fetchSemaphore.WaitOne();
                        Next = new AsyncItemFetcher(result, currentOffset + step, fetchSemaphore);
                        fetchSemaphore.Release(1);
                        nextItemsEvent.Set();
                    })));
                }
                else
                {
                    nextItemsEvent.Set();
                }

                FetchItems = new FetchItemsCallback(new Action<int, int>((itemOffset, itemsCount) =>
                {
                    if (result.TotalItemCount > (currentOffset + step))
                        Items = result.FetchItemRange(currentOffset, step);
                    else
                        Items = result.FetchItemRange(currentOffset, result.TotalItemCount - itemOffset);

                    itemsEvent.Set();
                }));

                FetchItems(offset, step);
            }

            protected override Item[] Items
            {
                get
                {
                    itemsEvent.WaitOne();               
                    return base.Items;
                }
                set
                {
                    base.Items = value;
                }
            }

            protected override ItemFetcher Next
            {
                get
                {
                    if (nextItems == null)
                        nextItemsEvent.WaitOne();

                    return nextItems;
                }
                set
                {
                    nextItems = value;
                }
            }
        }
    }
}
