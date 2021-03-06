﻿using System;
using Microsoft.Extensions.Primitives;

namespace E.Common.Support
{
    public class HashedStringSegment
    {
        public StringSegment Value { get; }
        private readonly int _hash;

        public HashedStringSegment(StringSegment value)
        {
            Value = value;
            _hash = ComputeHashCode(value);
        }

        public HashedStringSegment(string value) : this(new StringSegment(value))
        {
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(((HashedStringSegment)obj).Value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => _hash;

        public static int ComputeHashCode(StringSegment value)
        {
            var length = value.Length;
            if (length == 0)
                return 0;

            var offset = value.Offset;
            var hash = 37 * length;

            char c1 = Char.ToUpperInvariant(value.Buffer[offset]);
            hash += 53 * c1;

            if (length > 1)
            {
                char c2 = Char.ToUpperInvariant(value.Buffer[offset + length - 1]);
                hash += 37 * c2;
            }

            return hash;
        }
    }
}