using LiteDB;
using Newtonsoft.Json;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbedIO.Samples
{
    public class Database
    {
        private const string DATABASE_PATH = @"P:\UnityProjects\OctoberNights\OctoberNightDatabase.db";

        public static Database Instance;
        private LiteDatabase Db;
        public Database()
        {
            Instance = this;
            InitializeDB();

            AddUser(new User()
            {
                Name = "User1",
                EmailAddress = "mail@mail.ru",
                Password = "124",
                Token = "hibhu-jhvgv-jhvbuy"
            });
        }

        private void InitializeDB()
        {
            Db = new LiteDatabase(DATABASE_PATH);

            Db.DropCollection("Users");
            Db.DropCollection("Points");

            var colUsers = Db.GetCollection<User>("Users");
            var colPoints = Db.GetCollection<Point>("Points");
            colUsers.EnsureIndex(x => x.Name);
            colPoints.EnsureIndex(x => x.Id);
        }

        public IEnumerable<Point> GetPoints()
        {
            return Db.GetCollection<Point>("Points").Find(p => true);
        }

        public IEnumerable<User> GetUsers()
        {
            return Db.GetCollection<User>("Users").Find(p => true);
        }

        public void AddUser(User user)
        {
            ILiteCollection<User> col = Db.GetCollection<User>("Users");
            JsonConvert.SerializeObject(user).Info(nameof(Database));

            col.Insert(user);
            col.Update(user);
        }
    }
}