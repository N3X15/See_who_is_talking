using System;
using System.Collections.Generic;
using UnityEngine;

namespace See_who_is_talking;

// Token: 0x02000009 RID: 9
public class VoiceDisplayManager : MonoBehaviour
{
    // Token: 0x04000009 RID: 9
    private const float displayDuration = 2f;

    // Token: 0x04000006 RID: 6
    private static VoiceDisplayManager? instance;

    // Token: 0x04000007 RID: 7
    private readonly object dictLock = new();

    // Token: 0x04000008 RID: 8
    private readonly Dictionary<string, float> speakingPlayers = new();

    // Token: 0x06000014 RID: 20 RVA: 0x000022BC File Offset: 0x000004BC
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // Token: 0x06000016 RID: 22 RVA: 0x000023D8 File Offset: 0x000005D8
    private void Update()
    {
        lock (dictLock)
        {
            List<string> keys = new(speakingPlayers.Keys);
            foreach (var key in keys)
                if (string.IsNullOrWhiteSpace(key))
                {
                    speakingPlayers.Remove(key);
                }
                else
                {
                    speakingPlayers[key] -= Time.deltaTime;
                    var flag3 = speakingPlayers[key] <= 0f;
                    if (flag3) speakingPlayers.Remove(key);
                }
        }
    }

    // Token: 0x06000017 RID: 23 RVA: 0x000024D4 File Offset: 0x000006D4
    private void OnGUI()
    {
        try
        {
            List<KeyValuePair<string, float>> displayList;
            lock (dictLock)
            {
                var flag2 = speakingPlayers.Count == 0;
                if (flag2) return;
                displayList = new List<KeyValuePair<string, float>>(speakingPlayers);
            }

            displayList.Reverse();
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 21,
                alignment = (TextAnchor)5,
                wordWrap = false,
                normal =
                {
                    textColor = Color.yellow
                }
            };
            var padding = 2f;
            var height = 40f;
            var yStart = Screen.height - height - 20f;
            var index = 0;
            foreach (KeyValuePair<string, float> kvp in displayList)
            {
                var player = kvp.Key;
                var timeLeft = kvp.Value;
                var flag3 = string.IsNullOrWhiteSpace(player);
                if (!flag3)
                {
                    var alpha = Mathf.Clamp01(timeLeft / 2f);
                    style.normal.textColor = new Color(1f, 0.8f, 0f, alpha);
                    var text = player + " is speaking...";
                    var textSize = style.CalcSize(new GUIContent(text));
                    var x = Screen.width - 20 - textSize.x;
                    var y = yStart - index * (height + padding);
                    GUI.Label(new Rect(x, y, textSize.x, height), text, style);
                    index++;
                }
            }
        }
        catch (Exception ex)
        {
            See_who_is_talking.Logger.LogError(string.Format("[VoiceDisplayManager.OnGUI] Exception: {0}", ex));
        }
    }

    // Token: 0x06000015 RID: 21 RVA: 0x0000230C File Offset: 0x0000050C
    public static void SetSpeakingPlayer(string name)
    {
        if (instance == null || string.IsNullOrWhiteSpace(name)) return;
        name = name.Trim();
        var obj = instance.dictLock;
        lock (obj)
        {
            var flag3 = string.IsNullOrWhiteSpace(name);
            if (flag3)
            {
                See_who_is_talking.Logger.LogWarning("SetSpeakingPlayer called with null or empty name.");
            }
            else
            {
                var flag4 = instance.speakingPlayers.ContainsKey(name);
                if (flag4)
                    instance.speakingPlayers[name] = 2f;
                else
                    instance.speakingPlayers.Add(name, 2f);
            }
        }
    }
}