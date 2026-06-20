using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Application.Common.Interfaces
{
    public interface ICanCreate
    {
        [NotMapped]
        public bool CanCreate { get; set; }
    }
    public interface ICanUpdate
    {
        [NotMapped]
        public bool CanUpdate { get; set; }
    }
    public interface ICanDelete
    {
        [NotMapped]
        public bool CanDelete { get; set; }
    }
    public interface ICanExport
    {
        [NotMapped]
        public bool CanExport { get; set; }
    }
}
