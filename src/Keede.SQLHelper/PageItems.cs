using System;
using System.Collections.Generic;

namespace Keede.SQLHelper
{
    /// <summary>
    ///
    /// </summary>
    [Serializable]
    public class PageItems<T> where T : class ,new()
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="items"></param>
        public PageItems(int pageSize, long recordCount, IEnumerable<T> items)
        {
            PageSize = pageSize;
            RecordCount = recordCount;
            Items = items;
            PageCount = new Func<long>(delegate
                {
                    var pages = RecordCount / PageSize;
                    if (RecordCount % PageSize != 0)
                    {
                        pages = pages + 1;
                    }
                    if (PageIndex > pages)
                    {
                        PageIndex = pages;
                    }
                    return pages;
                }).Invoke();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="items"></param>
        public PageItems(int pageIndex, int pageSize, long recordCount, IEnumerable<T> items)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            RecordCount = recordCount;
            Items = items;
            PageCount = new Func<long>(delegate
            {
                var pages = RecordCount / PageSize;
                if (RecordCount % PageSize != 0)
                {
                    pages = pages + 1;
                }
                if (PageIndex > pages)
                {
                    PageIndex = pages;
                }
                return pages;
            }).Invoke();
        }

        /// <summary>
        ///
        /// </summary>
        public long PageIndex { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public long PageSize { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public long PageCount { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public long RecordCount { get; private set; }

        /// <summary>
        ///
        /// </summary>
        public IEnumerable<T> Items { get; private set; }
    }
}