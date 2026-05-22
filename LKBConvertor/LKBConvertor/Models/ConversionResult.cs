using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LKBConvertor.Models
{
    public class ConversionResult
    {
        public bool EstSucces { get; set; }
        public string CheminSortie { get; set; }
        public string MessageErreur { get; set; }
        public long TailleOctets { get; set; }
    }
}