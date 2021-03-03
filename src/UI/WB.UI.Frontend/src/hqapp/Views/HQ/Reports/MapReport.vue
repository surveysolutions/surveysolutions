<template>
    <HqLayout :hasFilter="true"
        :hasHeader="false">
        <Filters slot="filters">
            <FilterBlock :title="$t('Common.Questionnaire')">
                <Typeahead
                    control-id="questionnaireId"
                    no-clear
                    :placeholder="$t('Common.SelectQuestionnaire')"
                    :value="selectedQuestionnaireId"
                    :values="questionnaires"
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
            <FilterBlock :title="$t('Reports.Variables')">
                <Typeahead
                    control-id="gpsQuestion"
                    :placeholder="$t('Common.AllGpsQuestions')"
                    no-search
                    no-clear
                    :values="gpsQuestions"
                    :value="selectedQuestion"
                    @selected="selectGpsQuestion"/>
            </FilterBlock>
            <FilterBlock>
                <div class="center-block">
                    <Checkbox
                        :label="$t('Reports.HeatMapView')"
                        name="pivot"
                        v-model="showHeatmap"/>
                </div>
            </FilterBlock>
            <FilterBlock :title="$t('Reports.HeatRadius')"
                v-if="showHeatmap">
                <input
                    type="range"
                    min="1"
                    max="200"
                    value="50"
                    class="slider"
                    id="myRange"
                    v-model="heatMapOptions.radius"
                    @change="updateHeatMap"/>
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

            <InterviewFilter slot="additional"
                :questionnaireId="where.questionnaireId"
                :questionnaireVersion="where.questionnaireVersion"
                :value="conditions"
                :exposedValuesFilter="exposedValuesFilter"
                @change="questionFilterChanged"
                @changeFilter="changeexposedValuesFilter" />

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
            <div ref="tooltip">
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
import gql from 'graphql-tag'
import {isNull, chain, debounce, delay, forEach, find, toNumber, isNumber} from 'lodash'
import routeSync from '~/shared/routeSync'
import InterviewFilter from '../Interviews/InterviewQuestionsFilters'

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

const query = gql`query mapReport($workspace: String!, $questionnaireId: Uuid!, $questionnaireVersion: Long, 
$variable: String, $zoom: Int!, $clientMapWidth: Int!, $north: Float!, $south: Float!, $east: Float!, $west: Float!, $where: MapReportFilter) {
  mapReport(workspace: $workspace, questionnaireId:$questionnaireId, questionnaireVersion: $questionnaireVersion, 
  variable: $variable, zoom:$zoom , clientMapWidth: $clientMapWidth ,north: $north, south: $south, east: $east, west:$west, where:$where) {
      report{    
    featureCollection
    {
        type
        features
        {
            id
            type
            geometry            
            properties            
        }
    }
    totalPoint    
    initialBounds
    {
        north
        south
        east
        west
    }
  }
  }
}`

