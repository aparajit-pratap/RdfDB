using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

// Parse RDF file
// Store S, P, O data into data structure
// Insert documents into MongoDB
// Index
namespace AsimovAssignment
{
    class Program
    {
        private static readonly string uriPattern = @"(https?:\/\/[^\s>]+)";
        private static readonly string spPattern = @"<(https?:\/\/[^\s]+)>";
        private static readonly string whitespace = @"[\s]*";
        private static readonly string literalPattern = "\"(.+)\"";
        private static readonly string literalTriple = spPattern + whitespace + spPattern + whitespace + literalPattern + @"(.*)";
        private static readonly string pathPattern = @"(http[s]?:\/\/)([^\/\s(]+\/)+([^\s]*)";

        private static string ExtractPath(string uri)
        {
            var match = Regex.Match(uri, pathPattern);

            if(match.Success)
                return match.Groups[3].Value;

            return "";
        }

        private static void InsertDocument(Database db, ref PersonModel p, string name, string predicate, string obj)
        {
            if (p == null || name != p.Name)
            {
                if (p != null)
                {
                    // new name has been read
                    db.InsertDocument(p);
                }

                p = new PersonModel(name)
                {
                    Facts = new Dictionary<string, string> { { predicate, obj } }
                };
            }
            else
            {
                p.Facts.Add(predicate, obj);
            }
        }

        private static Tuple<string, string, string> ParseTriple(string line)
        {
            string name = "", predicate = "", obj = "";
            var triple = Regex.Match(line, literalTriple);
            if (triple.Success)
            {
                name = ExtractPath(triple.Groups[1].Value);
                predicate = ExtractPath(triple.Groups[2].Value);

                var literal = triple.Groups[3];
                obj = literal.Value;

                //InsertDocument(db, ref p, name, predicate, obj);
            }
            else
            {
                var uris = Regex.Matches(line, uriPattern);

                if (uris.Count == 4)
                {
                    name = ExtractPath(uris[0].Value);
                    predicate = ExtractPath(uris[1].Value);
                    obj = ExtractPath(uris[2].Value);

                    //InsertDocument(db, ref p, name, predicate, obj);
                }
            }
            if(!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(predicate) && !string.IsNullOrEmpty(obj))
                return new Tuple<string, string, string>(name, predicate, obj);
            
            return null;
        }

        private static void ParseRDF(Database db, string filePath)
        {
            using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {
                string line;
                PersonModel p = null;

                while ((line = sr.ReadLine()) != null)
                {
                    var tuple = ParseTriple(line);
                    if(tuple != null)
                        InsertDocument(db, ref p, tuple.Item1, tuple.Item2, tuple.Item3);
                }
                // insert last read person record
                db.InsertDocument(p);
            }
        }

        static void Main(string[] args)
        {
            var db = new Database("RdfPersonDb", "Persons");

            if (args[0] == "-p")
            {
                var filepath = args[1];
                if (File.Exists(filepath))
                {
                    db.CreateIndex();
                    ParseRDF(db, filepath);
                }
            }
            else if (args[0] == "-l")
            {
                var personName = args[1];
                var person = db.LoadDocumentForPerson(personName);
                Console.WriteLine(person.Name);
                foreach (var (key, value) in person.Facts)
                {
                    Console.WriteLine(key + " " + value);
                }
            }
            else if (args[0] == "-i")
            {
                var line = args[1];
                var tuple = ParseTriple(line);
                if (tuple != null)
                {
                    db.UpsertDocument(tuple.Item1, tuple.Item2, tuple.Item3);
                }
            }

        }
    }
}
