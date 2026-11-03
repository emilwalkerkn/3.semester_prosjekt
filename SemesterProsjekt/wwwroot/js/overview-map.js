window.PortalOverviewMap = (() => {
    const defaultCenter = [58.1467, 7.9956];

    function create(elementId, entries) {
        const element = document.getElementById(elementId);

        if (!element || typeof L === "undefined") {
            return;
        }

        const map = L.map(element).setView(defaultCenter, 12);
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "&copy; OpenStreetMap contributors"
        }).addTo(map);

        const layers = [];

        entries.forEach(entry => {
            if (!entry.geometryData) {
                return;
            }

            try {
                const layer = L.geoJSON(JSON.parse(entry.geometryData))
                    .addTo(map)
                    .bindPopup(entry.title || "Registrering");

                layers.push(layer);
            } catch {
                console.warn("Kunne ikke vise en kartplassering.");
            }
        });

        if (layers.length > 0) {
            map.fitBounds(L.featureGroup(layers).getBounds().pad(0.15));
        }
    }

    return { create };
})();
