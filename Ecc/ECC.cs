using Newtonsoft.Json;
using System.Text.Json;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Security.Cryptography;
using System.IO;
using System.Collections;

namespace Ecc
{

    public struct point
    {
        public BigInteger x;
        public BigInteger y;
        public bool inf;
        public point(BigInteger x, BigInteger y, bool inf)
        {
            this.x = x;
            this.y = y;
            this.inf = inf;
        }
    }

    public struct parameters
    {
        public BigInteger a, b, x1, y1, p;
        public parameters(BigInteger a, BigInteger b, BigInteger x1, BigInteger y1, BigInteger p)
        {
            this.a = a;
            this.b = b;
            this.x1 = x1;
            this.y1 = y1;
            this.p = p;
        }
    }

    public class ECC
    {

        public point publicKey = new point(0, 0, false);
        BigInteger secret = 0;

        private parameters parameters = new parameters();
        public parameters Parameters { get { return parameters; } }
        public BigInteger Secret { get { return secret; } }
        public BigInteger PrivateKey { get; }

        public string PublicKey()
        {
            return string.Format(publicKey.x + " " + publicKey.y);

        }
        public ECC(BigInteger a, BigInteger b, BigInteger x1, BigInteger y1, BigInteger p, BigInteger n)
        {
            point P = new point(x1, y1, false);
            parameters.a = a;
            parameters.b = b;
            parameters.x1 = x1;
            parameters.y1 = y1;
            parameters.p = p;
            PrivateKey = n;
            publicKey = CalculatePoint(a, b, P, p, n);
            secret = publicKey.x;

        }
        public ECC(parameters parameters, BigInteger n)
        {
            point P = new point(parameters.x1, parameters.y1, false);
            this.parameters = parameters;
            PrivateKey = n;
            publicKey = CalculatePoint(parameters.a, parameters.b, P, parameters.p, n);
            secret = publicKey.x;

        }

        point CalculatePoint(BigInteger a, BigInteger b, point P, BigInteger p, BigInteger n)
        {
            BigInteger privKey = n;

            string bin = Reverse(ToBinaryString(privKey));
            Console.WriteLine(bin);

            int len = bin.Length - 2;
            point temp = P;

            while (len >= 0)
            {
                // Console.WriteLine(temp.x + " " + temp.y + " " + temp.inf);
                temp = PointDouble(a, temp, p);

                if (bin[len] == '1')
                {
                    //temp = PointAddition(temp[0], P[0], temp[1], P[1], p);
                    temp = PointAddition(P, temp, p);
                    //Console.WriteLine("add");

                }
                len--;
            }
            //if (temp.inf == true) throw new Exception("Point is infinite");
            /*else*/
            return temp;
        }

        public point PointDouble(BigInteger a, point point, BigInteger p)
        {
            if (point.inf == true) return new point(0, 0, true);

            point point3 = new point(0, 0, false);
            BigInteger s = (3 * (point.x * point.x) + a) * modInverse(2 * point.y, p) % p;
            point3.x = (s * s - point.x * 2) % p;
            point3.x = ModNeg(point3.x, p);
            point3.y = (s * (point.x - point3.x) - point.y) % p;
            point3.y = ModNeg(point3.y, p);

            return point3;
        }

        point PointAddition(point P, point Q, BigInteger p)
        {
            point point3 = new point(0, 0, false);

            if (Q.inf == true)
                return P;
            else if (Q.Equals(P))
                return PointDouble(parameters.a, P, p);
            else if (Q.x - P.x == 0)
                return new point(0, 0, true);

            BigInteger s = ModNeg((Q.y - P.y) % p, p) * modInverse(ModNeg(Q.x - P.x, p), p) % p;
            point3.x = ModNeg((s * s - P.x - Q.x) % p, p);
            point3.y = ModNeg((s * (P.x - point3.x) - P.y) % p, p);

            return point3;
        }

        BigInteger modInverse(BigInteger a, BigInteger n)
        {
            BigInteger i = n, v = 0, d = 1;
            while (a > 0)
            {
                BigInteger t = i / a, x = a;
                a = i % x;
                i = x;
                x = d;
                d = v - t * x;
                v = x;
            }
            v %= n;
            if (v < 0) v = (v + n) % n;
            return v;
        }

