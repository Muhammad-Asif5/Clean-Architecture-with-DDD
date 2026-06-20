namespace YourApp.Application.Common.Exceptions
{
    public class PlanExpiredException : Exception
    {
        public PlanExpiredException() : base("Your subscription plan has expired.")
        {
        }

        public PlanExpiredException(string message) : base(message)
        {
        }

        public PlanExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}