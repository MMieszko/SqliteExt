using System;
using System.IO;
using NUnit.Framework;
using SQLite;


namespace SqliteExtensions.Tests
{
    [TestFixture]
    public class Class1
    {

        public class Entity
        {
            [PrimaryKey, AutoIncrement]
            public long Id { get; set; }

            public string Name { get; set; }
        }

        public class FirstEntity : Entity { }

        public class SecondEntity : Entity { }


        [Test]
        public async void Test()
        {
            //arr
            var db = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "db.db"));

            await db.CreateTableAsync<FirstEntity>();
            await db.CreateTableAsync<SecondEntity>();

            var fe1 = new FirstEntity {Name = "Jack"};
            var fe2 = new FirstEntity {Name = "Marc"};
            var fe3 = new FirstEntity {Name = "Thomas"};
            var fe4 = new FirstEntity {Name = "John"};


            var se1 = new FirstEntity {Name = "Jesica"};
            var se2 = new FirstEntity {Name = "Jesica"};
            var se3 = new FirstEntity {Name = "Jesica"};

            await db.InsertAsync(fe1);
            await db.InsertAsync(fe2);
            await db.InsertAsync(fe3);
            await db.InsertAsync(fe4);
            await db.InsertAsync(se1);
            await db.InsertAsync(se2);
            await db.InsertAsync(se3);


        }
    }
}