        BigInteger ModNeg(BigInteger st, BigInteger p)
        {
            if (st < 0)
            {
                BigInteger k = st / p - 1;
                st -= k * p;
            }
            return st;
        }
        public static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
        private string ToBinaryString(BigInteger bigint)
        {
            var bytes = bigint.ToByteArray();
            var idx = bytes.Length - 1;

            var base2 = new StringBuilder();

            var binary = Convert.ToString(bytes[idx], 2);

            if (binary != "0" || bytes[0] == 0)
                base2.Append(binary);

            for (idx--; idx >= 0; idx--)
                base2.Append(Convert.ToString(bytes[idx], 2).PadLeft(8, '0'));

            return base2.ToString();
        }
        void DiffieHellman(BigInteger a, BigInteger b, point point, BigInteger p, BigInteger n, point PK)
        {
            publicKey = CalculatePoint(a, b, point, p, n);
            secret = CalculatePoint(a, b, PK, p, n).x;
        }
        //void DiffieHellman(this ECC bob , ECC alice)
        //{
        //    if (bob.Parameters.a == alice.Parameters.a && bob.Parameters.b == alice.Parameters.b && bob.Parameters.p == alice.Parameters.p)
        //    {
        //        //publicKey = CalculatePoint(a, b, point, p, n);
        //        //secret = CalculatePoint(a, b, PK, p, n).x;
        //    }
        ////    publicKey = CalculatePoint(a, b, point, p, n);
        ////    secret = CalculatePoint(a, b, PK, p, n).x;
        //}

        public void ElGamal()
        {
            string message = "hello";
            int k = 26;
            point P = new point();
            point Q = new point();
            Q.x = 16;
            Q.y = 13;
            P.x = 5;
            P.y = 1;
            point R = CalculatePoint(2, 2, P, 17, k);
            point S = CalculatePoint(2, 2, Q, 17, k);
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            BitArray bitArray = new BitArray(bytes);
            BitArray bit = new BitArray(bitArray.Length);

            //bitArray.Xor(new BitArray(S.x.ToByteArray()));
            Console.WriteLine(S.x);

            point T = CalculatePoint(2, 2, R, 17, 25);
            //bitArray.Xor(new BitArray(T.x.ToByteArray()));
            //byte[] dec = new byte[bitArray.Count/8];
            //bitArray.CopyTo(dec, 0);
            Console.WriteLine(T.x);
        }
    }

    public class DFEcc
    {
        public parameters Parameters { get; set; }
        public point PK { get; set; }
        public DFEcc(parameters parameters, point PK)
        {
            Parameters = parameters;
            this.PK = PK;
        }

        public static DFEcc ConvertFromECC(ECC ecc)
        {
            DFEcc dFEcc = new DFEcc(ecc.Parameters, ecc.publicKey);
            return dFEcc;
        }


        public static void test(ECC ecc)
        {
            DFEcc dFEcc = new DFEcc(ecc.Parameters, ecc.publicKey);


            string ser = Newtonsoft.Json.JsonConvert.SerializeObject(dFEcc);
            Console.WriteLine(ser);
            DFEcc? des = Newtonsoft.Json.JsonConvert.DeserializeObject<DFEcc>(ser);
            Console.WriteLine(des.Parameters.a);

        }
    }



    public class DiffieHelman
    {
        public bool UseAes { get; set; } = false;
        bool ContinueStream { get; set; } = true;


        public Connect Connecter { get; set; }
        public BigInteger PrivateKey { get; set; }
        public DiffieHelman(BigInteger privateKey)
        {
            PrivateKey = privateKey;
            Connecter = new Connect(this);
            //this.Connecter = new Connect(this);
        }
        public BigInteger Secret { get; set; }
        public class Connect
        {
            DiffieHelman df;
            TcpClient client;
            NetworkStream stream;
            Task service;

            CancellationTokenSource cts = new CancellationTokenSource();

