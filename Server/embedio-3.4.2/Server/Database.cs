using HttpMultipartParser;
using LiteDB;
using Newtonsoft.Json;
using Server;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EmbedIO.Samples
{
    public class Database
    {
        private string DATA_PATH = @"P:\UnityProjects\OctoberNights\Data";
        private string DATABASE_PATH;
        private string RESOURCES_PATH;
        private string TEXTURES_PATH;

        public static Database Instance;
        private LiteDatabase Db;
        private ILiteStorage<string> Fs;

        public Database()
        {
            Instance = this;
            InitializeEnviroment();
            InitializeDB();
            //DropDatabase();
            //FillDbTest();
            //TestDb();
        }

        private void InitializeEnviroment()
        {
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.Parent.Parent.Parent.Parent.FullName;
            DATA_PATH = $@"{projectDirectory}\Data";
            DATABASE_PATH = $@"{DATA_PATH}\OctoberNightDatabase.db";
            RESOURCES_PATH = $@"{DATA_PATH}\Resources";
            TEXTURES_PATH = $@"{RESOURCES_PATH}\Textures";

            if (!Directory.Exists(DATA_PATH))
            {
                Directory.CreateDirectory(DATA_PATH);
            }
            if (!Directory.Exists(RESOURCES_PATH))
            {
                Directory.CreateDirectory(RESOURCES_PATH);
            }
            if (!Directory.Exists(TEXTURES_PATH))
            {
                Directory.CreateDirectory(TEXTURES_PATH);
            }
        }

        private void TestDb()
        {
            /*
            var points = Db.GetCollection<LocationModel>("Points");

            var point = points
                .Include(x => x.Owner)
                .Find(p=>p.LocationName == "New location").FirstOrDefault();

            if (point!=null)
            {
                point.LocationName.Info();
                point.SpriteName.Info();
                string filePath = $@"{RESOURCES_PATH}\Test\unnamed.png";
                string newFilePath = $@"{RESOURCES_PATH}\Test\unnamed.png";
               // File.Move(filePath, newFilePath);
                   
                using (var stream = File.OpenRead(filePath))
                {

                    //var image = System.Drawing.Image.FromStream(stream, false); // exception is thrown
                    AddOrUpdateTexture(stream, point.SpriteName, TextureType.Point);

                    FileStream readStream = File.OpenWrite($@"{RESOURCES_PATH}\Test\unnamed2.png");
                    GetTexture(point.SpriteName, TextureType.Point, readStream);
                    readStream.Close();
                    //image.Dispose();
                }
            }*/
/*
            foreach (LocationModel loc in GetPoints())
            {
                foreach (EventModel em in loc.Events)
                {
                    em.EventText.Info();
                    JsonConvert.SerializeObject(em).Info();
                }
            }
            
            foreach (EventModel em in GetEvents())
            {
                em.EventText.Info();
                JsonConvert.SerializeObject(em).Info();
            }*/

            string img = "LocationImg_9a29a78b-9db0-4588-8a6a-77a3ffebd52d.png";
            
            TextureType textureType = TextureType.Event;
            
            if (img.StartsWith("LocationImg"))
            {
                textureType = TextureType.Point;
            }
            else if (img.StartsWith("EventImg"))
            {
                textureType = TextureType.Event;
            }

            var file = Fs.FindById($@"{TEXTURES_PATH}\{textureType}\{img}");
            var fs = file.OpenRead();
            
            $@"{fs.ReadByte()}".Info();
        }

        private void GetTexture(string spriteName, TextureType textureType, Stream stream)
        {
            string path = $@"{TEXTURES_PATH}\{textureType}\{spriteName}";
            Fs.Download(path, stream);
        }

        private void FillDbTest()
        {
            Random r = new Random();

            User user = new User()
            {
                Name = "User1",
                EmailAddress = "mail@mail.ru",
                Password = "124",
                Token = "hibhu-jhvgv-jhvbuy"
            };

            AddUser(user);

            LocationModel location = new LocationModel()
            {
                Id = Guid.NewGuid(),
                LocationName = "New location",
                Position = new Position2d(11.2234, 22.42424),
                Owner = user,
            };

            for (int i = 0; i < 3; i++)
            {
                EventModel eventModel = new EventModel()
                {
                    Id = Guid.NewGuid(),
                    EventText = "Event_" + i
                };

                eventModel.Variants = new List<VariantModel>();

                for (int j = 0; j < 3; j++)
                {
                    VariantModel variant = new VariantModel()
                    {
                        Id = Guid.NewGuid(),
                        //VariantText = $"Variant_{j}",
                        //PaymentIds = new List<int>() { r.Next(0, 10), r.Next(0, 10), r.Next(0, 10) }
                    };
              
                    eventModel.Variants.Add(variant);

                    //AddVariant(variant);
                }

                location.Events.Add(eventModel);

                //AddEvent(eventModel);
            }

            AddOrUpdatePoint(location);


           
        }

       

        private void InitializeDB()
        {
            Db = new LiteDatabase(DATABASE_PATH);
            Fs = Db.GetStorage<string>("Textures", "Chunks");
            var colUsers = Db.GetCollection<User>("Users");
            var colPoints = Db.GetCollection<LocationModel>("Points");
            var colEvents = Db.GetCollection<EventModel>("Events");
            var colVariants = Db.GetCollection<VariantModel>("Variants");
        }

        private void DropDatabase()
        {
            //Db.DropCollection("Users");
            Db.DropCollection("Points");
            Db.DropCollection("Events");
            Db.DropCollection("Variants");
            Db.DropCollection("Textures");
            Db.DropCollection("Chunks");
        }

        public IEnumerable<LocationModel> GetPoints()
        {
            return Db.GetCollection<LocationModel>("Points").Include(x=>x.Events).Include("$.Events[*].Variants").Find(p => true);
        }

        public IEnumerable<User> GetUsers()
        {
            return Db.GetCollection<User>("Users").Find(p => true);
        }

        public IEnumerable<EventModel> GetEvents()
        {
            return Db.GetCollection<EventModel>("Events").Include(x=>x.Variants).Find(p => true);
        }

        public IEnumerable<VariantModel> GetVariants()
        {
            return Db.GetCollection<VariantModel>("Variants").Find(p => true);
        }

        public void AddUser(User user)
        {
            ILiteCollection<User> col = Db.GetCollection<User>("Users");
            JsonConvert.SerializeObject(user).Info(nameof(Database));

            col.Insert(user);
            col.Update(user);
        }

        public void UpdateUser(User user)
        {
            ILiteCollection<User> col = Db.GetCollection<User>("Users");
            col.Update(user);
        }

       
        public void AddOrUpdatePoint(LocationModel point)
        {
            LocationModel existingPoint = Db.GetCollection<LocationModel>("Points").FindOne(p => p.Position == point.Position && p.Id == point.Id);


            if (existingPoint == null)
            {
                AddPoint(point);
            }
            else
            {
                UpdatePoint(point);
            }
        }

        public void AddPoint(LocationModel point)
        {
            ILiteCollection<LocationModel> col = Db.GetCollection<LocationModel>("Points");
            JsonConvert.SerializeObject(point).Info(nameof(Database));

            foreach (EventModel ev in point.Events)
            {
                AddEvent(ev);
            }

            col.Insert(point);
            col.Update(point);
        }
        public void UpdatePoint(LocationModel point)
        {
            ILiteCollection<LocationModel> col = Db.GetCollection<LocationModel>("Points");
            ILiteCollection<EventModel> evCol = Db.GetCollection<EventModel>("Events");
            ILiteCollection<VariantModel> vaCol = Db.GetCollection<VariantModel>("Variants");

            LocationModel oldPoint = col.FindOne(p=>p.Id == point.Id);

            foreach (EventModel ev in oldPoint.Events)
            {
                foreach (VariantModel vm in ev.Variants)
                {
                   vaCol.Delete(vm.Id);
                }
                evCol.Delete(ev.Id);
            }


            col.Update(point);
        }

        public void AddorUpdateEvent(EventModel eventModel)
        {
            EventModel existingEvent = Db.GetCollection<EventModel>("Events").FindOne(p => p.Id == eventModel.Id);


            if (existingEvent == null)
            {
                AddEvent(eventModel);
            }
            else
            {
                UpdateEvent(eventModel);
            }
        }

        public void AddEvent(EventModel eventModel)
        {
            ILiteCollection<EventModel> col = Db.GetCollection<EventModel>("Events");
            JsonConvert.SerializeObject(eventModel).Info(nameof(Database));

            foreach (VariantModel vm in eventModel.Variants)
            {
                AddOrUpdateVariant(vm);
            }

            col.Insert(eventModel);
            col.Update(eventModel);
        }

      

        public void UpdateEvent(EventModel eventModel)
        {
            ILiteCollection<EventModel> col = Db.GetCollection<EventModel>("Events");
            col.Update(eventModel);
        }

        private void AddOrUpdateVariant(VariantModel vm)
        {
            VariantModel existingVariant = Db.GetCollection<VariantModel>("Variants").FindOne(p => p.Id == vm.Id);


            if (existingVariant == null)
            {
                AddVariant(vm);
            }
            else
            {
                UpdateVariant(vm);
            }
        }

        public void AddVariant(VariantModel variant)
        {
            ILiteCollection<VariantModel> col = Db.GetCollection<VariantModel>("Variants");
            JsonConvert.SerializeObject(variant).Info(nameof(Database));

            col.Insert(variant);
            col.Update(variant);
        }

        public void AddOrUpdateTexture(FilePart file)
        {
            string path = file.FileName;
            path.Info();
            

            if (path.StartsWith("LocationImg"))
            {
                AddOrUpdateTexture(file.Data, file.FileName,TextureType.Point);
            }
            else if (path.StartsWith("EventImg"))
            {
                AddOrUpdateTexture(file.Data, file.FileName, TextureType.Event);
            }
        }

        private void AddOrUpdateTexture(Stream stream, string name, TextureType textureType)
        {
            string path = $@"{TEXTURES_PATH}\{textureType}";
            path.Info();
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            $@"{TEXTURES_PATH}\{textureType}\{name}".Info();

            Fs.Upload($@"{TEXTURES_PATH}\{textureType}\{name}", $@"{TEXTURES_PATH}\{textureType}\{name}", stream);
        }

        public Stream GetTexture(string fileName)
        {
            TextureType textureType = TextureType.Event;
            
            if (fileName.StartsWith("LocationImg"))
            {
                textureType = TextureType.Point;
            }
            else if (fileName.StartsWith("EventImg"))
            {
                textureType = TextureType.Event;
            }

            var file = Fs.FindById($@"{TEXTURES_PATH}\{textureType}\{fileName}");
            if (file == null)
            {
                return null;
            }
            return file.OpenRead();
        }
        
        public void UpdateVariant(VariantModel variant)
        {
            ILiteCollection<VariantModel> col = Db.GetCollection<VariantModel>("Variants");
            col.Update(variant);
        }

        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
        }
    }
}