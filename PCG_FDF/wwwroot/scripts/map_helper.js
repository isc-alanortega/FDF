import { buildMap, createIcon, createMarker, createPolyline } from './Utils/utils_leaflet.js';
import { getRandomColor, transformStringToDatetime } from "./utils.js"; 

let map;
let gpsLayers = {};
let colors = new Set();

// labels de los Popups segun el lenguaje
const labels = {
    EN: ['Plate', 'GPS', 'Position', 'Altitude', 'Angle', 'Speed', 'Date-Time'],
    ES: ['Placa', 'GPS', 'Posición', 'Altitud', 'Ángulo', 'Velocidad', 'Fecha-Hora']
};

const OPACITY = {
    Show: 1,
    Hide: 0
}

// label seleccionado
let selectedLabel = [];

// Inicializa el mapa usando la referencia que se recibe
export function InitializeMap(element) {
    try {
        map = buildMap({
            element,
            view: [24.27, -101],
            zoomSnap: 0.25,
            zoom: 4.5,
            hasFullScreen: true,
        });
    }
    catch (ex) {
    }

    return map;
}

export function AddTruckToMap(truckId, plate, gpsData, map, language) {
    // Si existe una capa previa para este camión, se elimina
    if (gpsLayers[truckId]) {
        gpsLayers[truckId].clearLayers();
    }

    let truckLayerGroup = L.featureGroup().addTo(map);
    gpsLayers[truckId] = truckLayerGroup;

    // Generate icons
    const truckIcon = createIcon('./assets/img/truck_icon.png', [50, 50], [25, 25]);
    const pointIcon = createIcon('./assets/img/map_point.png', [20, 20], [10, 10]);
    
    // Get labes by lenguage for the popups
    selectedLabel = labels[language] || labels.ES;

    for (const [key, value] of Object.entries(gpsData)) {
        // Create markers for the truck
        const { firstMarker, lastMarker, points } = createMarkers(plate, value, truckLayerGroup, truckIcon, pointIcon);       

        const color = getRandomColor(colors);
        colors.add(color);

        gpsLayers[truckId][key] = {
            poly: createPolyline(value, truckLayerGroup, OPACITY.Hide, color),
            color,
            truckLayerGroup: truckLayerGroup,
            startMarker: firstMarker,
            finishMarker: lastMarker,
            tempFinishMarker: [],
            pathPoints: points,
        };
    }
}

function createMarkers(plate, value, truckLayerGroup, truckIcon, pointIcon) {
    // Get first and las point of the truck
    let [firstPoint, lastPoint] = [value[0], value[value.length - 1]];

    // Create first marker
    const firstMarker = (firstPoint) => {
        const pointMarker = createMarker(firstPoint.slice(0, 2), truckLayerGroup, {
            opacity: OPACITY.Hide,
            popup: buildPopupMarkup(plate, firstPoint)
        });

        return { pointMarker, pointData: firstPoint };
    };

    // Create last marker
    const lastMarker = (lastPoint) => {
        const pointMarker = createMarker(lastPoint.slice(0, 2), truckLayerGroup, {
            opacity: OPACITY.Hide,
            popup: buildPopupMarkup(plate, lastPoint),
            icon: truckIcon
        });

        return { pointMarker, pointData: lastPoint };
    };

    // Create intermediate Markers
    const points = value.length > 2 ? value.slice(1, -1).map(pointItem => {
        const pointMarker = createMarker(pointItem.slice(0, 2), truckLayerGroup, {
            opacity: OPACITY.Hide,
            popup: buildPopupMarkup(plate, pointItem),
            icon: pointIcon
        });

        return { pointMarker, pointData: pointItem };
    }) : [];

    return { firstMarker: firstMarker(firstPoint), lastMarker: lastMarker(lastPoint), points };
} 

export function buildPopupMarkup(plate, pointItems) {
    return `
        <div style="font-size: 14px;">
            <b>${selectedLabel[0]}:</b> ${plate}<br>
            <b>${selectedLabel[1]}:</b> ${pointItems[6]}<br>
            <b>${selectedLabel[2]}:</b> ${pointItems[0]}°, ${pointItems[1]}°<br>
            <b>${selectedLabel[3]}:</b> ${pointItems[3]} m<br>
            <b>${selectedLabel[4]}:</b> ${pointItems[5]}°<br>
            <b>${selectedLabel[5]}:</b> ${pointItems[4]} kph<br>
            <b>${selectedLabel[6]}:</b> ${pointItems[2]}<br>
        </div>`;
}

