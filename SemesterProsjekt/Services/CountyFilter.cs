using System.Text.Json;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SemesterProsjekt.Models;
using NetTopologySuite.Geometries.Utilities;

namespace SemesterProsjekt.Services;

public class CountyFilter
{
    private readonly Dictionary<string, Geometry> _counties;

    public CountyFilter(IWebHostEnvironment environment)
    {
        // Leser fylkesgrensene fra filen vi lastet ned.
        var path = Path.Combine(
            environment.WebRootPath, "data", "fylker.geojson");

        var json = File.ReadAllText(path);
        var reader = new GeoJsonReader();
        var collection = reader.Read<FeatureCollection>(json);

        // Reparerer ugyldige fylkesområder når kartfilen leses inn.
        _counties = collection.ToDictionary(
            feature => (string)feature.Attributes["name"],
            feature => feature.Geometry.IsValid
                ? feature.Geometry
                : GeometryFixer.Fix(feature.Geometry),
            StringComparer.OrdinalIgnoreCase);
    }

    public bool IsInCounty(Resource resource, string? county)
    {
        // «Alle fylker» skal ikke begrense resultatene.
        if (string.IsNullOrWhiteSpace(county))
            return true;

        if (!_counties.TryGetValue(county.Trim(), out var boundary))
            return false;

        if (string.IsNullOrWhiteSpace(resource.GeometryData))
            return false;

        try
        {
            using var document = JsonDocument.Parse(resource.GeometryData);
            var geometryJson = document.RootElement;

            // Leaflet legger kartplasseringen inni et Feature-objekt.
            if (geometryJson.TryGetProperty("geometry", out var geometry))
                geometryJson = geometry;

            var reader = new GeoJsonReader();
            var resourceGeometry =
                reader.Read<Geometry>(geometryJson.GetRawText());

            if (resourceGeometry == null ||
                resourceGeometry.IsEmpty ||
                !resourceGeometry.IsValid)
                return false;

            // Treffer både punkter i fylket og linjer som krysser fylket.
            return boundary.Intersects(resourceGeometry);
        }
        catch (Exception exception) when (
            exception is System.Text.Json.JsonException or
                Newtonsoft.Json.JsonException or
                ArgumentException or
                TopologyException)
        {
            Console.WriteLine(
                $"Fylkesfilter: ressurs {resource.ResourceId}, fylke {county}: {exception}");
            // Uleselig kartplassering regnes ikke som et fylkestreff.
            return false;
        }
    }
}
