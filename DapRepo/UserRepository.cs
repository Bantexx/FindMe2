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
        void UpdateAvatar(string Path, int id_user);
        User GetUserByEmail(string email);
        Task ResetPass(string login, string pass);
        List<Tag> GetUserTags(int id);
        void UpdateInfoUser(EditProfileVM profile, int id);
        User GetUserById(string id);
        List<Tag> GetPopularTags(int count_tags);
        NewsInLent GetNewsByUserTags(int id);
        NewsInLent GetPopularNews(int count_tag);
        NewsInLent GetAllNews();
        List<ChatUserProfile> GetUserChats(int id, string su);
        List<Message> GetChatMessages(int idchat);
        List<Attachment> GetChatAttach(int idchat);
        int GetChat(int _IdUser1, int _IdUser2);
        void AddMessage(int IdSender, string mess, DateTime timemess, int chatId);
        List<AuthorNewsVM> GetNewsByStrSearch(string str);
        void DelUserTag(int id_user, int id_tag);
        void AddTagToUser(int id_user, int id_tag);
        void UpdateLocation(int id_user,string loc);
        void CreateChat(int id_user, int id_comp);
        void AddToFavoNews(int id_news, int id_user);
        void RemoveFromFavoNews(int id_news, int id_user);
        List<FavoritesNews> GetFavNews(int id_user);
        void AddNews(string title, string text, int id_user, DateTime dt);
        int GetInterestedNews(int id_news);
        void DeleteChat(int chat_id,int id_user);
        void AddToFavMess(int chat_id, int mess_id, int user_id);
        List<FavoMess> GetMessFavo(int chat_id, int user_id);
        void DelFromFavMess(int chat_id, int mess_id, int user_id);
        void DelMessFromChat(int chat_id, int mess_id, int user_id);
        Message GetLastMessage(int chat_id);
        Task AddAttachToMess(int chat_id, int id_mess, string path);
        List<Attachment> GetMessAttach(int chat_id, int id_mess);
        List<Tag> GetAllTags();
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
            try
            { 
                var id_number = Convert.ToInt32(id);
                using (IDbConnection db = new SqlConnection(connectionString))
                {
                    return db.QueryFirstOrDefault<User>("SELECT * FROM Users WHERE Id =@id_number", new { id_number });
                }
            }
            catch (FormatException)
            {
                return null;
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
        public void UpdateAvatar(string Path, int id_user)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "UPDATE Users SET Picture =@Path WHERE Id =@id_user";
                db.Execute(sqlQuery, new { Path, id_user });
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
        public NewsInLent GetNewsByUserTags(int id)
        {
            NewsInLent lentnews = new NewsInLent();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT DISTINCT n.Id_News as idNews,n.Id_Author as IdAuthor,n.Text,n.Date as DateNews,n.Interested,n.Picture,us.Login as LoginAuthor,us.Picture as Avatar,n.Title " +
                                " FROM News n" + 
                                " JOIN Users us ON n.Id_Author=us.Id" +
                                " JOIN Profile_tags p_t ON p_t.Id_User = @id" +
                                " JOIN News_tags n_t ON n.Id_News = n_t.Id_News AND n_t.Id_tag = p_t.Id_tag";
                var sqlQuery_1 = "SELECT n_t.*,t.Title" +
                                " FROM News n" +
                                " JOIN Users us ON n.Id_Author=us.Id" +
                                " JOIN Profile_tags p_t ON p_t.Id_User = @id" +
                                " JOIN News_tags n_t ON n.Id_News = n_t.Id_News AND n_t.Id_tag = p_t.Id_tag" +
                                " JOIN Tags t ON t.Id = n_t.Id_Tag";
                var sqlQuery_2 = "SELECT DISTINCT n_at.*" +
                                " FROM News n" +
                                " JOIN Users us ON n.Id_Author=us.Id" +
                                " JOIN Profile_tags p_t ON p_t.Id_User = @id" +
                                " JOIN News_tags n_t ON n.Id_News = n_t.Id_News AND n_t.Id_tag = p_t.Id_tag" +
                                " JOIN News_Attachments n_at ON n.Id_News = n_at.Id_news";
                lentnews.lent_tags = db.Query<News_tags>(sqlQuery_1, new { id }).ToList();
                lentnews.lent_news = db.Query<AuthorNewsVM>(sqlQuery, new { id }).ToList();
                lentnews.lent_attach = db.Query<News_Attachment>(sqlQuery_2, new { id }).ToList();
                return lentnews;
            }
        }
        public NewsInLent GetPopularNews(int count_tag)
        {
            NewsInLent lentnews = new NewsInLent();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT n.Id_News as idNews,n.Id_Author as IdAuthor,n.Text,n.Date as DateNews,n.Interested,n.Picture,us.Login as LoginAuthor,us.Picture as Avatar,n.Title" +
                    " FROM News n JOIN Users us ON n.Id_Author=us.Id WHERE EXISTS(SELECT NULL FROM News_tags" +
                    " WHERE Id_News = n.Id_News AND Id_Tag IN (SELECT TOP(@count_tag) Id_Tag FROM News_tags GROUP BY Id_Tag ORDER BY COUNT(Id_Tag) DESC))";
                var sqlQuery_1 = "SELECT n_t.*,t.Title" +
                    " FROM News n JOIN Users us ON n.Id_Author=us.Id" +
                    " JOIN News_tags n_t ON n.Id_News = n_t.Id_News" +
                    " JOIN Tags t ON t.Id = n_t.Id_Tag" +
                    " WHERE EXISTS(SELECT NULL FROM News_tags" +
                    " WHERE Id_Tag IN (SELECT TOP(@count_tag) Id_Tag FROM News_tags GROUP BY Id_Tag ORDER BY COUNT(Id_Tag) DESC))";
                var sqlQuery_2 = "SELECT DISTINCT n_at.*" +
                    " FROM News n JOIN Users us ON n.Id_Author=us.Id" +
                    " JOIN News_tags n_t ON n.Id_News = n_t.Id_News" +
                    " JOIN News_Attachments n_at ON n.Id_News = n_at.Id_news" +
                    " WHERE EXISTS(SELECT NULL FROM News_tags" +
                    " WHERE Id_Tag IN (SELECT TOP(@count_tag) Id_Tag FROM News_tags GROUP BY Id_Tag ORDER BY COUNT(Id_Tag) DESC))";
                lentnews.lent_news = db.Query<AuthorNewsVM>(sqlQuery, new { count_tag }).ToList();
                lentnews.lent_tags = db.Query<News_tags>(sqlQuery_1, new { count_tag }).ToList();
                lentnews.lent_attach = db.Query<News_Attachment>(sqlQuery_2, new { count_tag }).ToList();
                return lentnews;
            }
        }

        public NewsInLent GetAllNews()
        {
            NewsInLent lentnews = new NewsInLent();
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT DISTINCT n.Id_News as idNews,n.Id_Author as IdAuthor,n.Text,n.Date as DateNews,n.Interested,n.Picture,us.Login as LoginAuthor,us.Picture as Avatar,n.Title " +
                                " FROM News n" +
                                " JOIN Users us ON n.Id_Author=us.Id" +
                                " LEFT JOIN News_tags n_t ON n.Id_News = n_t.Id_News";
                var sqlQuery_1 = "SELECT n_t.*,t.Title" +
                                " FROM News n" +
                                " JOIN News_tags n_t ON n.Id_News = n_t.Id_News" +
                                " JOIN Tags t ON t.Id = n_t.Id_Tag";
                var sqlQuery_2 = "SELECT DISTINCT n_at.*" +
                                " FROM News n" +
                                " JOIN News_Attachments n_at ON n.Id_News = n_at.Id_news";
                lentnews.lent_tags = db.Query<News_tags>(sqlQuery_1).ToList();
                lentnews.lent_news = db.Query<AuthorNewsVM>(sqlQuery).ToList();
                lentnews.lent_attach = db.Query<News_Attachment>(sqlQuery_2).ToList();
                return lentnews;
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
        public Chat GetChat(int chat_id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "SELECT * FROM Chats WHERE Id_Chat =@chat_id";
                return db.Query<Chat>(sqlQuery, new { chat_id }).FirstOrDefault();
            }
        }
        public void AddMessage(int IdSender, string mess, DateTime timemess, int chatId)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                var sqlQuery = "INSERT INTO Messages (Id_chat,Id_sender,date_create,isRead,Text) VALUES(@chatid,@IdSender,@timemess,1,@mess)";
                db.Execute(sqlQuery, new { chatId,IdSender,timemess,mess});
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
        public void CreateChat(int id_user,int id_comp)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string check_chat = "SELECT * FROM Chats WHERE (Id_User1 = @id_user AND Id_User2 = @id_comp) OR (Id_User1 = @id_comp AND Id_User2 = @id_user)";
                Chat has_chat = db.Query<Chat>(check_chat, new { id_user, id_comp }).FirstOrDefault();
                if (has_chat == null)
                {
                    var sqlQuery = "INSERT INTO Chats(Id_User1,Id_User2) VALUES(@id_user,@id_comp)";
                    db.Execute(sqlQuery, new { id_user, id_comp });
                }             
            }
        }
        public void AddToFavoNews(int id_news, int id_user)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                News current_news = db.Query<News>("SELECT * FROM News WHERE Id_News =@id_news", new { id_news }).FirstOrDefault();
                if (current_news != null)
                {
                    string sql_insert = "INSERT INTO FavoritesNews(Id_User,Id_News) VALUES(@id_user,@id_news)";
                    db.Execute(sql_insert, new { id_news, id_user });
                    int count_intr = GetInterestedNews(id_news)+1;
                    string sql_update = "UPDATE News SET Interested = @count_intr WHERE Id_News=@id_news";
                    db.Execute(sql_update, new { id_news, count_intr });
                }
            }
        }
        public void RemoveFromFavoNews(int id_news, int id_user)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                News current_news = db.Query<News>("SELECT * FROM News WHERE Id_News =@id_news",new { id_news}).FirstOrDefault();
                if(current_news != null)
                {
                    string sql_del = "DELETE FROM FavoritesNews WHERE Id_User = @id_user AND Id_News = @id_news";
                    db.Execute(sql_del, new { id_user, id_news });
                    int count_intr = GetInterestedNews(id_news);
                    if (count_intr > 0)
                    {
                        count_intr = count_intr - 1;
                        string sql_update = "UPDATE News SET Interested =@count_intr WHERE Id_News=@id_news";
                        db.Execute(sql_update, new { id_news, count_intr });
                    }
                }
            }
        }
        public List<FavoritesNews> GetFavNews(int id_user)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql_select = "SELECT * FROM FavoritesNews WHERE Id_User = @id_user";
                return db.Query<FavoritesNews>(sql_select, new { id_user }).ToList();
            }
        }
        public void AddNews(string title,string text,int id_user,DateTime dt)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql_insert = "INSERT INTO News(Id_Author,Text,Date,Title,Interested) VALUES(@id_user,@text,@dt,@title,0)";
                db.Execute(sql_insert, new { id_user,dt,text,title});
            }
        }
        public int GetInterestedNews(int id_news)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string select_int = "SELECT Interested FROM News WHERE Id_News=@id_news";
                return db.Query<int>(select_int, new { id_news }).FirstOrDefault();
            }
        }
        public void DeleteChat(int chat_id,int id_user)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql_del = "DELETE FROM Chats WHERE Id_Chat =@chat_id AND (Id_User1=@id_user OR Id_User2=@id_user)";
                db.Execute(sql_del, new { chat_id, id_user });
            }
        }
        public void AddToFavMess(int chat_id, int mess_id, int user_id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                int[] messages = GetMessFavo(chat_id, user_id).Select(x=>x.Id_Mess).ToArray();
                if (!messages.Contains(mess_id))
                {
                    string sql_del = "INSERT INTO FavoMessages(Id_Chat,Id_User,Id_Mess) VALUES(@chat_id,@user_id,@mess_id)";
                    db.Execute(sql_del, new { chat_id, mess_id, user_id });
                }
            }
        }
        public List<FavoMess> GetMessFavo(int chat_id, int user_id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string select_int = "SELECT * FROM FavoMessages WHERE Id_Chat=@chat_id AND Id_User = @user_id";
                return db.Query<FavoMess>(select_int, new { chat_id, user_id }).ToList();
            }
        }
        public void DelFromFavMess(int chat_id, int mess_id, int user_id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql_del = "DELETE FROM FavoMessages WHERE Id_Chat =@chat_id AND Id_User=@user_id AND Id_Mess =@mess_id";
                db.Execute(sql_del, new { chat_id, mess_id, user_id });
            }
        }
        public void DelMessFromChat(int chat_id,int mess_id, int user_id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                Chat check_chat = GetChat(chat_id);
                if(check_chat.Id_User1 == user_id || check_chat.Id_User2 == user_id)
                {
                    string sql_del = "DELETE FROM Messages WHERE Id_chat =@chat_id AND Id_Mess =@mess_id";
                    db.Execute(sql_del, new { chat_id, mess_id });
                }
            }
        }
        public Message GetLastMessage(int chat_id)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string select_int = "SELECT TOP(1) * FROM Messages WHERE Id_chat=@chat_id ORDER BY date_create DESC";
                return db.Query<Message>(select_int, new { chat_id }).FirstOrDefault();
            }
        }
        public async Task AddAttachToMess(int chat_id, int id_mess,string path)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string sql_del = "INSERT INTO Chat_Attachments(Id_Chat,Id_Mess,Attachment) VALUES(@chat_id,@id_mess,@path)";
                await db.ExecuteAsync(sql_del, new { chat_id, id_mess, path });
            }
        }
        public List<Attachment> GetMessAttach(int chat_id, int id_mess)
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string select_int = "SELECT Id_Att,Id_Chat,Id_Mess,Attachment as Attach FROM Chat_Attachments WHERE Id_chat=@chat_id AND Id_Mess=@id_mess";
                return db.Query<Attachment>(select_int, new { chat_id, id_mess }).ToList();
            }
        }
        public List<Tag> GetAllTags()
        {
            using (IDbConnection db = new SqlConnection(connectionString))
            {
                string select_int = "SELECT * FROM Tags";
                return db.Query<Tag>(select_int).ToList();
            }
        }
    }
}
