(() => {
    const defaultLocation = [58.1467, 7.9956];

    document.querySelectorAll("[data-registration-map]").forEach((mapElement) => {
        if (!window.L || mapElement.dataset.mapReady === "true") {
            return;
        }

        const geometryType = document.getElementById(mapElement.dataset.geometryTypeId);
        const geometryData = document.getElementById(mapElement.dataset.geometryDataId);
        const status = document.getElementById(mapElement.dataset.mapStatusId);

        if (!geometryType || !geometryData || !status) {
            return;
        }

        mapElement.dataset.mapReady = "true";
        const map = L.map(mapElement).setView(defaultLocation, 13);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "&copy; OpenStreetMap contributors"
        }).addTo(map);

        // The same registration flow supports either one point or one line.
        // The FeatureGroup is also what Leaflet Draw uses for edit/delete actions.
        const selectedGeometry = new L.FeatureGroup();
        map.addLayer(selectedGeometry);
        map.addControl(new L.Control.Draw({
            draw: {
                marker: true,
                polyline: true,
                polygon: false,
                circle: false,
                rectangle: false,
                circlemarker: false
            },
            edit: { featureGroup: selectedGeometry, edit: true, remove: true }
        }));

        const saveGeometry = (layer) => {
            selectedGeometry.clearLayers();
            selectedGeometry.addLayer(layer);

            const isLine = layer instanceof L.Polyline && !(layer instanceof L.Polygon);
            geometryType.value = isLine ? "polyline" : "marker";
            geometryData.value = JSON.stringify(layer.toGeoJSON());
            status.textContent = isLine
                ? "Linje valgt. Du kan redigere eller slette linjen i kartet."
                : "Punkt valgt. Du kan flytte eller slette punktet i kartet.";
            status.classList.add("is-selected");
        };

        map.on("draw:created", (event) => saveGeometry(event.layer));
        map.on("draw:edited", (event) => event.layers.eachLayer((layer) => saveGeometry(layer)));
        map.on("draw:deleted", () => {
            geometryType.value = "";
            geometryData.value = "";
            status.textContent = "Ingen plassering valgt ennå.";
            status.classList.remove("is-selected");
        });
        if (geometryData.value) {
            try {
                L.geoJSON(JSON.parse(geometryData.value), {
                    onEachFeature: (_feature, layer) => selectedGeometry.addLayer(layer)
                });
                if (selectedGeometry.getLayers().length > 0) {
                    map.fitBounds(selectedGeometry.getBounds(), { maxZoom: 15, padding: [30, 30] });
                    status.textContent = "Lagret plassering vises. Du kan redigere eller slette plasseringen i kartet.";
                    status.classList.add("is-selected");
                }
            } catch (_error) {
                status.textContent = "Plasseringen kunne ikke vises. Velg den på nytt.";
            }
        }
        document.addEventListener("fullscreenchange", () => map.invalidateSize());
    });
})();
