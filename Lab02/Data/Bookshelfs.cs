using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab02.Data
{
    public class Bookshelfs
    {
        public int Id { get; set; }

        public string UserName { get; set; }

        public Dictionary<string, string> Books { get; set; }
    }
}
