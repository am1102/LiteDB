# LiteDB - A .NET NoSQL Document Store in a single data file

Same LiteDB features:

- Serverless NoSQL Document Store
- Simple API similar to MongoDB
- 100% C# code for .NET 3.5 in a single DLL - install from NuGet: `Install-Package LiteDB`
- Transaction control - ACID
- Recovery in writing failure (journal mode)
- Use with POCO class or BsonDocument
- Store files and `Stream` too (like GridFS in MongoDB)
- Single data file storage (like SQLite)
- Up to 8 indexes per collection
- Open source and free for everyone - including commercial use

## How install

LiteDB is a serverless database, so there is not install. Just copy LiteDB.dll to your Bin folder and add as Reference.
If you prefer, you can use NuGet package: `Install-Package LiteDB`

## How to use

In LiteDB, document are storage in collections. Each collection have many documents with same type. Each document have an `Id`
(like 'Primary Key' on relation databases). Id datatype can support any indexed datatype (see below)

```C#
// Open data file (or create if not exits)
using(var db = new LiteEngine(@"C:\Temp\MyDataFile.db"))
{
    // Get a collection (or create, if not exits)
    var customers = db.GetCollection<Customer>("customers");
    
    // Insert new document, using 'int' ID 
    var cust = customers.Insert(new Customer { Id = 1, Name = "John Doe" });
    
    // Update a document inside a collection
    cust.Name = "Joana Doe";
    
    customers.Update(cust);
    
    // Delete a document
    customers.Delete(cust.Id);
}
```
## Documents

Falar sobre os 2 tipos: Poco/BsonDocument.

### POCO Documents

Falar sobre como identificar o PK

### BsonDocument

Falar sobre o bson

## Indexes
falar de como indexa. Skiplist, index por colecao, só prop diretas, que tipo de dados sao aceitos. PK = _id_

## Query

In LiteDB, queries use indexes to search documents. At this moment, LiteDB do not support Linq, only `Query` helper class to create indexed query results. But, the result it´s a `IEnumerable<T>`, so you can Linq after query execute.

```C#
var customers = db.GetCollection<Customer>("customers");

// Create a new index (if not exists)
customers.EnsureIndex("Name");

// Query documents using 'Name' index
var results = customers.Find(Query.StartsWith("Name", "John"));

// Return document by ID (PK index)
var customer = customers.FindById(1);

// Count only documents where ID >= 2
var count = customers.Count(Query.GTE("_id", 2));

// All query results returns an IEnumerable<T>, so you can use Linq after
var linq = customers.Find(Query.Between("Salary", 500, 1000)) 
    .Where(x => x.LastName.Length > 5 && x.Age > 22)
    .Select(x => new { x.Name, x.Salary })
    .OrderBy(x => x.Name);
```

`Query` class supports `All`, `Equals`, `Not`, `GreaterThan`, `LessThan`, `Between`, `In`, `StartsWtih`, `AND` and `OR`.
All operations need an index to be executed.

##Transactions

All write operations are created inside a transaction. If you do not use `BeginTrans` and `Commit`, transaction are implicit
for each operation.

For simplicity, LiteDB do not support concurrency transactions. LiteDB locks your datafile to guarantee that 2 users are
not changing data at same time.

If there is any error during write data file, journaling save a redo log file with database dirty pages, to recovery your
datafile when datafile open again. 

```C#
using(var db = new LiteEngine(dbpath))
{
    db.BeginTrans();
    
    // do many write operations (insert, updates, deletes)...
    
    if(...)
    {
        db.Rollback(); // Discard all dirty pages - no data file changes
    }
    else
    {
        db.Commit(); // Persist dirty pages to disk (use journal redo log file)
    }
}
```

## BsonDocument

You can use POCO class (as showed before) or `BsonDocument` to store scheme less documents. Document size limit
is 256Kb for each document.

```C#
using(var db = new LiteEngine(connectionString))
{
    // Create a BsonDocument and populate
    var doc = new BsonDocument();
    doc["Name"] = "John Doe";
    doc["Phones"] = new BsonArray();
    doc["Phones"].Add("55(51)9900-0000");
    
    // Get the collection
    var col = db.GetCollection("customers");
    
    col.Insert(1, doc);
    
    Debug.Print("Nome: " + doc["Name"].AsString);
    Debug.Print("Phone:" + doc["Phones"][0].AsString);
}
```

## Storing Files

Sametimes we need store files in database. For this, LiteDB has a special `Files` collection to store files without
document size limit (file limit is 2Gb per file). It's works like MongoDB `GridFS`.

```C#
// Storing a file stream inside database with NameValueCollection metadata related
db.Files.Upload("my_key.png", stream, metadata);

// Get file reference using key
var file = db.Files.FindByKey("my_key.png");

// Find all files using StartsWith
var files = db.Files.Find("my_");

// Get file stream
var stream = file.OpenRead(db);

// Write file stream in a external stream
db.Files.Download("my_key.png", stream);

```

## Connection String

Connection string options to initialize LiteEngine class:

- **Filename**: Path for datafile. You can use only path as connection string (required)
- **Timeout**: timeout for wait for unlock datafile (default: 00:01:00)
- **Journal**: Enabled journal mode - recovery support (default: true)
- **MaxFileLength**: Max datafile length, in bytes (default: 4TB)

## Where to use?

- Desktop/local applications
- Small web applications
- One database **per account/user** data store
- Few concurrency write users operations

## Dependency

LiteDB has no external dependency, but use [fastBinaryJson](http://fastbinaryjson.codeplex.com/) as Bson converter 
from/to .NET objects. All source are included inside LiteDB source.

## Roadmap

Currently, LiteDB is in early development version. There are many tests to be done before ready for production.
Please, be careful on use.

Same features/ideas for future

- More tests!!
- A repository pattern
- Linq support OR string query engine
- Compound index: one index for multiple fields
- Multikey index: index for array values
- Full text search
- Simple admin GUI program