export default {
    components: {
        InterviewFilter,
    },
    mixins: [routeSync],

    data() {
        return {
            gpsQuestions: null,
            infoWindow: null,
            selectedTooltip: {},
            readyToUpdate: false,

            // Mark map data as loaded.
            // required to be true initially, as Google Maps will call bounds_change upon initial load
            isMapReloaded: true,
            map: null,
            heatmap: null,
            showHeatmap: false,
            isLoading: false,
            heatMapOptions: {
                radius: 30,
                dissipating: true,
                gradient: '',
                maxIntensity: null,
            },
            totalAnswers: 0,

            conditions: [],
            exposedValuesFilter: null,
        }
    },

    watch: {
        selectedQuestionnaireId(to) {
            if (to == null) {
                this.showHeatmap = false
            }
        },

        selectedVersion(to) {
            if (to == null) {
                this.showHeatmap = false
            }
        },

        selectedQuestion(to) {
            if (to == null) {
                this.showHeatmap = false
            }

            if (isNull(to)) {
                this.readyToUpdate = false
                return
            }

            this.showPointsOnMap(180, 180, -180, -180, true)
            this.readyToUpdate = true
        },

        showHeatmap() {
            this.reloadMarkersInBounds()
        },
    },

    computed: {
        model() {
            return this.$config.model
        },

        questionnaires() {
            return this.model.questionnaires
        },

        questionnaireVersions() {
            if (this.selectedQuestionnaireId == null) return []
            return this.selectedQuestionnaireId.versions
        },

        selectedVersionValue() {
            return this.selectedVersion == null ? null : this.selectedVersion.key
        },

        api() {
            return this.$hq.Report.MapReport
        },

        queryString() {
            return {
                name: this.query.name,
                version: this.query.version,
                question: this.query.question,
            }
        },

        selectedQuestionnaireId() {
            if (this.query == null || this.query.name == null) {
                return null
            }

            return find(this.model.questionnaires, {
                value: this.query.name,
            })
        },

        selectedVersion() {
            if (this.selectedQuestionnaireId == null || this.query.version == null) return null

            return find(this.selectedQuestionnaireId.versions, {
                key: this.query.version,
            })
        },

        selectedQuestion() {
            if (this.query.question == null || this.gpsQuestions == null) return null
            return find(this.gpsQuestions, {key: this.query.question})
        },
        where() {
            const data = {}

            if (this.selectedQuestionnaireId) data.questionnaireId = this.selectedQuestionnaireId.key
            if (this.selectedVersionValue) data.questionnaireVersion = toNumber(this.selectedVersionValue)

            return data
        },
        whereQuery() {
            const and = []
            if(this.conditions != null && this.conditions.length > 0) {

                var identifyingData = []
                this.conditions.forEach(cond => {
                    if(cond.value == null) return

                    const value_filter = { entity: {variable: {eq: cond.variable}}}
                    const value = isNumber(cond.value) ? cond.value : cond.value.toLowerCase()

                    var field_values = cond.field.split('|')
                    var value_part = {}
                    value_part[field_values[1]] = value
                    value_filter[field_values[0]] = value_part

                    and.push({identifyingData : {some: value_filter}})
                })

            }

            if(this.exposedValuesFilter != null) {
                and.push(this.exposedValuesFilter)
            }

            return and
        },

    },

    mounted() {
        this.setMapCanvasStyle()
        this.initializeMap()

        if (this.selectedQuestionnaireId == null && this.questionnaires.length > 0) {
            this.selectQuestionnaire(this.questionnaires[0])
        } else if (this.selectedQuestionnaireId != null) {
            this.selectQuestionnaire(this.selectedQuestionnaireId)
        }
    },

    methods: {
        questionFilterChanged(conditions) {
            this.conditions = conditions
            this.reloadMarkersInBounds()
        },
        changeexposedValuesFilter(exposedValuesFilter) {
            this.exposedValuesFilter = exposedValuesFilter
            this.reloadMarkersInBounds()
        },
        setMapCanvasStyle() {
            $('body').addClass('map-report')
            var windowHeight = $(window).height()
            var navigationHeight = $('.navbar.navbar-fixed-top').height()
            $('#map-canvas').css('min-height', windowHeight - navigationHeight + 'px')
        },

        updateHeatMap() {
            this.heatmap.setOptions({
                radius: this.heatMapOptions.radius,
                dissipating: this.heatMapOptions.dissipating,
                maxIntensity: this.heatMapOptions.maxIntensity,
            })
        },

        selectQuestionnaireVersion(value) {
            this.questionnaireVersion = value
            this.onChange(s => (s.version = value == null ? null : value.key))
            this.loadQuestions()
        },

        selectQuestionnaire(value) {
            this.questionnaireId = value

            if (this.$route.query.name !== value.value) this.selectQuestionnaireVersion(null)
            else this.selectQuestionnaireVersion(this.$route.query.version ? {key: this.$route.query.version} : null)

            this.selectGpsQuestion(null)
            this.gpsQuestions = []

            this.onChange(q => {
                q.name = value == null ? null : value.value
            })

            if (isNull(value)) return
            this.loadQuestions().then(() => this.onChange(s => (s.name = value.value)))
        },

        loadQuestions() {
            if (this.questionnaireId == null) return

            return this.api
                .GpsQuestionsByQuestionnaire(this.questionnaireId.key, this.selectedVersionValue)
                .then(response => {
                    this.gpsQuestions = chain(response.data)
                        .filter(d => d != null && d != '')
                        .map(d => {
                            return {key: d, value: d}
                        })
                        .value()

                    if (this.gpsQuestions.length > 0) {
                        this.selectGpsQuestion(this.gpsQuestions[0])
                    } else {
                        toastr.info(this.$t('MapReport.NoGpsQuestionsByQuestionnaire'))
                    }
                })
        },

        selectGpsQuestion(value) {
            this.gpsQuestionId = value
            this.onChange(s => (s.question = value == null ? null : value.key))
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

            this.heatmap = new google.maps.visualization.HeatmapLayer({
                map: this.map,
            })

            this.infoWindow = new google.maps.InfoWindow()

            const delayedMapReload = debounce(() => {
                if (this.selectedQuestion == null) return

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
                const count = feature.getProperty('count')

                if (count > 1) {
                    const max = self.totalAnswers
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
                if (event.feature.getProperty('count') > 1) {
                    const expand = event.feature.getProperty('expand')
                    self.map.setZoom(expand)
                    self.map.panTo(event.latLng)
                } else {
                    const interviewId = event.feature.getProperty('interviewId')

                    const response = await this.api.InteriewSummaryUrl(interviewId)
                    const data = response.data

                    if (data != null) {

                        data['interviewId'] = interviewId

                        self.selectedTooltip = data

                        Vue.nextTick(function() {
                            self.infoWindow.setContent($(self.$refs.tooltip).html())
                            self.infoWindow.setPosition(event.latLng)
                            self.infoWindow.setOptions({
                                pixelOffset: new google.maps.Size(0, -30),
                            })
                            self.infoWindow.open(self.map)
                        })
                    }
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

            if (this.selectedQuestionnaireId == null || this.selectedQuestion == null) {
                this.setMapData({
                    featureCollection: [],
                    totalPoint: 0,
                })
                return
            }

            const self = this

            var request = {
                variable: this.selectedQuestion.key,
                questionnaireId: this.selectedQuestionnaireId.key.replaceAll('-',''),
                questionnaireVersion: this.selectedVersionValue ? toNumber(this.selectedVersionValue) : null,
                zoom: this.showHeatmap && zoom != -1 ? zoom + 3 : zoom,
                east,
                north,
                west,
                south,
                clientMapWidth: this.map.getDiv().clientWidth,
                workspace: self.$store.getters.workspace,
            }

            let stillLoading = true

            delay(() => {
                if (stillLoading == true) this.isLoading = true
            }, 5000)

            //const response = await this.api.Report(request)

            const where = {
                and: [...self.whereQuery],
            }

            if(where.and.length > 0) {
                request.where = {interviewFilter : where}
            }

            const report = await this.$apollo.query({
                query,
                variables: request,
                fetchPolicy: 'network-only',
            })

            var mapReport = report.data.mapReport.report
            forEach(mapReport.featureCollection.features, feature => {
                if(!Array.isArray(feature.geometry.coordinates))
                {
                    var coordinates = feature.geometry.coordinates
                    feature.geometry.coordinates = [coordinates.longitude, coordinates.latitude]
                }
            })

            this.setMapData(mapReport, extendBounds)

            stillLoading = false
            this.isLoading = false
        },

        setMapData(data, extendBounds) {
            const toRemove = {}

            this.infoWindow.close(self.map)

            this.totalAnswers = data.totalPoint
            const features = data.featureCollection.features
            const heatmapData = {data: []}

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

                const coords = feature.geometry.coordinates
                const count = feature.properties.count || 1

                heatmapData.data.push({
                    location: new google.maps.LatLng(coords[1], coords[0]),
                    weight: count,
                })
            })

            if (this.showHeatmap) {
                this.map.data.forEach(feature => {
                    this.map.data.remove(feature)
                })

                this.heatmap.setData(heatmapData.data)
            } else {
                this.map.data.addGeoJson(markers)
                this.heatmap.setData([])
            }

            forEach(Object.keys(toRemove), key => {
                this.map.data.remove(toRemove[key])
            })

            if (extendBounds) {
                if (this.totalAnswers === 0) {
                    this.map.setZoom(1)
                } else {
                    const bounds = data.initialBounds

                    const sw = new google.maps.LatLng(bounds.south, bounds.west)
                    const ne = new google.maps.LatLng(bounds.north, bounds.east)
                    const latlngBounds = new google.maps.LatLngBounds(sw, ne)
                    this.isMapReloaded = true
                    this.map.fitBounds(latlngBounds)
                }
            }
        },
    },
}
</script>
