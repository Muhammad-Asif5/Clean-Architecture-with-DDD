namespace YourApp.Domain.Enums
{
    public enum UserType
    {
        SuperAdmin = 1,
        Academic = 2,
        Principal = 3,
        Teacher = 4,
        Student = 5,
    }

    //public static class UserTypeExtensions
    //{
    //    public static string ToDisplayName(this UserType userType)
    //    {
    //        return userType switch
    //        {
    //            UserType.Admin => "Administrator",
    //            UserType.Manager => "Manager",
    //            UserType.User => "Regular User",
    //            UserType.Guest => "Guest",
    //            _ => userType.ToString()
    //        };
    //    }
    //}
}