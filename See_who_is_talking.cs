using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace See_who_is_talking;

// Token: 0x02000007 RID: 7
[BepInPlugin("XiaoLei_See_who_is_talking", "See_who_is_talking", "1.0")]
public class See_who_is_talking : BaseUnityPlugin
{
    // Token: 0x17000001 RID: 1
    // (get) Token: 0x06000008 RID: 8 RVA: 0x000020D4 File Offset: 0x000002D4
    // (set) Token: 0x06000009 RID: 9 RVA: 0x000020DB File Offset: 0x000002DB
    internal static See_who_is_talking Instance { get; private set; }

    // Token: 0x17000002 RID: 2
    // (get) Token: 0x0600000A RID: 10 RVA: 0x000020E3 File Offset: 0x000002E3
    internal static ManualLogSource Logger => Instance._logger;

    // Token: 0x17000003 RID: 3
    // (get) Token: 0x0600000B RID: 11 RVA: 0x000020EF File Offset: 0x000002EF
    private ManualLogSource _logger => base.Logger;

    // Token: 0x17000004 RID: 4
    // (get) Token: 0x0600000C RID: 12 RVA: 0x000020F7 File Offset: 0x000002F7
    // (set) Token: 0x0600000D RID: 13 RVA: 0x000020FF File Offset: 0x000002FF
    internal Harmony? Harmony { get; set; }

    // Token: 0x0600000E RID: 14 RVA: 0x00002108 File Offset: 0x00000308
    private void Awake()
    {
        Instance = this;
        gameObject.transform.parent = null;
        gameObject.hideFlags = HideFlags.HideAndDontSave;
        if (FindObjectOfType<VoiceDisplayManager>() == null)
        {
            var go = new GameObject("VoiceDisplay");
            DontDestroyOnLoad(go);
            go.hideFlags = HideFlags.HideAndDontSave;
            go.AddComponent<VoiceDisplayManager>();
        }

        Patch();
        Logger.LogInfo(string.Format("{0} v{1} has loaded!", Info.Metadata.GUID, Info.Metadata.Version));
    }

    // Token: 0x0600000F RID: 15 RVA: 0x000021AC File Offset: 0x000003AC
    internal void Patch()
    {
        if (Harmony == null) Harmony = new Harmony(Info.Metadata.GUID);
        Harmony.PatchAll();
    }

    // Token: 0x06000010 RID: 16 RVA: 0x000021EC File Offset: 0x000003EC
    internal void Unpatch()
    {
        var harmony = Harmony;
        if (harmony != null) harmony.UnpatchSelf();
    }
}