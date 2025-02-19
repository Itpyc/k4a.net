﻿using System;
using System.IO;
using System.Text;

namespace K4AdotNet
{
    internal static class Helpers
    {
        public static int UIntPtrToInt32(UIntPtr value)
            => checked((int)value.ToUInt32());

        public static UIntPtr Int32ToUIntPtr(int value)
            => new UIntPtr((ulong)value);

        public delegate NativeCallResults.BufferResult GetInByteBufferMethod<T>(T parameter, byte[] data, ref UIntPtr dataSize);

        public static bool TryGetValueInByteBuffer<T>(GetInByteBufferMethod<T> getMethod, T parameter, out byte[] result)
        {
            var buffer = new byte[0];
            var bufferSize = UIntPtr.Zero;
            var size = 0;

            var res = getMethod(parameter, buffer, ref bufferSize);
            if (res == NativeCallResults.BufferResult.TooSmall)
            {
                size = UIntPtrToInt32(bufferSize);
                if (size > 1)
                {
                    buffer = new byte[size];
                    bufferSize = Int32ToUIntPtr(size);
                    res = getMethod(parameter, buffer, ref bufferSize);
                }
            }

            if (res != NativeCallResults.BufferResult.Succeeded)
            {
                result = null;
                return false;
            }

            result = buffer;
            return true;
        }

        public static byte[] StringToBytes(string value, Encoding encoding)
        {
            if (value == null)
                return null;

            var byteCount = encoding.GetByteCount(value);
            var result = new byte[byteCount + 1];      // null-terminated string
            var len = encoding.GetBytes(value, 0, value.Length, result, 0);
            System.Diagnostics.Debug.Assert(len == byteCount);

            return result;
        }

        public static bool IsAsciiCompatible(string value)
        {
            if (value == null)
                return true;
            foreach (var c in value)
                if (c > sbyte.MaxValue)
                    return false;
            return true;
        }

        public static byte[] FilePathNameToBytes(string path)
        {
            var isAsciiCompatible = IsAsciiCompatible(path);
            if (!isAsciiCompatible && Environment.OSVersion.Platform == PlatformID.Win32NT)
                throw new NotSupportedException("Non-ASCII characters in file paths are not supported in Windows version of library");
            var encoding = isAsciiCompatible ? Encoding.ASCII : Encoding.UTF8;
            return StringToBytes(path, encoding);
        }

        public static bool IsSubdirOf(DirectoryInfo subdir, DirectoryInfo parentDir)
        {
            for (; subdir != null; subdir = subdir.Parent)
                if (subdir.FullName.Equals(parentDir.FullName, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            return false;
        }

    }
}
