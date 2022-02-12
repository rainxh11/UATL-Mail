﻿namespace UATL.MailSystem.Models.Response
{
    public interface IResultResponse<TData,T>
    {
        public T Results { get; }
        public TData Data { get; }
    }
}
