namespace Mnemo.Contracts
{
    public class PageValue<T>
    {
        public IReadOnlyList<T> Items { get; }
        public int Page { get; }
        public int PageSize { get; }
        public int TotalPages { get; }


        public PageValue(int page, int pageSize, int totalPages, IReadOnlyList<T> items)
        {
            Page = page;
            PageSize = pageSize;
            TotalPages = totalPages;
            Items = items;
        }
    }
}
