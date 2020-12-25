<template>
    <HqLayout :hasFilter="true"
        :hasHeader="false">
        <Filters slot="filters">
            <FilterBlock :title="$t('Common.Questionnaire')">
                <Typeahead
                    control-id="questionnaireId"
                    :placeholder="$t('Common.AllQuestionnaires')"
                    :ajax-params="{ }"
                    :fetch-url="model.questionnaires"
                    :value="selectedQuestionnaireId"
                    :selectedValue="this.query.questionnaire"
                    v-on:selected="selectQuestionnaire"/>
            </FilterBlock>
            <FilterBlock :title="$t('Common.QuestionnaireVersion')">
                <Typeahead
                    control-id="questionnaireVersion"
                    :placeholder="$t('Common.AllVersions')"
                    :value="selectedVersion"
                    :values="selectedQuestionnaireId == null ? null : selectedQuestionnaireId.versions"
                    v-on:selected="selectQuestionnaireVersion"
                    :disabled="selectedQuestionnaireId == null"/>
            </FilterBlock>
            <FilterBlock :title="$t('Common.Responsible')">
                <Typeahead
                    control-id="responsibleId"
                    :placeholder="$t('Common.AllResponsible')"
                    :value="responsibleId"
                    :ajax-params="responsibleParams"
                    :selectedValue="this.query.responsible"
                    v-on:selected="selectResponsible"
                    :fetch-url="model.responsible"></Typeahead>
            </FilterBlock>
            <FilterBlock :title="$t('Pages.Filters_Assignment')">
                <div class="input-group">
                    <input
                        class="form-control with-clear-btn"
                        :placeholder="$t('Common.AllAssignments')"
                        type="text"
                        v-model="assignmentId"/>
                    <div class="input-group-btn"
                        @click="clearAssignmentFilter">
                        <div class="btn btn-default">
                            <span class="glyphicon glyphicon-remove"
                                aria-hidden="true"></span>
                        </div>
                    </div>
                </div>
            </FilterBlock>
            <FilterBlock v-if="isLoading"
                :title="$t('Reports.MapDataLoading')">
                <div class="progress">
                    <div
                        class="progress-bar progress-bar-striped active"
                        role="progressbar"
                        aria-valuenow="100"
                        aria-valuemin="0"
                        aria-valuemax="100"
                        style="width: 100%"></div>
                </div>
            </FilterBlock>
            <div class="preset-filters-container">
                <div class="center-block"
                    style="margin-left: 0">
                    <button
                        class="btn btn-default btn-lg"
                        id="reloadMarkersInBounds"
                        v-if="readyToUpdate"
                        @click="reloadMarkersInBounds">{{$t("MapReport.ReloadMarkers")}}</button>
                </div>
            </div>
        </Filters>
        <div style="display:none;">
            <div ref="interviewTooltip">
                <div class="row-fluid">
                    <strong>{{$t("Common.InterviewKey")}}:</strong>
                    &nbsp;{{selectedTooltip.interviewKey}}
                </div>
                <div class="row-fluid">
                    <strong>{{$t("Common.Responsible")}}:</strong>
                    &nbsp;{{selectedTooltip.interviewerName}}
                </div>
                <div class="row-fluid">
                    <strong>{{$t("Users.Supervisor")}}:</strong>
                    &nbsp;{{selectedTooltip.supervisorName}}
                </div>
                <div class="row-fluid">
                    <strong>{{$t("Common.Status")}}:</strong>
                    &nbsp;{{selectedTooltip.lastStatus}}
                </div>
                <div class="row-fluid">
                    <strong>{{$t("Reports.LastUpdatedDate")}}:</strong>
                    &nbsp;{{selectedTooltip.lastUpdatedDate}}
                </div>
                <div class="row-fluid"
                    style="white-space:nowrap;">
                    <strong>{{$t("MapReport.ViewInterviewContent")}}:</strong>&nbsp;
                    <a
                        v-bind:href="api.GetInterviewDetailsUrl(selectedTooltip.interviewId)"
                        target="_blank">{{$t("MapReport.details")}}</a>
                </div>
                <div class="row-fluid"
                    style="white-space:nowrap;">
                    <button
                        class="btn btn-lg btn-success"
                        v-if="selectedTooltip.status == 'InterviewerAssigned'"
                        @click="assignInterview">{{ $t("Common.Assign") }}</button>
                    <button
                        class="btn btn-lg btn-success"
                        v-if="selectedTooltip.status == ''"
                        @click="approveInterview">{{ $t("Common.Approve")}}</button>
                    <button
                        class="btn btn-lg reject"
                        v-if="selectedTooltip.status == ''"
                        @click="rejectInterview">{{ $t("Common.Reject")}}</button>
                    <button
                        class="btn btn-lg btn-primary"
                        v-if="selectedTooltip.status == '' && !config.isSupervisor"
                        @click="unapproveInterview">{{ $t("Common.Unapprove")}}</button>
                    <button
                        class="btn btn-link"
                        v-if="selectedTooltip.status == '' && !config.isSupervisor"
                        @click="deleteInterview">{{ $t("Common.Delete")}}</button>
                </div>
            </div>

            <div ref="assignmentTooltip">
                <div class="row-fluid">
                    <strong>{{$t("Assignments.AssignmentId")}}:</strong>
                    &nbsp;{{selectedTooltip.assignmentId}}
                </div>
                <div class="row-fluid">
                    <strong>{{$t("Common.Responsible")}}:</strong>
                    &nbsp;{{selectedTooltip.responsibleName}}
                </div>
                <div class="row-fluid">
                    <strong>{{$t("Reports.LastUpdatedDate")}}:</strong>
                    &nbsp;{{selectedTooltip.lastUpdatedDate}}
                </div>
                <div class="row-fluid"
                    style="white-space:nowrap;">
                    <strong>{{$t("MapDashboard.ViewAssignmentDetails")}}:</strong>&nbsp;
                    <a
                        v-bind:href="api.GetAssignmentDetailsUrl(selectedTooltip.assignmentId)"
                        target="_blank">{{$t("MapReport.details")}}</a>
                </div>
            </div>
        </div>
        <div id="map-canvas"></div>
    </HqLayout>
