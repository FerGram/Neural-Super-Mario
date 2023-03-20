using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level
{
    public int[] levelLayout;

    private string layoutString = "";

    public Level(int[] levelLayout)
    {
        this.levelLayout = (int[]) levelLayout.Clone();

        for (int i = 0; i < this.levelLayout.Length; i++)
        {
            layoutString += this.levelLayout[i].ToString();
        }
    }

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (obj is not Level) return false;

        Level level = obj as Level;
        return this.layoutString == level.layoutString;
    }

    public override int GetHashCode()
    {
        return layoutString.GetHashCode();
    }
}
