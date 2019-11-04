using System;
using System.Text;

namespace Universal.IO.Sockets.Crypto
{
    public class DiffieHellman : IDisposable
    {
        private readonly int _bits = 521;
        private BigInteger _prime;
        private BigInteger _g;
        private BigInteger _mine;
        private string _representation;
        public byte[] Key { get; private set; }

        public DiffieHellman()
        {
        }

        public DiffieHellman(int bits)
        {
            _bits = bits;
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
            _prime = BigInteger.GenPseudoPrime(_bits, 30);
            _mine = BigInteger.GenPseudoPrime(_bits, 30);
            _g = 5;

            // Generate the string.
            var rep = new StringBuilder();
            rep.Append(_prime.ToString(36));
            rep.Append("|");
            rep.Append(_g.ToString(36));
            rep.Append("|");

            // Generate the send BigInt.
            using (var send = _g.ModPow(_mine, _prime))
            {
                rep.Append(send.ToString(36));
            }

            _representation = rep.ToString();
            return this;
        }

        /// <summary>
        /// Generate a response packet.
        /// </summary>
        /// <param name="request">The string representation of the request.</param>
        /// <returns></returns>
        public DiffieHellman GenerateResponse(string request)
        {
            var parts = request.Split('|');

            // Generate the would-be fields.
            using (var prime = new BigInteger(parts[0], 36))
            using (var g = new BigInteger(parts[1], 36))
            using (var mine = BigInteger.GenPseudoPrime(_bits, 30))
            {
                // Generate the key.
                using (var given = new BigInteger(parts[2], 36))
                using (var key = given.ModPow(mine, prime))
                {
                    Key = key.GetBytes();
                }
                // Generate the response.
                using (var send = g.ModPow(mine, prime))
                {
                    _representation = send.ToString(36);
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
            using (var given = new BigInteger(response, 36))
            using (var key = given.ModPow(_mine, _prime))
            {
                Key = key.GetBytes();
            }
            Dispose();
        }


        public override string ToString() => _representation;

        public void Dispose()
        {
            _prime?.Dispose();
            _mine?.Dispose();
            _g?.Dispose();

            _prime = null;
            _mine = null;
            _g = null;

            _representation = null;
        }
    }
}