            public List<string> messages = new List<string>();
            bool countinuedStreamStopped { get; set; }
            bool UseAes { get; set; } = true;
            bool streamOpened = false;
            public bool ConnectionMade { get { return streamOpened; } }
            public Connect(DiffieHelman df)
            {
                this.df = df;
            }
            public void ConnectTo(ECC bob, string server, int port = 34545)
            {
                string message = JsonConvert.SerializeObject(DFEcc.ConvertFromECC(bob));
                try
                {
                    //Vzpostavi povezavo in poslje podatke
                    client = new TcpClient(server, port);
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream = client.GetStream();
                    streamOpened = true;
                    countinuedStreamStopped = false;
                    stream.Write(data, 0, data.Length);
                    cts = new CancellationTokenSource();

                    //Caka na odgovor
                    data = new byte[256];
                    int bytes = stream.Read(data, 0, data.Length);
                    string responseData = Encoding.UTF8.GetString(data, 0, bytes);
                    DFEcc? dFEcc = JsonConvert.DeserializeObject<DFEcc>(responseData);
                    ECC bobP = new ECC(dFEcc.Parameters.a, dFEcc.Parameters.b, dFEcc.PK.x, dFEcc.PK.y, dFEcc.Parameters.p, bob.PrivateKey);
                    Console.WriteLine("the secret is " + bobP.Secret);
                    df.Secret = bobP.Secret;

                    if (!df.ContinueStream)
                    {
                        // Close everything.
                        streamOpened = false;
                        stream.Close();
                        client.Close();
                    }
                    else
                    {
                        service = Task.Factory.StartNew(StreamCountinued, cts.Token);
                    }

                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("ArgumentNullException: {0}", e);
                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }

            }
            void StreamCountinued()
            {
                while (!countinuedStreamStopped)
                {
                    try
                    {
                        byte[] data = new byte[512];
                        int bytes = stream.Read(data, 0, data.Length);
                        string read = Encoding.UTF8.GetString(data, 0, bytes);

                        string FinalRead = DecryptStringFromBytes_Aes(Convert.FromBase64String(read), df.Secret.ToByteArray(), BitConverter.GetBytes(1578881937));
                        messages.Add(FinalRead);
                        Thread.Sleep(50);
                    }
                    catch
                    {
                        break;
                    }
                }
                client.Close();
            }

            public void SendMessage(string message)
            {
                if (streamOpened)
                {
                    if (message == "" || message == null)
                    {
                        throw new Exception("Message is null");
                        return;
                    }
                    //Byte[] data = Encoding.UTF8.GetBytes(message);
                    string data = EncryptStringToBytes_Aes(message, df.Secret.ToByteArray(), BitConverter.GetBytes(1578881937));
                    byte[] SendData = Encoding.UTF8.GetBytes(data);
                    stream.Write(SendData, 0, SendData.Length);
                }
                else throw new Exception("No stream!!");
            }

            public string GetLastMessage()
            {
                if (messages.Count != 0)
                {
                    return messages.Last();
                }
                else
                {
                    return "";
                }
            }

            public void StopMessaging()
            {
                //service.Dispose();
                
                countinuedStreamStopped = true;
                stream.Close();
                client.Close();
                cts.Cancel();
                service.Wait();
                streamOpened = false;
                

            }



        }

        public class Listen
        {
            DiffieHelman df;
            NetworkStream stream;
            TcpListener server;
            TcpClient client;
            Task task;
            Task service;
            CancellationTokenSource cts = new CancellationTokenSource();

            public bool ConnectionMade { get { return streamOpened; } }
            public List<string> messages { get; } = new List<string>();
            public bool countinuedStreamStopped { get; set; } = false;
            public string IP { get; set; }
            public string RemoteIp { get; set; }

            bool streamOpened = false;

            int port = 34545;
            public Listen(DiffieHelman df, string IP)
            {
                this.df = df;
                this.IP = IP;
            }

            public void StartListening(int port = 34545)
            {
                this.port = port;
                Task.Factory.StartNew(StartService);
                //StartService();

            }

