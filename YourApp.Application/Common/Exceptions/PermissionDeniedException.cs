namespace YourApp.Application.Common.Exceptions
{
    public class PermissionDeniedException : Exception
    {
        public PermissionDeniedException() : base("You do not have the required permissions to perform this action.")
        {
        }

        public PermissionDeniedException(string message) : base(message)
        {
        }

        public PermissionDeniedException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
