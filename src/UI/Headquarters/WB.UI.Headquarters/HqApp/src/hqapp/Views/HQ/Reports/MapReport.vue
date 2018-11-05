<template>
    <HqLayout :hasFilter="true" :hasHeader="false">
        <Filters slot="filters">
            <FilterBlock :title="$t('Reports.Questionnaire')">
                <Typeahead :placeholder="$t('Common.AllQuestionnaires')" :values="questionnaires" :value="questionnaireId" noSearch @selected="selectQuestionnaire" />
            </FilterBlock>
            <FilterBlock :title="$t('Reports.Variables')">
                <Typeahead :placeholder="$t('Common.AllGpsQuestions')" :values="gpsQuestions" :value="gpsQuestionId" noSearch @selected="selectGpsQuestion" />
            </FilterBlock>
            <div class="preset-filters-container">
                <div class="center-block" style="margin-left: 0">
                    <button class="btn btn-default btn-lg" id="reloadMarkersInBounds" v-if="readyToUpdate" @click="reloadMarkersInBounds">{{$t("MapReport.ReloadMarkers")}}</button>
                </div>
            </div>
        </Filters>
        <div style="display:none;">
            <div ref="tooltip">
                <div class="row-fluid">
                    <strong>{{$t("Common.InterviewKey")}}:</strong>&nbsp;{{selectedTooltip.InterviewKey}}</div>
                <div class="row-fluid">
                    <strong>{{$t("Common.Responsible")}}:</strong>&nbsp;{{selectedTooltip.InterviewerName}}</div>
                <div class="row-fluid">
                    <strong>{{$t("Users.Supervisor")}}:</strong>&nbsp;{{selectedTooltip.SupervisorName}}</div>
                <div class="row-fluid">
                    <strong>{{$t("Common.Status")}}:</strong>&nbsp;{{selectedTooltip.LastStatus}}</div>
                <div class="row-fluid">
                    <strong>{{$t("Reports.LastUpdatedDate")}}:</strong>&nbsp;{{selectedTooltip.LastUpdatedDate}}</div>
                <div class="row-fluid" style="white-space:nowrap;">
                    <strong>{{$t("MapReport.ViewInterviewContent")}}:</strong>&nbsp;
                    <a v-bind:href="model.api.interviewDetailsUrl + '/' + selectedTooltip.InterviewId" target="_blank">{{$t("MapReport.details")}}</a>
                </div>
            </div>
        </div>
        <div id="map-canvas"></div>
    </HqLayout>
</template>
<script>

import * as toastr from "toastr";
import Vue from "vue";

