using InSync.eConnect.APPSeCONNECT.Storage.Base;
using InSync.eConnect.APPSeCONNECT.Storage.Tables;
using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSync.eConnect.ZohoDesk
{
    /// <summary>
    /// Generate table based on rows defined here
    /// </summary>
    [Table(Name = "SyncInfo")] // represents Table name
    public class SyncInfoDataTable : ObjectBase
    {
        [Column(Name = "Id", IsPrimaryKey = true, Type = DbType.String, Length = 100)] //represents columns
        public string Id { get; set; }
        
        //ToDo : Create all data columns you need to store in database
    }
}
