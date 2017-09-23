// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Binary;

namespace Microsoft.AspNetCore.SignalR.Internal.Formatters
{
    public static class BinaryMessageParser
    {
        public static bool TryParseMessage(ref ReadOnlyBuffer<byte> buffer, out ReadOnlyBuffer<byte> payload)
        {
            payload = default;

            if (buffer.IsEmpty)
            {
                return false;
            }

            var length = 0U;
            var numBytes = 0;

            byte byteRead;
            do
            {
                byteRead = buffer.Span.Slice(numBytes, sizeof(byte)).Read<byte>();
                length = length | (((uint)(byteRead & 0x7f)) << (numBytes * 7));
                numBytes++;
            }
            while (numBytes < Math.Min(5, buffer.Length) && ((byteRead & 0x80) != 0));

            // size bytes are missing
            if ((byteRead & 0x80) != 0 && (numBytes < 5))
            {
                return false;
            }

            if ((byteRead & 0x80) != 0 || (numBytes == 5 && byteRead > 7))
            {
                throw new FormatException("Messages over 2GB in size are not supported.");
            }

            // We don't have enough data
            if (buffer.Length < length + numBytes)
            {
                return false;
            }

            // Get the payload
            payload = buffer.Slice(numBytes, (int)length);

            // Skip the payload
            buffer = buffer.Slice(numBytes + (int)length);
            return true;
        }
    }
}
