class Area {
    public string id;
    public string name;
    public List<GtfsObject> stops;
    
    public Area (string id, List<GtfsObject> stops) {
        this.id = id;
        this.stops = stops;
        this.name = stops[0]["stop_name"];
    }

    public bool IncludesStop(string stopId) {
        foreach (var s in stops) {
            if (s["stop_id"] == stopId) return true;
        }
        return false;
    }
}