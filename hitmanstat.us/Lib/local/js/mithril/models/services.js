var services = services || {};
var chartRendered = false;

mapboxgl.accessToken = 'pk.eyJ1Ijoic2F2ZW5zZXBpMGwiLCJhIjoiY2tjM2J1ejVzMWR4eTJ2bXJ0b29saXVweCJ9.SUaiNbskaseteDeKrWvNug';

var heatmap = new mapboxgl.Map({
    container: 'heatmap',
    style: 'mapbox://styles/mapbox/dark-v10',
    center: [0, 20],
    zoom: 1.3,
    maxZoom: 5
});

services.list = [
    { ref: "h2pc", group: "h2", name: 'hitman 2 pc', endpoint: 'pc2-service.hitman.io', platform: 'azure' },
    { ref: "h2xb", group: "h2", name: 'hitman 2 xbox one', endpoint: 'xboxone2-service.hitman.io', platform: 'azure' },
    { ref: "h2ps", group: "h2", name: 'hitman 2 ps4', endpoint: 'ps42-service.hitman.io', platform: 'azure' },
    { ref: "h1pc", group: "h1", name: 'hitman pc', endpoint: 'pc-service.hitman.io', platform: 'azure' },
    { ref: "h1xb", group: "h1", name: 'hitman xbox one', endpoint: 'xboxone-service.hitman.io', platform: 'azure' },
    { ref: "h1ps", group: "h1", name: 'hitman ps4', endpoint: 'ps4-service.hitman.io', platform: 'azure' },
    { ref: "h2st", group: "ot", name: 'hitman 2 stadia', endpoint: 'stadia2-service.hitman.io', platform: 'azure' },
    { ref: "hmfc", group: "ot", name: 'hitmanforum.com', endpoint: 'hitmanforum', platform: 'discourse', url: 'https://www.hitmanforum.com/' }
];

services.refresh = function () {
    // --------- HITMAN ---------
    m.request({
        method: 'GET',
        url: '/status/hitman',
    })
    .then(function (result) {
        var lastCheck = moment();
        if (result.state) {
            errorElement.style.display = 'block';
            errorElement.innerHTML = '<h1>All hitman services are unavailable</h1><span></span><h2>' + result.status + '</h2>';
            services.list.map(function (service) {
                if (service.platform == 'azure') {
                    service.status = 'down';
                    service.title = '';
                    service.lastCheck = lastCheck;
                    service.elusive = null;
                    service.nextWindow = null;
                }
            });
        } else {
            lastCheck = moment(result.timestamp);
            errorElement.style.display = 'none';
            errorElement.innerHTML = '';
            services.list.map(function (service) {
                if (service.platform != 'azure')
                    return;
                if (service.name == 'hitman authentication') {
                    service.status = 'up';
                    service.lastCheck = lastCheck;
                    return;
                }
                // Next maintenance
                var nextWindow = result.services[service.endpoint].nextWindow;
                var state = result.services[service.endpoint].status;
                // Service main state
                switch (state) {
                    case 'UI_GAME_SERVICE_NOT_AVAILABLE':
                        if (!nextWindow) break;
                        // if the service is in maintenance during the next window
                        if (nextWindow.status == 'UI_GAME_SERVICE_DOWN_MAINTENANCE') {
                            service.status = 'maintenance';
                            service.title = '';
                            service.nextWindow = nextWindow;
                            service.lastCheck = lastCheck;
                            return;
                        }
                }
                // Service health (unknown, down, maintenance, slow, healthy)
                var status = result.services[service.endpoint].health;
                var map = { healthy: 'up', slow: 'warn' };
                var regex = new RegExp(Object.keys(map).join("|"), "gi");
                status = status.replace(regex, function (match) {
                    return map[match];
                });
                service.status = status;
                service.state = (state) ? state : null;
                service.title = (service.status == 'warn') ? 'high load' : '';
                service.nextWindow = (nextWindow) ? nextWindow : null;
                service.lastCheck = lastCheck;
                // Elusives status
                if (result.elusives) {
                    var elusive = result.elusives[service.endpoint][0];
                    if (elusive) {
                        service.elusive = {
                            name: elusive.name,
                            tile: elusive.tile,
                            description: elusive.description,
                            location: elusive.location,
                            nextWindow: elusive.nextWindow,
                        };
                    }
                }
            });
        }
    });
    //  --------- Hitmanforum ---------
    m.request({
        method: 'GET',
        url: '/status/hitmanforum',
    })
    .then(function (result) {
        var lastCheck = moment();
        services.list.map(function (service) {
            if (service.platform != 'discourse')
                return;
            service.status = result.state;
            service.lastCheck = lastCheck;
            if (result.status)
                service.title = result.status;
            else
                service.title = '';
        });
    });
};

services.renderStats = function () {
    m.request({
        method: 'GET',
        url: '/reports',
    })
    .then(function (result) {

        /**
         * Heatmap update
         */
        heatmap.getSource('reports').setData(JSON.parse(result.geoData));

        if (chartRendered) {

            /**
             * Chart update
             */
            ApexCharts.exec('playersReports', 'updateOptions', {
                xaxis: {
                    categories: result.categories,
                }
            }, false, true);

            ApexCharts.exec('playersReports', 'updateSeries', result.series, true);

        } else {

            /**
             * Chart initial load
             */

            var options = {
                chart: {
                    height: 350,
                    type: 'area',
                    id: 'playersReports',
                    shadow: {
                        enabled: true,
                        color: '#000',
                        top: 18,
                        left: 7,
                        blur: 10,
                        opacity: 1
                    },
                    toolbar: {
                        show: false
                    }
                },
                annotations: {
                    yaxis: [{
                        y: 30,
                        borderColor: '#FF0000',
                        label: {
                            borderColor: '#FF4560',
                            style: {
                                color: '#fff',
                                background: '#FF4560',
                            },
                            text: 'threshold',
                            show: true,
                        }
                    }]
                },
                colors: ['#cc2d00', '#6d9e01', '#017db5', '#ff3800', '#97dc00', '#00a7f3', '#b067a8', '#cacaca'],
                dataLabels: {
                    enabled: false
                },
                stroke: {
                    width: 3,
                    curve: 'smooth'
                },
                series: result.series,
                grid: {
                    borderColor: '#e7e7e7',
                    row: {
                        colors: ['#f3f3f3', 'transparent'],
                        opacity: 0.5
                    },
                },
                xaxis: {
                    categories: result.categories,
                    labels: {
                        style: {
                            fontSize: '14px'
                        }
                    }
                },
                yaxis: {
                    tickAmount: 6,
                    labels: {
                        style: {
                            fontSize: '14px'
                        },
                        formatter: function (value) {
                            return Math.round(value);
                        }
                    }
                },
                legend: {
                    show: true
                }
            }

            var chart = new ApexCharts(document.querySelector("#chart"), options);
            chart.render();
            chartRendered = true;
        }
    });
};

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

    services.renderStats();

});