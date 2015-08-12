using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nsteam.ConfigServer.Demo.StrongTyped
{
    public class Global
    {
        public string MailServer { get; set; }
        public string Welcome { get; set; }
    }

    public class DefaultApplication
    {
        public string name { get; set; }
    }

    public class DataContractSerializer
    {
        public string maxItemsInObjectGraph { get; set; }
    }

    public class Behavior
    {
        public string name { get; set; }
        public DataContractSerializer dataContractSerializer { get; set; }
    }

    public class EndpointBehaviors
    {
        public Behavior behavior { get; set; }
    }

    public class Behaviors
    {
        public EndpointBehaviors endpointBehaviors { get; set; }
    }

    public class ReaderQuotas
    {
        public string maxDepth { get; set; }
        public string maxStringContentLength { get; set; }
        public string maxArrayLength { get; set; }
        public string maxBytesPerRead { get; set; }
        public string maxNameTableCharCount { get; set; }
    }

    public class Transport
    {
        public string protectionLevel { get; set; }
    }

    public class Security
    {
        public string mode { get; set; }
        public Transport transport { get; set; }
    }

    public class Binding
    {
        public string name { get; set; }
        public string closeTimeout { get; set; }
        public string openTimeout { get; set; }
        public string receiveTimeout { get; set; }
        public string sendTimeout { get; set; }
        public string transactionFlow { get; set; }
        public string transferMode { get; set; }
        public string maxBufferPoolSize { get; set; }
        public string maxBufferSize { get; set; }
        public string maxReceivedMessageSize { get; set; }
        public string portSharingEnabled { get; set; }
        public ReaderQuotas readerQuotas { get; set; }
        public Security security { get; set; }
    }

    public class NetTcpBinding
    {
        public Binding binding { get; set; }
    }

    public class Bindings
    {
        public NetTcpBinding netTcpBinding { get; set; }
    }

    public class Endpoint
    {
        public string address { get; set; }
        public string binding { get; set; }
        public string bindingConfiguration { get; set; }
        public string behaviorConfiguration { get; set; }
        public string contract { get; set; }
        public string name { get; set; }
    }

    public class Client
    {
        public List<Endpoint> endpoint { get; set; }
    }

    public class SystemServiceModel
    {
        public Behaviors behaviors { get; set; }
        public Bindings bindings { get; set; }
        public Client client { get; set; }
    }


    public class DefaultEnviroment
    {
        public string name { get; set; }
        public List<DefaultApplication> applications { get; set; }
    }

    public class Base
    {
        public DefaultApplication defaultApplication { get; set; }
        public List<DefaultApplication> applications { get; set; }
        public DefaultEnviroment defaultEnviroment { get; set; }
    }

    public class ApplicationLogServer : DefaultApplication
    {
        public string ServerAddr { get; set; }
        public string MailServer { get; set; }
        public SystemServiceModel systemServiceModel { get; set; }
    }

    public class ApplicationCommonData : DefaultApplication
    {
        public SystemServiceModel systemServiceModel { get; set; }
    }

    public class Enviroment : DefaultEnviroment
    {

    }

    public class Configuration
    {
        public Global global { get; set; }
        public Base @base { get; set; }
        public List<Enviroment> enviroments { get; set; }
    }

}
