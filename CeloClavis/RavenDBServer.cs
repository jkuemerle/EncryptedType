using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raven;
using Raven.Client.Document;
using EncryptedType;

namespace CeloClavis
{
    public class RavenDBServer : IKeyServer
    {

        Raven.Client.IDocumentStore docStore;
        Raven.Client.IDocumentSession session;
        
        private RavenDBServer()
        {
        }

        public RavenDBServer(string Url, string Database)
        {
            docStore = new Raven.Client.Document.DocumentStore() { Url = Url, DefaultDatabase = "Keys" };
            docStore.Initialize();
            session = docStore.OpenSession();
        }

        public IList<string> Keys
        {
            get
            {
                return (from k in session.Query<Key>() select k.Name).ToList(); 
            }
        }

        public string GetKey(string KeyName)
        {
            return (from k in session.Query<Key>() where k.Name == KeyName select k.KeyValue).First();
        }

        public IDictionary<string, string> Map
        {
            get
            {
                return new Dictionary<string, string>();
            }
        }
    }
}
