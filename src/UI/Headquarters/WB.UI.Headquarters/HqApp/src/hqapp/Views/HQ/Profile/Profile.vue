<template>
<div>
    <div ref="map" id="map-canvas" style="width:100%; height: 400px">

    </div>
    <div style="display:none;">
        <div ref="tooltip" >
            <div class="map-tooltip-info" v-for="selectedTooltip in selectedTooltips" :key="selectedTooltip.InterviewKey">
                <p><span>#{{selectedTooltip.InterviewKey}}</span> <span>({{$t("MapReport.Assignment")}} {{selectedTooltip.AssignmentId}})</span></p>
                <p><strong>{{$t("Users.Interviewer")}} :</strong>&nbsp;{{selectedTooltip.InterviewerName}}</p>
                <p><strong>{{$t("Users.Supervisor")}} :</strong>&nbsp;{{selectedTooltip.SupervisorName}}</p>
                <p><strong>{{$t("Common.Status")}} :</strong>&nbsp;{{selectedTooltip.LastStatus}}</p>
                <p><strong>{{$t("Reports.LastUpdatedDate")}} :</strong>&nbsp;{{selectedTooltip.LastUpdatedDate}}</p>
                <p><a v-bind:href="model.api.interviewDetailsUrl + '/' + selectedTooltip.InterviewId" target="_blank">{{$t("MapReport.ViewInterviewContent")}}</a></p>
            </div>
        </div>
    </div>
</div>
</template>

