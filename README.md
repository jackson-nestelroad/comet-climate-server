# Comet Climate Application Server
This repository features an application server I created in C# and .NET Core for my *CS-1200 Introduction to Computer Science* class' final group project. 
## About the Project
**Comet Climate** is a weather application to be developed for iOS and Android devices designed specifically for students, faculty, and staff at the University of Texas at Dallas in Richardson, Texas. The application will deliver the current weather alongside a five-hour forecast for the local area. Users may also view upcoming events given directly from the university’s Twitter feed and online calendars in hopes to push students to get more involved on campus.
## About the Server
For a simple application, our team decided to collect data from the National Weather Service's website and Twitter's official API. Opposed to each device sending their own pair of requests, each device would connect to this application server to receieve the data to display. This server acts as a hub that collects the data in a single, centralized location for each application to connect to.

The server works very much like an Application Program Interface (API); however, the server attempts to update its own data if it finds its data is stale. While this results in an occasional slower load time, the self-updating feature keeps all of our code in one place.