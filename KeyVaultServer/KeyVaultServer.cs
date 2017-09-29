using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

using Microsoft.Azure.KeyVault;
using EncryptedType;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Security.Cryptography.X509Certificates;

namespace KeyVaultServer
{
    public class KeyVaultServer : IKeyServer
    {
        private KeyVaultClient _kv;
        private string _vaultURL;

        public KeyVaultServer()
        {
            _kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(this.GetAccessToken));
            _vaultURL = ConfigurationManager.AppSettings["KeyVaultURL"];
        }

        private ClientAssertionCertificate _assertionCert { get; set; }

        public IList<string> Keys
        {
            get {
                var ret = new List<string>();
                var secrets = _kv.GetSecretsAsync(_vaultURL).GetAwaiter().GetResult();
                if (null == secrets )
                {
                    return ret;
                }
                foreach (var s in secrets)
                {
                    ret.Add(s.Identifier.Name);
                }
                return ret;
            }
        }

        public IDictionary<string, string> Map => throw new NotImplementedException();

        public string GetKey(string KeyName)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetAccessToken(string authority, string resource, string scope)
        {
            var context = new AuthenticationContext(authority, TokenCache.DefaultShared);
            var result = await context.AcquireTokenAsync(resource, _assertionCert);
            return result.AccessToken;
        }

        internal X509Certificate2 GetCertificateByName(string name)
        {
            X509Certificate2 cert = null;
            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                cert = store.Certificates.OfType<X509Certificate2>().Where(x => x.NotBefore < DateTime.Now && x.NotAfter > DateTime.Now).FirstOrDefault(x => x.SubjectName.Name.Contains(name));
            }
            finally
            {
                store.Close();
            }
            return cert;
        }
    }
}
