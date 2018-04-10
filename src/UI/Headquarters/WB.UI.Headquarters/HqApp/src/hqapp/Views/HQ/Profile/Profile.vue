<template>
<div>
    <div ref="map" id="map-canvas" style="width:100%; height: 400px">

    </div>
    <div style="display:none;">
        <div ref="tooltip">
            <div class="row-fluid">
                <strong>{{$t("Users.Interviewer")}} :</strong>&nbsp;{{selectedTooltip.InterviewerName}}</div>
            <div class="row-fluid">
                <strong>{{$t("Users.Supervisor")}} :</strong>&nbsp;{{selectedTooltip.SupervisorName}}</div>
            <div class="row-fluid">
                <strong>{{$t("Common.Status")}} :</strong>&nbsp;{{selectedTooltip.LastStatus}}</div>
            <div class="row-fluid">
                <strong>{{$t("Reports.LastUpdatedDate")}} :</strong>&nbsp;{{selectedTooltip.LastUpdatedDate}}</div>
            <div class="row-fluid" style="white-space:nowrap;">
                <strong>{{$t("MapReport.ViewInterviewContent")}} :</strong>&nbsp;
                <a v-bind:href="model.api.interviewDetailsUrl + '/' + selectedTooltip.InterviewId" target="_blank">{{$t("MapReport.details")}}</a>
            </div>
        </div>
    </div>
</div>
</template>

<script>
    import Vue from "vue";

    export default {
        data: function() {
            return {
                map: null,
                points: [],
                interviewerId: this.$route.params.interviewerId,
                interviewDetailsTooltip: new InfoBubble(),
                selectedTooltip: {},
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
                        self.points = response.data || [];
                        if (self.points.length > 0) {
                            var pathPoints = _.map(self.points, p => {
                                return {
                                    lat: p.Latitude,
                                    lng: p.Longitude
                                };
                            });

                            var infowindow = new google.maps.InfoWindow();
                            var bounds = new google.maps.LatLngBounds();
                            self.points.forEach(point => {
                                var marker = new google.maps.Marker({
                                    icon: {
                                        path: google.maps.SymbolPath.CIRCLE,
                                        fillColor: 'gold',
                                        fillOpacity: 0.8,
                                        scale: 10,
                                        strokeColor: 'gold'
                                    },
                                    position: new google.maps.LatLng(
                                        point.Latitude,
                                        point.Longitude,
                                    ),
                                    label: point.Index === -1 ? "": point.Index + "",
                                    labelClass: "interviewerPoint",
                                    map: self.map
                                });

                                google.maps.event.addListener(
                                    marker,
                                    "click",
                                    (function(marker, point) {
                                        return function() {
                                            self.$http
                                            .post(self.model.api.interiewSummaryUrl, {
                                                InterviewId: point.InterviewId
                                            })
                                            .then(response => {
                                                const data = response.data;

                                                if (data == undefined || data == null) return;

                                                data["InterviewId"] = point.InterviewId;
                                                self.selectedTooltip = data;

                                                Vue.nextTick(function() {
                                                    self.interviewDetailsTooltip.setContent(
                                                        $(self.$refs.tooltip).html()
                                                    );
                                                    self.interviewDetailsTooltip.open(self.map, marker);
                                                });
                                            });
                                        };
                                    })(marker, point)
                                );

                                google.maps.event.addListener(marker, "click", function() {
                                    var marker = this;
                                });

                                bounds.extend(marker.getPosition());
                            });

                            var path = new google.maps.Polyline({
                                path: pathPoints,
                                icons: [{
                                    icon: {
                                        path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW
                                    },
                                    offset: '100%',
                                    repeat: '20px'
                                }],
                                geodesic: true,
                                strokeColor: "#FF0000",
                                strokeOpacity: 0.75,
                                strokeWeight: 1
                            });

                            path.setMap(self.map);
                            self.map.fitBounds(bounds);
                        } else {
                            toastr.error(this.$t("MapReport.NoGpsQuestionsByQuestionnaire"));
                        }
                    });
            },
            initializeMap() {
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
                //   var flightPlanCoordinates = [
                //     { lat: 37.772, lng: -122.214 },
                //     { lat: 21.291, lng: -157.821 },
                //     { lat: -18.142, lng: 178.431 },
                //     { lat: -27.467, lng: 153.027 }
                //   ];
                //   var flightPath = new google.maps.Polyline({
                //     path: flightPlanCoordinates,
                //     geodesic: true,
                //     strokeColor: "#FF0000",
                //     strokeOpacity: 1.0,
                //     strokeWeight: 2
                //   });

                //   flightPath.setMap(this.map);
            }
        },
        computed: {
            model() {
                return this.$config.model;
            }
        }
    }; 
</script>
