﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// DO NOT EDIT THIS FILE
namespace Editor {
  [Serializable]
  public class StickerMeta {
    [Serializable]
    public class Proxy {
      public string ImageName;
      public bool SymmetricalFlip;
    }

    [HideInInspector]
    public string name;

    public Texture2D Image;
    public bool SymmetricalFlip;

    public void Validate() {
      name = Image ? Image.name : "unknown";
    }
  }

  [CreateAssetMenu(fileName = "__meta", menuName = "Kino/Create sticker pack meta", order = 1)]
  public class PackMeta : ScriptableObject {
    [Serializable]
    public class Proxy {
      public string AuthorName;
      public string Name;
      public string Description;
      public int Version;

      public string IconName;
      public List<StickerMeta.Proxy> Images;
    }

    internal const string PACK_META_RESERVED_NAME = "__pack_meta.txt";
    internal const string PACK_ICON_RESERVED_NAME = "__pack_icon.png";

    public bool SelectedToBuild;
    public string Name = "sticker_pack";
    [TextArea(4, 20)]
    public string Description = string.Empty;
    public string EncryptionKey;
    public int Version = 100;

    public Texture2D PackIcon;

    public List<StickerMeta> Stickers;

    public Proxy GetProxyMeta() {
      var meta = new Proxy {
        Name = Name,
        Version = Version,
        IconName = PackIcon ? PackIcon.name : "",
        Description = Description,
        Images = new List<StickerMeta.Proxy>()
      };

      if (Stickers == null) {
        return null;
      }

      foreach (var s in Stickers) {
        var stickerMeta = new StickerMeta.Proxy {
          ImageName = s.Image ? s.Image.name : "",
          SymmetricalFlip = s.SymmetricalFlip
        };

        if (string.IsNullOrWhiteSpace(stickerMeta.ImageName)) {
          continue;
        }

        meta.Images.Add(stickerMeta);
      }

      return meta;
    }

    public void RefreshAssets() {
      string rootFolder = GetRoot();
      if (string.IsNullOrWhiteSpace(rootFolder)) {
        return;
      }

      string[] files = Directory.GetFiles(rootFolder, "*.png", SearchOption.TopDirectoryOnly);

      Debug.Log($"Kino: Found {files.Length} images, processing");

      int removed = Stickers.RemoveAll(meta => !meta.Image);
      if (removed != 0) {
        Debug.Log($"Kino: Removed {removed} broken images");
      }

      foreach (var filePath in files) {
        string fileName = Path.GetFileName(filePath);
        if (fileName == PACK_ICON_RESERVED_NAME) {
          continue;
        }

        string imagePath = Path.Combine(rootFolder, fileName);

        var sticker = new StickerMeta {
          Image = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath)
        };

        int index = Stickers.FindIndex(s => s.Image.name == sticker.Image.name);
        if (index == -1) {
          Debug.Log($"Kino: Added new image '{sticker.Image.name}'");
          Stickers.Add(sticker);
        }
      }
    }

    public void SetAllStickersSymmetry(bool symmetry) {
      if (Stickers == null) {
        return;
      }

      Debug.Log($"Kino: Set symmetry for all layers to {symmetry}");

      foreach (var sticker in Stickers) {
        sticker.SymmetricalFlip = symmetry;
      }
    }

    public static IEnumerable<PackMeta> GetAllInstances() {
      var guids = AssetDatabase.FindAssets($"t:{nameof(PackMeta)}");

      var packsList = new List<PackMeta>();

      foreach (var guid in guids) {
        string path = AssetDatabase.GUIDToAssetPath(guid);
        var meta = AssetDatabase.LoadAssetAtPath<PackMeta>(path);

        if (meta) {
          packsList.Add(meta);
        }
      }

      return packsList;
    }

    internal string GetRoot() {
      string assetPath = AssetDatabase.GetAssetPath(this);
      var rootFolder = Path.GetDirectoryName(assetPath);
      if (string.IsNullOrWhiteSpace(rootFolder)) {
        Debug.LogError("Kino: Unable to get current asset directory");
        return null;
      }

      return rootFolder;
    }

    private void OnValidate() {
      if (Stickers == null) {
        return;
      }

      foreach (var s in Stickers) {
        s.Validate();
      }
    }
  }

  [CustomEditor(typeof(PackMeta))]
  public class PackMetaEditor : UnityEditor.Editor {
    public override void OnInspectorGUI() {
      base.OnInspectorGUI();
      var script = (PackMeta)target;

      if (GUILayout.Button("Refresh assets")) {
        script.RefreshAssets();
      }

      GUILayout.BeginHorizontal();

      if (GUILayout.Button("Set symmetry for all to TRUE")) {
        script.SetAllStickersSymmetry(true);
      }

      if (GUILayout.Button("Set symmetry for all to FALSE")) {
        script.SetAllStickersSymmetry(false);
      }

      GUILayout.EndHorizontal();
    }
  }
}