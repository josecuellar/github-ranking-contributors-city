using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GitHub.API.Repository.Impl
{
    public class SQLite3PersitenceLocalDataRepository : IPersistenceLocalDataRepository
    {

        private static string DbFile
        {
            get { return Environment.CurrentDirectory + "\\Data\\LocalDataRepository.db"; }
        }

        private static SQLiteConnection DbConnection()
        {
            return new SQLiteConnection("Data Source=" + DbFile + ";Version=3;Pooling=True;Max Pool Size=150;FailIfMissing=True;Journal Mode=Off;");
        }

        public void CreateListIfNotExists(List<Model.User> users)
        {
            users.ForEach(async item =>
            {
                await CreateIfNotExists(item);
            });
        }

        public void SaveList(List<Model.User> users)
        {
            users.ForEach(async item =>
            {
                await Save(item);
            });
        }

        public async Task CreateIfNotExists(Model.User user)
        {
            if (user.Id <= 0)
                throw new ArgumentOutOfRangeException("User.Id is mandatory for save entity");

            try
            {
                using (var cnn = DbConnection())
                {
                    if (!cnn.Query<bool>(@"SELECT 1 FROM Users WHERE GitHubId = @GitHubId", new { GitHubId = user.Id }).FirstOrDefault())
                    {
                        await cnn.ExecuteAsync(
                            @"INSERT INTO Users ( GitHubId, Login, Location, Url, TotalCommits ) VALUES ( @GitHubId, @Login, @Location, @Url, @TotalCommits );",
                            new { GitHubId = user.Id, Login = user.Login, Location = user.Location, Url = user.Url, TotalCommits = user.TotalCommits });
                    }
                }
            }
            catch (Exception err)
            {
                Debug.Print("CreateIfNotExists: " + err.Message);
            }
        }

        public async Task Save(Model.User user)
        {
            if (user.Id <= 0)
                throw new ArgumentOutOfRangeException("User.Id is mandatory for save entity");

            try
            {
                using (var cnn = DbConnection())
                {

                    var paramsUser = new { GitHubId = user.Id, Login = user.Login, Location = user.Location, Url = user.Url, TotalCommits = user.TotalCommits };

                    if (cnn.Query<bool>(@"SELECT 1 FROM Users WHERE GitHubId = @GitHubId", new { GitHubId = user.Id }).FirstOrDefault())
                        await cnn.ExecuteAsync(@"UPDATE Users SET Login = @Login, Location = @Location, Url = @Url, TotalCommits = @TotalCommits WHERE GitHubId = @GitHubId;", paramsUser);
                    else
                        await cnn.ExecuteAsync(@"INSERT INTO Users ( GitHubId, Login, Location, Url, TotalCommits ) VALUES ( @GitHubId, @Login, @Location, @Url, @TotalCommits);", paramsUser);
                }
            }
            catch (Exception err)
            {
                Debug.Print("Save: " + err.Message);
            }
        }
    }
}
