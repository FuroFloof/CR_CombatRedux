using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class cr_game_file_parser : cr_MonoBehavior
{
    public static cr_game_file_parser _instance;
    public static cr_game_file_parser Instance
    {
        get 
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<cr_game_file_parser>();
            }
            return _instance;
        }
    }
    
    public T GetFileVar<T>(string path, string varName, object defaultValue)
    {
        object val = GetVarFromFile(path, varName, defaultValue);
        if (val == null) return (T)defaultValue;
        return (T)Convert.ChangeType(val, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }
    
    public T GetFileVarOrAdd<T>(string path, string varName, object defaultValue)
    {
        object val = GetVarFromFile(path, varName, defaultValue);
        if (val == null)
        {
            if(path.EndsWith(".cfg"))
            {
                WriteFileVar(path, varName, val);
            }
            
            return (T)defaultValue;
        } 
        return (T)Convert.ChangeType(val, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
    }
    
    public void WriteFileVar(string path, string varName, object value) // what the actual fuck does this shit even mean bruh
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException(nameof(path));
        if (string.IsNullOrWhiteSpace(varName)) throw new ArgumentException(nameof(varName));
        if (!string.Equals(Path.GetExtension(path), ".cfg", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException("Only .cfg files are supported for writing.");

        // Ensure parent directory exists
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        string Serialize(object v)
        {
            if (v == null) return "null";

            switch (v)
            {
                case string s:
                    // Quote strings and escape quotes and backslashes
                    return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";

                case bool b:
                    // Write as 1 or 0 for simple cfg use
                    return b ? "1" : "0";

                case int _:
                case long _:
                case short _:
                case byte _:
                case sbyte _:
                case uint _:
                case ulong _:
                case ushort _:
                    return Convert.ToString(v, CultureInfo.InvariantCulture);

                case float _:
                case double _:
                case decimal _:
                    return Convert.ToString(v, CultureInfo.InvariantCulture);

                case string[] sa:
                    {
                        var pieces = new List<string>(sa.Length);
                        foreach (var item in sa)
                            pieces.Add("\"" + (item ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");
                        return "[" + string.Join(", ", pieces) + "]";
                    }

                case IEnumerable<int> ia:
                    return "[" + string.Join(", ", ia) + "]";

                case IEnumerable<float> fa:
                    {
                        var list = new List<string>();
                        foreach (var f in fa) list.Add(Convert.ToString(f, CultureInfo.InvariantCulture));
                        return "[" + string.Join(", ", list) + "]";
                    }

                case System.Collections.IEnumerable anyEnum:
                    {
                        // Fallback for mixed arrays
                        var list = new List<string>();
                        foreach (var item in anyEnum) list.Add(Serialize(item));
                        return "[" + string.Join(", ", list) + "]";
                    }

                default:
                    // Fallback to ToString using invariant
                    return Convert.ToString(v, CultureInfo.InvariantCulture);
            }
        }

        string line = $"{varName} = {Serialize(value)}";

        if (!File.Exists(path))
        {
            File.WriteAllText(path, line + Environment.NewLine);
            return;
        }

        string text = File.ReadAllText(path);
        // Match a full assignment line for this var, ignoring leading spaces
        var pattern = new Regex(@"(?m)^\s*" + Regex.Escape(varName) + @"\s*=.*?$");

        if (pattern.IsMatch(text))
        {
            // Replace existing line
            string updated = pattern.Replace(text, line, 1);
            File.WriteAllText(path, updated);
        }
        else
        {
            // Append with a spacing line
            bool endsWithNewline = text.EndsWith("\n") || text.EndsWith("\r");
            string sep = endsWithNewline ? "" : Environment.NewLine;
            string toAppend = sep + Environment.NewLine + line + Environment.NewLine;
            File.WriteAllText(path, text + toAppend);
        }
    }



    
    public object GetVarFromFile(string path, string varName, object defaultValue)
    {
        if (string.IsNullOrWhiteSpace(path)) return defaultValue;
        if (string.IsNullOrWhiteSpace(varName)) return defaultValue;
        if (!File.Exists(path)) return defaultValue;

        string text = File.ReadAllText(path);

        // Heuristic: extension first, then content sniff
        string ext = Path.GetExtension(path).ToLowerInvariant();
        bool looksJson = ext == ".json" || LooksLikeJson(text);

        var result = looksJson ? GetVarFromJson(text, varName) : GetVarFromCfg(text, varName);

        if (result == null) return defaultValue;

        return result;
    }

    // -------- CFG parsing --------

    private object GetVarFromCfg(string text, string varName)
    {
        string noComments = StripCfgComments(text);

        // Multiline regex: varName = value
        // Capture the raw value, not trimming quotes here
        var pattern = new Regex(@"(?m)^\s*" + Regex.Escape(varName) + @"\s*=\s*(.+?)\s*$");
        var match = pattern.Match(noComments);
        if (!match.Success) return null;

        string raw = match.Groups[1].Value.Trim();
        return ParseValue(raw);
    }

    private string StripCfgComments(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        bool inString = false;
        bool inBlockComment = false;

        for (int i = 0; i < s.Length; i++)
        {
            if (inBlockComment)
            {
                if (i + 1 < s.Length && s[i] == '*' && s[i + 1] == '/')
                {
                    inBlockComment = false;
                    i++; // skip '/'
                }
                continue;
            }

            char c = s[i];

            if (!inString)
            {
                // Start of block comment
                if (i + 1 < s.Length && s[i] == '/' && s[i + 1] == '*')
                {
                    inBlockComment = true;
                    i++; // skip '*'
                    continue;
                }

                // Start of line comment
                if (i + 1 < s.Length && s[i] == '/' && s[i + 1] == '/')
                {
                    // skip until end of line, but keep the newline
                    while (i < s.Length && s[i] != '\n' && s[i] != '\r') i++;
                    if (i < s.Length) sb.Append(s[i]); // append newline char
                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    sb.Append(c);
                    continue;
                }

                sb.Append(c);
            }
            else
            {
                // Inside string: honor escapes and do NOT treat // or /* as comments
                if (c == '\\' && i + 1 < s.Length)
                {
                    sb.Append(c);
                    sb.Append(s[++i]);
                    continue;
                }
                if (c == '"') inString = false;

                sb.Append(c);
            }
        }

        return sb.ToString();
    }


    // -------- JSON parsing (flat, simple) --------

    private object GetVarFromJson(string text, string varName)
    {
        int i = 0;
        SkipWs(text, ref i);
        if (i >= text.Length || text[i] != '{') return null;
        i++; // skip {

        while (true)
        {
            SkipWs(text, ref i);
            if (i >= text.Length) break;
            if (text[i] == '}') break;

            string key = ParseJsonString(text, ref i);
            SkipWs(text, ref i);
            if (i >= text.Length || text[i] != ':') return null;
            i++; // :

            SkipWs(text, ref i);
            if (key == varName)
            {
                return ParseJsonValue(text, ref i);
            }
            else
            {
                // Skip value
                _ = ParseJsonValue(text, ref i);
            }

            SkipWs(text, ref i);
            if (i < text.Length && text[i] == ',')
            {
                i++; // next pair
                continue;
            }
            else if (i < text.Length && text[i] == '}')
            {
                break;
            }
        }

        return null;
    }

    private static bool LooksLikeJson(string s)
    {
        foreach (char c in s)
        {
            if (!char.IsWhiteSpace(c)) return c == '{' || c == '[';
        }
        return false;
    }

    // -------- Shared value parsing --------

    private object ParseValue(string raw)
    {
        // String in quotes
        if (raw.Length >= 2 && raw[0] == '"' && raw[raw.Length - 1] == '"')
            return UnescapeString(raw.Substring(1, raw.Length - 2));

        // Array
        if (raw.Length >= 2 && raw[0] == '[' && raw[raw.Length - 1] == ']')
            return ParseArray(raw.Substring(1, raw.Length - 2));

        // Number: int or float
        if (TryParseNumber(raw, out object num)) return num;

        // Barewords are not supported in your spec, so throw
        throw new FormatException($"Unsupported value: {raw}");
    }

    private object ParseArray(string inner)
    {
        // Split by commas that are not inside quotes
        var list = new List<string>();
        int i = 0;
        int start = 0;
        bool inStr = false;
        while (i <= inner.Length)
        {
            if (i == inner.Length || (inner[i] == ',' && !inStr))
            {
                string piece = inner.Substring(start, i - start).Trim();
                if (piece.Length > 0) list.Add(piece);
                start = i + 1;
            }
            else if (inner[i] == '"')
            {
                inStr = !inStr;
            }
            else if (inner[i] == '\\' && inStr)
            {
                i++; // skip next char in escaped string
            }
            i++;
        }

        // Determine array type by first element
        if (list.Count == 0) return Array.Empty<object>();

        // Try string array
        bool allStrings = true;
        foreach (var s in list)
        {
            if (!(s.Length >= 2 && s[0] == '"' && s[s.Length - 1] == '"'))
            {
                allStrings = false;
                break;
            }
        }
        if (allStrings)
        {
            var arr = new string[list.Count];
            for (int k = 0; k < list.Count; k++)
                arr[k] = UnescapeString(list[k].Substring(1, list[k].Length - 2));
            return arr;
        }

        // Try int array
        bool allInts = true;
        foreach (var s in list)
        {
            if (!int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out _))
            {
                allInts = false;
                break;
            }
        }
        if (allInts)
        {
            var arr = new int[list.Count];
            for (int k = 0; k < list.Count; k++)
                arr[k] = int.Parse(list[k], CultureInfo.InvariantCulture);
            return arr;
        }

        // Fallback to float array
        bool allNums = true;
        foreach (var s in list)
        {
            if (!float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                allNums = false;
                break;
            }
        }
        if (allNums)
        {
            var arr = new float[list.Count];
            for (int k = 0; k < list.Count; k++)
                arr[k] = float.Parse(list[k], NumberStyles.Float, CultureInfo.InvariantCulture);
            return arr;
        }

        throw new FormatException("Mixed or unsupported array element types");
    }

    private bool TryParseNumber(string raw, out object number)
    {
        // Prefer int when possible
        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i))
        {
            number = i;
            return true;
        }
        if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float f))
        {
            number = f;
            return true;
        }
        number = null;
        return false;
    }

    private string UnescapeString(string s)
    {
        // Simple unescape for common sequences
        return Regex.Unescape(s);
    }

    // -------- Tiny JSON value parser --------

    private void SkipWs(string t, ref int i)
    {
        while (i < t.Length && char.IsWhiteSpace(t[i])) i++;
    }

    private string ParseJsonString(string t, ref int i)
    {
        if (t[i] != '"') throw new FormatException("Expected string");
        i++; // skip first quote
        var sb = new System.Text.StringBuilder();
        while (i < t.Length)
        {
            char c = t[i++];
            if (c == '"') break;
            if (c == '\\')
            {
                if (i >= t.Length) throw new FormatException("Invalid escape");
                char e = t[i++];
                switch (e)
                {
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    case '/': sb.Append('/'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'u':
                        if (i + 4 > t.Length) throw new FormatException("Invalid unicode escape");
                        string hex = t.Substring(i, 4);
                        sb.Append((char)Convert.ToInt32(hex, 16));
                        i += 4;
                        break;
                    default: throw new FormatException($"Bad escape: \\{e}");
                }
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }

    private object ParseJsonValue(string t, ref int i)
    {
        SkipWs(t, ref i);
        if (i >= t.Length) throw new FormatException("Unexpected end of JSON");

        char c = t[i];

        if (c == '"')
        {
            return ParseJsonString(t, ref i);
        }
        if (c == '[')
        {
            return ParseJsonArray(t, ref i);
        }
        if (c == '{')
        {
            throw new NotSupportedException("Nested objects are not supported in this minimal parser");
        }
        if (char.IsDigit(c) || c == '-' || c == '+')
        {
            return ParseJsonNumber(t, ref i);
        }
        if (StartsWith(t, i, "true")) { i += 4; return true; }
        if (StartsWith(t, i, "false")) { i += 5; return false; }
        if (StartsWith(t, i, "null")) { i += 4; return null; }

        throw new FormatException($"Unexpected JSON token at {i}");
    }

    private object ParseJsonNumber(string t, ref int i)
    {
        int start = i;
        if (t[i] == '-' || t[i] == '+') i++;
        while (i < t.Length && char.IsDigit(t[i])) i++;
        if (i < t.Length && t[i] == '.')
        {
            i++;
            while (i < t.Length && char.IsDigit(t[i])) i++;
        }
        if (i < t.Length && (t[i] == 'e' || t[i] == 'E'))
        {
            i++;
            if (i < t.Length && (t[i] == '+' || t[i] == '-')) i++;
            while (i < t.Length && char.IsDigit(t[i])) i++;
        }

        string slice = t.Substring(start, i - start);
        if (int.TryParse(slice, NumberStyles.Integer, CultureInfo.InvariantCulture, out int iv))
            return iv;
        if (float.TryParse(slice, NumberStyles.Float, CultureInfo.InvariantCulture, out float fv))
            return fv;

        throw new FormatException($"Bad number: {slice}");
    }

    private object ParseJsonArray(string t, ref int i)
    {
        if (t[i] != '[') throw new FormatException("Expected [");
        i++; // skip [
        var values = new List<object>();

        SkipWs(t, ref i);
        if (i < t.Length && t[i] == ']')
        {
            i++;
            return Array.Empty<object>();
        }

        while (true)
        {
            var v = ParseJsonValue(t, ref i);
            values.Add(v);
            SkipWs(t, ref i);

            if (i < t.Length && t[i] == ',')
            {
                i++;
                continue;
            }
            else if (i < t.Length && t[i] == ']')
            {
                i++;
                break;
            }
            else
            {
                throw new FormatException("Expected , or ] in array");
            }
        }

        // Normalize to typed arrays when possible
        if (values.Count == 0) return Array.Empty<object>();

        bool allStr = values.TrueForAll(x => x is string);
        if (allStr) return values.ConvertAll(x => (string)x).ToArray();

        bool allInt = values.TrueForAll(x => x is int);
        if (allInt) return values.ConvertAll(x => (int)x).ToArray();

        bool allFloat = values.TrueForAll(x => x is float || x is int);
        if (allFloat)
        {
            var arr = new float[values.Count];
            for (int k = 0; k < values.Count; k++)
                arr[k] = Convert.ToSingle(values[k], CultureInfo.InvariantCulture);
            return arr;
        }

        // Mixed types fallback
        return values.ToArray();
    }

    private bool StartsWith(string t, int i, string token)
    {
        return i + token.Length <= t.Length && string.Compare(t, i, token, 0, token.Length, StringComparison.Ordinal) == 0;
    }
}
