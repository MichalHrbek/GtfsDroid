Console.WriteLine("Start");

var watch = System.Diagnostics.Stopwatch.StartNew();
var stops = GtfsParser.ParseFile("PID_GTFS/stops.txt");
var trips = GtfsParser.ParseFile("PID_GTFS/trips.txt");
var routes = GtfsParser.ParseFile("PID_GTFS/routes.txt");
var stopTimes = GtfsParser.ParseFile("PID_GTFS/stop_times.txt", 4);
watch.Stop();
Console.WriteLine($"Took {watch.Elapsed.TotalSeconds}s");

var tripsById = GtfsParser.SortByProp(trips, "trip_id");
var routesById = GtfsParser.SortByProp(routes, "route_id");
var stopsByNode = GtfsParser.SortByProp(stops, "asw_node_id");
var areas = stopsByNode.Select(x => new Area(x.Key, x.Value));

var sel = areas.Where(x => x.name == "Vozovna Kobylisy").First();

watch = System.Diagnostics.Stopwatch.StartNew();
var tripTimes = new List<(string, TimeOnly)>();

foreach (var st in stopTimes) {
    if (sel.IncludesStop(st["stop_id"])) {
        tripTimes.Add((routesById[tripsById[st["trip_id"]][0]["route_id"]][0]["route_short_name"], GtfsParser.ParseTime(st["departure_time"])));
    }
}

watch.Stop();
Console.WriteLine($"Took {watch.Elapsed.TotalSeconds}s");


foreach (var i in tripTimes) {
    Console.WriteLine($"{i.Item1}; {i.Item2}");
}