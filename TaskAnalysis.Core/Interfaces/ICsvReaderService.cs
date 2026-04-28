using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskAnalysis.Core.Entities;

namespace TaskAnalysis.Core.Interfaces
{
    public interface ICsvReaderService
    {
        List<TaskRecord> ReadAllCsv(string folderPath);
        List<TaskRecord> ReadCsv(string filePath);
    }
}
