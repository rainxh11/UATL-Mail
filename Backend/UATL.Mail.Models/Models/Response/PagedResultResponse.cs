namespace UATL.MailSystem.Models.Response
{
    public class PagedResultResponse<TData> 
    {
        public PagedResultResponse(TData data, long total, long pageCount, long pageSize, long page)
        {
            Data = data;
            Total = total;
            PageSize = pageSize;
            PageCount = pageCount;
            Page = page;
        }

        public long Total { get; private set; }
        public long Page { get; private set; }
        public long PageCount { get; private set; }
        public long PageSize { get; private set; }
        public TData Data { get; private set; }

    }
}
