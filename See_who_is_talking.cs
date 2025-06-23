using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Photon.Voice.Unity;
using System.Collections.Generic;
using UnityEngine;

namespace See_who_is_talking
{
    [BepInPlugin("XiaoLei_See_who_is_talking", "See_who_is_talking", "1.0")]
    public class See_who_is_talking : BaseUnityPlugin
    {
        internal static See_who_is_talking Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;
        internal Harmony? Harmony { get; set; }

        private void Awake()
        {
            Instance = this;

            // Prevent the plugin from being deleted
            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            var go = new GameObject("VoiceDisplay");
            GameObject.DontDestroyOnLoad(go); // 不随场景销毁
            go.hideFlags = HideFlags.HideAndDontSave; // 隐藏
            go.AddComponent<VoiceDisplayManager>();

            Patch();
            Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        }

        internal void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
        }

        private void Update()
        {
            // Code that runs every frame goes here
        }
    }
    [HarmonyPatch(typeof(Speaker), "OnAudioFrame")]
    class SpeakerPatch
    {
        static void Prefix(Speaker __instance)
        {
            var remote = __instance.RemoteVoice;
            if (remote == null) return;

            int playerId = remote.PlayerId;
            var player = Photon.Pun.PhotonNetwork.CurrentRoom?.GetPlayer(playerId);
            string name = player?.NickName ?? $"Player {playerId}";

            //MyPlugin1.Logger.LogInfo($"[Voice] {name} is speaking.");

            // 你可以在这里通知 UI 显示：
            VoiceDisplayManager.SetSpeakingPlayer(name);
        }
    }
    public class VoiceDisplayManager : MonoBehaviour
    {
        private static VoiceDisplayManager instance;

        // 记录玩家说话状态和剩余时间
        private Dictionary<string, float> speakingPlayers = new Dictionary<string, float>();

        private const float displayDuration = 2f; // 显示2秒后渐隐消失

        void Awake()
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            if (FindObjectsOfType<VoiceDisplayManager>().Length > 1)
            {
                Destroy(this.gameObject);
                return;
            }
        }

        public static void SetSpeakingPlayer(string name)
        {
            if (instance == null) return;

            // 如果玩家已经在字典里，刷新显示时间，否则添加
            if (instance.speakingPlayers.ContainsKey(name))
                instance.speakingPlayers[name] = displayDuration;
            else
                instance.speakingPlayers.Add(name, displayDuration);
        }

        void Update()
        {
            // 每帧减少剩余时间，到0则移除
            var keys = new List<string>(speakingPlayers.Keys);
            foreach (var key in keys)
            {
                speakingPlayers[key] -= Time.deltaTime;
                if (speakingPlayers[key] <= 0)
                    speakingPlayers.Remove(key);
            }
        }

        void OnGUI()
        {
            try
            {
                if (speakingPlayers.Count == 0) return;

                // 拷贝键值对列表，避免遍历期间被修改
                List<KeyValuePair<string, float>> displayList = new List<KeyValuePair<string, float>>(speakingPlayers);

                var style = new GUIStyle(GUI.skin.label);
                style.fontSize = 21;
                style.alignment = TextAnchor.MiddleRight; // 右对齐文字
                style.wordWrap = false;

                float padding = 2;
                float height = 40;
                float yStart = Screen.height - height - 20;

                int index = 0;

                // 倒序显示，最新说话的在下方
                displayList.Reverse();

                foreach (var kvp in displayList)
                {
                    string player = kvp.Key;
                    float timeLeft = kvp.Value;

                    float alpha = Mathf.Clamp01(timeLeft / displayDuration);
                    style.normal.textColor = new Color(1f, 0.8f, 0f, alpha);

                    string text = $"{player} is speaking...";

                    Vector2 textSize = style.CalcSize(new GUIContent(text));

                    float x = Screen.width - 20 - textSize.x;
                    float y = yStart - index * (height + padding);

                    GUI.Label(new Rect(x, y, textSize.x, height), text, style);

                    index++;
                }
            }
            catch (System.Exception ex)
            {
                //See_who_is_talking.Logger.LogError($"[VoiceDisplayManager.OnGUI] Exception: {ex}");
            }
        }
    }
}