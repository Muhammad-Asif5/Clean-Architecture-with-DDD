using MediatR;

namespace YourApp.Application.Users.Commands.UnlockAccount
{
    public class UnlockAccountCommand : IRequest<UnlockAccountResponse>
    {
        public string UserId { get; set; }
    }
}
