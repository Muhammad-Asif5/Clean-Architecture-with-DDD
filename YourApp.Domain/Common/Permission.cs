using System.ComponentModel.DataAnnotations.Schema;

namespace YourApp.Domain.Common
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

    public interface ICanCreate
    {
        [NotMapped]
        bool CanCreate { get; set; }
    }

    public interface ICanUpdate
    {
        [NotMapped]
        bool CanUpdate { get; set; }
    }

    public interface ICanDelete
    {
        [NotMapped]
        bool CanDelete { get; set; }
    }

    public interface ICanExport
    {
        [NotMapped]
        bool CanExport { get; set; }
    }
}