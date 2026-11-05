window.PortalOverviewMap = (() => {
    const defaultCenter = [58.1467, 7.9956];

    function createPopup(entry) {
        const statuses = {
            "New": ["Ny", "new"], "In progress": ["Under behandling", "progress"],
            "Resolved": ["Løst", "resolved"], "Available": ["Tilgjengelig", "available"],
            "Unavailable": ["Ikke tilgjengelig", "unavailable"]
        };
        const priorities = { Low: "Lav", Normal: "Normal", High: "Høy", Critical: "Kritisk" };
        const popup = document.createElement("div");
        popup.className = "map-popup";
        const type = document.createElement("div");
        type.className = "map-popup-type";
        type.textContent = entry.typeLabel || "Registrering";
        popup.append(type);
        const title = document.createElement("h3");
        title.textContent = entry.title || "Registrering";
        popup.append(title);
        const [statusText, statusClass] = statuses[entry.status] || [entry.status || "Ukjent", "unknown"];
        const status = document.createElement("span");
        status.className = `map-popup-status map-status-${statusClass}`;
        status.textContent = `Status: ${statusText}`;
        popup.append(status);
        const addRow = (label, value) => {
            if (value === null || value === undefined || value === "") return;
            const row = document.createElement("p");
            const name = document.createElement("strong");
            name.textContent = `${label}: `;
            row.append(name, document.createTextNode(String(value)));
            popup.append(row);
        };
        if (entry.priority) addRow("Prioritet", priorities[entry.priority] || entry.priority);
        if (entry.quantity != null) addRow("Mengde", `${entry.quantity}${entry.unit ? ` ${entry.unit}` : ""}`);
        addRow(entry.timeLabel, entry.time);
        if (entry.detailsUrl) {
            const link = document.createElement("a");
            link.href = entry.detailsUrl;
            link.textContent = "Se alle detaljer";
            link.className = "map-popup-link";
            popup.append(link);
        }
        return popup;
    }

    function create(elementId, entries) {
        const element = document.getElementById(elementId);

        if (!element || typeof L === "undefined") {
            return;
        }

        const map = L.map(element).setView(defaultCenter, 12);
        const panel = element.closest(".overview-map-card");
        const fullscreenButton = panel?.querySelector(".overview-fullscreen");
        fullscreenButton?.addEventListener("click", async () => {
            try {
                if (document.fullscreenElement === panel) await document.exitFullscreen();
                else await panel.requestFullscreen();
            } catch {
                fullscreenButton.textContent = "Fullskjerm er ikke tilgjengelig i denne nettleseren";
            }
        });
        document.addEventListener("fullscreenchange", () => {
            if (fullscreenButton) fullscreenButton.textContent = document.fullscreenElement === panel ? "Lukk fullskjerm" : "Vis kart i fullskjerm";
            map.invalidateSize();
        });
        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "&copy; OpenStreetMap contributors"
        }).addTo(map);

        const layers = [];

        entries.forEach(entry => {
            if (!entry.geometryData) {
                return;
            }

            try {
                const iconContent = document.createElement("img");
                iconContent.src = entry.symbol || "/images/types/other.svg";
                iconContent.alt = "";
                const icon = L.divIcon({
                    className: "portal-map-icon",
                    html: iconContent,
                    iconSize: [42, 42],
                    iconAnchor: [21, 21],
                    popupAnchor: [0, -22]
                });
                const label = `${entry.typeLabel || "Registrering"}: ${entry.title || "Registrering"}`;
                const layer = L.geoJSON(JSON.parse(entry.geometryData), {
                    pointToLayer: (_feature, latlng) => L.marker(latlng, { icon, title: label, alt: label })
                }).addTo(map).bindPopup(() => createPopup(entry), { maxWidth: 340, minWidth: 250 });

                // Linjer får også en kategorimarkør, plassert midt på kartutsnittet for linjen.
                layer.eachLayer(feature => {
                    if (feature instanceof L.Polyline && feature.getBounds().isValid()) {
                        L.marker(feature.getBounds().getCenter(), { icon, title: label, alt: label })
                            .bindPopup(() => createPopup(entry), { maxWidth: 340, minWidth: 250 })
                            .addTo(layer);
                    }
                });

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
