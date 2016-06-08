namespace MusicStore.Cassandra

open System
open Cassandra

/// A representation of the album details we're logging in the Music Store.
type AlbumDetails =
    { Id: int
      Name: string
      Cached: bool
      Time: DateTimeOffset }

/// A set of access functions to a Cassandra cluster.
type ICassandraRepo =
    /// Sends the given album details to Cassandra.
    /// Returns a Cassandra RowSet corresponding to the items sent to the cluster.
    abstract member Send: AlbumDetails -> RowSet
    
    /// Naiively gets all items in the cassandra table defined by the repo.
    /// Basically, this is a 'select * from <table>' query, but the table isn't a parameter.
    /// I should fix that.
    abstract member GetAll: unit -> RowSet

/// A repository which allows you to interact with a Cassandra Cluster.
type CassandraRepo(ipaddress:string, port:int) =
    let cluster = Cluster.Builder()
                         .AddContactPoint(ipaddress)
                         .WithPort(port).Build()
    let session = cluster.Connect()

    do // First-time setup.
        session.Execute(@"
            create keyspace if not exists demo
            with replication = {'class': 'SimpleStrategy', 'replication_factor': 1};"
        ) |> ignore

        session.Execute("use demo") |> ignore

        session.Execute(@"
            CREATE TABLE IF NOT EXISTS demo.album_details (
                id int,
                name text,
                cached boolean,
                time_accessed timestamp,
                PRIMARY KEY(id, time_accessed)
            ) WITH CLUSTERING ORDER BY (time_accessed desc);
        ") |> ignore

    interface ICassandraRepo with
        member this.Send details =
            let cql = 
                String.Format("insert into demo.album_details (id, name, cached, time_accessed) values ({0}, '{1}', {2}, {3});",
                                details.Id, 
                                details.Name, 
                                details.Cached, 
                                details.Time.ToUnixTimeMilliseconds()) // Cassandra timestamps are 64-bit ints representing ms since epoch.
            
            session.Execute cql

        member this.GetAll() =
            session.Execute("select * from demo.album_details")