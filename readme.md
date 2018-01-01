#So-far-Unnamed-UnityMMO

### Setup
Create a mySQL Database and use the provided sql files in the DBschema directory to setup the database.

Create an App.config file (using App.config.template in MMOServer directory as the guide). Replace the connection string with your own local dev
DB connection string.
Copy the App.config to the MMOWorldServer directory also with a slightly different port.
Packets library should compile and set it's dll in the UnityMMO assets folder. This is necessary for Unity to include the reference into it's project.

Current supported Unity version: 2017.3
