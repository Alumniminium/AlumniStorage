using System;

namespace AlumniStorageCore
{
    public class AlumniStorageClient
    {
        private string _userSecret;
        private string _userToken;

        public string UserToken { get => _userToken; set => _userToken = value; }
        public string UserSecret { get => _userSecret; set => _userSecret = value; }

        public AlumniStorageClient()
        {

        }
        public AlumniStorageClient(string token, string secret)
        {

        }
    }
}