            public void StartService(/*int PrivateKey, int port = 34545*/)
            {
                try
                {
                    
                    //Odpre port in caka na povezavo
                    IPAddress localAddr = IPAddress.Parse(IP);
                    server = new TcpListener(localAddr, port);
                    server.Start();
                    byte[] bytes = new byte[256];
                    string data = null;
                    while (true)
                    {
                        client = server.AcceptTcpClient();
                        RemoteIp = client.Client.RemoteEndPoint.AddressFamily.ToString();
                        data = null;
                        stream = client.GetStream();
                        
                        //sprejme in bere stream
                        int i = stream.Read(bytes, 0, bytes.Length);
                        streamOpened = true;
                        countinuedStreamStopped = false;
                        cts = new CancellationTokenSource();
                        data = Encoding.UTF8.GetString(bytes, 0, i);
                        DFEcc? dFEcc = JsonConvert.DeserializeObject<DFEcc>(data);

                        //izracuna public key ter posle nazaj
                        ECC bob = new ECC(dFEcc.Parameters, df.PrivateKey);
                        string resp = JsonConvert.SerializeObject(DFEcc.ConvertFromECC(bob));
                        byte[] byt = new byte[256];
                        byt = Encoding.UTF8.GetBytes(resp);
                        stream.Write(byt, 0, byt.Length);

                        //izracuna secret
                        ECC bobP = new ECC(dFEcc.Parameters.a, dFEcc.Parameters.b, dFEcc.PK.x, dFEcc.PK.y, dFEcc.Parameters.p, df.PrivateKey);
                        //Console.WriteLine("the secret is " + bobP.Secret);
                        df.Secret = bobP.Secret;

                        if (!df.ContinueStream)
                        {
                            client.Close();
                            streamOpened = false;
                        }
                        else
                        {

                            //service = Task.Factory.StartNew(StreamCountinued);
                            service = Task.Run(() => StreamCountinued(), cts.Token);
                            //while (!countinuedStreamStopped)
                            //{

                            //}
                        }
                    }

                }
                catch (SocketException e)
                {
                    Console.WriteLine("SocketException: {0}", e);
                }
                finally
                {
                    server.Stop();
                }
                Console.WriteLine("\nHit enter to continue...");
                Console.Read();
            }

            void StreamCountinued()
            {
                while (!countinuedStreamStopped)
                {
                    try
                    {
                        byte[] data = new byte[512];
                        int bytes = stream.Read(data, 0, data.Length);
                        string read = Encoding.UTF8.GetString(data, 0, bytes);


                        string FinalRead = DecryptStringFromBytes_Aes(Convert.FromBase64String(read), df.Secret.ToByteArray(), BitConverter.GetBytes(1578881937));
                        messages.Add(FinalRead);
                        Thread.Sleep(50);
                    }
                    catch
                    {
                        break;
                    }
                }
                client.Close();
            }

            public void SendMessage(string message)
            {
                if (streamOpened)
                {
                    if (message == "" || message == null)
                    {
                        throw new Exception("Message is null");
                        return;
                    }
                    //Byte[] data = Encoding.UTF8.GetBytes(message);
                    string data = EncryptStringToBytes_Aes(message, df.Secret.ToByteArray(), BitConverter.GetBytes(1578881937));
                    byte[] SendData = Encoding.UTF8.GetBytes(data);
                    stream.Write(SendData, 0, SendData.Length);
                }
                else throw new Exception("No stream!!");
            }

            public string GetLastMessage()
            {
                if (messages.Count != 0)
                {
                    return messages.Last();
                }
                else
                {
                    return "";
                }
            }

            public void StopMessaging()
            {
                //service.Dispose();
                client.Close();
                cts.Cancel();
                service.Wait();
                //countinuedStreamStopped = true;
                streamOpened = false;
            }
        }
        static string EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");
            string encrypted;
            byte[] IVv = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            using (Aes aesAlg = Aes.Create())
            {
                HashAlgorithm hash = MD5.Create();

                aesAlg.Key = hash.ComputeHash(Key);
                aesAlg.IV = IVv;

                byte[] bytes = Encoding.UTF8.GetBytes(plainText);

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        csEncrypt.Write(bytes, 0, bytes.Length);
                    }
                    encrypted = Convert.ToBase64String(msEncrypt.ToArray());
                }
            }


            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            string plaintext = null;
            byte[] IVv = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };

            using (Aes aesAlg = Aes.Create())
            {
                HashAlgorithm hash = MD5.Create();

                aesAlg.Key = hash.ComputeHash(Key);
                aesAlg.IV = IVv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        byte[] decryptedBytes = new byte[cipherText.Length];
                        csDecrypt.Read(decryptedBytes, 0, decryptedBytes.Length);
                        plaintext = Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            return plaintext;
        }

        static string GetIP()
        {
            var n = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (var i in n)
            {
                if (i.AddressFamily == AddressFamily.InterNetwork)
                {
                    return i.ToString();
                }
            }

            return null;
        }

    }
}