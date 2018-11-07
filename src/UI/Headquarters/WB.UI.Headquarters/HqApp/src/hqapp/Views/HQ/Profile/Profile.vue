<template>
<div class="container-fluid">
    <div class="row" v-if="showMap">
        <div class="col-sm-9 map">
            <div ref="map" id="map-canvas" class="extra-margin-bottom" style="width:100%; height: 400px"></div>
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
    </div>
    <div class="row">
        <div class="col-sm-12 clearfix">
            <h3>{{$t("Pages.InterviewerProfile_TrafficUsageHeader")}}</h3>
            <div class="graphic-wrapper traffic-usage">
                <div class="t-monthly-usage" v-for="monthlyUsage in trafficUsage" :key="monthlyUsage.month">
                    <div class="t-month">{{monthlyUsage.month}}</div>
                    <div class="t-daily-usage" v-for="dailyUsage in monthlyUsage.dailyUsage" :key="dailyUsage.timestamp">
                        <div class="t-unit-wrapper">
                            <div class="t-up" data-toggle="tooltip" data-placement="right" :title="formatKb(dailyUsage.up)" :style="{ height: dailyUsage.upInPer + '%' }" ></div>
                            <div class="t-down" data-toggle="tooltip" data-placement="right"  :title="formatKb(dailyUsage.down)" :style="{ height: dailyUsage.downInPer + '%' }"></div>
                        </div>
                        <div class="t-day" :class="{'t-no-sync': dailyUsage.up + dailyUsage.down === 0 }">{{dailyUsage.day}}</div>
                    </div>
                </div>   
                <div class="graphic-explanation">
                    <div class="legend-block">
                        <div class="legend">
                            <div class="legend-unit up-unit"><span></span>{{$t("Pages.InterviewerProfile_IncomingTraffic")}}</div>
                            <div class="legend-unit down-unit"><span></span>{{$t("Pages.InterviewerProfile_OutgoingTraffic")}}</div>
                        </div>
                    </div>
                    <div class="total">
                        <p class="primary-text">{{$t("Pages.InterviewerProfile_TotalTrafficUsed")}}: <span>{{formatKb(totalTrafficUsed)}}</span></p>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
</template>

<script>
    import MarkerClusterer from "@google/markerclustererplus";
    import Vue from "vue";

    export default {
        data: function() {
            return {
                map: null,
                markerCluster: null,
                points: new Map(),
                lines: [],
                interviewerId: this.$route.params.interviewerId,
                interviewDetailsTooltip: new InfoBubble(),
                selectedTooltips: {},
                colorMap:{
                    red: "#e74924",
                    green: "#2c7613",
                    blue: "#0042c8"
                },
                minimumClusterSize: 5,
                totalTrafficUsed: 0,
                trafficUsage: [],
                maxDailyUsage: 0
            };
        },
        mounted() {
            this.initializeMap();
            this.initializeTrafficUsage();
        },
        methods: {
            formatKb(kb){
                return kb.toLocaleString() + " Kb";

            },
            initializeTrafficUsage(){
                const self = this;
                this.$http
                    .get(this.model.api.interviewerTrafficUsage + "/" + this.interviewerId)
                    .then(response => {
                        var trafficUsage = response.data || {};
                        this.totalTrafficUsed = trafficUsage.totalTrafficUsed || 0;
                        this.trafficUsage = trafficUsage.trafficUsages || [];
                        this.maxDailyUsage =  trafficUsage.maxDailyUsage  || 0;
                        Vue.nextTick(function () {
                            $('[data-toggle="tooltip"]').tooltip()
                        })
                    });
            },
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
                        position: new google.maps.LatLng(
                            point.Latitude,
                            point.Longitude,
                        ),
                        label: {text: point.Index === -1 ? "": point.Index + "", color: "white"},
                        map: self.map,
                        opacity: 1,
                        zIndex: point.Index === -1 ? 1000 : 1000 + point.Index 
                    });
                    marker.set("id", point.Index);

                    google.maps.event.addListener(marker, "click",
                        (function(marker, point) {
                            return function() { 
                                self.loadPointDetails(point.InterviewIds, marker);
                            };
                        })(marker, point)
                    );
                    markers.push(marker);
                    bounds.extend(marker.getPosition());

                    self.points.set(point.Index, { 
                        index: point.Index,
                        cluster: 0,
                        point: {
                            lat: point.Latitude,
                            lng: point.Longitude
                        },
                        marker: marker
                    });
                });
                
                this.markerCluster = new MarkerClusterer(this.map, markers,
                {
                    imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m',
                    enableRetinaIcons: true,
                    minimumClusterSize: this.minimumClusterSize,
                    averageCenter: true
                });
                
                google.maps.event.addListener(this.markerCluster, 'clusteringend', () => {
                    this.drawLines();
                });
                
                this.map.fitBounds(bounds);
            },
            drawLines(){
                if (!this.markerCluster)
                    return;

                this.points.forEach(point => {
                    point.cluster = 0;
                });

                this.lines.forEach(line => {
                   line.setMap(null);
                });
                this.lines = [];

                var clusters = this.markerCluster.getClusters();
                var clustersMap = new Map();

                for(let i=1;i<clusters.length+1;i++)
                {
                    let cluster = clusters[i-1];
                    let center = cluster.getCenter();
                    let markers = cluster.getMarkers();
                    clustersMap.set(i, {
                        center: center,
                        size: cluster.getSize()
                    });
                    markers.forEach((marker) => {
                        let markerId = marker.get("id");

                        let point = this.points.get(markerId);
                        point.cluster = i
                    });
                }

                for(let i=1; i< this.points.size; i++)
                {
                    let left = this.points.get(i);
                    let right = this.points.get(i+1);

                    let leftCluster =  clustersMap.get(left.cluster) || { size: -1 };
                    let rightCluster = clustersMap.get(right.cluster) || { size: -1 };

                    let leftPointsAreGroupedInCluster = leftCluster.size >= this.minimumClusterSize;
                    let rightPointsAreGroupedInCluster = rightCluster.size >= this.minimumClusterSize;

                    let pointsAreGroupedInTheSameCluster = left.cluster == right.cluster && leftPointsAreGroupedInCluster;
                    
                    if (pointsAreGroupedInTheSameCluster) continue;

                    let leftPoint = left.point;
                    let rightPoint = right.point;

                    if (left.cluster > 0 && leftPointsAreGroupedInCluster)
                    {
                        leftPoint = leftCluster.center;
                    }

                    if (right.cluster > 0 && rightPointsAreGroupedInCluster)
                    {
                        rightPoint = rightCluster.center;
                    }
                   
                    var path = new google.maps.Polyline({
                        path: [leftPoint, rightPoint],
                        geodesic: true,
                        strokeColor: "#2a81cb",
                        strokeOpacity: 0.75,
                        strokeWeight: 1
                    });

                    path.setMap(this.map);

                    this.lines.push(path);
                }
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
                    mapTypeControlOptions: {
                        position: google.maps.ControlPosition.LEFT_TOP
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
            },
            showMap(){
                return this.$config.model.showMap;
            }
        }
    }; 
</script>
