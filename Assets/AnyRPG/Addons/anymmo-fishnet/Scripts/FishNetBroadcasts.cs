using FishNet.Broadcast;

namespace AnyRPG
{
    public struct PasswordBroadcast : IBroadcast
    {
        public string Username;
        public string Password;
        public string ClientVersion;
    }

    public struct ResponseBroadcast : IBroadcast
    {
        public int AccountId;
        public bool AuthenticationPassed;
        public bool ClientPassed;
        public string RequiredClientVersion;
        public NetworkServerMode ClientMode;
        // 정지 · 점검으로 막힌 까닭 — 비어 있으면 실패 창 기본 문구
        public string FailureMessage;
    }

}