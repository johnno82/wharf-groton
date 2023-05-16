using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Groton.Core.Data.Collections
{
    public class PagedList<TEntity>
    {
        #region Properties

        public int PageIndex { get; private set; }

        public int PageSize { get; private set; }

        public IReadOnlyCollection<TEntity> Items { get; private set; }

        public int TotalItems { get; private set; }

        #endregion

        #region Constructors

        public PagedList(IEnumerable<TEntity> items, int pageIndex, int pageSize, int totalItems)
        {
            this.Items = items.ToList();

            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.TotalItems = totalItems;
        }

        #endregion
    }
}
