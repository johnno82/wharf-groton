using Groton.Core.Data.Collections;

namespace Groton.WebApp.Models
{
    public class PagedListModel<TEntity> : PagedList<TEntity>
    {
        public bool HasPreviousPage => this.PageIndex > 0;

        public bool HasNextPage => this.PageIndex < (int)Math.Floor(this.TotalItems / (float)this.PageSize);

        public PagedListModel(PagedList<TEntity> list)
            : base(list.Items, list.PageIndex, list.PageSize, list.TotalItems)
        {

        }
    }
}
