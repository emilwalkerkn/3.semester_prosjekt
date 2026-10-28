// Kartet opprettes først når vinduet åpnes, slik at det får riktig størrelse.
(() => {
    const dialog = document.getElementById('need-map-dialog');
    const location = document.getElementById('Location');
    const latitude = document.getElementById('Latitude');
    const longitude = document.getElementById('Longitude');
    const geometryType = document.getElementById('GeometryType');
    const geometryData = document.getElementById('GeometryData');
    const feedback = document.getElementById('map-feedback');
    const searchFeedback = document.getElementById('address-feedback');
    const query = document.getElementById('address-query');
    const results = document.getElementById('address-results');
    const searchButton = document.getElementById('search-address');
    let map;
    let drawnItems;
    let drawing = false;
    let searchRequest;
    let municipalityRequest;

    // Kommuneoppslaget bruker selve kartpunktet, også uten en gateadresse.
    async function findMunicipality(point) {
        municipalityRequest?.abort();
        const request = new AbortController();
        municipalityRequest = request;
        location.value = '';
        feedback.textContent = 'Finner kommune for valgt posisjon …';
        try {
            const url = new URL('https://api.kartverket.no/kommuneinfo/v1/punkt');
            url.search = new URLSearchParams({ nord: point.lat, ost: point.lng, koordsys: '4258' });
            const response = await fetch(url, { signal: request.signal });
            if (!response.ok) throw new Error('Kommuneoppslaget feilet');
            const data = await response.json();
            if (municipalityRequest !== request) return;
            if (!data.kommunenavn) throw new Error('Ingen kommune for punktet');
            location.value = data.kommunenavn;
            feedback.textContent = `Valgt kommune: ${data.kommunenavn}. Posisjonen er valgt.`;
        } catch (error) {
            if (error.name !== 'AbortError' && municipalityRequest === request) {
                feedback.textContent = 'Kunne ikke finne kommunen. Velg et punkt i Norge eller prøv igjen. Du kan også lukke kartet og skrive kommunen i Sted-feltet; kartpunktet beholdes.';
            }
        }
    }

    // Alle valg oppdaterer de samme skjulte skjemafeltene.
    function saveLayer(layer, type, municipality) {
        municipalityRequest?.abort();
        municipalityRequest = null;
        geometryType.value = type;
        geometryData.value = JSON.stringify(layer.toGeoJSON());
        if (type === 'marker') {
            const point = layer.getLatLng();
            latitude.value = point.lat.toString();
            longitude.value = point.lng.toString();
            if (municipality) {
                location.value = municipality;
                feedback.textContent = `Valgt kommune: ${municipality}. Posisjonen er valgt.`;
            } else {
                findMunicipality(point);
            }
        } else {
            latitude.value = '';
            longitude.value = '';
            findMunicipality(layer.getBounds().getCenter());
        }
    }

    function choosePoint(point, label) {
        drawnItems.clearLayers();
        const marker = L.marker(point, { draggable: true }).addTo(drawnItems);
        marker.on('dragend', () => saveLayer(marker, 'marker'));
        saveLayer(marker, 'marker', label);
    }

    function initializeMap() {
        if (!window.L) {
            feedback.textContent = 'Kartet kunne ikke lastes. Kontroller internettforbindelsen og last siden på nytt.';
            return;
        }
        map = L.map('map').setView([58.1467, 7.9956], 13);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            attribution: '&copy; OpenStreetMap contributors'
        }).addTo(map);
        drawnItems = new L.FeatureGroup().addTo(map);
        map.on('click', event => {
            if (!drawing) choosePoint(event.latlng);
        });

        // Beholder eksisterende verktøy for markør, linje, redigering og sletting.
        if (L.Control.Draw) {
            map.addControl(new L.Control.Draw({
                draw: { polygon: false, polyline: true, circle: false, rectangle: false, circlemarker: false, marker: true },
                edit: { featureGroup: drawnItems, remove: true }
            }));
            map.on('draw:drawstart draw:editstart draw:deletestart', () => { drawing = true; });
            map.on('draw:drawstop draw:editstop draw:deletestop', () => { drawing = false; });
            map.on('draw:created', event => {
                drawnItems.clearLayers();
                drawnItems.addLayer(event.layer);
                saveLayer(event.layer, event.layerType);
            });
            map.on('draw:edited', event => event.layers.eachLayer(layer => {
                saveLayer(layer, layer instanceof L.Marker ? 'marker' : 'polyline');
            }));
            map.on('draw:deleted', () => {
                if (drawnItems.getLayers().length === 0) {
                    municipalityRequest?.abort();
                    municipalityRequest = null;
                    latitude.value = longitude.value = geometryType.value = geometryData.value = location.value = '';
                    feedback.textContent = 'Stedet er fjernet. Velg et nytt sted.';
                }
            });
        }

        // Beholder valgt posisjon dersom serveren viser skjemaet på nytt.
        if (geometryType.value === 'marker' && latitude.value && longitude.value &&
            Number.isFinite(Number(latitude.value)) && Number.isFinite(Number(longitude.value))) {
            const label = location.value;
            choosePoint([Number(latitude.value), Number(longitude.value)], label);
            map.setView([Number(latitude.value), Number(longitude.value)], 16);
        } else if (geometryType.value === 'polyline' && geometryData.value) {
            try {
                L.geoJSON(JSON.parse(geometryData.value)).eachLayer(layer => drawnItems.addLayer(layer));
                if (drawnItems.getBounds().isValid()) map.fitBounds(drawnItems.getBounds());
            } catch {
                feedback.textContent = 'Den tidligere kartlinjen kunne ikke vises. Velg stedet på nytt.';
            }
        }
    }

    function openMap(event) {
        event?.preventDefault();
        if (!dialog.open) dialog.showModal();
        if (!map) initializeMap();
        requestAnimationFrame(() => map?.invalidateSize());
    }
    location.addEventListener('click', openMap);
    document.getElementById('choose-location').addEventListener('click', openMap);
    document.getElementById('open-need-map').addEventListener('click', openMap);
    document.getElementById('close-need-map').addEventListener('click', () => dialog.close());

    // Manuelt kommunenavn er mulig hvis kommuneoppslaget er utilgjengelig.
    location.addEventListener('input', () => {
        municipalityRequest?.abort();
        municipalityRequest = null;
        feedback.textContent = location.value ? `Kommune: ${location.value}` : '';
    });
    location.addEventListener('keydown', event => {
        if (event.key === 'Enter' || event.key === 'ArrowDown') openMap(event);
    });

    async function searchAddress() {
        searchRequest?.abort();
        results.replaceChildren();
        const text = query.value.trim();
        if (text.length < 3) {
            searchFeedback.textContent = 'Skriv minst tre tegn i adressen.';
            return;
        }
        const request = new AbortController();
        searchRequest = request;
        searchButton.disabled = true;
        searchFeedback.textContent = 'Søker etter adresser …';
        try {
            const url = new URL('https://api.kartverket.no/adresser/v1/sok');
            url.search = new URLSearchParams({ sok: text, treffPerSide: '5', utkoordsys: '4258' });
            const response = await fetch(url, { signal: request.signal });
            if (!response.ok) throw new Error('Adressesøket feilet');
            const data = await response.json();
            if (searchRequest !== request) return;
            const addresses = (data.adresser || []).filter(address =>
                Number.isFinite(address.representasjonspunkt?.lat) &&
                Number.isFinite(address.representasjonspunkt?.lon));
            searchFeedback.textContent = addresses.length ? 'Velg en adresse fra listen.' : 'Ingen treff. Prøv gatenavn, husnummer og sted, eller velg direkte i kartet.';
            addresses.forEach(address => {
                const label = `${address.adressetekst}, ${address.postnummer} ${address.poststed}`;
                const item = document.createElement('li');
                const button = document.createElement('button');
                button.type = 'button';
                button.textContent = label;
                button.addEventListener('click', () => {
                    if (!map) {
                        searchFeedback.textContent = 'Kartet er ikke lastet. Last siden på nytt og prøv igjen.';
                        return;
                    }
                    const point = [address.representasjonspunkt.lat, address.representasjonspunkt.lon];
                    choosePoint(point, address.kommunenavn);
                    map.setView(point, 17);
                    query.value = label;
                    results.replaceChildren();
                    searchFeedback.textContent = 'Adressen er valgt. Trykk Ferdig for å gå tilbake til skjemaet.';
                });
                item.append(button);
                results.append(item);
            });
        } catch (error) {
            if (error.name !== 'AbortError') searchFeedback.textContent = 'Adressesøket er utilgjengelig. Prøv igjen, eller velg et punkt i kartet.';
        } finally {
            if (searchRequest === request) searchButton.disabled = false;
        }
    }
    searchButton.addEventListener('click', searchAddress);
    query.addEventListener('keydown', event => {
        if (event.key === 'Enter') {
            event.preventDefault();
            searchAddress();
        }
    });
})();
