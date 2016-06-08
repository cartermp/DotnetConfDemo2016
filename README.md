# .NET Conf Demo 2016

This is the demo I was using in the F# talk.

It extends the [ASP.NET Music Store](https://github.com/aspnet/musicstore) application to include a project which sends album details to a Cassandra cluster using F#!  The F# project which does this is under `src/MusicStore.Cassandra`.

I've modified the music store project to be fixed to .NET Core RC2 packages.

## Prerequisites: Cassandra

You can communicate with Cassandra over TCP.  As such, there is no requirement to install Cassandra in some particular way.  The application needs two things:

- An IP Address
- An Open port

Beyond that, any custom configuration is up to you.

## Cassandra on Docker on the local machine

**Note: I set up Docker on OS X.  Instructions for Windows and Linux may differ.  I might add these later on.**

This is mostly taken from [Alena Hall's excellent post on getting F# talking to Cassandra on Docker](https://lenadroid.github.io/posts/cassandra-docker-fsharp.html).

I chose to have a Cassandra cluster run in a Docker VM on my local machine.  Here's how you can do that:

1. [Get Docker](https://docs.docker.com/engine/installation/)
2. Open a command line.
3. Pull Cassandra and create a new image using the latest Cassandra tag.  I've chosen `cassandra-1` as the name, but you can use whatever you like.

	```
	$ docker pull cassandra
	$ docker run --name cassandra-1	 -d cassandra
	```

4. (Optional) Log into the Cassandra instance and check the health of the node.

	```
	$ docker exec -it cassandra-1 bash
	$ nodetool status
	```
5. (Optional) Create a second node and check it health.

	```
	$ docker run --name cassandra-2	 -d cassandra
	$ docker exec -it cassandra-2 bash
	$ nodetool status
	```
	
	You can create as many nodes as you see fit.

6. Log into each Cassandra instance you created and install vim (or you favorite text editor).

	```
	$ docker exec -it cassandra-1 bash
	$ apt-get update
	$ apt-get install vim
	```
	
7. Open up the `/etc/cassandra/cassandra.yaml` config file

	```
	$ vim /etc/cassandra/cassandra.yaml
	```
	
8. The rest of this assumes you are using vim.  

	* Search for `cluster_name`.  `/cluster_name:` ---> you can name this whatever you want
	* Search for `listen_address`. `/listen_address:` ---> Just make sure you have IP addresses for each node.
	* Search for `rpc_address`.  `/rpc_address:` ---> replace `0.0.0.0` with the result of `:r ! hostname-I`
	* (Optional) search for `seeds`. `/seeds` ---> Since this is a small cluster, you can use a CSV of the IP addresses of your nodes.  Example with one node: `172.17.0.2`.
	
	Save your changes!  Note that you can open this file in new tabs in your terminal and broadcast your output to all tabs to automate the above for each node.
	
9. Cassandra will communicate over Docker Port 9042.  Set the IP Port to a number you feel is appropriate.  I chose 8881, just like in the blog post I've linked above.

[image goes here]

And that's it!

## Setting the IP Address and Port in the AnalyticsApi config file:

The `Web.config` file has the following lines:

```xml
<appSettings>
	<!-- Change this to your own IP Address and port -->
    <add key="ipaddress" value="192.168.99.100"/>
    <add key="port" value="8881"/>
</appSettings>
```

As the comment suggests, you'll have to change these values to the IP address and port which correspond to the Docker Port that Cassandra listen on.
