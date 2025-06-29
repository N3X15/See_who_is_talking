using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace See_who_is_talking
{
	// Token: 0x02000009 RID: 9
	public class VoiceDisplayManager : MonoBehaviour
	{
		// Token: 0x06000014 RID: 20 RVA: 0x000022BC File Offset: 0x000004BC
		private void Awake()
		{
			if (VoiceDisplayManager.instance != null && VoiceDisplayManager.instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				VoiceDisplayManager.instance = this;
				UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x0000230C File Offset: 0x0000050C
		public static void SetSpeakingPlayer(string name)
		{
			if (VoiceDisplayManager.instance == null || string.IsNullOrWhiteSpace(name)) return;
			name = name.Trim();
			object obj = VoiceDisplayManager.instance.dictLock;
			lock (obj)
			{
				bool flag3 = string.IsNullOrWhiteSpace(name);
				if (flag3)
				{
					See_who_is_talking.Logger.LogWarning("SetSpeakingPlayer called with null or empty name.");
				}
				else
				{
					bool flag4 = VoiceDisplayManager.instance.speakingPlayers.ContainsKey(name);
					if (flag4)
					{
						VoiceDisplayManager.instance.speakingPlayers[name] = 2f;
					}
					else
					{
						VoiceDisplayManager.instance.speakingPlayers.Add(name, 2f);
					}
				}
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x000023D8 File Offset: 0x000005D8
		private void Update()
		{
			lock (dictLock)
			{
				List<string> keys = new List<string>(this.speakingPlayers.Keys);
				foreach (string key in keys)
				{
					if (string.IsNullOrWhiteSpace(key))
					{
						this.speakingPlayers.Remove(key);
					}
					else
					{
						speakingPlayers[key] -= Time.deltaTime;
						bool flag3 = this.speakingPlayers[key] <= 0f;
						if (flag3)
						{
							this.speakingPlayers.Remove(key);
						}
					}
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
					bool flag2 = this.speakingPlayers.Count == 0;
					if (flag2)
					{
						return;
					}
					displayList = new List<KeyValuePair<string, float>>(this.speakingPlayers);
				}
				displayList.Reverse();
				GUIStyle style = new GUIStyle(GUI.skin.label)
				{
					fontSize = 21,
					alignment = (UnityEngine.TextAnchor)5,
					wordWrap = false,
					normal = 
					{
						textColor = Color.yellow
					}
				};
				float padding = 2f;
				float height = 40f;
				float yStart = (float)Screen.height - height - 20f;
				int index = 0;
				foreach (KeyValuePair<string, float> kvp in displayList)
				{
					string player = kvp.Key;
					float timeLeft = kvp.Value;
					bool flag3 = string.IsNullOrWhiteSpace(player);
					if (!flag3)
					{
						float alpha = Mathf.Clamp01(timeLeft / 2f);
						style.normal.textColor = new Color(1f, 0.8f, 0f, alpha);
						string text = player + " is speaking...";
						Vector2 textSize = style.CalcSize(new GUIContent(text));
						float x = (float)(Screen.width - 20) - textSize.x;
						float y = yStart - (float)index * (height + padding);
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

		// Token: 0x04000006 RID: 6
		private static VoiceDisplayManager? instance;

		// Token: 0x04000007 RID: 7
		private readonly object dictLock = new object();

		// Token: 0x04000008 RID: 8
		private readonly Dictionary<string, float> speakingPlayers = new Dictionary<string, float>();

		// Token: 0x04000009 RID: 9
		private const float displayDuration = 2f;
	}
}
