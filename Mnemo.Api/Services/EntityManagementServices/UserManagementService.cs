using Mnemo.Infrastructure;
using Mnemo.Infrastructure.Queries;
using Mnemo.Entities;
using Mnemo.Shared.Extensions;
using Mnemo.Infrastructure.RequestResults;
using Mnemo.Infrastructure.ErrorCodes;

namespace Mnemo.Services.ManagementServices
{
    public class UserManagementService
    {
        private AppDbContext _context;

        private UserQueries _accountQueries;


        public UserManagementService(AppDbContext context, UserQueries accountQueries)
        {
            _context = context;
            _accountQueries = accountQueries;
        }



        public async Task<RequestResult<bool>> CreateAsync(string username)
        {
            if (await _accountQueries.ExistsByUsernameAsync(username))
                return RequestResult<bool>.Failure(ErrorCode.UsernameTaken, $"Username '{username}' is taken");


            var user = new User();
            user.Username = username;

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return RequestResult<bool>.Success(true);
        }
    }
}
