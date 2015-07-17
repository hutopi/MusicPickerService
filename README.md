Deprecation notice
==================
Musicpicker has been rewritten in Node.js, with many feature additions and performance improvements.
This repository is the original C#/.NET web backend and is no longer supported.

Please check out the project's Node rewrite on [GitHub](https://github.com/musicpicker/musicpicker/) and
the managed cloud service http://musicpicker.net.

# MusicPickerService

Web service that implements APIs, data storage and SignalR hub for the MusicPicker project.

MusicPicker is a music playback system that unifies your music metadata into a central cloud service.

The web service offers a single place for someone to manage all his MusicPicker-enabled devices, and control what
music plays on each.

Features
===========
MusicPickerService is the central piece of MusicPicker's ecosystem.

- User login and registration
- Device management and registration
- Metadata database
- Device collection submission
- Device playback control
- Artwork fetching
- Web UI

Managed cloud version is available at [musicpicker.cloudapp.net](http://musicpicker.cloudapp.net)

Dependencies
===========
MusicPickerService is an ASP.NET MVC 5 project built in C# on .NET Framework 4.5. 

Project dependencies are managed by NuGet in a *packages.config* file at the project root.
The most notable ones are :

- [EntityFramework](https://github.com/aspnet/EntityFramework) as ORM for relational database handling.
- [StackExchange.Redis](https://github.com/StackExchange/StackExchange.Redis/) as Redis access library.
- [Hangfire](https://github.com/HangfireIO/Hangfire) for background jobs management.

Dependencies should be retrieved by calling Nuget's restore command.

    nuget restore


Database Migrations
-----------
MusicPickerService uses Entity Framework Code First database migration.

Before first service start, database should be initialized in Package Manager's Console via command

    Update-Database

Redis setup
-----------
MusicPickerService makes heavy use of Redis as device state storage. 
Redis must be running on your server for SignalR Music Hub to function correctly.

Microsoft provides [Redis binaries for Windows x64 on GitHub](https://github.com/MSOpenTech/redis/releases).

If the provided MSI-packaged service fails to install, the ZIP download contains redis-server.exe which could
be launched by hand before starting MusicPickerService.

Sometimes Redis on Windows fails to start, complaining about memory heap usage boundaries.
One quick workaround is to adjust the *maxheap* flag on startup.

    redis-server.exe --maxheap 1gb


MSMQ setup
-----------
MusicPicker configures its Hangfire dependency to use MSMQ message queues for job creation events.

MSMQ is an optional system feature bundled with Windows 7, 8 and Server 2008/2012.

It can be activated via Windows' *Add optional features* screen in Control Panel, or via this PowerShell command :

    Enable-WindowsOptionalFeature -Online -FeatureName MSMQ-Container -All

License
===========
© 2015 Hugo Caille, Pierre Defache & Thomas Fossati. 

MusicPicker is released upon the terms of the Apache 2.0 License.
