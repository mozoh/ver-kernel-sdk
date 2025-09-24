#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace com.anotherworld.kernel {
    public class JsonConfig : MonoBehaviour {
        public TextAsset Json;

        private void Apply() {
            if (!Json) {
                return;
            }

            var jsonContent = MinifyJson(Json.text);
            var match = Regex.Match(gameObject.name, @"\[(.*?)\]");
            var extractedName = match.Success ? match.Groups[1].Value : "NoName";
            var newName = $"[{extractedName}] {jsonContent}";

            if (gameObject.name == newName) {
                return;
            }

            gameObject.name = newName;

            EditorUtility.SetDirty(gameObject);
        }

        private static string MinifyJson(string json) {
            var result = "";
            var inString = false;

            foreach (var c in json) {
                if (c == '\"') {
                    result += c;
                    inString = !inString;
                } else if (inString || !char.IsWhiteSpace(c)) {
                    result += c;
                }
            }

            return result;
        }

        [PostProcessScene(2)]
        public static void OnPostprocessScene() {
            foreach (var jsonConfig in FindObjectsOfType<JsonConfig>()) {
                jsonConfig.Apply();
                DestroyImmediate(jsonConfig);
            }
        }
    }
}
#endif