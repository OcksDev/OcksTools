using System;

public static class Extensions
{
    /// <summary>
    /// Retrieves a sub-array from the specified array. The sub-array starts at
    /// startIndex and has the specified length.
    /// Returns an empty array if the input is null/empty or the range is invalid.
    /// Returns the original array if the range covers the whole array.
    /// </summary>
    public static T[] SubArray<T>(this T[] array, int startIndex, int length)
    {
        int len;
        if (array == null || (len = array.Length) == 0)
            return new T[0];

        if (startIndex < 0 || length <= 0 || startIndex + length > len)
            return new T[0];

        if (startIndex == 0 && length == len)
            return array;

        var subArray = new T[length];
        Array.Copy(array, startIndex, subArray, 0, length);

        return subArray;
    }

    /// <summary>
    /// Same as above, but with long indices.
    /// </summary>
    public static T[] SubArray<T>(this T[] array, long startIndex, long length)
    {
        long len;
        if (array == null || (len = array.LongLength) == 0)
            return new T[0];

        if (startIndex < 0 || length <= 0 || startIndex + length > len)
            return new T[0];

        if (startIndex == 0 && length == len)
            return array;

        var subArray = new T[length];
        Array.Copy(array, startIndex, subArray, 0, length);

        return subArray;
    }

    /// <summary>
    /// Returns true if the string is null or has a length of 0.
    /// </summary>
    public static bool IsNullOrEmpty(this string value)
    {
        return value == null || value.Length == 0;
    }
}
