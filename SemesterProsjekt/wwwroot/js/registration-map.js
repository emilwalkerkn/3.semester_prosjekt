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

        const selectedPoint = new L.FeatureGroup();
        map.addLayer(selectedPoint);
        map.addControl(new L.Control.Draw({
            draw: { polyline: false, polygon: false, circle: false, rectangle: false, circlemarker: false, marker: true },
            edit: { featureGroup: selectedPoint, edit: true, remove: true }
        }));

        const savePoint = (layer) => {
            selectedPoint.clearLayers();
            selectedPoint.addLayer(layer);
            geometryType.value = "marker";
            geometryData.value = JSON.stringify(layer.toGeoJSON());
            status.textContent = "Plassering valgt. Du kan flytte eller slette punktet i kartet.";
            status.classList.add("is-selected");
        };

        map.on("draw:created", (event) => savePoint(event.layer));
        map.on("draw:edited", (event) => event.layers.eachLayer((layer) => savePoint(layer)));
        map.on("draw:deleted", () => {
            geometryType.value = "";
            geometryData.value = "";
            status.textContent = "Ingen plassering valgt ennå.";
            status.classList.remove("is-selected");
        });
    });
})();
