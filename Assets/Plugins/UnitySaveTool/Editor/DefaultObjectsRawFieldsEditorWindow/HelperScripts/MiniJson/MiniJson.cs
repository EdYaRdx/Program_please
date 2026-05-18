#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace UnitySaveTool.EditorTools
{
    public class MiniJson
    {
        public static object Deserialize(string json)
        {
            if (json == null)
                return null;

            Parser parser = new Parser(json);
            return parser.ParseValue();
        }

        public static string Serialize(object obj, bool pretty)
        {
            StringBuilder sb = new StringBuilder(1024);
            Writer writer = new Writer(sb, pretty);
            writer.WriteValue(obj);
            return sb.ToString();
        }

        private sealed class Parser
        {
            private readonly string _s;
            private int _i;

            public Parser(string s)
            {
                _s = s;
                _i = 0;
            }

            public object ParseValue()
            {
                SkipWs();

                if (_i >= _s.Length)
                    return null;

                char c = _s[_i];

                if (c == '{')
                    return ParseObject();

                if (c == '[')
                    return ParseArray();

                if (c == '"')
                    return ParseString();

                if (c == 't' || c == 'f')
                    return ParseBool();

                if (c == 'n')
                    return ParseNull();

                return ParseNumber();
            }

            private Dictionary<string, object> ParseObject()
            {
                Dictionary<string, object> obj = new Dictionary<string, object>();

                Expect('{');
                SkipWs();

                if (Peek('}'))
                {
                    _i++;
                    return obj;
                }

                while (true)
                {
                    SkipWs();
                    string key = ParseString();

                    SkipWs();
                    Expect(':');

                    object val = ParseValue();
                    obj[key] = val;

                    SkipWs();

                    if (Peek('}'))
                    {
                        _i++;
                        break;
                    }

                    Expect(',');
                }

                return obj;
            }

            private List<object> ParseArray()
            {
                List<object> arr = new List<object>();

                Expect('[');
                SkipWs();

                if (Peek(']'))
                {
                    _i++;
                    return arr;
                }

                while (true)
                {
                    object v = ParseValue();
                    arr.Add(v);

                    SkipWs();

                    if (Peek(']'))
                    {
                        _i++;
                        break;
                    }

                    Expect(',');
                }

                return arr;
            }

            private string ParseString()
            {
                Expect('"');

                StringBuilder sb = new StringBuilder();

                while (_i < _s.Length)
                {
                    char c = _s[_i++];
                    if (c == '"')
                        break;

                    if (c == '\\')
                    {
                        if (_i >= _s.Length)
                            break;

                        char e = _s[_i++];

                        if (e == '"' || e == '\\' || e == '/')
                            sb.Append(e);
                        else if (e == 'b')
                            sb.Append('\b');
                        else if (e == 'f')
                            sb.Append('\f');
                        else if (e == 'n')
                            sb.Append('\n');
                        else if (e == 'r')
                            sb.Append('\r');
                        else if (e == 't')
                            sb.Append('\t');
                        else if (e == 'u')
                        {
                            if (_i + 4 <= _s.Length)
                            {
                                string hex = _s.Substring(_i, 4);
                                _i += 4;

                                int code;
                                if (int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out code))
                                    sb.Append((char)code);
                            }
                        }

                        continue;
                    }

                    sb.Append(c);
                }

                return sb.ToString();
            }

            private object ParseNumber()
            {
                int start = _i;

                if (Peek('-'))
                    _i++;

                while (_i < _s.Length && char.IsDigit(_s[_i]))
                    _i++;

                bool isFloat = false;

                if (_i < _s.Length && _s[_i] == '.')
                {
                    isFloat = true;
                    _i++;

                    while (_i < _s.Length && char.IsDigit(_s[_i]))
                        _i++;
                }

                if (_i < _s.Length && (_s[_i] == 'e' || _s[_i] == 'E'))
                {
                    isFloat = true;
                    _i++;

                    if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-'))
                        _i++;

                    while (_i < _s.Length && char.IsDigit(_s[_i]))
                        _i++;
                }

                string num = _s.Substring(start, _i - start);

                if (isFloat)
                {
                    double d;
                    if (double.TryParse(num, NumberStyles.Float, CultureInfo.InvariantCulture, out d))
                        return d;

                    return 0.0;
                }

                long l;
                if (long.TryParse(num, NumberStyles.Integer, CultureInfo.InvariantCulture, out l))
                    return l;

                return 0L;
            }

            private object ParseBool()
            {
                if (Match("true"))
                    return true;

                if (Match("false"))
                    return false;

                throw new FormatException("Invalid boolean token at " + _i);
            }

            private object ParseNull()
            {
                if (Match("null"))
                    return null;

                throw new FormatException("Invalid null token at " + _i);
            }

            private bool Match(string token)
            {
                SkipWs();

                if (_i + token.Length > _s.Length)
                    return false;

                for (int k = 0; k < token.Length; k++)
                {
                    if (_s[_i + k] != token[k])
                        return false;
                }

                _i += token.Length;
                return true;
            }

            private void SkipWs()
            {
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
                    {
                        _i++;
                        continue;
                    }

                    break;
                }
            }

            private void Expect(char c)
            {
                SkipWs();

                if (_i >= _s.Length || _s[_i] != c)
                    throw new FormatException("Expected '" + c + "' at " + _i);

                _i++;
            }

            private bool Peek(char c)
            {
                SkipWs();
                return _i < _s.Length && _s[_i] == c;
            }
        }

        private sealed class Writer
        {
            private readonly StringBuilder _sb;
            private readonly bool _pretty;
            private int _indent;

            public Writer(StringBuilder sb, bool pretty)
            {
                _sb = sb;
                _pretty = pretty;
                _indent = 0;
            }

            public void WriteValue(object v)
            {
                if (v == null)
                {
                    _sb.Append("null");
                    return;
                }

                if (v is string s)
                {
                    WriteString(s);
                    return;
                }

                if (v is bool b)
                {
                    _sb.Append(b ? "true" : "false");
                    return;
                }

                if (v is double d)
                {
                    _sb.Append(d.ToString("R", CultureInfo.InvariantCulture));
                    return;
                }

                if (v is float f)
                {
                    _sb.Append(f.ToString("R", CultureInfo.InvariantCulture));
                    return;
                }

                if (v is long l)
                {
                    _sb.Append(l.ToString(CultureInfo.InvariantCulture));
                    return;
                }

                if (v is int i)
                {
                    _sb.Append(i.ToString(CultureInfo.InvariantCulture));
                    return;
                }

                Dictionary<string, object> obj = v as Dictionary<string, object>;
                if (obj != null)
                {
                    WriteObject(obj);
                    return;
                }

                List<object> arr = v as List<object>;
                if (arr != null)
                {
                    WriteArray(arr);
                    return;
                }

                _sb.Append("null");
            }

            private void WriteObject(Dictionary<string, object> obj)
            {
                _sb.Append("{");
                if (_pretty)
                {
                    _sb.Append("\n");
                    _indent++;
                }

                int n = 0;
                foreach (KeyValuePair<string, object> kv in obj)
                {
                    if (_pretty)
                        WriteIndent();

                    WriteString(kv.Key);
                    _sb.Append(_pretty ? ": " : ":");
                    WriteValue(kv.Value);

                    n++;
                    if (n < obj.Count)
                        _sb.Append(",");

                    if (_pretty)
                        _sb.Append("\n");
                }

                if (_pretty)
                {
                    _indent--;
                    WriteIndent();
                }

                _sb.Append("}");
            }

            private void WriteArray(List<object> arr)
            {
                _sb.Append("[");
                if (_pretty)
                {
                    _sb.Append("\n");
                    _indent++;
                }

                for (int i = 0; i < arr.Count; i++)
                {
                    if (_pretty)
                        WriteIndent();

                    WriteValue(arr[i]);

                    if (i < arr.Count - 1)
                        _sb.Append(",");

                    if (_pretty)
                        _sb.Append("\n");
                }

                if (_pretty)
                {
                    _indent--;
                    WriteIndent();
                }

                _sb.Append("]");
            }

            private void WriteString(string s)
            {
                _sb.Append("\"");
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];

                    if (c == '\\' || c == '"')
                    {
                        _sb.Append("\\");
                        _sb.Append(c);
                    }
                    else if (c == '\n')
                    {
                        _sb.Append("\\n");
                    }
                    else if (c == '\r')
                    {
                        _sb.Append("\\r");
                    }
                    else if (c == '\t')
                    {
                        _sb.Append("\\t");
                    }
                    else
                    {
                        _sb.Append(c);
                    }
                }
                _sb.Append("\"");
            }

            private void WriteIndent()
            {
                if (_pretty == false)
                    return;

                for (int i = 0; i < _indent; i++)
                    _sb.Append("  ");
            }
        }
    }
}
#endif