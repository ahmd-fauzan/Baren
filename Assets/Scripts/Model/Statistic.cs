using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statistic : ScriptableObject
{
    private int win;
    private int lose;
    private int draw;

    public Statistic() { }

    public Statistic(int win, int lose, int draw) 
    {
        this.win = win;
        this.lose = lose;
        this.draw = draw;
    }

    public int Win
    {
        get
        {
            return this.win;
        }

        set
        {
            this.win = value;
        }
    }

    public int Lose
    {
        get
        {
            return this.lose;
        }

        set
        {
            this.lose = value;
        }
    }

    public int Draw
    {
        get
        {
            return this.draw;
        }

        set
        {
            this.draw = value;
        }
    }
}