export function ShowSingleGps(truckId, gpsId, dateFrom, dateTo, chkFilter = true) {
    HideAllPolylines();
    try {

        if (gpsLayers[truckId]?.[gpsId]) {
            const singleGpsLayer = gpsLayers[truckId][gpsId];

            // Show markers
            setGpsLayerOpacity(singleGpsLayer, OPACITY.Show, { dateFrom, dateTo, chkFilter});

            // Show polylines
            singleGpsLayer.poly.setStyle({ opacity: OPACITY.Show });
            map.fitBounds(singleGpsLayer.poly.getBounds());

            const lastMarker = singleGpsLayer.tempFinishMarker;
            if (lastMarker.length > 0) {
                map.setView([lastMarker[0], lastMarker[1]], 25);
            }
        }        
    }
    catch (ex) {
    }
}

export function ShowAllGpsForTruck(truckId, dateFrom, dateTo, chkFilter = true) {
    HideAllPolylines();

    const truckLayers = gpsLayers[truckId];
    if (truckLayers) {
        // Show markers
        handleGpsLayers(Object.values(truckLayers), OPACITY.Show, {
            chkFilter,
            dateFrom,
            dateTo
        });

        map.fitBounds(truckLayers.getBounds());
    }
}

export function ShowAllGpsForAllTrucks(dateFrom, dateTo, chkFilter = true) {
    HideAllPolylines();

    // Show markers
    handleTruckLayerGroups(Object.values(gpsLayers), OPACITY.Show, {
        chkFilter,
        dateFrom,
        dateTo
    });

    map.fitBounds(Object.values(gpsLayers)[0].getBounds());
}

export function HideAllPolylines() {
    handleTruckLayerGroups(Object.values(gpsLayers), OPACITY.Hide);
    map.setView([24.27, -101], 4.5);
}

function handleTruckLayerGroups(trunksGroups, opacity, filter = null) {
    if (trunksGroups) {
        trunksGroups.forEach(truckLayerGroup => {
            handleGpsLayers(Object.values(truckLayerGroup), opacity, filter);
        });
    }
}

function handleGpsLayers(layers, opacity, filter = null) {
    if (layers) {
        layers.forEach(layer => {
            setGpsLayerOpacity(layer, opacity, filter);
        });
    }
}

// Show markers
export function setGpsLayerOpacity(gpsLayer, opacity, filter = null) {
    if (gpsLayer.startMarker && gpsLayer.finishMarker) {
        const { dateFrom, dateTo, chkFilter } = filter || {};        
        let filterData = [];
        gpsLayer.tempFinishMarker = [];
        gpsLayer.poly = null;

        const setOpacity = (marker, condition, filterData) => {
            try {
                marker.pointMarker.setOpacity(condition ? 1 : 0);
                if (condition) {
                    filterData.push(marker.pointData);
                }
            } catch (ex) {
                //console.log(ex)
            }            
        };

        if (filter) {
            const initDate = gpsLayer.startMarker.pointData[7];
            const endDate = gpsLayer.finishMarker.pointData[7];
            const dateFromValidation = initDate >= dateFrom;
            const endDateValidation = dateTo <= initDate;
            const validation = dateFromValidation && endDateValidation;

            setOpacity(
                gpsLayer.startMarker,
                gpsLayer.startMarker.pointData[7] >= dateFrom && gpsLayer.startMarker.pointData[7] <= dateTo,
                filterData
            );

            gpsLayer.pathPoints.forEach(point => {
                setOpacity(
                    point,
                    point.pointData[7] >= dateFrom && point.pointData[7] <= dateTo,
                    filterData
                );
            });

            setOpacity(
                gpsLayer.finishMarker,
                gpsLayer.finishMarker.pointData[7] >= dateFrom && gpsLayer.finishMarker.pointData[7] <= dateTo,
                filterData
            );
        } else {
            const show = OPACITY.Show == opacity;
            setOpacity(gpsLayer.startMarker, show);
            setOpacity(gpsLayer.finishMarker, show);
            gpsLayer.pathPoints.forEach(point =>
                setOpacity(point, show, filterData)
            );
        }

        if (filterData.length > 1) {
            gpsLayer.tempFinishMarker = filterData[filterData.length - 1];
            gpsLayer.poly = createPolyline(filterData, gpsLayer.truckLayerGroup, opacity, gpsLayer.color);
            gpsLayer.poly.setStyle({ opacity: opacity });
        }
    }
}