// 시연용 AnyMMO 데모를 명령줄에서 Android QA APK · Linux 헤드리스 게임 서버로 굽는 빌드 진입점
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Profile;
using UnityEditor.Build.Reporting;

public static class ShowcaseBuild {
    // 장면 목록은 애드온이 들고 오는 빌드 프로필을 그대로 따른다 — 애드온이 장면을 더해도 여기를 안 고쳐도 되게.
    private const string ProfilePath = "Assets/AnyRPG/Addons/anymmo-fishnet/Build Profiles/AnyMMO Demo Game.asset";

    // control_cli instrument 를 돌린 뒤 부른다 — CONTROL_CLI 가 있어야 SDK 코드가 빌드에 들어간다.
    public static void AndroidQa() {
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.maxihan.anymmo");
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        Build(new BuildPlayerOptions {
            scenes = Scenes(),
            target = BuildTarget.Android,
            locationPathName = "Builds/android/anymmo-qa.apk",
            extraScriptingDefines = new[] { "CONTROL_CLI" },
        });
    }

    public static void LinuxServer() {
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Server, ScriptingImplementation.Mono2x);
        EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
        Build(new BuildPlayerOptions {
            scenes = Scenes(),
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            locationPathName = "Builds/linux-server/AnyMMOServer.x86_64",
        });
    }

    private static string[] Scenes() {
        var profile = AssetDatabase.LoadAssetAtPath<BuildProfile>(ProfilePath);
        if (profile == null) throw new InvalidOperationException("빌드 프로필이 없습니다: " + ProfilePath);
        return profile.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }

    // 배치 모드에서 예외를 던지면 Unity 가 종료 코드 1 로 끝난다 — 빌드 스크립트가 실패를 알아채게.
    private static void Build(BuildPlayerOptions options) {
        var summary = BuildPipeline.BuildPlayer(options).summary;
        if (summary.result != BuildResult.Succeeded)
            throw new InvalidOperationException($"빌드 실패({options.target}): {summary.result}, 오류 {summary.totalErrors}건");
    }
}
