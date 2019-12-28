# RdfDB
MongoDB based storage Service to Build and Store RDF Dataset

The service to store the RDF format dataset (e.g. DBPedia Person data) has been implemented using MongoDB. It uses the MongoDB .NET driver to communicate with MongoDB. The service includes the following:

1. Script to bulk index a local RDF file - this is achieved by the "CreateIndex" and "ParseRDF" methods on the “Program” class.
2. Interface to index new RDF triples - this is done by the "UpsertDocument" method on the "Database" class.
3. Interface to retrieve all metadata about a Person - this can be queried using the "LoadDocumentForPerson" method on the "Database" class.

Building the service:

The solution is a .NET Core Console App. In order to setup and build the project, you will need to install the following dependencies:
1.	Download MongoDB Community Server (4.2.2)
2.	Install MongoDB.Driver Nuget Package (2.10.0)

When installing the MongoDB Community Server, make sure to also install the MongoDB Compass Community App as it will be easy to verify the contents of the database in the App, reset it, add new collections (tables), etc. through the App.

Open the “.sln” file in Visual Studio (might also work in VS Code). The “Database” class is a wrapper on top of the MongoDB .NET Driver database type. 

Running the service:

The console application can be invoked using different command line argument options. See “Main” entry point method on the “Program” class:

“-p” to index the database, parse local RDF triple file, and store all records in the DB. This command line option must be followed by the file path pointing to the triple file:

RdfDB.exe “-p” “C:\Users\pratapa.ADS\source\repos\RdfDB\persondata_en.tql”

“-l” to query from the DB all metadata for a person given his/her name. This option must be followed by the person name to be searched for in the database. Note the person names are entered as they exist in the URI path in the triples file. For example, the name searched for in the URI entry: http://dbpedia.org/resource/Karel_Matěj_Čapek-Chod must be specified as “Karel_Matěj_Čapek-Chod”:

RdfDB.exe “-l” “Karel_Matěj_Čapek-Chod”

“-i” to insert a new triple into the database. This creates a new record (MongoDB document) if the person doesn’t already exist and appends subject-predicate-object triple to an existing Person record if the name for that person already exists in the DB. This command line option must be following by the triple string as it is specified in the DBPedia dataset file. For example, this command will look like: 

RdfDB.exe “-i” “<http://dbpedia.org/resource/Karel_Matěj_Čapek-Chod> <http://dbpedia.org/ontology/birthPlace> <http://dbpedia.org/resource/Domažlice> <http://www.wikidata.org/wiki/Q1000005?oldid=357660876> .”

The schema for the “Person” record is represented by the “PersonModel” class. This contains the “Name” property for the person and a dictionary of one or more predicate vs. object strings for that person object. See “PersonModel.cs.”


Performance and Assumptions:

This solution takes a few minutes to run to populate all the 1.4+ million person records from the DBPedia RDF file. The 1.4+M records were verified using the MongoDB Compass client app:
 

The logic used makes an assumption that all facts for a single person are grouped together in the file; i.e. the birthplace fact of “Joe Smith”, for example, exists alongside all other predicate-object pairs for “Joe Smith” and not arbitrarily placed in the file. This assumption makes the insertion of all person records into the database faster, as they are first all read for a single person into a “PersonModel” object in memory and finally inserted into the DB. If, on the other hand, the facts were arbitrarily spaced, it would have necessitated searching for an existing person record (with the name) in the database and then appending the fact to that record, which would have a major performance impact (even with indexing, given the volume of records) while populating the DB.

The database has been indexed for the “Name” property on “PersonModel”, which makes it performant to query all metadata for a given person object by name.

One of the limitations with this solution is that more complex semantic queries like “find for all persons born in Zurich, who are painters”, for example, would be quite complex and non-trivial to create for the non-relational DB. Furthermore, even if such queries were created, they would be very expensive. A graph DB would be a lot more suitable and performant for such complex semantic queries.

