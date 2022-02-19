using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

public class Texture2DCreator : OdinEditorWindow {
    private Texture2DArray textureArray;

    [InlineEditor(InlineEditorModes.LargePreview)]
    public List<Texture2D> textures;

    [MenuItem("Window/Texture2DCreator")]
    public static void OpenWindow() {
        GetWindow(typeof(Texture2DCreator)).Show();
    }

    [Button("Save Texture2D Asset", ButtonSizes.Large), GUIColor(0.4f, 0.8f, 1)]
    private void SaveTexture2DArray() {
        const string path = "Assets/Resources/Textures/TextureArray.Asset";

        var t = textures[0];
        textureArray = new Texture2DArray(t.width, t.height, textures.Count, t.format, t.mipmapCount > 1) {
            anisoLevel = t.anisoLevel,
            filterMode = t.filterMode,
            wrapMode = t.wrapMode
        };

        for (var i = 0; i < textures.Count; i++) {
            for (var m = 0; m < t.mipmapCount; m++) {
                Graphics.CopyTexture(textures[i], 0, m, textureArray, i, m);
            }
        }

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(textureArray, path);

        AssetDatabase.Refresh();

        // blocksMaterial.SetTexture(Textures, textureArray);
    }
}
