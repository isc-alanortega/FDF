////////////////////////////////////////////////////
//      ALL GENERIC METHODS FROM LEAFLET HER      //
////////////////////////////////////////////////////

export function buildMap({ element, view, zoomSnap, zoom, hasFullScreen }) {
    // Inicializa el mapa usando la referencia que se recibe
    let map = L.map(element, {
        zoomControl: true,
        zoomSnap: zoomSnap
    }).setView(view, zoom);

    // Carga la capa principal del mapa (estilo = light, con etiquetas)
    let positron = L.tileLayer('https://{s}.basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png', {
        attribution: '©OpenStreetMap, ©CartoDB'
    });

    // Añade la capa anterior al mapa
    map.addLayer(positron);

    // Añade el control full screen al mapa
    if (hasFullScreen) {
        map.addControl(new L.Control.Fullscreen());
    }

    return map;
}

export function createIcon(url, size, anchor) {
    return L.icon({
        iconUrl: url,
        iconSize: size,
        iconAnchor: anchor,
        popupAnchor: [0, -15]
    });
}

export function createMarker(point, layerGroup, options = {}) {
    try {
        const { opacity = 0, popup = null, icon = null } = options;

        let marker = icon ?
            L.marker(point, { icon }).addTo(layerGroup)
            : L.marker(point).addTo(layerGroup);

        marker.setOpacity(opacity);

        if (popup != null) {
            marker.bindPopup(popup);
        }

        return marker;
    }
    catch (ex) {
        //console.log(ex);
    }
}

// Crear la polilínea a partir de los puntos GPS
export function createPolyline(gpsPoints, layerGroup, opacity, color) {
    let polyline = L.polyline(gpsPoints.map(([lat, lng]) => [lat, lng])).addTo(layerGroup);
    polyline.setStyle({ opacity: opacity, color: color })
    return polyline;
}