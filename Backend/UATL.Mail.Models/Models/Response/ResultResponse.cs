using System;
using System.Collections.Generic;
using System.Text;

namespace UATL.MailSystem.Models.Response
{

    public class ResultResponse<TData, T> : IResultResponse<TData, T>
    {
        public ResultResponse(TData data, T results)
        {
            Results = results;
            Data = data;
        }
        public T Results { get; private set; }

        public TData Data { get; private set; }
    }
    public class MessageResponse<T>
    {
        public MessageResponse(T message)
        {
            Message = message;
        }
        public T Message { get; private set; }
    }
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
