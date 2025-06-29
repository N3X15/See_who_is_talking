using HarmonyLib;
using Photon.Pun;
using Photon.Voice.Unity;

namespace See_who_is_talking;

// Token: 0x02000008 RID: 8
[HarmonyPatch(typeof(Speaker), "OnAudioFrame")]
internal class SpeakerPatch
{
    // Token: 0x06000012 RID: 18 RVA: 0x0000220C File Offset: 0x0000040C
    // ReSharper disable once InconsistentNaming
    private static void Prefix(Speaker __instance)
    {
        if (__instance == null || __instance.RemoteVoice == null) return;
        var playerId = __instance.RemoteVoice.PlayerId;
        string? name;
        try
        {
            var currentRoom = PhotonNetwork.CurrentRoom;
            var player = currentRoom?.GetPlayer(playerId);
            name = player?.NickName;
            if (string.IsNullOrWhiteSpace(name)) name = string.Format("Player {0}", playerId);
        }
        catch
        {
            name = string.Format("Player {0}", playerId);
        }

        VoiceDisplayManager.SetSpeakingPlayer(name);
    }
}