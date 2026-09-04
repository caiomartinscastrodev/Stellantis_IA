using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace services.interfaces
{
    public interface IIngestaoService
    {
        public Task Ingestao();
    }
}