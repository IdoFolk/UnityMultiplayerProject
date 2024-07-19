using UnityEngine;

/// <summary>
/// A class that have reference to colors that represents a system or object log message color
/// </summary>
public static class ColorLogHelper
{
    public static string SetColor(this string message, Color color)
        => $"<color={ToRGBHex(color)}>{message}</color>";

    public static string SetColor(this string message, string hexCode)
        => $"<color={hexCode}>{message}/<color>";

    private static string ToRGBHex(Color c)
        => $"#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}";

    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
}