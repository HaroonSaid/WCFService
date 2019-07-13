using Amazon.CertificateManager;
using Amazon.CertificateManager.Model;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Collections;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Store;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CertifcateManager
{
    public class AcmClient
    {
        private readonly IAmazonCertificateManager amazonCertificateManager;

        public AcmClient(IAmazonCertificateManager amazonCertificateManager)
        {
            this.amazonCertificateManager = amazonCertificateManager;
        }

        public async Task<X509Certificate2> GetCertificate(string arn)
        {
            var passPharse = Guid.NewGuid().ToString();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(passPharse));
            var request = new ExportCertificateRequest
            {
                CertificateArn = arn,
                Passphrase = ms
            };
            try
            {
                var response = await amazonCertificateManager.ExportCertificateAsync(request);
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    throw new Exception("ugh!");
                var pub = GetBytesFromPEM(response.Certificate);
                var chain = new X509Certificate2(GetBytesFromPEM(response.CertificateChain));
                var privateKeyBytes = GetPrivateBytes(response.PrivateKey);
                var privateKey = DecodePrivateKey(response.PrivateKey, passPharse);
                var rsaPrivateKey = DotNetUtilities.ToRSA(privateKey.Private as RsaPrivateCrtKeyParameters);
                var cert = new X509Certificate2(pub)
                {
                    PrivateKey = rsaPrivateKey,
                    FriendlyName = "axs",
                };
                var certChain = Verify(cert, chain);
                return cert;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw e;
            }
        }
        public static byte[] GetBytesFromPEM(string pemString)
        {
            var header = "-----BEGIN CERTIFICATE-----\n";
            var footer = "-----END CERTIFICATE-----\n";
            return GetBytes(pemString, header, footer);

        }
        private static byte[] GetPrivateBytes(string pemString)
        {
            return GetBytes(pemString, "-----BEGIN ENCRYPTED PRIVATE KEY-----\n",
                "-----END ENCRYPTED PRIVATE KEY-----\n");
        }
        private static byte[] GetBytes(string pemString, string header, string footer)
        {
            var start = pemString.IndexOf(header, StringComparison.CurrentCultureIgnoreCase);
            if (start < 0)
                throw new IndexOutOfRangeException($"not found {header}");

            start += header.Length;
            var end = pemString.IndexOf(footer, start, StringComparison.CurrentCultureIgnoreCase) - start;

            if (end < 0)
                throw new IndexOutOfRangeException($"not found {footer}");
            var str = pemString.Substring(start, end);
            return Convert.FromBase64String(str);
        }
        public static AsymmetricCipherKeyPair DecodePrivateKey(string encryptedPrivateKey, string password)
        {
            var textReader = new StringReader(encryptedPrivateKey);
            var pemReader = new PemReader(textReader, new PasswordFinder(password));
            object privateKeyObject = pemReader.ReadObject();
            var rsaPrivatekey = (RsaPrivateCrtKeyParameters)privateKeyObject;
            var rsaPublicKey = new RsaKeyParameters(false, rsaPrivatekey.Modulus, rsaPrivatekey.PublicExponent);
            var kp = new AsymmetricCipherKeyPair(rsaPublicKey, rsaPrivatekey);
            return kp;
        }
        private class PasswordFinder : IPasswordFinder
        {
            private readonly string password;

            public PasswordFinder(string password)
            {
                this.password = password;
            }


            public char[] GetPassword()
            {
                return password.ToCharArray();
            }
        }
        static bool Verify(X509Certificate2 client, X509Certificate2 authority)
        {
            X509Chain chain = new X509Chain();
            chain.ChainPolicy.ExtraStore.Add(authority);

            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;

            if (!chain.Build(client))
                return false;

            return chain.ChainElements
                .Cast<X509ChainElement>()
                .Any(x => x.Certificate.Thumbprint == authority.Thumbprint);
        }
        //static IEnumerable<Org.BouncyCastle.X509.X509Certificate> BuildCertificateChainBC(byte[] primary, IEnumerable<byte[]> additional)
        //{
        //    X509CertificateParser parser = new X509CertificateParser();
        //    PkixCertPathBuilder builder = new PkixCertPathBuilder();

        //    // Separate root from itermediate
        //    List<Org.BouncyCastle.X509.X509Certificate> intermediateCerts = new List<Org.BouncyCastle.X509.X509Certificate>();
        //    HashSet rootCerts = new HashSet();

        //    foreach (byte[] cert in additional)
        //    {
        //        Org.BouncyCastle.X509.X509Certificate x509Cert = parser.ReadCertificate(cert);

        //        // Separate root and subordinate certificates
        //        if (x509Cert.IssuerDN.Equivalent(x509Cert.SubjectDN))
        //            rootCerts.Add(new TrustAnchor(x509Cert, null));
        //        else
        //            intermediateCerts.Add(x509Cert);
        //    }

        //    // Create chain for this certificate
        //    X509CertStoreSelector holder = new X509CertStoreSelector
        //    {
        //        Certificate = parser.ReadCertificate(primary)
        //    };

        //    // WITHOUT THIS LINE BUILDER CANNOT BEGIN BUILDING THE CHAIN
        //    intermediateCerts.Add(holder.Certificate);

        //    PkixBuilderParameters builderParams = new PkixBuilderParameters(rootCerts, holder);
        //    builderParams.IsRevocationEnabled = false;

        //    X509CollectionStoreParameters intermediateStoreParameters =
        //        new X509CollectionStoreParameters(intermediateCerts);

        //    builderParams.AddStore(X509StoreFactory.Create(
        //        "Certificate/Collection", intermediateStoreParameters));

        //    PkixCertPathBuilderResult result = builder.Build(builderParams);

        //    return result.CertPath.Certificates.Cast<Org.BouncyCastle.X509.X509Certificate>();
        //}

    }
}