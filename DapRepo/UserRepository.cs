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
        List<Tag> GetUserTags(int id);
        void UpdateInfoUser(EditProfileVM profile, int id);
        User GetUserById(string id);
        List<Tag> GetPopularTags(int count_tags);
        List<AuthorNewsVM> GetNewsByUserTags(int id);
        List<AuthorNewsVM> GetPopularNews(int count_tag);
    }
    public class UserRepository : IUserRepository
    {
        string connectionString = null;
        public UserRepository(string connect)
        {
            connectionString = connect;
        }
        public User GetUserById(string id)
        {
            var id_number = Convert.ToInt32(id);
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id =@id_number", new { id_number });
            }
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
        public List<Tag> GetUserTags(int id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return db.Query<Tag>("SELECT * FROM Tags INNER JOIN Profile_tags ON Id_Tag=Id WHERE Id_User = @id", new { id }).ToList();
            }
        }
        public void UpdateInfoUser(EditProfileVM profile,int id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "UPDATE Users SET Login =@Login,Email=@Email WHERE Id=@id";
                db.Execute(sqlQuery, new { profile.Login, profile.Email, id });
            }
        }
        public List<Tag> GetPopularTags(int count_tags)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT * FROM Tags WHERE Id IN ( SELECT TOP (@count_tags) Id_Tag FROM News_tags GROUP BY Id_Tag ORDER BY COUNT(Id_Tag) DESC)";
                return db.Query<Tag>(sqlQuery, new { count_tags }).ToList();
            }
        }
        public List<AuthorNewsVM> GetNewsByUserTags(int id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT DISTINCT n.Id_News as idNews,n.Id_Author as IdAuthor,n.Text,n.Date as DateNews,n.Interested,n.Picture,us.Login as LoginAuthor,us.Picture as Avatar " +
                                " FROM News n" + 
                                " JOIN Users us ON n.Id_Author=us.Id" +
                                " JOIN Profile_tags p_t ON p_t.Id_User = @id" +
                                " JOIN News_tags n_t ON n.Id_News = n_t.Id_News AND n_t.Id_tag = p_t.Id_tag";
                return db.Query<AuthorNewsVM>(sqlQuery, new { id }).ToList();
            }
        }
        public List<AuthorNewsVM> GetPopularNews(int count_tag)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT n.Id_News as idNews,n.Id_Author as IdAuthor,n.Text,n.Date as DateNews,n.Interested,n.Picture,us.Login as LoginAuthor,us.Picture as Avatar" +
                    " FROM News n JOIN Users us ON n.Id_Author=us.Id WHERE EXISTS(SELECT NULL FROM News_tags" +
                    " WHERE Id_Tag IN (SELECT TOP(@count_tag) Id_Tag FROM News_tags GROUP BY Id_Tag ORDER BY COUNT(Id_Tag) DESC))";
                return db.Query<AuthorNewsVM>(sqlQuery, new { count_tag }).ToList();
            }
        }
    }
}
