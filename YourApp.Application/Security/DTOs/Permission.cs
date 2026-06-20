using System.ComponentModel.DataAnnotations.Schema;
using YourApp.Application.Common.Interfaces;

namespace YourApp.Application.Security.DTOs
{
    public class Permission : ICanCreate, ICanUpdate, ICanDelete, ICanExport
    {
        [NotMapped]
        public bool CanCreate { get; set; }
        [NotMapped]
        public bool CanRead { get; set; }
        [NotMapped]
        public bool CanUpdate { get; set; }
        [NotMapped]
        public bool CanDelete { get; set; }
        [NotMapped]
        public bool CanExport { get; set; }
    }
}
