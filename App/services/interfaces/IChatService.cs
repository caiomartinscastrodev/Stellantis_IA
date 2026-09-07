using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.services.interfaces
{
    public interface IChatService
    {
        public Task<string> Chat (string message);

        public IAsyncEnumerable<string> ChatStreaming (string message);
    }
}