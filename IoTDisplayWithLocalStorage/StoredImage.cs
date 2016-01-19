using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IoTDisplayWithLocalStorage
{
    public class StoredImage
    {
        [PrimaryKey, AutoIncrement]
        public int ImageID { get; set; }
        public byte[] Image { get; set; }
    }
}
