namespace YourApp.Domain.Constants
{
    public static class Permissions
    {
        public static Dictionary<string, string[]> ControllerPermissions = new Dictionary<string, string[]>
        {
            { Roles.Product, new[] { ProductPermission.CanRead, ProductPermission.CanCreate, ProductPermission.CanUpdate, ProductPermission.CanDelete, ProductPermission.CanExport } }            
        };

        public static class ProductPermission
        {
            public const string CanRead = $"{Roles.Product}.{Claims.CanRead}";
            public const string CanCreate = $"{Roles.Product}.{Claims.CanCreate}";
            public const string CanUpdate = $"{Roles.Product}.{Claims.CanUpdate}";
            public const string CanDelete = $"{Roles.Product}.{Claims.CanDelete}";
            public const string CanExport = $"{Roles.Product}.{Claims.CanExport}";
        }

      
        public static class Claims
        {
            public const string CanRead = "Can Read";
            public const string CanCreate = "Can Create";
            public const string CanUpdate = "Can Update";
            public const string CanDelete = "Can Delete";
            public const string CanExport = "Can Export";
            public const string CanPrint = "Can Print";
            public const string CanCollect = "Can Collect";
            public const string CanDrop = "Can Drop";
            public const string CanRegister = "Can Register";
            public const string CanUpdateSection = "Can Update Section";
        }
    }
}