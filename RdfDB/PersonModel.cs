using System.Collections.Generic;
using MongoDB.Bson;

namespace AsimovAssignment
{
    public class PersonModel
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }

        public Dictionary<string, string> Facts { get; set; }

        public PersonModel(string name) { Name = name; }
    }
}