using System;

public static class ArrayExtensions
{
    public static T[] InsertIntoArray<T>(this T[] array, int index, T element)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));

        index = Math.Max(index, 0);
        index = Math.Min(index, array.Length);

        var result = new T[array.Length + 1];

        if (index > 0)
            Array.ConstrainedCopy(array, 0, result, 0, index);

        result[index] = element;

        if (index < array.Length)
            Array.ConstrainedCopy(array, index, result, index + 1, array.Length - index);

        return result;
    }
}