using Bitqap.Middleware.Entity.BusinessEntity;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace Bitqap.Middleware.Business.Utils
{
    public class CryptoUtility
    {
        private static AsymmetricCipherKeyPair GenerateKeys(int keySizeInBits)
        {
            var r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(), keySizeInBits));
            var keys = r.GenerateKeyPair();
            return keys;
        }

        public static UserKeyPair GenerateKeyPair(int keySizeInBits)
        {
            var keys = GenerateKeys(keySizeInBits);

            PrivateKeyInfo pkInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keys.Private);
            String privateKey = Convert.ToBase64String(pkInfo.GetDerEncoded());

            SubjectPublicKeyInfo info = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keys.Public);
            String publicKey = Convert.ToBase64String(info.GetDerEncoded());

            return new UserKeyPair { PrivateKey = $"-----BEGIN PRIVATE KEY-----\n{privateKey}\n-----END PRIVATE KEY-----", 
                PublicKey = $"-----BEGIN PUBLIC KEY-----\n{publicKey}\n-----END PUBLIC KEY-----" };
        }
    }
}
