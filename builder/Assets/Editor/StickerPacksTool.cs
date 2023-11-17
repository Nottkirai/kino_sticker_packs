﻿using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

// DO NOT EDIT THIS FILE
namespace Editor {
  public class StickerPacksTool : EditorWindow {
    private const float LINE_OFFSET = 5.0f;
    private const float PACKS_OFFSET = 5.0f;

    private readonly PacksToolCache cache_ = new PacksToolCache();
    private readonly PacksBuilder builder_ = new PacksBuilder();

    private Vector2 scrollPos_ = Vector2.zero;

    [MenuItem("Kino/Sticker packs tool")]
    public static void ShowWindow() {
      GetWindow(typeof(StickerPacksTool));
    }

    [MenuItem("Kino/Open sticker packs folder")]
    private static void OpenBuildFolder() {
      string buildPath = Path.Combine(Directory.GetCurrentDirectory(), PacksToolCache.BUILD_DIR);
      if (Directory.Exists(buildPath)) {
        Process.Start(buildPath);
      }
      else {
        Debug.LogError("Kino: Unable to open sticker packs folder, the folder doesn't exists");
      }
    }

    private void OnFocus() {
      cache_.Load();
    }

    private void OnGUI() {
      if (DrawAuthorMeta()) {
        cache_.Save();
      }

      DrawPacks();

      DrawCacheAndBuildActions();
    }

    private bool DrawAuthorMeta() {
      bool changed = false;

      GUILayout.Label("Author name:", EditorStyles.boldLabel);
      string prevName = cache_.Author.Name;
      cache_.Author.Name = EditorGUILayout.TextField(cache_.Author.Name);
      if (prevName != cache_.Author.Name) {
        changed = true;
      }

      GUILayout.Label("Author SteamID:", EditorStyles.boldLabel);
      ulong prevSid = cache_.Author.SteamId;
      cache_.Author.SteamId = (ulong)EditorGUILayout.LongField((long)cache_.Author.SteamId);
      if (prevSid != cache_.Author.SteamId) {
        changed = true;
      }

      GUILayout.Label("Author DiscordID:", EditorStyles.boldLabel);
      ulong prevDiscordId = cache_.Author.DiscordId;
      cache_.Author.DiscordId = (ulong)EditorGUILayout.LongField((long)cache_.Author.DiscordId);
      if (prevDiscordId != cache_.Author.DiscordId) {
        changed = true;
      }

      DrawHorizontalGUILine();

      return changed;
    }

    private void DrawPacks() {
      GUILayout.Label("Sticker packs to build:", EditorStyles.boldLabel);

      scrollPos_ = GUILayout.BeginScrollView(scrollPos_);
      foreach (var pack in cache_.Packs) {
        pack.SelectedToBuild = EditorGUILayout.Toggle(pack.Name, pack.SelectedToBuild);
        EditorGUILayout.Space(PACKS_OFFSET);
      }

      GUILayout.EndScrollView();
    }

    private void DrawCacheAndBuildActions() {
      DrawHorizontalGUILine();

      if (GUILayout.Button($"Build for '{EditorUserBuildSettings.activeBuildTarget}'")) {
        builder_.Build(EditorUserBuildSettings.activeBuildTarget, cache_);
      }

      EditorGUILayout.Space(PACKS_OFFSET);

      GUILayout.Label("Danger zone", EditorStyles.boldLabel);
      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Reload cache")) {
        cache_.Load();
      }

      if (GUILayout.Button("Wipe cache")) {
        cache_.Wipe();
      }

      EditorGUILayout.EndHorizontal();
    }

    private void DrawHorizontalGUILine(int height = 1) {
      GUILayout.Space(LINE_OFFSET);

      var rect = GUILayoutUtility.GetRect(1.0f, height, GUILayout.ExpandWidth(true));
      rect.height = height;
      rect.xMin = 0;
      rect.xMax = EditorGUIUtility.currentViewWidth;

      var lineColor = new Color32(0x19, 0x19, 0x19, 0xff);
      EditorGUI.DrawRect(rect, lineColor);
      GUILayout.Space(LINE_OFFSET);
    }
  }
}