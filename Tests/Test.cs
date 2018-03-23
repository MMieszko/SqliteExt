using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using SqliteExtensions;
using SQLite;


namespace Tests
{
    public static class TestClass
    {
        static SQLiteAsyncConnection database;

        private static async Task InitAsync()
        {
            database = new SQLiteAsyncConnection(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "db.db"));


            await database.DropTableAsync<FirstEntity>();
            await database.DropTableAsync<SecondEntity>();

            await database.CreateTableAsync<FirstEntity>();
            await database.CreateTableAsync<SecondEntity>();

            var fe1 = new FirstEntity { Name = "Jack" };
            var fe2 = new FirstEntity { Name = "Marc" };
            var fe3 = new FirstEntity { Name = "Thomas" };
            var fe4 = new FirstEntity { Name = "John" };


            var se1 = new SecondEntity { Name = "Jesica" };
            var se2 = new SecondEntity { Name = "Tom" };
            var se3 = new SecondEntity { Name = "Jerru" };

            await database.InsertAsync(fe1);
            await database.InsertAsync(fe2);
            await database.InsertAsync(fe3);
            await database.InsertAsync(fe4);
            await database.InsertAsync(se1);
            await database.InsertAsync(se2);
            await database.InsertAsync(se3);

        }


        public static async Task Test()
        {
            
        }

        public static async Task Join()
        {
            await InitAsync();


            var res = await database.MieszkoQuery<FirstEntity>().Select()
                .Marching<SecondEntity, long, long>(x => x.Id, x => x.Id, (first, second) => first == second)
                .ToListAsync();
        }

        public static async Task WhereTests()
        {
            await InitAsync();


            

            var enumerableJack = await database.MieszkoQuery<FirstEntity>().Select().Where(x => x.Name == "Jack").ToListAsync();
            var onlyJack = await database.MieszkoQuery<FirstEntity>().Select().Where(x => x.Name == "Jack").FirstOrDefaultAsync();
            var nullResult = await database.MieszkoQuery<FirstEntity>().Select().Where(x => x.Name == "Katarzyna").FirstOrDefaultAsync();
            var jackAndJesica = await database.MieszkoQuery<FirstEntity>().Select().WhereIn(x => x.Id, 1,2,3).ToListAsync();

            var jackAndJesicaOrder = await database.MieszkoQuery<FirstEntity>().Select().WhereIn(x => x.Id, 1,2,3).OrderBy(x => x.Id).ToListAsync();
            var jackAndJesicaOrderDesc = await database.MieszkoQuery<FirstEntity>().Select().WhereIn(x => x.Id, 1,2,3).OrderByDescending(x => x.Id).ToListAsync();
            var jackAndJesicaOrderName = await database.MieszkoQuery<FirstEntity>().Select().WhereIn(x => x.Id, 1,2,3).OrderByDescending(x => x.Name).ToListAsync();



            var jackAndJesica1 = await database.MieszkoQuery<FirstEntity>().Select().WhereIn(x => x.Id, 1,2,3).OrderByDescending(x => x.Name).FirstOrDefaultAsync();

            var just5 = await database.MieszkoQuery<FirstEntity>().Select().TakeAsync(5);

        }



    }


    public class Entity
    {
        [PrimaryKey, AutoIncrement]
        public long Id { get; set; }

        public string Name { get; set; }
    }

    public class FirstEntity : Entity
    {
        
    }

    public class SecondEntity : Entity
    {
 
    }
}

