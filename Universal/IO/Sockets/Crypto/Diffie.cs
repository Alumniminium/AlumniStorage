using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace Universal.IO.Sockets.Crypto
{
    public class DiffieHellman : IDisposable
    {
        private int bits = 521;
        BigInteger prime;
        BigInteger g;
        BigInteger mine;
        byte[] key;
        string representation;
        public byte[] Key => key;

        public DiffieHellman()
        {
        }

        public DiffieHellman(int bits)
        {
            this.bits = bits;
        }

        ~DiffieHellman()
        {
            Dispose();
        }

        /// <summary>
        /// Generates a request packet.
        /// </summary>
        /// <returns></returns>
        public DiffieHellman GenerateRequest()
        {
            // Generate the parameters.
            prime = BigInteger.GenPseudoPrime(bits, 30);
            mine = BigInteger.GenPseudoPrime(bits, 30);
            g = 5;

            // Gemerate the string.
            StringBuilder rep = new StringBuilder();
            rep.Append(prime.ToString(36));
            rep.Append("|");
            rep.Append(g.ToString(36));
            rep.Append("|");

            // Generate the send BigInt.
            using (BigInteger send = g.ModPow(mine, prime))
            {
                rep.Append(send.ToString(36));
            }

            representation = rep.ToString();
            return this;
        }

        /// <summary>
        /// Generate a response packet.
        /// </summary>
        /// <param name="request">The string representation of the request.</param>
        /// <returns></returns>
        public DiffieHellman GenerateResponse(string request)
        {
            string[] parts = request.Split('|');

            // Generate the would-be fields.
            using (BigInteger prime = new BigInteger(parts[0], 36))
            using (BigInteger g = new BigInteger(parts[1], 36))
            using (BigInteger mine = BigInteger.GenPseudoPrime(bits, 30))
            {
                // Generate the key.
                using (BigInteger given = new BigInteger(parts[2], 36))
                using (BigInteger key = given.ModPow(mine, prime))
                {
                    this.key = key.GetBytes();
                }
                // Generate the response.
                using (BigInteger send = g.ModPow(mine, prime))
                {
                    representation = send.ToString(36);
                }
            }

            return this;
        }

        /// <summary>
        /// Generates the key after a response is received.
        /// </summary>
        /// <param name="response">The string representation of the response.</param>
        public void HandleResponse(string response)
        {
            // Get the response and modpow it with the stored prime.
            using (BigInteger given = new BigInteger(response, 36))
            using (BigInteger key = given.ModPow(mine, prime))
            {
                this.key = key.GetBytes();
            }
            Dispose();
        }


        public override string ToString() => representation;

        public void Dispose()
        {
            if (!ReferenceEquals(prime, null))
                prime.Dispose();
            if (!ReferenceEquals(mine, null))
                mine.Dispose();
            if (!ReferenceEquals(g, null))
                g.Dispose();

            prime = null;
            mine = null;
            g = null;

            representation = null;
        }
    }
}