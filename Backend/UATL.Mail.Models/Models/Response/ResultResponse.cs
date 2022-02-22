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
}
