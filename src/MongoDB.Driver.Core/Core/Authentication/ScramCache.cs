/* Copyright 2019–present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using System.Security;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// A cache for Client and Server keys, to be used during authentication.
    /// </summary>
    internal class ScramCache
    {
        private ScramCacheKey _cacheKey;
        private ScramCacheEntry _cachedEntry;

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public bool TryGet(ScramCacheKey key, out ScramCacheEntry entry)
        {
            if (_cacheKey.Equals(key))
            {
                entry = _cachedEntry;
                return true;
            }
            else
            {
                entry = null;
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="entry"></param>
        public void Set(ScramCacheKey key, ScramCacheEntry entry) //Tuple<string, string, int> key, byte[] clientKey, byte[] serverKey)
        {
            _cacheKey = key;
            _cachedEntry = entry;
        }
    }

    internal class ScramCacheKey
    {
        private int _iterationCount;
        private SecureString _password;
        private byte[] _salt;

        internal ScramCacheKey(SecureString password, byte[] salt, int iterationCount)
        {
            _iterationCount = iterationCount;
            _password = password;
            _salt = salt;
        }

        private bool Equals(SecureString x, SecureString y)
        {
            using (var dx = new DecryptedSecureString(x))
            using (var dy = new DecryptedSecureString(y))
            {
                var xchars = dx.GetChars();
                var ychars = dy.GetChars();
                return xchars.SequenceEqual(ychars);
            }
        }
        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (obj == null || obj.GetType() != obj.GetType())
            {
                return false;
            }

            ScramCacheKey other = (ScramCacheKey) obj;

            return
                Equals(_password,other._password) &&
                _iterationCount == other._iterationCount &&
                _salt.SequenceEqual(other._salt);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + _iterationCount.GetHashCode();
            hash = 37 * hash + _password.GetHashCode();
            hash = 37 * hash + _salt.GetHashCode();
            return hash;
        }
    }

    internal class ScramCacheEntry
    {
        private byte[] _clientKey;
        private byte[] _serverKey;

        public ScramCacheEntry(byte[] clientKey, byte[] serverKey)
        {
            _clientKey = clientKey;
            _serverKey = serverKey;
        }

        public byte[] ClientKey => _clientKey;

        public byte[] ServerKey => _serverKey;
    }
}
