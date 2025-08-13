using UnityEngine;
using System.Collections.Generic;

public class TextLogger : MonoBehaviour
{
    private static TextLogger instance;
    private List<string> logs = new List<string>();

    public static TextLogger Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("TextLogger");
                instance = go.AddComponent<TextLogger>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Write(string text)
    {
        logs.Add(text);
    }

    public void WriteColored(string text, Color color)
    {
        string coloredText = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        logs.Add(coloredText);
    }

    public List<string> GetLogs()
    {
        return logs;
    }

    public void ClearLogs()
    {
        logs.Clear();
    }
}
