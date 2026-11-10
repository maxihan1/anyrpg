// 게임에 들어가면 운영툴 공지를 API 에서 읽어 화면 알림 · 채팅 창에 띄운다 — 들어가 있는 동안 1분마다 새 공지만 더 띄운다
using System.Collections;
using System.Collections.Generic;
using AnyRPG;
using UnityEngine;
using UnityEngine.Networking;

public class ServerNotice : MonoBehaviour {
    // ponytail: 앱이 아는 API 주소가 없어 박아 둔다. 서버를 여럿 두게 되면 SystemConfigurationManager 칸으로 옮긴다.
    private const string StatusUrl = "https://anyrpg-api.maxihan.com/api/serverstatus";
    private const float PollSeconds = 60f;

    [System.Serializable]
    private class Notice {
        public int id;
        public string title;
        public string body;
    }

    [System.Serializable]
    private class Status {
        public List<Notice> notices;
    }

    private readonly HashSet<int> shown = new HashSet<int>();
    private SystemGameManager systemGameManager;
    private Coroutine polling;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create() {
        // 헤드리스 게임 서버에는 띄울 화면이 없다.
        if (Application.isBatchMode) return;
        var go = new GameObject("ServerNotice");
        DontDestroyOnLoad(go);
        go.AddComponent<ServerNotice>();
    }

    private void Start() {
        systemGameManager = FindAnyObjectByType<SystemGameManager>();
        SystemEventManager.StartListening("OnPlayerConnectionSpawn", HandleSpawn);
        SystemEventManager.StartListening("OnPlayerConnectionDespawn", HandleDespawn);
    }

    private void OnDestroy() {
        SystemEventManager.StopListening("OnPlayerConnectionSpawn", HandleSpawn);
        SystemEventManager.StopListening("OnPlayerConnectionDespawn", HandleDespawn);
    }

    private void HandleSpawn(string eventName, EventParamProperties eventParamProperties) {
        // 들어갈 때 채팅 창이 비워지므로 지금 걸린 공지를 다시 다 띄운다.
        shown.Clear();
        if (polling != null) StopCoroutine(polling);
        polling = StartCoroutine(Poll());
    }

    private void HandleDespawn(string eventName, EventParamProperties eventParamProperties) {
        if (polling != null) StopCoroutine(polling);
        polling = null;
    }

    private IEnumerator Poll() {
        while (true) {
            using (UnityWebRequest request = UnityWebRequest.Get(StatusUrl)) {
                yield return request.SendWebRequest();
                if (request.result == UnityWebRequest.Result.Success) {
                    Show(JsonUtility.FromJson<Status>(request.downloadHandler.text));
                } else {
                    // 공지를 못 읽어도 게임은 계속한다 — 다음 차례에 다시 읽는다.
                    Debug.LogWarning($"ServerNotice: {StatusUrl} {request.responseCode} {request.error}");
                }
            }
            yield return new WaitForSeconds(PollSeconds);
        }
    }

    private void Show(Status status) {
        if (status?.notices == null) return;
        foreach (Notice notice in status.notices) {
            if (!shown.Add(notice.id)) continue;
            systemGameManager.UIManager.MessageFeedManager.WriteMessage($"[Notice] {notice.title}");
            systemGameManager.MessageLogClient.WriteSystemMessage($"[Notice] {notice.title}\n{notice.body}");
        }
    }
}
