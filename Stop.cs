class Stop {
    string stop_id;
    string stop_name;
    [int.Parse() GtfsProperty(null,null)]
    float stop_lat;
    float stop_lon;
    string zone_id;
    string stop_url;
    string location_type;
    string parent_station;
    string wheelchair_boarding;
    string level_id;
    string platform_code;
    int asw_node_id;
    int asw_stop_id;
}