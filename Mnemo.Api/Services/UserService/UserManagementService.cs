using Mnemo.Data;
using Mnemo.Data.Entities;
using Mnemo.Data.Queries;
using Mnemo.Shared;
using Mnemo.Shared.Enums;
using Mnemo.Shared.Extensions;

namespace Mnemo.Services.UserService
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
