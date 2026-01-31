using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SDLMMSharp.Babylon3D
{
    /// <summary>
    /// Enum for scene format types.
    /// </summary>
    public enum SceneFormat
    {
        Scene2D = 0,
        Scene3D = 1
    }

    /// <summary>
    /// Simple JSON token types.
    /// </summary>
    internal enum JsonTokenType
    {
        None,
        ObjectStart,
        ObjectEnd,
        ArrayStart,
        ArrayEnd,
        Colon,
        Comma,
        String,
        Number,
        True,
        False,
        Null
    }

    /// <summary>
    /// Simple JSON parser for scene loading. Does not depend on System.Text.Json.
    /// </summary>
    internal class SimpleJsonParser
    {
        private string json;
        private int pos;

        public SimpleJsonParser(string jsonString)
        {
            json = jsonString;
            pos = 0;
        }

        private void SkipWhitespace()
        {
            while (pos < json.Length && char.IsWhiteSpace(json[pos]))
                pos++;
        }

        private string ReadString()
        {
            if (json[pos] != '"') return null;
            pos++; // skip opening quote
            
            StringBuilder sb = new StringBuilder();
            while (pos < json.Length)
            {
                char c = json[pos++];
                if (c == '"') break;
                if (c == '\\' && pos < json.Length)
                {
                    c = json[pos++];
                    switch (c)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        default: sb.Append(c); break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        private object ReadValue()
        {
            SkipWhitespace();
            if (pos >= json.Length) return null;

            char c = json[pos];
            if (c == '"')
            {
                return ReadString();
            }
            else if (c == '{')
            {
                return ReadObject();
            }
            else if (c == '[')
            {
                return ReadArray();
            }
            else if (c == 't' && json.Substring(pos, 4) == "true")
            {
                pos += 4;
                return true;
            }
            else if (c == 'f' && json.Substring(pos, 5) == "false")
            {
                pos += 5;
                return false;
            }
            else if (c == 'n' && json.Substring(pos, 4) == "null")
            {
                pos += 4;
                return null;
            }
            else if (c == '-' || char.IsDigit(c))
            {
                return ReadNumber();
            }
            return null;
        }

        private double ReadNumber()
        {
            int start = pos;
            if (json[pos] == '-') pos++;
            while (pos < json.Length && (char.IsDigit(json[pos]) || json[pos] == '.' || json[pos] == 'e' || json[pos] == 'E' || json[pos] == '+' || json[pos] == '-'))
            {
                if ((json[pos] == '+' || json[pos] == '-') && pos > start && json[pos - 1] != 'e' && json[pos - 1] != 'E')
                    break;
                pos++;
            }
            string numStr = json.Substring(start, pos - start);
            double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out double result);
            return result;
        }

        public Dictionary<string, object> ReadObject()
        {
            var result = new Dictionary<string, object>();
            SkipWhitespace();
            if (json[pos] != '{') return null;
            pos++; // skip {

            SkipWhitespace();
            if (pos < json.Length && json[pos] == '}')
            {
                pos++;
                return result;
            }

            while (pos < json.Length)
            {
                SkipWhitespace();
                if (json[pos] != '"') break;
                string key = ReadString();
                
                SkipWhitespace();
                if (json[pos] != ':') break;
                pos++; // skip :
                
                object value = ReadValue();
                result[key] = value;
                
                SkipWhitespace();
                if (pos >= json.Length) break;
                if (json[pos] == '}')
                {
                    pos++;
                    break;
                }
                if (json[pos] == ',') pos++;
            }
            return result;
        }

        private List<object> ReadArray()
        {
            var result = new List<object>();
            SkipWhitespace();
            if (json[pos] != '[') return null;
            pos++; // skip [

            SkipWhitespace();
            if (pos < json.Length && json[pos] == ']')
            {
                pos++;
                return result;
            }

            while (pos < json.Length)
            {
                object value = ReadValue();
                result.Add(value);
                
                SkipWhitespace();
                if (pos >= json.Length) break;
                if (json[pos] == ']')
                {
                    pos++;
                    break;
                }
                if (json[pos] == ',') pos++;
            }
            return result;
        }
    }

    /// <summary>
    /// Utility class for loading 3D scenes from JSON files.
    /// </summary>
    public static class SceneLoader
    {
        private static float GetFloat(Dictionary<string, object> dict, string key, float defaultValue = 0)
        {
            if (dict != null && dict.TryGetValue(key, out object val) && val is double d)
                return (float)d;
            return defaultValue;
        }

        private static int GetInt(Dictionary<string, object> dict, string key, int defaultValue = 0)
        {
            if (dict != null && dict.TryGetValue(key, out object val) && val is double d)
                return (int)d;
            return defaultValue;
        }

        private static string GetString(Dictionary<string, object> dict, string key, string defaultValue = null)
        {
            if (dict != null && dict.TryGetValue(key, out object val) && val is string s)
                return s;
            return defaultValue;
        }

        private static float[] GetFloatArray(Dictionary<string, object> dict, string key, float[] defaultValue = null)
        {
            if (dict != null && dict.TryGetValue(key, out object val) && val is List<object> list)
            {
                float[] arr = new float[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is double d)
                        arr[i] = (float)d;
                }
                return arr;
            }
            return defaultValue;
        }

        private static Vector3 GetVector3(Dictionary<string, object> dict, string key, Vector3 defaultValue)
        {
            float[] arr = GetFloatArray(dict, key);
            if (arr != null && arr.Length >= 3)
                return new Vector3(arr[0], arr[1], arr[2]);
            return defaultValue;
        }

        /// <summary>
        /// Get the scene format from a JSON file.
        /// </summary>
        public static SceneFormat GetSceneFormat(string filename)
        {
            try
            {
                string json = File.ReadAllText(filename);
                var parser = new SimpleJsonParser(json);
                var root = parser.ReadObject();
                return (SceneFormat)GetInt(root, "format", 0);
            }
            catch
            {
                return SceneFormat.Scene2D;
            }
        }

        /// <summary>
        /// Load a 3D scene from a JSON file and create a Babylon3DScene.
        /// </summary>
        public static Babylon3DScene LoadSceneFromFile(SDLMMSharp.Engine.BaseEngine engine, string filename)
        {
            try
            {
                string basePath = Path.GetDirectoryName(filename) ?? "";
                string json = File.ReadAllText(filename);
                var parser = new SimpleJsonParser(json);
                var root = parser.ReadObject();

                if (root == null) return null;

                int format = GetInt(root, "format", 0);
                if (format != 1) return null; // Only 3D scenes supported

                int width = GetInt(root, "width", 800);
                int height = GetInt(root, "height", 600);
                int bgColor = GetInt(root, "backgroundColor", 0);

                var scene = new Babylon3DScene(engine, width, height);
                scene.BackgroundColor = bgColor;

                // Load camera
                if (root.TryGetValue("camera", out object camObj) && camObj is Dictionary<string, object> camDict)
                {
                    scene.Camera.Position = GetVector3(camDict, "position", new Vector3(0, 0, -10));
                    scene.Camera.Target = GetVector3(camDict, "target", Vector3.Zero);
                }

                // Load lights
                if (root.TryGetValue("lights", out object lightsObj) && lightsObj is List<object> lightsList)
                {
                    if (lightsList.Count > 0 && lightsList[0] is Dictionary<string, object> lightDict)
                    {
                        scene.LightPosition = GetVector3(lightDict, "position", new Vector3(0, 10, 10));
                    }
                }

                // Load models
                if (root.TryGetValue("models", out object modelsObj) && modelsObj is List<object> modelsList)
                {
                    foreach (object modelObj in modelsList)
                    {
                        if (!(modelObj is Dictionary<string, object> modelDict)) continue;

                        Mesh mesh = null;
                        string primitive = GetString(modelDict, "primitive");
                        string modelFile = GetString(modelDict, "modelFile");

                        if (!string.IsNullOrEmpty(primitive))
                        {
                            if (primitive.ToLower() == "cube")
                                mesh = Mesh.CreateCube("Cube");
                        }
                        else if (!string.IsNullOrEmpty(modelFile))
                        {
                            string modelPath = Path.Combine(basePath, modelFile);
                            mesh = Mesh.LoadObj(modelPath);
                        }

                        if (mesh != null)
                        {
                            mesh.Position = GetVector3(modelDict, "position", Vector3.Zero);
                            mesh.Rotation = GetVector3(modelDict, "rotation", Vector3.Zero);

                            string textureFile = GetString(modelDict, "textureFile");
                            if (!string.IsNullOrEmpty(textureFile))
                            {
                                string texturePath = Path.Combine(basePath, textureFile);
                                mesh.Texture = Texture.Load(texturePath);
                            }

                            scene.AddMesh(mesh);
                        }
                    }
                }

                return scene;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load scene from {filename}: {ex.Message}");
                return null;
            }
        }
    }
}