export default {
    data() {
        return {
            questionnaireId: null,
            gpsQuestionId: null,
            gpsQuestions: null,
            infoWindow: null,
            selectedTooltip: {},
            readyToUpdate: false,
            map: null,
            totalAnswers: 0,
            mapClustererOptions: {
                styles: [
                    {
                        url: "../Content/img/google-maps-markers/m1.png",
                        dark: true
                    },
                    {
                        url: "../Content/img/google-maps-markers/m2.png",
                        dark: false
                    },
                    {
                        url: "../Content/img/google-maps-markers/m3.png",
                        dark: true
                    },
                    {
                        url: "../Content/img/google-maps-markers/m4.png",
                        dark: true
                    },
                    {
                        url: "../Content/img/google-maps-markers/m5.png",
                        dark: true
                    }
                ]
            }
        };
    },
    computed: {
        model() {
            return this.$config.model;
        },
        questionnaires() {
            return this.model.questionnaires;
        }
    },
    mounted() {
        this.setMapCanvasStyle();
        this.initializeMap();

        if (this.questionnaires.length > 0)
            this.selectQuestionnaire(this.questionnaires[0]);
    },
    methods: {
        setMapCanvasStyle() {
            $("body").addClass("map-report");
            var windowHeight = $(window).height();
            var navigationHeight = $(".navbar.navbar-fixed-top").height();
            $("#map-canvas").css(
                "min-height",
                windowHeight - navigationHeight + "px"
            );
        },

        selectQuestionnaire(value) {
            this.questionnaireId = value;

            this.selectGpsQuestion(null);
            this.gpsQuestions = [];

            if (_.isNull(value)) return;

            const self = this;
            this.$http
                .get(
                    this.model.api.gpsQuestionsByQuestionnaireUrl +
                        "/" +
                        this.questionnaireId.key
                )
                .then(response => {
                    self.gpsQuestions = response.data;
                    if (self.gpsQuestions.length > 0) {
                        if (self.gpsQuestions.length === 1) {
                            self.selectGpsQuestion(self.gpsQuestions[0]);
                        }
                    } else {
                        toastr.info(
                            this.$t("MapReport.NoGpsQuestionsByQuestionnaire")
                        );
                    }
                });
        },

        selectGpsQuestion(value) {
            this.gpsQuestionId = value;

            if (_.isNull(value)) {
                this.readyToUpdate = false;
                return;
            }

            this.showPointsOnMap(180, 180, -180, -180, true);
            this.readyToUpdate = true;
        },

        getMapOptions() {
            return {
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
                streetViewControl: false
            };
        },

        initializeMap() {
            const self = this;

            this.map = new google.maps.Map(
                document.getElementById("map-canvas"),
                this.getMapOptions()
            );

            this.infoWindow = new google.maps.InfoWindow();

            this.map.addListener("zoom_changed", () => {
                if (this.gpsQuestionId != null) delayedReload();
            });

            const delayedReload = _.debounce(
                () => this.reloadMarkersInBounds(),
                100
            );

            this.map.addListener("bounds_changed", () => {
                if (this.gpsQuestionId != null) delayedReload();
            });

            this.map.data.setStyle(function(feature) {
                const styles = self.mapClustererOptions.styles;
                const count = feature.getProperty("count");

                if (count > 0) {
                    const max = self.totalAnswers;
                    const percent = (count / max) * styles.length;
                    const index = Math.min(
                        styles.length - 1,
                        Math.round(percent)
                    );
                    const style = styles[index];

                    const ratio = 1;
                    const extend = 20;
                    const radius = 60 + index * extend * ratio;
                    style.scaledSize = new google.maps.Size(radius, radius);
                    style.anchor = new google.maps.Point(
                        radius / 2,
                        radius / 2
                    );
                    return {
                        label: {
                            fontSize: "12px",
                            text: count,
                            color: style.dark ? "#fff" : "#000"
                        },
                        icon: style
                    };
                }
                return {};
            });

            this.map.data.addListener("click", event => {
                
                if (event.feature.getProperty("count") > 0) {
                    const expand = event.feature.getProperty("expand")
                    const sw = new google.maps.LatLng(expand.South, expand.West);
                    const ne = new google.maps.LatLng(expand.North, expand.East);
                    const latlngBounds = new google.maps.LatLngBounds(sw, ne);

                    self.map.fitBounds(latlngBounds);
                } else {
                    const interviewId = event.feature.getProperty(
                        "interviewId"
                    );
                    self.$http
                        .post(self.model.api.interiewSummaryUrl, {
                            InterviewId: interviewId
                        })
                        .then(response => {
                            const data = response.data;

                            if (data == undefined || data == null) return;

                            data["InterviewId"] = interviewId;

                            self.selectedTooltip = data;

                            Vue.nextTick(function() {
                                self.infoWindow.setContent($(self.$refs.tooltip).html())
                                self.infoWindow.setPosition(event.latLng);
                                self.infoWindow.setOptions({
                                  pixelOffset: new google.maps.Size(0, -30)
                                });
                                self.infoWindow.open(self.map);
                            });
                        });
                }
            });

            var washingtonCoordinates = new google.maps.LatLng(
                38.895111,
                -77.036667
            );

            if (!("geolocation" in navigator)) {
                this.map.setCenter(washingtonCoordinates);
            } else {
                navigator.geolocation.getCurrentPosition(
                    position => {
                        self.map.setCenter(
                            new google.maps.LatLng(
                                position.coords.latitude,
                                position.coords.longitude
                            )
                        );
                    },
                    () => {
                        self.map.setCenter(washingtonCoordinates);
                    }
                );
            }
        },

        reloadMarkersInBounds() {
            var bounds = this.map.getBounds();

            if (bounds == null) {
                this.showPointsOnMap(180, 180, -180, -180, true);
            } else {
                this.showPointsOnMap(
                    bounds.getNorthEast().lng(),
                    bounds.getNorthEast().lat(),
                    bounds.getSouthWest().lng(),
                    bounds.getSouthWest().lat(),
                    false
                );
            }
        },

        showPointsOnMap(
            east,
            north,
            west,
            south,
            extendBounds
        ) {
            const zoom = extendBounds ? -1 : this.map.getZoom()
           
            var request = {
                Variable: this.gpsQuestionId.key,
                QuestionnaireId: this.questionnaireId.key,
                Zoom: zoom,
                MapWidth: this.map.getDiv().clientWidth,
                east, north, west, south
            };

            const self = this;

            this.$http
                .post(self.model.api.mapReportUrl, request)
                .then(response => {
                    const toRemove = {};
                    this.totalAnswers = response.data.TotalPoint;

                    this.map.data.forEach(feature => {
                        toRemove[feature.getId()] = feature;
                    });

                    this.map.data.addGeoJson(response.data.FeatureCollection);

                    _.forEach(
                        response.data.FeatureCollection.features,
                        feature => {
                            delete toRemove[feature.id];
                        }
                    );

                    if (extendBounds) {
                        const bounds = response.data.InitialBounds;

                        const sw = new google.maps.LatLng(bounds.South, bounds.West);
                        const ne = new google.maps.LatLng(bounds.North, bounds.East);
                        const latlngBounds = new google.maps.LatLngBounds(
                            sw, ne
                        );

                        self.map.fitBounds(latlngBounds);
                    }

                    _.delay(() => {
                        _.forEach(Object.keys(toRemove), key => {
                            self.map.data.remove(toRemove[key]);
                        });
                    }, 50);
                });
        }
    }
};
</script>
