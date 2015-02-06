using System;
using System.Web.Hosting;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;

namespace InfoSpace.DeviceLab.Web
{
    public class DatabaseConfig
    {
        public static IDocumentStore DocumentStore
        {
            get;
            private set;
        }

        public static void InitializeDatabase()
        {
            DocumentStore = new EmbeddableDocumentStore
            {
                Conventions =
                {
                    DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite
                },
                ConnectionStringName = "RavenDB"
            };
            DocumentStore.Initialize();
            //DocumentStore.AggressivelyCacheFor(TimeSpan.FromSeconds(30));
        }

        public static void Shutdown()
        {
            DocumentStore.Dispose();
        }
    }
}
