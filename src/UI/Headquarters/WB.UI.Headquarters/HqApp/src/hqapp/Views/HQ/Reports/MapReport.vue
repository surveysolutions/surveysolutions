<template>
    <HqLayout :hasFilter="true" :hasHeader="false">
        <Filters slot="filters">
            <FilterBlock :title="$t('Reports.Questionnaire')">
                <Typeahead :placeholder="$t('Common.AllQuestionnaires')" :values="questionnaires" :value="questionnaireId" noSearch @selected="selectQuestionnaire" />
            </FilterBlock>
            <FilterBlock :title="$t('Reports.Variables')">
                <Typeahead :placeholder="$t('Common.AllGpsQuestions')" :values="gpsQuestions" :value="gpsQuestionId" noSearch @selected="selectGpsQuestion" />
            </FilterBlock>
            <FilterBlock v-if="markerlimitReached">
                <div class="alert-warning">
                    <span>{{$t('MapReport.NotAllMarkers')}}</span>
                </div>
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
import MarkerClusterer from "@google/markerclustererplus";
import infoBubble from "js-info-bubble";
import * as toastr from "toastr";
import Vue from "vue";

export default {
  data() {
    return {
      questionnaireId: null,
      gpsQuestionId: null,
      gpsQuestions: null,
      interviewDetailsTooltip: new InfoBubble(),
      selectedTooltip: {},
      readyToUpdate: false,
      map: null,
      markers: [],
      markersLimit: 50000,
      markerlimitReached: false,
      mapClusterer: null,
      mapClustererOptions: {
        gridSize: 50,
        maxZoom: 15,
        styles: [
          {
            height: 53,
            url: "../Content/img/google-maps-markers/m1.png",
            width: 53
          },
          {
            height: 56,
            url: "../Content/img/google-maps-markers/m2.png",
            width: 56
          },
          {
            height: 66,
            url: "../Content/img/google-maps-markers/m3.png",
            width: 66
          },
          {
            height: 78,
            url: "../Content/img/google-maps-markers/m4.png",
            width: 78
          },
          {
            height: 90,
            url: "../Content/img/google-maps-markers/m5.png",
            width: 90
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
        .get(this.model.api.gpsQuestionsByQuestionnaireUrl + "/" + this.questionnaireId.key)
        .then(response => {
          self.gpsQuestions = response.data;
          if (self.gpsQuestions.length > 0) {
            if (self.gpsQuestions.length === 1) {
              self.selectGpsQuestion(self.gpsQuestions[0]);
            }
          } else {
            toastr.info(this.$t("MapReport.NoGpsQuestionsByQuestionnaire"));
          }
        });
    },
    selectGpsQuestion(value) {
      this.gpsQuestionId = value;

      this.clearAllMarkers();

      if (_.isNull(value)) {
        this.readyToUpdate = false;
        return;
      }

      this.showPointsOnMap(180, 90, -180, -90, true);
      this.readyToUpdate = true;
    },

    clearAllMarkers() {
      for (var i = 0; i < this.markers.length; i++) {
        this.markers[i].setMap(null);
      }

      this.markers = [];
      this.mapClusterer.clearMarkers();
      this.interviewDetailsTooltip.close();
      this.markerlimitReached = false;
    },

    initializeMap() {
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
        streetViewControl: false
      };

      this.map = new google.maps.Map(
        document.getElementById("map-canvas"),
        mapOptions
      );
      this.mapClusterer = new MarkerClusterer(
        this.map,
        [],
        this.mapClustererOptions
      );

      var washingtonCoordinates = new google.maps.LatLng(38.895111, -77.036667);

      var self = this;

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
      this.clearAllMarkers();
      var bounds = this.map.getBounds();

      this.showPointsOnMap(
        bounds.getNorthEast().lng(),
        bounds.getNorthEast().lat(),
        bounds.getSouthWest().lng(),
        bounds.getSouthWest().lat(),
        false
      );
    },
    showPointsOnMap(
      northEastCornerLongtitude,
      northEastCornerLatitude,
      southWestCornerLongtitude,
      southWestCornerLatitude,
      extendBounds
    ) {
      var request = {
        Variable: this.gpsQuestionId.key,
        QuestionnaireId: this.questionnaireId.key,

        NorthEastCornerLongtitude: northEastCornerLongtitude,
        NorthEastCornerLatitude: northEastCornerLatitude,
        SouthWestCornerLongtitude: southWestCornerLongtitude,
        SouthWestCornerLatitude: southWestCornerLatitude
      };

      const self = this;

      this.$http.post(self.model.api.mapReportUrl, request).then(response => {
        var mapPoints = response.data.Points;

        if (mapPoints.length == 0) {
          toastr.error(window.input.settings.messages.notifyNoMarkersText);
          return;
        }

        if (mapPoints.length >= self.markersLimit) {
          toastr.error(
            window.input.settings.messages.notifyNoMarkersLimitReachedText
          );
          self.markerlimitReached = true;
        }

        var bounds = new google.maps.LatLngBounds();

        for (var i = 0; i < mapPoints.length; i++) {
          var mapPoint = mapPoints[i];
          var mapPointAnswers = mapPoint.Answers.split("|");

          for (var j = 0; j < mapPointAnswers.length; j++) {
            var points = mapPointAnswers[j].split(";");
            var marker = new google.maps.Marker({
              position: new google.maps.LatLng(points[0] * 1, points[1] * 1)
            });
            marker.interviewId = mapPoint.Id;

            google.maps.event.addListener(marker, "click", function() {
              var marker = this;
              self.$http
                .post(self.model.api.interiewSummaryUrl, {
                  InterviewId: marker.interviewId
                })
                .then(response => {
                  const data = response.data;

                  if (data == undefined || data == null) return;

                  data["InterviewId"] = marker.interviewId;

                  self.selectedTooltip = data;

                  Vue.nextTick(function() {
                    self.interviewDetailsTooltip.setContent(
                      $(self.$refs.tooltip).html()
                    );
                    self.interviewDetailsTooltip.open(self.map, marker);
                  });
                });
            });
            self.markers.push(marker);
            if (extendBounds) bounds.extend(marker.getPosition());
          }
        }

        self.mapClusterer.addMarkers(self.markers);
        if (extendBounds) self.map.fitBounds(bounds);
      });
    }
  }
};
</script>
