/* Copyright 2018–present MongoDB Inc.
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

namespace MongoDB.Driver.Core.Authentication
{
    /// <summary>
    /// A cache for Client and Server keys, to be used during authentication.
    /// </summary>
    internal class ScramCache
    {
        private Object _cacheKey;
        private Object _cacheValue;

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public ScramCacheEntry Get(Object key)
        {
            if (_cacheKey != null && _cacheKey.Equals(key))
            {
                return (ScramCacheEntry)_cacheValue;
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Set(Object key, Object value) //Tuple<string, string, int> key, byte[] clientKey, byte[] serverKey)
        {
            _cacheKey = key;
            _cacheValue = value;
        }
    }

    internal class ScramCacheKey
    {
        private int _iterationCount;
        private byte[] _hashedPassword;
        private byte[] _salt;

        internal ScramCacheKey(byte[] hashedPassword, byte[] salt, int iterationCount)
        {
            _iterationCount = iterationCount;
            _hashedPassword = hashedPassword;
            _salt = salt;
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
                _hashedPassword.SequenceEqual(other._hashedPassword) &&
                _iterationCount == other._iterationCount &&
                _salt.SequenceEqual(other._salt);
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + _iterationCount.GetHashCode();
            hash = 37 * hash + _hashedPassword.GetHashCode();
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
