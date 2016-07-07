using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoUniversity.DAL
{
    class NotFoundException : Exception
    {
        // Four constructors.
        public NotFoundException() : base() { }
        public NotFoundException(string message) : base(message) { }
        public NotFoundException(string message, Exception innerException) : base(message, innerException) { }

        // Override base class toString () method which returns an error message.
        public override string ToString()
        {
            return this.Message;
        }
    }
}
