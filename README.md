# CONFIG SERVER A CENTRALIZED AND DISTRIBUTED WAY TO STORE YOUR APP CONFIGS #

Configuration never has been so easy.

### What is this about? ###

* Ok, have you ever been worry where to store your configurations variables to be accessed by all the components of your application, like Website, Application Servers, Web Services, All servers in your web farm/cluster.
* Have you ever been in a situation of having different configurations per environments (Production/Testing/Staging) and you have some troubles how to organize the files and settings in a easy way to understand?
* Well the mision of this project is to make easy this process and more than that, take advantages of some concepts like inheritance and references to create an stronger configuration plan.

### How do I get set up? ###

This solution works as a self hosted web application that expose an API to access to all your configuration settings using a single endpoint.
![config1.JPG](https://bitbucket.org/repo/A4rabL/images/2531022188-config1.JPG)

The way to store the configuration is the most important thing here, All are json files that support intelligent operations like:

- references
- merging
- inheritance

###Lets go with examples, Examples always are best way to understand###

Lets suppose our config file is: config.json and contain

```
#!json
{
    "configuration": {

        "base": {
            "field0": "value0"
        },
        "baseobject": {
            "field0": "value0X",
            "field1": "value1"
        },
        "newobject": {
            "field0": "value0XY",
            "field1": "value1XY",
            "field2": "value2XY"
        }
    }
}

```
Now how can I access to all those values?

Ok first lets use a client tool for connecting to the ConfigServer and setup in our traditional config file the endpoint connection:

web.config or app.config
```
#!xml
<configuration>
  <configSections>
    <section name="ConfigServer" type="ConfigServer.Client.ClientConfigSection, ConfigServer.Client"/>
  </configSections>

  <ConfigServer Server="http://localhost:9000/" BaseNode="configuration"/>
</configuration>
```

Now having specified your node base as "configuration"

```
#!c#
var srv = new ConfigService();
dynamic obj1 = srv.GetTree();
```

obj1 is now a dynamic object that contains all the configuration node, so now you only specify the field you want as any object.

```
#!c#
var bofield0 = obj1.baseobject.field0; //= “value0X”
```

If you dont want to use the client settings in web.config then you have to specify the connection parameters in the moment you instance the class:

```
#!c#
var basenode = "configuration";
var srv = new ConfigService(("http://localhost:9000/", basenode);
dynamic obj1 = srv.GetTree();
```

###Get a single node instead of the base node###

Now you dont want to get the whole base node because your configuration file is so big you only are interested in part of the file:
```
#!c#
dynamic obj1 = srv.GetTree("baseobject");
```

Now obj1 contain the relative object corresponding to the node configuration.baseobject, so you can:
```
#!c#
var value = obj1.field0; //= “value0X”
```

###Dont like dynamic types?###

Lets do it with concrete types, now you can take advantages of intellisense:

```
#!c#
class BaseObject{
     public string field0{get; set;}
     public string field1{get; set;}
}

var srv = new ConfigService("http://localhost:9000/", "configuration");
BaseObject bo = srv.GetTree<BaseObject>("baseobject");
```

###Now array, yes let store array values###

```
#!json
{
    "configuration": {

      "applications": [
       {
           "name" : "LogServer" 
           "port" : 1090,
           "address": "127.0.0.1"
       },
       {
            "name" : "CommonData" 
            "port" : 1090,
            "url": "http://127.0.0.1/"
       }]
}

```

Lets grab the "address" of the first element of the array "applications"

```
#!c#
var srv = new ConfigService("http://localhost:9000/", "configuration");
dynamic apps = srv.GetTree("applications");
var address = apps[0].address; 
```

Ok, I dont like numeric indexes, I want to refer to the item with a "Name" Lets grab the "address" from my item named: "LogServer" does not matter where position is in the array.

```
#!c#
dynamic app = srv.GetTree("applications[@LogServer]");
var address = app.address;
```



* Configuration
* Dependencies
* Database configuration
* How to run tests
* Deployment instructions

### Contribution guidelines ###

* Writing tests
* Code review
* Other guidelines

### Who do I talk to? ###

* Repo owner or admin
* Other community or team contact