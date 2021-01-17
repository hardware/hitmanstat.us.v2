/*
 * MAP INIT
 * ------------------------------------------------
 */

var services = services || {};

mapboxgl.accessToken = 'pk.eyJ1Ijoic2F2ZW5zZXBpMGwiLCJhIjoiY2tjM2J1ejVzMWR4eTJ2bXJ0b29saXVweCJ9.SUaiNbskaseteDeKrWvNug';

var heatmap = new mapboxgl.Map({
    container: 'heatmap',
    style: 'mapbox://styles/mapbox/dark-v10',
    center: [0, 20],
    zoom: 1.3,
    maxZoom: 5
});

/*
 * COUNTER INIT
 * ------------------------------------------------
 */

var count = 30;
var timer = {
    oninit: tick,
    view: function () {
        return m("span", count);
    }
};

function tick() {
    setInterval(function () {
        if (count === 0) {
            count = 30;
            services.renderMap();
        } else
            count--;
        m.redraw();
    }, 1000);
}

m.mount(timerView, timer);

/*
 * MAP DATA UPDATE
 * ------------------------------------------------
 */

services.renderMap = function () {
    m.request({
        method: 'GET',
        url: '/reports',
    })
    .then(function (result) {
        heatmap.getSource('reports').setData(JSON.parse(result.geoData));
    });
};

/*
 * MAP LAYER INIT ON LOAD
 * ------------------------------------------------
 */

heatmap.on('load', function () {

    heatmap.addSource('reports', {
        type: 'geojson',
        data: JSON.parse("{\"type\":\"FeatureCollection\",\"features\":[]}")
    });

    heatmap.addLayer({
        id: 'reports-heat',
        type: 'heatmap',
        source: 'reports',
        maxzoom: 6,
        paint: {
            // increase intensity as zoom level increases
            'heatmap-intensity': {
                stops: [
                    [11, 1],
                    [15, 3]
                ]
            },
            // assign color values be applied to points depending on their density
            'heatmap-color': [
                'interpolate',
                ['linear'],
                ['heatmap-density'],
                0, 'rgba(236,222,239,0)',
                0.2, 'rgb(208,209,230)',
                0.4, 'rgb(166,189,219)',
                0.6, 'rgb(103,169,207)',
                0.8, 'rgb(28,144,153)'
            ],
            // increase radius as zoom increases
            'heatmap-radius': {
                stops: [
                    [11, 15],
                    [15, 20]
                ]
            },
            // decrease opacity to transition into the circle layer
            'heatmap-opacity': {
                default: 1,
                stops: [
                    [14, 1],
                    [15, 0]
                ]
            },
        }
    }, 'waterway-label');

    services.renderMap();

});