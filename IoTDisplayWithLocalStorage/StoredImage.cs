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
        /// <summary>
        /// Image ID
        /// </summary>
        [PrimaryKey, AutoIncrement]
        public int ImageID { get; set; }

        /// <summary>
        /// Image itself as BLOB
        /// </summary>
        public byte[] Image { get; set; }
    }
}
