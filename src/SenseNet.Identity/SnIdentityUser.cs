using Microsoft.AspNetCore.Identity;
using SenseNet.ContentRepository;

namespace SenseNet.Identity
{
    public class SnIdentityUser : IdentityUser<int>
    {
        public static SnIdentityUser FromUser(User user)
        {
            if (user == null)
                return null;

            return new SnIdentityUser
            {
                Id = user.Id,
                PasswordHash = user.PasswordHash,
                UserName = user.LoginName,
                Email = user.Email,
                EmailConfirmed = true
            };
        }
    }
}
