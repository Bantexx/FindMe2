using Dapper;
using FindMe2.MainClasses;
using FindMe2.Models;
using FindMe2.ViewModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace FindMe2.DapRepo
{
    public interface IUserRepository
    {
        List<User> GetUsers();
        User GetUser(LoginModel model);
        User GetUserByLogin(string login);
        void CreateUser(RegisterModel model, (byte[] salt, string hash) hashvalues);
        void UpdateAvatar(string Path, string Login);
        User GetUserByEmail(string email);
        Task ResetPass(string login, string pass);
    }
    public class UserRepository : IUserRepository
    {
        string connectionString = null;
        public UserRepository(string connect)
        {
            connectionString = connect;
        }
        public async Task ResetPass(string login,string pass)
        {
            var new_pass = UserAuth.HashPass(pass);
            byte[] salt = new_pass.Item1;
            string hash = new_pass.Item2;
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "UPDATE Users SET Salt =@salt WHERE Login =@login";
                var sqlQuery1 = "INSERT INTO Hashes (Hash) VALUES(@hash)";
                await db.ExecuteAsync(sqlQuery, new { salt, login });
                await db.ExecuteAsync(sqlQuery1, new { hash });
            }
        }
        public void UpdateAvatar(string Path, string Login)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "UPDATE Users SET Picture =@Path WHERE Login =@Login";
                db.Execute(sqlQuery, new { Path, Login });
            }
        }
        public List<User> GetUsers()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return db.Query<User>("SELECT * FROM Users").ToList();
            }
        }
        public User GetUser(LoginModel model)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var User = db.Query<User>("SELECT * FROM Users WHERE login =@Login", new { model.Login }).FirstOrDefault();
                if (User != null)
                {
                    string hash = UserAuth.GetHash(User.Salt, model.Password);
                    return db.Query("SELECT * FROM Hashes WHERE Hash =@hash", new { hash }).FirstOrDefault() != null ? User : null;
                }
                return null;
                
            }
        }
        public User GetUserByLogin(string login)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE login =@login", new { login });
            }
        }
        public User GetUserByEmail(string Email)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Email =@Email", new { Email });
            }
        }
        public void CreateUser(RegisterModel model, (byte[] salt, string hash) hashvalues)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "INSERT INTO Users (Login,Email,Salt) VALUES(@Login,@Email,@salt)";
                var sqlQuery1 = "INSERT INTO Hashes (Hash) VALUES(@hash)";
                db.Execute(sqlQuery, new { model.Login, model.Email, hashvalues.salt });
                db.Execute(sqlQuery1, new { hashvalues.hash });
            }
        }
    }
}
