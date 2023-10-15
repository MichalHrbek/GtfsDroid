/*public class LineReader {
    FileStream fileStream;
    StreamReader streamReader;
    public LineReader(string fileName) {
        fileStream = File.OpenRead(fileName);
        streamReader = new StreamReader(fileStream);
    }

    public string? ReadLine() {
        lock (streamReader) {
            if (fileStream.CanRead) {
                return streamReader.ReadLine();
            }
        }
        return null;
    }
    
    public void Close() {
        streamReader.Close();
        fileStream.Close();
    }

    ~LineReader() {
        Close();
    }
}*/

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
    public static List<GtfsObject> ParseFile(string fileName, int threadCount = 1) {
        var lineReader = new LineReader(fileName);
        var values = new List<String>[lineReader.size-1];
        var props = lineReader.ReadLine().Split(',');
        int pos = 0;

        var results = new List<GtfsObject>();

        for (int i = 0; i < threadCount; i++)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(doWork));
        }

        while (threadCount != 0) {
            Thread.Sleep(10);
        } 

        for (int i = 0; i < values.Length; i++)
        {
            var o = new GtfsObject();
            for (int j = 0; j < props.Length; j++)
            {
                o.Add(props[j], values[i][j]);
            }
            results.Add(o);
        }
        return results;

        void doWork(object? obj) {
            while (true) {
                string? line = lineReader.ReadLine();
                if (line == null) break;
                else {
                    var v = ParseLine(line);
                    values[Interlocked.Increment(ref pos)-1] = v;
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