</template>
<style>
.progress {
    margin: 15px;
}

.progress .progress-bar.active {
    font-weight: 700;
    animation: progress-bar-stripes 0.5s linear infinite;
}

.dotdotdot:after {
    font-weight: 300;
    content: '...';
    display: inline-block;
    width: 20px;
    text-align: left;
    animation: dotdotdot 1.5s linear infinite;
}

@keyframes dotdotdot {
    0% {
        content: '...';
    }
    25% {
        content: '';
    }
    50% {
        content: '.';
    }
    75% {
        content: '..';
    }
}
</style>
<script>
import * as toastr from 'toastr'
import Vue from 'vue'
import {isNull, chain, debounce, delay, forEach, find } from 'lodash'
import routeSync from '~/shared/routeSync'

const mapStyles = [
    {
        url: '/img/google-maps-markers/m1.png',
        dark: true,
    },
    {
        url: '/img/google-maps-markers/m2.png',
        dark: false,
    },
    {
        url: '/img/google-maps-markers/m3.png',
        dark: true,
    },
    {
        url: '/img/google-maps-markers/m4.png',
        dark: true,
    },
    {
        url: '/img/google-maps-markers/m5.png',
        dark: true,
    },
]

export default {
    mixins: [routeSync],

    data() {
        return {
            infoWindow: null,
            selectedTooltip: {},
            readyToUpdate: false,
            isMapReloaded: true,
            map: null,
            isLoading: false,
            totalMarkers: 0,
            selectedQuestionnaireId: null,
            responsibleId: null,
            responsibleParams: {showArchived: true, showLocked: true},
            assignmentId: null,
        }
    },

    watch: {
        'assignmentId'(to) {
            this.onChange(q => {
                q.assignmentId = to
            })
            this.reloadMarkersInBounds()
        },
    },

    computed: {
        model() {
            return this.$config.model
        },

        selectedVersionValue() {
            return this.selectedVersion == null ? null : this.selectedVersion.key
        },

        queryString() {
            return {
                name: this.query.name,
                version: this.query.version,
            }
        },

        selectedVersion() {
            if (this.selectedQuestionnaireId == null || this.query.version == null) return null

            return find(this.selectedQuestionnaireId.versions, {
                key: this.query.version,
            })
        },

        api() {
            return this.$hq.MapDashboard
        },
    },

    mounted() {
        this.setMapCanvasStyle()
        this.initializeMap()
        this.showPointsOnMap(180, 180, -180, -180, true)
    },

    methods: {
        setMapCanvasStyle() {
            $('body').addClass('map-report')
            var windowHeight = $(window).height()
            var navigationHeight = $('.navbar.navbar-fixed-top').height()
            $('#map-canvas').css('min-height', windowHeight - navigationHeight + 'px')
        },

        selectQuestionnaireVersion(value) {
            this.questionnaireVersion = value
            this.onChange(s => (s.version = value == null ? null : value.key))
            this.reloadMarkersInBounds()
        },

        selectQuestionnaire(value) {
            this.selectedQuestionnaireId = value

            if (value == null || this.$route.query.questionnaire !== value.value)
                this.selectQuestionnaireVersion(null)
            else
                this.selectQuestionnaireVersion(this.$route.query.version ? {key: this.$route.query.version} : null)

            this.onChange(q => {
                q.questionnaire = value == null ? null : value.value
            })
        },

        selectResponsible(newValue) {
            this.responsibleId = newValue
            this.onChange(q => {
                q.responsible = newValue == null ? null : newValue.value
            })
            this.reloadMarkersInBounds()
        },

        clearAssignmentFilter() {
            this.assignmentId = null
            this.reloadMarkersInBounds()
        },

        getMapOptions() {
            return {
                zoom: 9,
                mapTypeControl: true,
                mapTypeControlOptions: {
                    style: google.maps.MapTypeControlStyle.HORIZONTAL_BAR,
                    position: google.maps.ControlPosition.TOP_CENTER,
                },
                panControl: true,
                panControlOptions: {
                    position: google.maps.ControlPosition.TOP_RIGHT,
                },
                zoomControl: true,
                zoomControlOptions: {
                    style: google.maps.ZoomControlStyle.LARGE,
                    position: google.maps.ControlPosition.TOP_RIGHT,
                },
                minZoom: 3,
                scaleControl: true,
                streetViewControl: false,
            }
        },

        initializeMap() {
            const self = this

            this.map = new google.maps.Map(document.getElementById('map-canvas'), this.getMapOptions())

            this.infoWindow = new google.maps.InfoWindow()

            const delayedMapReload = debounce(() => {
                // this is required to separate bounds/zoom change by user or because of map data reload
                // i.e. we don't want to load map data twice
                if (this.isMapReloaded == true) {
                    this.isMapReloaded = false
                    return
                }

                this.reloadMarkersInBounds()
            }, 100)

            let mapInitialized = false
            this.map.addListener('zoom_changed', () => delayedMapReload())
            this.map.addListener('bounds_changed', () => {
                if (!mapInitialized) {
                    mapInitialized = true
                    return
                }
                delayedMapReload()
            })

            this.map.data.setStyle(function(feature) {
                const styles = mapStyles
                const type = feature.getProperty('type')

                if (type == 'Interview')
                {
                    const status = feature.getProperty('status')

                    let interviewStyle ={
                        label: {
                            fontSize: '12px',
                            text: '',
                        },
                        icon: {
                            url: '/img/google-maps-markers/m2.png',
                            dark: false,
                        },
                    }

                    if (status == 'Created'
                        || status == 'InterviewerAssigned'
                        || status == 'SupervisorAssigned'
                        || status == 'Restarted'
                    ) {
                        interviewStyle.icon.url = '/img/google-maps-markers/donut.png'
                    }

                    if (status == 'Completed'
                    ) {
                        interviewStyle.icon.url = 'http://maps.google.com/mapfiles/ms/icons/green-dot.png'
                    }

                    if (status == 'RejectedBySupervisor'
                        || status == 'RejectedByHeadquarters'
                    ) {
                        interviewStyle.icon.url = 'http://maps.google.com/mapfiles/ms/icons/red-dot.png'
                    }

                    if (status == 'ApprovedBySupervisor'
                        || status == 'ApprovedByHeadquarters'
                    ) {
                        interviewStyle.icon.url = 'http://maps.google.com/mapfiles/ms/icons/purple-dot.png'
                    }

                    return interviewStyle
                }
                if (type == 'Assignment')
                {
                    let interviewStyle ={
                        label: {
                            fontSize: '12px',
                            text: 'I',
                        },
                        icon: {
                            url: 'http://maps.google.com/mapfiles/ms/icons/white-dot.png',
                        },
                    }
                }
                if (type == 'Cluster') {
                    const count = feature.getProperty('count')
                    const max = self.totalMarkers
                    const percent = (count / max) * styles.length

                    const index = Math.min(styles.length - 1, Math.round(percent))

                    const style = styles[index]

                    const ratio = 1
                    const extend = 20
                    const radius = 60 + index * extend * ratio
                    style.scaledSize = new google.maps.Size(radius, radius)
                    style.anchor = new google.maps.Point(radius / 2, radius / 2)

                    return {
                        label: {
                            fontSize: '12px',
                            text: count,
                            color: style.dark ? '#fff' : '#000',
                        },
                        icon: style,
                    }
                }
                return {}
            })

            this.map.data.addListener('click', async event => {
                const type = event.feature.getProperty('type')

                if (type == 'Cluster') {
                    const expand = event.feature.getProperty('expand')
                    self.map.setZoom(expand)
                    self.map.panTo(event.latLng)
                }
                else if (type == 'Interview')
                {
                    const interviewId = event.feature.getProperty('interviewId')

                    const response = await this.api.InteriewSummaryUrl(interviewId)
                    const data = response.data

                    if (data != null) {

                        data['interviewId'] = interviewId

                        self.selectedTooltip = data

                        Vue.nextTick(function() {
                            self.infoWindow.setContent($(self.$refs.interviewTooltip).html())
                            self.infoWindow.setPosition(event.latLng)
                            self.infoWindow.setOptions({
                                pixelOffset: new google.maps.Size(0, -30),
                            })
                            self.infoWindow.open(self.map)
                        })
                    }
                }
                else if (type == 'Assignment')
                {
                    const assignmentId = event.feature.getProperty('assignmentId')

                    const response = await this.api.AssignmentUrl(assignmentId)
                    const data = response.data
                    data['assignmentId'] = assignmentId

                    self.selectedTooltip = data

                    Vue.nextTick(function() {
                        self.infoWindow.setContent($(self.$refs.assignmentTooltip).html())
                        self.infoWindow.setPosition(event.latLng)
                        self.infoWindow.setOptions({
                            pixelOffset: new google.maps.Size(0, -30),
                        })
                        self.infoWindow.open(self.map)
                    })
                }
            })

            var washingtonCoordinates = new google.maps.LatLng(38.895111, -77.036667)

            if (!('geolocation' in navigator)) {
                this.map.setCenter(washingtonCoordinates)
            } else {
                navigator.geolocation.getCurrentPosition(
                    position => {
                        self.map.setCenter(new google.maps.LatLng(position.coords.latitude, position.coords.longitude))
                    },
                    () => {
                        self.map.setCenter(washingtonCoordinates)
                    }
                )
            }
        },

        reloadMarkersInBounds() {
            var bounds = this.map.getBounds()

            if (bounds == null) {
                this.showPointsOnMap(180, 180, -180, -180, true)
            } else {
                this.showPointsOnMap(
                    bounds.getNorthEast().lng(),
                    bounds.getNorthEast().lat(),
                    bounds.getSouthWest().lng(),
                    bounds.getSouthWest().lat(),
                    false
                )
            }
        },

        async showPointsOnMap(east, north, west, south, extendBounds) {
            const zoom = extendBounds ? -1 : this.map.getZoom()

            var request = {
                QuestionnaireId: (this.selectedQuestionnaireId || {}).key || null,
                QuestionnaireVersion: this.selectedVersionValue,
                ResponsibleId: (this.responsibleId || {}).key || null,
                AssignmentId: this.assignmentId,
                Zoom: zoom,
                east,
                north,
                west,
                south,
                clientMapWidth: this.map.getDiv().clientWidth,
            }

            const self = this

            let stillLoading = true

            delay(() => {
                if (stillLoading == true) this.isLoading = true
            }, 5000)

            const response = await this.api.GetMarkers(request)

            this.setMapData(response.data, extendBounds)

            stillLoading = false
            this.isLoading = false
        },

        setMapData(data, extendBounds) {
            const toRemove = {}

            this.infoWindow.close(self.map)

            this.totalMarkers = data.totalPoint
            const features = data.featureCollection.features

            this.map.data.forEach(feature => {
                toRemove[feature.getId()] = feature
            })

            const markers = {
                features: [],
                type: 'FeatureCollection',
            }

            forEach(features, feature => {
                if (!extendBounds && toRemove[feature.id]) {
                    delete toRemove[feature.id]
                } else {
                    markers.features.push(feature)
                }
            })

            this.map.data.addGeoJson(markers)

            forEach(Object.keys(toRemove), key => {
                this.map.data.remove(toRemove[key])
            })

            if (extendBounds) {
                if (this.totalMarkers === 0) {
                    this.map.setZoom(1)
                } else {
                    const sw = new google.maps.LatLng(data.bounds.south, data.bounds.west)
                    const ne = new google.maps.LatLng(data.bounds.north, data.bounds.east)
                    const latlngBounds = new google.maps.LatLngBounds(sw, ne)
                    this.isMapReloaded = true
                    this.map.fitBounds(latlngBounds)
                }
            }
        },
    },
}
</script>
