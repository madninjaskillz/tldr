using System.Security;
using System.Security.Cryptography;
using System.Text;
using LiteDB;
using tldr.Models;

namespace tldr
{
    public class DBService
    {
        private ILiteCollection<Book> Books;
        private ILiteCollection<User> Users;
        private ILiteDatabase database;
        private string dbPath = "/tldr/tldr.db";
        public DBService(params string[] args)
        {
            if (args.Length > 0)
            {
                if (args.Any(arg => arg.StartsWith("dbpath=")))
                {
                    var dbarg = args.First(x => x.StartsWith("dbpath="));
                    dbarg = dbarg.Substring(7);
                    dbPath = dbarg;
                }

                if (Environment.GetEnvironmentVariables().Contains("dbpath"))
                {
                    dbPath = Environment.GetEnvironmentVariable("dbpath");
                }
            }
            Console.WriteLine("Getting db at "+dbPath);
            database = new LiteDatabase(dbPath);
            Books = database.GetCollection<Book>("books");
            Users = database.GetCollection<User>("users");
        }

        public User CreateUser(string username, string password)
        {
            var users = Users.Find(x => x.Username.ToLower() == username.ToLower());
            var user = users.FirstOrDefault();
            if (user != null)
            {
                throw new SecurityException("User already exists");
            }

            User newUser = new User();
            newUser.Username = username;
            newUser.EncryptedPassword = Encrypt(password, newUser.PrivateKey);
            Users.Insert(newUser);
            return newUser;
        }
        public User GetUser(string username, string password)
        {
            var users = Users.Find(x => x.Username.ToLower() == username.ToLower());
            var user = users.FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            if (Encrypt(password, user.PrivateKey) == user.EncryptedPassword)
            {
                return user;
            }

            throw new SecurityException("Incorrect Password");
        }

        public Book AddReview(Book book, Review review)
        {
            Book existing = Books.FindOne(x => x.Id == book.Id);
            if (existing != null)
            {
                existing.Review.Add(review);

                Books.Update(existing);
                return existing;
            }

            book.Review.Add(review);
            AddBook(book);
            return book;
        }

        public void AddBook(Book book)
        {
            Books.Insert(book);
        }
        public List<Book> GetBooks()
        {
            return Books.Find(x => x.Review.Count > 0).OrderByDescending(p=>p.Review.Count).Take(10).ToList();
        }

        public List<Book> GetBooks(string search)
        {
            return Books.Find(x =>
                x.Title.Contains(search) || x.Authors.Any(a => a.Contains(search)) ||
                x.ASIN.ToLower().Contains(search.ToLower())).ToList();
        }

        private string Encrypt(string toEncrypt, string myKey)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);


            string key = myKey;

            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hashmd5.Clear();


            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        private string Decrypt(string cipherString, string myKey)
        {
            byte[] keyArray;
            byte[] toEncryptArray = Convert.FromBase64String(cipherString);



            string key = myKey;


            MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
            keyArray = hashmd5.ComputeHash(UTF8Encoding.UTF8.GetBytes(key));
            hashmd5.Clear();


            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.PKCS7;

            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            tdes.Clear();
            return UTF8Encoding.UTF8.GetString(resultArray);
        }

        public int GetVotes(Guid bookId, Guid reviewId)
        {
            Book existing = Books.FindOne(x => x.Id == bookId);
            if (existing != null)
            {
                Review? existingReview = existing.Review.FirstOrDefault(x => x.Id == reviewId);
                return (int)((existingReview.Votes.Sum(x => (int) x.Vote) / (float)existingReview.Votes.Count())*10);
            }

            return 0;

        }

            public void AddVote(Guid bookId, Guid reviewId, Vote vote, User user)
        {
            Book existing = Books.FindOne(x => x.Id == bookId);
            if (existing != null)
            {
                var existingReview = existing.Review.FirstOrDefault(x => x.Id == reviewId);
                if (existingReview != null)
                {
                    var existingVote = existingReview.Votes.FirstOrDefault(p => p.UserId == user.Id);
                    if (existingVote != null)
                    {
                        existingReview.Votes.Remove(existingVote);
                    }

                    existingReview.Votes.Add(new ReviewVote
                    {
                        Id = Guid.NewGuid(),
                        ReviewId = reviewId,
                        UserId = user.Id,
                        Vote = vote
                    });

                    Books.Update(existing);

                }
            }
        }
    }
}
