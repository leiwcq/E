using System;

namespace E.Security
{
    /// <summary>
    /// Provides the CityHash string extensions.
    /// </summary>
    public static class CityHashExtension
    {

        /// <summary>
        /// Computes the 128-bit city hash for the specified string.
        /// This algorithm is tuned for strings of at least a few hundred bytes.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>The 128-bit city hash.</returns>
        /// <exception cref="ArgumentNullException">value</exception>
        /// <remarks>This function encodes the string using the unicode block (ISO/IEC 8859-1).</remarks>
        public static UInt128 GetCityHash128(this string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return CityHash.CityHash128(value);
        }

        /// <summary>
        /// Computes the 128-bit city hash using a specific <paramref name="seed" />. 
        /// This algorithm is tuned for strings of at least a few hundred bytes.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="seed">Specifies the seed for the CityHash algorithm.</param>
        /// <returns>The 128-bit city hash.</returns>
        /// <exception cref="ArgumentNullException">value</exception>
        /// <remarks>This function encodes the string using the unicode block (ISO/IEC 8859-1).</remarks>
        public static UInt128 GetCityHash128(this string value, UInt128 seed)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return CityHash.CityHash128(value, seed);
        }

        /// <summary>
        /// Computes the 32-bit city hash value for the specified string.
        /// </summary>
        /// <param name="value">The string to evaluate.</param>
        /// <returns>The computed 32-bit CityHash.</returns>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <remarks>This function encodes the string using the unicode block (ISO/IEC 8859-1).</remarks>
        public static uint GetCityHash32(this string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return CityHash.CityHash32(value);
        }

        /// <summary>
        /// Computes the 64-bit city hash value for the specified string.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <returns>The computed 64-bit CityHash.</returns>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <remarks>This function encodes the string using the unicode block (ISO/IEC 8859-1).</remarks>
        public static ulong GetCityHash64(this string value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return CityHash.CityHash64(value);
        }

        /// <summary>
        /// Computes the 64-bit city hash value for the specified string using a specific <paramref name="seed" />.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="seed">Specifies the seed for the CityHash algorithm.</param>
        /// <returns>The computed 64-bit CityHash.</returns>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <remarks>This function encodes the string using the unicode block (ISO/IEC 8859-1).</remarks>
        public static ulong GetCityHash64(this string value, ulong seed)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return CityHash.CityHash64(value, seed);
        }

        /// <summary>
        /// Computes the 64-bit city hash value for the specified string using a low and high order 64-bit seeds.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="seed0">The low-order 64-bits seed used by the algorithm.</param>
        /// <param name="seed1">The high-order 64-bits seed used by the algorithm.</param>
        /// <returns>The computed 64-bit city hash.</returns>
        /// <exception cref="System.ArgumentNullException">value</exception>
        /// <remarks>This function encodes the string using the unicode block (ISO/IEC 8859-1).</remarks>
        public static ulong GetCityHash64(this string value, ulong seed0, ulong seed1)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            return CityHash.CityHash64(value, seed0, seed1);
        }
    }
}
