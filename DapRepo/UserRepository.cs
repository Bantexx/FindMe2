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
        List<User> GetUsers(string login);
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
        List<ChatUserProfile> GetUserChats(int id, string su);
        List<Message> GetChatMessages(int idchat);
        List<Attachment> GetChatAttach(int idchat);
        int GetChat(int _IdUser1, int _IdUser2);
        Task AddMessage(int IdSender, string mess, DateTime timemess, int chatId);
        List<AuthorNewsVM> GetNewsByStrSearch(string str);
        void DelUserTag(int id_user, int id_tag);
        void AddTagToUser(int id_user, int id_tag);
        void UpdateLocation(int id_user,string loc);

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
        public List<User> GetUsers(string login)
        {
            string likestr = "%" + login + "%";
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                return db.Query<User>("SELECT * FROM Users WHERE Login LIKE @likestr ", new { likestr }).ToList();
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
                var sqlQuery = "SELECT DISTINCT n.Id_News as idNews,n.Id_Author as IdAuthor,n.Text,n.Date as DateNews,n.Interested,n.Picture,us.Login as LoginAuthor,us.Picture as Avatar,n.Title " +
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
                var sqlQuery = "SELECT n.Id_News as idNews,n.Id_Author as IdAuthor,n.Text,n.Date as DateNews,n.Interested,n.Picture,us.Login as LoginAuthor,us.Picture as Avatar,n.Title" +
                    " FROM News n JOIN Users us ON n.Id_Author=us.Id WHERE EXISTS(SELECT NULL FROM News_tags" +
                    " WHERE Id_Tag IN (SELECT TOP(@count_tag) Id_Tag FROM News_tags GROUP BY Id_Tag ORDER BY COUNT(Id_Tag) DESC))";
                return db.Query<AuthorNewsVM>(sqlQuery, new { count_tag }).ToList();
            }
        }
        public List<ChatUserProfile> GetUserChats(int id,string su)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                if(su != null)
                {
                    var sub = "%"+su+"%";
                    var sqlQuery = "select u.Id as IdUser,u.Login,u.Picture as Avatar,GeoLocation,subquery.Id_Chat as ChatId from Users u" +
                               " JOIN(SELECT Id_User1 as Id, Id_Chat FROM Chats WHERE Id_User2 = @id UNION SELECT Id_User2 as ID, Id_Chat FROM Chats WHERE Id_User1 = @id) subquery" +
                               " ON subquery.Id = u.Id WHERE u.Login LIKE @sub";
                    return db.Query<ChatUserProfile>(sqlQuery, new { id, sub }).ToList();
                }
                else
                {
                    var sqlQuery = "select u.Id as IdUser,u.Login,u.Picture as Avatar,GeoLocation,subquery.Id_Chat as ChatId from Users u" +
                               " JOIN(SELECT Id_User1 as Id, Id_Chat FROM Chats WHERE Id_User2 = @id UNION SELECT Id_User2 as ID, Id_Chat FROM Chats WHERE Id_User1 = @id) subquery" +
                               " ON subquery.Id = u.Id";
                    return db.Query<ChatUserProfile>(sqlQuery, new { id }).ToList();
                }
                
                
            }
        }
        public List<Message> GetChatMessages(int idchat)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT * FROM Messages WHERE Id_chat = @idchat";
                return db.Query<Message>(sqlQuery, new { idchat }).ToList();
            }
        }
        public List<Attachment> GetChatAttach(int idchat)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT Id_Att,Id_Chat,Id_Mess,Attachment as Attach FROM Chat_Attachments WHERE Id_Chat = @idchat";
                return db.Query<Attachment>(sqlQuery, new { idchat }).ToList();
            }
        }
        public int GetChat(int _IdUser1,int _IdUser2)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT Id_Chat FROM Chats WHERE (Id_User1=@_IdUser1 AND Id_User2=@_IdUser2) OR (Id_User1=@_IdUser2 AND Id_User2=@_IdUser1)";
                return db.Query<int>(sqlQuery, new { _IdUser1, _IdUser2 }).FirstOrDefault();
            }
        }
        public async Task AddMessage(int IdSender, string mess, DateTime timemess, int chatId)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "INSERT INTO Messages (Id_chat,Id_sender,date_create,isRead,Text) VALUES(@chatid,@IdSender,@timemess,1,@mess)";
                await db.ExecuteAsync(sqlQuery, new { chatId,IdSender,timemess,mess});
            }
        }
        public List<AuthorNewsVM> GetNewsByStrSearch(string str)
        {
            var likestr = "%" + str + "%";
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT DISTINCT n.Id_News as idNews,n.Id_Author as IdAuthor,n.Text,n.Date as DateNews,n.Interested,n.Picture,us.Login as LoginAuthor,us.Picture as Avatar,n.Title" +
                                " FROM News n" +
                                " JOIN Users us ON n.Id_Author=us.Id" +
                                " JOIN News_tags n_t ON n.Id_News = n_t.Id_News"+
                                " WHERE n.Text LIKE @likestr OR n.Title LIKE @likestr";
                return db.Query<AuthorNewsVM>(sqlQuery, new { likestr }).ToList();
            }
        }
        public void DelUserTag(int id_user,int id_tag)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "DELETE FROM Profile_tags WHERE Id_User = @id_user AND Id_Tag = @id_tag";
                db.Execute(sqlQuery, new { id_user, id_tag });
            }
        }
        public void AddTagToUser(int id_user, int id_tag)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "INSERT INTO Profile_tags(Id_User,Id_Tag) VALUES(@id_user,@id_tag)";
                db.Execute(sqlQuery, new { id_user, id_tag });
            }
        }
        public void UpdateLocation(int id_user,string loc)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "UPDATE Users SET GeoLocation = @loc WHERE Id= @id_user";
                db.Execute(sqlQuery, new { id_user, loc });
            }
        }
    }
}
