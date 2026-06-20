namespace YourApp.Application.Users.Commands.UnlockAccount
{
    public class UnlockAccountResponse
    {
        public bool IsSuccess { get; set; }
        public string[] Errors { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        public UnlockAccountResponse()
        {
            Errors = Array.Empty<string>();
        }
    }
}