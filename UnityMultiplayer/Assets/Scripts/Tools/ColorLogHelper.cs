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

    public static string ToRGBHex(this Color c)
        => $"#{ToByte(c.r):X2}{ToByte(c.g):X2}{ToByte(c.b):X2}";

    public static Color FromHexToColor(this string hexCode)
    {
        if (hexCode.Length != 7 || hexCode[0] != '#')
            throw new System.ArgumentException("Invalid hex code");
        return new Color(
            byte.Parse(hexCode.Substring(1, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
            byte.Parse(hexCode.Substring(3, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
            byte.Parse(hexCode.Substring(5, 2), System.Globalization.NumberStyles.HexNumber) / 255f
        );
        
    }
    private static byte ToByte(float f)
    {
        f = Mathf.Clamp01(f);
        return (byte)(f * 255);
    }
}