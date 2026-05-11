using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.Entities.CSVEntities;

namespace TaskAnalysis.Core.Interfaces.ICsvReader
{
    public interface ICsvReadersHelper
    {
        public bool IsMeaningful(TaskRecord record);

        public string GetDirektorlukFromFileName(string fileName); 
        // --> Dosya adını okunabilir bir direktörlük adı haline getiriyor.

    }
}
