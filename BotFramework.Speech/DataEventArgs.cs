using System;
namespace BotFramework.Speech
{
    public class DataEventArgs<T> : EventArgs
    {
        public DataEventArgs(T data)
        {
            this.Data = data;
        }

        public T Data { get; private set; }
    }
}
