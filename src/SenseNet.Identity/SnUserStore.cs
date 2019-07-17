using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Search;
using Task = System.Threading.Tasks.Task;

namespace SenseNet.Identity
{
    public class SnUserStore : IUserPasswordStore<SnIdentityUser>, IUserEmailStore<SnIdentityUser>
    {
        #region IUserStore<SnIdentityUser> members
        public Task<string> GetUserIdAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(SnIdentityUser user, string userName, CancellationToken cancellationToken)
        {
            // remove domain if provided
            var sepIndex = userName.IndexOf("\\", StringComparison.Ordinal);
            var un = sepIndex > -1 ? userName.Substring(sepIndex + 1) : userName;

            user.UserName = un;

            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(SnIdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            // remove domain if provided
            var sepIndex = normalizedName.IndexOf("\\", StringComparison.Ordinal);
            var un = sepIndex > -1 ? normalizedName.Substring(sepIndex + 1) : normalizedName;

            user.NormalizedUserName = un;

            return Task.CompletedTask;
        }

        public Task<IdentityResult> CreateAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            using (new SystemAccount())
            {
                var name = ContentNamingProvider.GetNameFromDisplayName(user.UserName);

                var content = Content.CreateNew("User", Node.LoadNode("/Root/IMS/BuiltIn/Portal"), name);
                content["DisplayName"] = user.UserName;
                content["LoginName"] = user.UserName;
                content["Email"] = user.Email;
                ((User) content.ContentHandler).PasswordHash = user.PasswordHash;
                content.Save();

                user.Id = content.Id;

                //UNDONE: every user is administrator now
                Group.Administrators.AddMember(content.ContentHandler as User);
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            using (new SystemAccount())
            {
                var userContent = Node.Load<User>(user.Id);
                userContent.LoginName = user.UserName;
                userContent.Email = user.Email;
                userContent.PasswordHash = user.PasswordHash;
                userContent.Save(SavingMode.KeepVersion);
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            using (new SystemAccount())
            {
                Content.DeletePhysical(user.Id);
            }

            return Task.FromResult(IdentityResult.Success);
        }

        public Task<SnIdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            var id = Convert.ToInt32(userId);
            var user = SystemAccount.Execute(() => Node.Load<User>(id));

            return Task.FromResult(SnIdentityUser.FromUser(user));
        }

        public Task<SnIdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var user = SystemAccount.Execute(() => User.Load(normalizedUserName));

            return Task.FromResult(SnIdentityUser.FromUser(user));
        }
        #endregion

        #region IUserPasswordStore<SnIdentityUser> members
        public Task SetPasswordHashAsync(SnIdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task<string> GetPasswordHashAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }
        #endregion

        #region IUserEmailStore<SnIdentityUser> members
        public Task SetEmailAsync(SnIdentityUser user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;

            return Task.CompletedTask;
        }

        public Task<string> GetEmailAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }

        public Task SetEmailConfirmedAsync(SnIdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task<SnIdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var user = SystemAccount.Execute(() => ContentQuery.Query($"+TypeIs:User +Email:'{normalizedEmail}'")
                .Nodes.FirstOrDefault() as User);

            return Task.FromResult(SnIdentityUser.FromUser(user));
        }

        public Task<string> GetNormalizedEmailAsync(SnIdentityUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task SetNormalizedEmailAsync(SnIdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;

            return Task.CompletedTask;
        }
        #endregion

        public void Dispose()
        {
        }
    }
}
