using UnityEngine;

public static class NumberFormatter
{
    private static readonly string[] suffixes = { "", "K", "M", "B", "T", "aa", "ab", "ac" };

    public static string Format(float number)
    {
        if (number < 1000)
        {
            return Mathf.RoundToInt(number).ToString();
        }

        int magnitude = 0;
        float reducedNumber = number;

        while (reducedNumber >= 1000 && magnitude < suffixes.Length - 1)
        {
            reducedNumber /= 1000f;
            magnitude++;
        }

        if (reducedNumber >= 100)
        {
            return Mathf.FloorToInt(reducedNumber).ToString() + suffixes[magnitude];
        }
        else if (reducedNumber >= 10)
        {
            return reducedNumber.ToString("F1") + suffixes[magnitude];
        }
        else
        {
            return reducedNumber.ToString("F2") + suffixes[magnitude];
        }
    }

    public static string FormatInt(int number)
    {
        return Format(number);
    }

    public static string FormatLong(long number)
    {
        if (number < 1000)
        {
            return number.ToString();
        }

        int magnitude = 0;
        double reducedNumber = number;

        while (reducedNumber >= 1000 && magnitude < suffixes.Length - 1)
        {
            reducedNumber /= 1000.0;
            magnitude++;
        }

        if (reducedNumber >= 100)
        {
            return System.Math.Floor(reducedNumber).ToString() + suffixes[magnitude];
        }
        else if (reducedNumber >= 10)
        {
            return reducedNumber.ToString("F1") + suffixes[magnitude];
        }
        else
        {
            return reducedNumber.ToString("F2") + suffixes[magnitude];
        }
    }
}

