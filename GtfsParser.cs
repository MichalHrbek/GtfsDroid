using System.Reflection;
using System.Runtime.Serialization;

public class LineReader {
    private string[] lines;
    private int pos = 0;
    public readonly int size;
    public LineReader(string fileName) {
        lines = File.ReadAllLines(fileName);
        size = lines.Length;
    }

    public string? ReadLine() {
        if (pos == lines.Length) {
            return null;
        }
        return lines[Interlocked.Increment(ref pos) - 1];
    }
}


static class GtfsParser {
    public static List<T> ParseFile<T>(string fileName, int threadCount = 1) {
        var lineReader = new LineReader(fileName);
        var props = lineReader.ReadLine().Split(',');
        int pos = 0;

        var results = new List<T>(lineReader.size-1);

        for (int i = 0; i < threadCount; i++)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(doWork));
        }

        while (threadCount != 0) {
            Thread.Sleep(10);
        }

        return results;

        void doWork(object? state) {
            while (true) {
                string? line = lineReader.ReadLine();
                if (line == null) break;
                else {
                    var v = ParseLine(line);
                    T obj = (T)FormatterServices.GetUninitializedObject(typeof(T));
                    foreach (var p in typeof(T).GetProperties()) {
                        var attr = p.GetCustomAttribute<GtfsProperty>(false);
                        for (int j = 0; j < props.Length; j++)
                        {
                            if ((attr == null && p.Name == props[j]) || attr?.name == props[j]) {
                                if ()
                                var methods = p.PropertyType.GetMethods(BindingFlags.Public | BindingFlags.Static);
                                p.PropertyType
                                p.SetValue(obj, v[j]);
                            }
                        }
                    }
                    results[Interlocked.Increment(ref pos)-1] = obj;
                }
            }
            Interlocked.Decrement(ref threadCount);
        }
    }

    public static List<String> ParseLine(string line) {
        var end = 0;
        var start = 0;
        var values = new List<string>();
        while (end <= line.Length) {
            if (end == line.Length) {
                values.Add(line.Substring(start, end-start));
                break;
            }
            if (line[start] == '"') {
                if ((line[end] == '"') && ((end == line.Length-1) || (line[end+1] == ','))) {
                    values.Add(line.Substring(start+1, end-start-1));
                    end += 2;
                    start = end;
                    continue;
                }
            } else if (line[end] == ','){
                values.Add(line.Substring(start, Math.Max(start, end)-start));
                end += 1;
                start = end;
                continue;
            }
            end++;
        }
        return values;
    }

    public static Dictionary<string, List<GtfsObject>> SortByProp(List<GtfsObject> list, string prop) {
        var sorted = new Dictionary<string, List<GtfsObject>>();
        foreach (var o in list) {
            if (sorted.ContainsKey(o[prop])) {
                sorted[o[prop]].Add(o);
            }
            else {
                sorted[o[prop]] = new List<GtfsObject>() {o};
            }
        }
        return sorted;
    }

    public static TimeOnly ParseTime (string text) {
        var split = text.Split(":");
        var h = int.Parse(split[0]);
        var m = int.Parse(split[1]);
        var s = int.Parse(split[2]);
        //Console.WriteLine($"{text};{h}:{m}:{s}");
        //TODO: Figure out service days
        return new TimeOnly(h%24, m%60, s%60);
    }
}


public delegate object GtfsPropertyConstructor(string s);
public class GtfsProperty : Attribute
{
    public string name;
    public GtfsProperty (string propName)
    {
        this.name = propName;
    }
}


class GtfsObject: Dictionary<string, string> {
    public override string ToString() {
        var s = "{";
        foreach(KeyValuePair<string, string> entry in this)
        {
            s += $"{entry.Key}: \"{entry.Value}\"; ";
        }
        return s + "}";
    }
}
