using System;

namespace SqliteExtensions
{
    public class MieszkoQueryException : Exception
    {
        public MieszkoQueryException(string message)
            : base(message)
        {

        }
    }
}
