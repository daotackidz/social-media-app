using Social.Common.Handlers;
using Social.Data.Model.Base;
using Social.Data.Model.User;
using Social.Data.Seed.Interface;

namespace Social.Data.Seed.Models.User
{
    internal class UserSeed : ISeed<Users>
    {
        public Users[] GetSeedData => GetData();
        string salt = Convert.ToBase64String(new byte[256 / 8]);

        private Users[] GetData()
        {
            var results = new List<Users>()
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    UserName = "Administrator",
                    PassWord = PasswordHashHandler.HashPassWord("admin123"),
                    SaltPassWord = salt,
                    Email = "admin@example.com"
                }
            };

            return results.ToArray();
        }
    }
}
