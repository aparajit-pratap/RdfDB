using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AsimovAssignment
{
    public class Database
    {
        private readonly IMongoCollection<PersonModel> collection;

        public Database(string database, string table)
        {
            var client = new MongoClient();
            var db = client.GetDatabase(database);
            collection = db.GetCollection<PersonModel>(table);
        }

        public void CreateIndex()
        {
            var indexOptions = new CreateIndexOptions();
            var indexKeys = Builders<PersonModel>.IndexKeys.Ascending(x => x.Name);
            var indexModel = new CreateIndexModel<PersonModel>(indexKeys, indexOptions);
            collection.Indexes.CreateOne(indexModel);
        }

        public void InsertDocument(PersonModel document)
        {
            collection.InsertOne(document);
        }

        public PersonModel LoadDocumentForPerson(string name)
        {
            var filter = Builders<PersonModel>.Filter.Eq("Name", name);
            var persons = collection.Find(filter);
            return persons.Any() ? persons.First() : null;
        }

        public void UpsertDocument(string name, string predicate, string obj)
        {
            var personDoc = LoadDocumentForPerson(name);
            if (personDoc != null)
            {
                personDoc.Facts.Add(predicate, obj);

                collection.ReplaceOne(new BsonDocument("Name", name), personDoc,
                    new ReplaceOptions { IsUpsert = true });
            }
            else
            {
                var document = new PersonModel(name)
                {
                    Facts = new Dictionary<string, string> {{predicate, obj}}
                };
                collection.InsertOne(document);
            }
        }
    }
}