<script>
    import googleMarkers from "js-marker-clusterer";
    import Vue from "vue";

    const twoColorsMarker = '<svg width="48" height="48" xmlns="http://www.w3.org/2000/svg">\
        <defs>\
        <linearGradient id="myGradient" gradientTransform="rotate(90 16 16)">\
            <stop offset="0%" stop-color="{{color1}}" />\
            <stop offset="100%" stop-color="{{color2}}" />\
        </linearGradient>\
        </defs>\
        <path fill="url(#myGradient)" d="M 22.6 0.3c-0.6 0.1-1.7 0.5-2.2 0.7 -2.2 1-3.8 1.9-6.1 3.7 -3.7 2.8-7.7 6.8-10.4 10.6 -0.7 1-1.6 2.3-1.9 2.9 -0.4 0.7-1 2-1.1 2.2 0 0.1-0.1 0.2-0.1 0.2 -0.2 0.3-0.6 1.8-0.7 2.7 -0.1 0.7-0.1 0.8 0 1.6 0.1 0.9 0.5 2.4 0.7 2.7 0 0 0.1 0.1 0.1 0.2 0.1 0.2 0.9 2 1.2 2.4 1.8 3.1 4.8 6.8 8 9.8 3.6 3.4 6.9 5.7 10.2 7.2 0.5 0.2 1.8 0.6 2.4 0.7 2.4 0.4 5.5-0.6 9.3-3.2 2.4-1.6 4.3-3.2 6.8-5.7 2.1-2.1 3.9-4.2 5-5.8 0.1-0.1 0.4-0.5 0.6-0.9 1-1.4 2.1-3.3 2.5-4.5 0.1-0.3 0.2-0.5 0.2-0.6 0.1-0.2 0.3-1 0.5-1.6 0.2-1.1 0.2-2.5-0.3-3.8 -0.1-0.2-0.2-0.5-0.2-0.6s-0.1-0.4-0.2-0.6c-0.4-1.2-1.5-3.1-2.5-4.5 -0.3-0.4-0.6-0.8-0.6-0.9 -0.5-0.7-1.8-2.3-2.7-3.4 -1-1.1-3.7-3.8-4.8-4.8 -2.7-2.3-5.5-4.3-7.7-5.4 -2.4-1.2-4.3-1.5-6-1.2z"/>\
        </svg>';

    const threeColorsMarker = '<svg width="48" height="48" xmlns="http://www.w3.org/2000/svg">\
        <defs>\
        <linearGradient id="myGradient" gradientTransform="rotate(90 16 16)">\
            <stop offset="0%" stop-color="{{color1}}" />\
            <stop offset="50%" stop-color="{{color2}}" />\
            <stop offset="100%" stop-color="{{color3}}" />\
        </linearGradient>\
        </defs>\
        <path fill="url(#myGradient)" d="M 22.6 0.3c-0.6 0.1-1.7 0.5-2.2 0.7 -2.2 1-3.8 1.9-6.1 3.7 -3.7 2.8-7.7 6.8-10.4 10.6 -0.7 1-1.6 2.3-1.9 2.9 -0.4 0.7-1 2-1.1 2.2 0 0.1-0.1 0.2-0.1 0.2 -0.2 0.3-0.6 1.8-0.7 2.7 -0.1 0.7-0.1 0.8 0 1.6 0.1 0.9 0.5 2.4 0.7 2.7 0 0 0.1 0.1 0.1 0.2 0.1 0.2 0.9 2 1.2 2.4 1.8 3.1 4.8 6.8 8 9.8 3.6 3.4 6.9 5.7 10.2 7.2 0.5 0.2 1.8 0.6 2.4 0.7 2.4 0.4 5.5-0.6 9.3-3.2 2.4-1.6 4.3-3.2 6.8-5.7 2.1-2.1 3.9-4.2 5-5.8 0.1-0.1 0.4-0.5 0.6-0.9 1-1.4 2.1-3.3 2.5-4.5 0.1-0.3 0.2-0.5 0.2-0.6 0.1-0.2 0.3-1 0.5-1.6 0.2-1.1 0.2-2.5-0.3-3.8 -0.1-0.2-0.2-0.5-0.2-0.6s-0.1-0.4-0.2-0.6c-0.4-1.2-1.5-3.1-2.5-4.5 -0.3-0.4-0.6-0.8-0.6-0.9 -0.5-0.7-1.8-2.3-2.7-3.4 -1-1.1-3.7-3.8-4.8-4.8 -2.7-2.3-5.5-4.3-7.7-5.4 -2.4-1.2-4.3-1.5-6-1.2z"/>\
        </svg>';

    export default {
        data: function() {
            return {
                map: null,
                interviewerId: this.$route.params.interviewerId,
                interviewDetailsTooltip: new InfoBubble(),
                selectedTooltips: {},
                colorMap:{
                    red: "#e74924",
                    green: "#2c7613",
                    blue: "#0042c8"
                }
            };
        },
        mounted() {
            this.initializeMap();
        },
        methods: {
            loadPoints() {
                const self = this;
                this.$http
                    .get(this.model.api.interviewerPoints + "/" + this.interviewerId)
                    .then(response => {
                        var points = response.data || [];
                        if (points.length > 0) {
                            self.showPointsOnMap(points);
                        } else {
                            toastr.error(this.$t("MapReport.NoGpsQuestionsByQuestionnaire"));
                        }
                    });
            },
            isIE() {
                var ua = navigator.userAgent;
                /* MSIE used to detect old browsers and Trident used to newer ones*/
                var is_ie = ua.indexOf("MSIE ") > -1 || ua.indexOf("Trident/") > -1;
                
                return is_ie; 
            },
            createMarkerIcon(colors)
            {
                var icon = {
                    scale: 1,
                    size: new google.maps.Size(48,48),
                    anchor: new google.maps.Point(24,48),
                    labelOrigin: new google.maps.Point(24, 24),
                };
                if (colors.length == 1 || this.isIE())
                {
                    icon["path"] = "M 22.6 0.3c-0.6 0.1-1.7 0.5-2.2 0.7 -2.2 1-3.8 1.9-6.1 3.7 -3.7 2.8-7.7 6.8-10.4 10.6 -0.7 1-1.6 2.3-1.9 2.9 -0.4 0.7-1 2-1.1 2.2 0 0.1-0.1 0.2-0.1 0.2 -0.2 0.3-0.6 1.8-0.7 2.7 -0.1 0.7-0.1 0.8 0 1.6 0.1 0.9 0.5 2.4 0.7 2.7 0 0 0.1 0.1 0.1 0.2 0.1 0.2 0.9 2 1.2 2.4 1.8 3.1 4.8 6.8 8 9.8 3.6 3.4 6.9 5.7 10.2 7.2 0.5 0.2 1.8 0.6 2.4 0.7 2.4 0.4 5.5-0.6 9.3-3.2 2.4-1.6 4.3-3.2 6.8-5.7 2.1-2.1 3.9-4.2 5-5.8 0.1-0.1 0.4-0.5 0.6-0.9 1-1.4 2.1-3.3 2.5-4.5 0.1-0.3 0.2-0.5 0.2-0.6 0.1-0.2 0.3-1 0.5-1.6 0.2-1.1 0.2-2.5-0.3-3.8 -0.1-0.2-0.2-0.5-0.2-0.6s-0.1-0.4-0.2-0.6c-0.4-1.2-1.5-3.1-2.5-4.5 -0.3-0.4-0.6-0.8-0.6-0.9 -0.5-0.7-1.8-2.3-2.7-3.4 -1-1.1-3.7-3.8-4.8-4.8 -2.7-2.3-5.5-4.3-7.7-5.4 -2.4-1.2-4.3-1.5-6-1.2z";
                    icon["fillColor"] = this.colorMap[colors[0]];
                    icon["fillOpacity"] = 1;
                    
                    return icon;
                }

                var svg = (colors.length == 2 ? twoColorsMarker : threeColorsMarker).replace('{{color1}}', colors[0]);
                svg = svg.replace('{{color2}}', colors[1]);

                if (colors.length == 2) 
                {
                    icon["url"] = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg);
                    return icon;                
                }

                if (colors.length == 3) 
                {
                    svg = svg.replace('{{color3}}', colors[2]);
                    icon["url"] = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg);
                    return icon;                
                }
            },
            showPointsOnMap(points){
                const self = this;
               
                const arrowMarker = {
                    icon: {
                        path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW
                    },
                    offset: '100%'
                };

                var infowindow = new google.maps.InfoWindow();
                var bounds = new google.maps.LatLngBounds();
                var markers = [];
                points.forEach(point => {
                    var marker = new google.maps.Marker({
                        icon: self.createMarkerIcon(point.Colors),
                        position: new google.maps.LatLng(
                            point.Latitude,
                            point.Longitude,
                        ),
                        label: {text: point.Index === -1 ? "": point.Index + "", color: "white"},
                        map: self.map,
                        opacity: 1,
                        zIndex: point.Index === -1 ? 1000 : 1000 + point.Index 
                    });

                    google.maps.event.addListener(marker, "click",
                        (function(marker, point) {
                            return function() { 
                                self.loadPointDetails(point.InterviewIds, marker);
                            };
                        })(marker, point)
                    );
                    markers.push(marker);
                    bounds.extend(marker.getPosition());
                });
                
                var markerCluster = new MarkerClusterer(this.map, markers,
                {
                    imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m'
                });
      
                var pathPoints = _.map(points, p => {
                    return {
                        lat: p.Latitude,
                        lng: p.Longitude
                    };
                });
                
                // for(var i=0; i< pathPoints.length - 1; i++)
                // {
                //     var path = new google.maps.Polyline({
                //         path: [pathPoints[i], pathPoints[i+1]],
                //         icons: [ arrowMarker ],
                //         geodesic: true,
                //         strokeColor: "#2a81cb",
                //         strokeOpacity: 0.75,
                //         strokeWeight: 2
                //     });

                //     path.setMap(self.map);
                // }
                self.map.fitBounds(bounds);
            },
            loadPointDetails(interviewIds, marker){
                const self = this;
                this.$http
                    .post(this.model.api.interiewSummaryUrl, {
                        InterviewIds: interviewIds
                    })
                    .then(response => {
                        const data = response.data;

                        if (data == undefined || data == null) return;

                        self.selectedTooltips = data;

                        Vue.nextTick(function() {
                            self.interviewDetailsTooltip.setContent(
                                $(self.$refs.tooltip).html()
                            );
                            self.interviewDetailsTooltip.open(self.map, marker);
                        });
                    });
            },
            initializeMap() {
                if (!this.model.showMap) return;

                var self = this;
                var mapOptions = {
                    zoom: 9,
                    mapTypeControl: true,
                    mapTypeControlOptions: {
                        style: google.maps.MapTypeControlStyle.HORIZONTAL_BAR,
                        position: google.maps.ControlPosition.TOP_CENTER
                    },
                    panControl: true,
                    panControlOptions: {
                        position: google.maps.ControlPosition.TOP_RIGHT
                    },
                    zoomControl: true,
                    zoomControlOptions: {
                        style: google.maps.ZoomControlStyle.LARGE,
                        position: google.maps.ControlPosition.TOP_RIGHT
                    },
                    minZoom: 3,
                    scaleControl: true,
                    streetViewControl: false,
                    center: this.model.initialLocation
                };

                this.map = new google.maps.Map(this.$refs.map, mapOptions);

                this.loadPoints();
            }
        },
        computed: {
            model() {
                return this.$config.model;
            }
        }
    }; 
</script>
