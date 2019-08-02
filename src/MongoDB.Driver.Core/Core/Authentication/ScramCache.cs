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
        public Object Get(Object key)
        {
            if (_cacheKey != null && _cacheKey.Equals(key))
            {
                return _cacheValue;
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

    internal class CacheKey
    {
        private byte[] hashedPasswordAndSalt;
        private byte[] salt;
        private int iterationCount;

        internal CacheKey(byte[] hashedPasswordAndSalt, byte[] salt, int iterationCount)
        {
            this.hashedPasswordAndSalt = hashedPasswordAndSalt;
            this.salt = salt;
            this.iterationCount = iterationCount;
        }

        public override bool Equals(Object o)
        {
            if (this == o)
            {
                return true;
            }

            if (o == null || o.GetType() != o.GetType())
            {
                return false;
            }

            CacheKey that = (CacheKey) o;

            if (iterationCount != that.iterationCount)
            {
                return false;
            }

            if (!hashedPasswordAndSalt.Equals(that.hashedPasswordAndSalt))
            {
                return false;
            }

            return salt.Equals(that.salt);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (hashedPasswordAndSalt != null ? hashedPasswordAndSalt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (salt != null ? salt.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ iterationCount;
                return hashCode;
            }
        }
    }

    internal class CacheValue
    {
        private byte[] _clientKey;
        private byte[] _serverKey;

        public CacheValue(byte[] clientKey, byte[] serverKey)
        {
            this._clientKey = clientKey;
            this._serverKey = serverKey;
        }

        public byte[] GetClientKey()
        {
            return _clientKey;
        }

        public byte[] GetServerKey()
        {
            return _serverKey;
        }
    }
}
