using System;

namespace EVotingSystem.Security
{
    public static class HmacKey
    {
        // 32 byte key for HMAC
        public static readonly byte[] MetadataHmacKey =
            Convert.FromBase64String("yE5r9m4x2n8XbZqT6N0vJrKp5A1CwH7QF8S9D0L2M4==");
    }
}
