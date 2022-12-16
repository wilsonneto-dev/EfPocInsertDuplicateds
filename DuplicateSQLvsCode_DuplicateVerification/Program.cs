using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

// Console.WriteLine("Hello, World!");

using var db = new AppContext();

var sw = new Stopwatch();

var conflictHash1 = Convert.ToHexString(
            MD5.HashData(
                Encoding.ASCII.GetBytes("111111111111111111111")));

var conflictHash2 = Convert.ToHexString(
            MD5.HashData(
                Encoding.ASCII.GetBytes("22222222222222222222")));

var conflictHashWarm = Convert.ToHexString(
            MD5.HashData(
                Encoding.ASCII.GetBytes("33333333333333333333333")));
//sw.Start();

try
{
    db.Articles.Add(new Article()
    {
        Texto= "bla bla",
        Hash = conflictHashWarm
    });
    db.SaveChanges();
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

//sw.Stop();

Console.WriteLine(" ------------  without find  -------------- ");

var sw2 = new Stopwatch();
sw2.Start();

try
{
    db.Articles.Add(new Article()
    {
        Texto= "bla bla",
        Hash = conflictHash2
    });
    db.SaveChanges();
    Console.WriteLine("** inserted direct");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

sw2.Stop();


sw.Start();

try
{
    var article = db.Articles.FirstOrDefault(x => x.Hash == conflictHash1);

    if (article is null)
    {
        db.Articles.Add(new Article()
        {
            Texto= "bla bla",
            Hash = conflictHash1
        });
        db.SaveChanges();
        Console.WriteLine("** inserted");
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

sw.Stop();

Console.WriteLine($"with find before time: {sw.ElapsedMilliseconds} ms");
Console.WriteLine($"without find: {sw2.ElapsedMilliseconds} ms");

//Enumerable.Range(0, 10_000).ToList().ForEach(_ => {
//    Console.WriteLine("inserting...");
//    db.Articles.Add(new Article()
//    {
//        Texto= Guid.NewGuid().ToString(),
//        Hash = Convert.ToHexString(
//            MD5.HashData(
//                Encoding.ASCII.GetBytes(Guid.NewGuid().ToString())))
//    });
//});

public class Article
{
    public string? Hash { get; set; }
    public string? Texto { get; set; }
}

public class AppContext : DbContext
{
    public DbSet<Article> Articles { get; set; }

    public AppContext() : base()
    {
        // Database.EnsureDeleted();
        // Database.EnsureCreated();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information)
            .EnableSensitiveDataLogging()
            .UseSqlServer(@"Server=localhost;Database=company02;User Id=sa;Password=Str0ngP455W0RD;trustServerCertificate=true;");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Article>().HasKey(x => x.Hash);
    }
}

