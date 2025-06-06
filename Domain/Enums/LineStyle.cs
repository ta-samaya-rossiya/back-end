﻿namespace Domain.Enums;

public enum LineStyle
{
    Solid = 0,
    Dashed = 1,
    Dotted = 2
}

public static class LineStyleExtensions
{
    public static string ToResponseString(this LineStyle lineStyle)
    {
        return lineStyle switch
        {
            LineStyle.Solid => "solid",
            LineStyle.Dotted => "dotted",
            LineStyle.Dashed => "dashed",
            _ => throw new ArgumentOutOfRangeException(nameof(lineStyle), lineStyle, null)
        };
    }

    public static LineStyle ParseFromString(string str)
    {
        return str switch
        {
            "solid" => LineStyle.Solid,
            "dotted" => LineStyle.Dotted,
            "dashed" => LineStyle.Dashed,
            _ => LineStyle.Solid
        };
    }
}