using UnityEngine;

public static class Debu
{
    public static void Log(params object[] objs)
    {
        //return;
        string str = objs[0].ToString();
        for (int i = 1; i < objs.Length; i++)
        {
            str += $", {objs[i]}";
        }
        Debug.Log(str);
    }
}