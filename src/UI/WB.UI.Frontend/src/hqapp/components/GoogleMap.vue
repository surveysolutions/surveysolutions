<template>
    <div style="display: none">
        <div ref="interviewTooltip">
            <div class="row-fluid">
                <strong>{{ $t('Common.InterviewKey') }}:</strong>
                &nbsp;{{ selectedTooltip.interviewKey }}
            </div>
            <div class="row-fluid">
                <strong>{{ $t('Common.Responsible') }}:</strong>
                &nbsp;{{ selectedTooltip.interviewerName }}
            </div>
            <div class="row-fluid">
                <strong>{{ $t('Users.Supervisor') }}:</strong>
                &nbsp;{{ selectedTooltip.supervisorName }}
            </div>
            <div class="row-fluid">
                <strong>{{ $t('Common.Status') }}:</strong>
                &nbsp;{{ selectedTooltip.lastStatus }}
            </div>
            <div class="row-fluid">
                <strong>{{ $t('Reports.LastUpdatedDate') }}:</strong>
                &nbsp;{{ selectedTooltip.lastUpdatedDate }}
            </div>
            <div class="row-fluid" v-for="answer in selectedTooltip.identifyingData">
                <strong>{{ answer.title }}:</strong>
                &nbsp;{{ answer.answer || $t('Details.NoAnswer') }}
            </div>

            <div class="row-fluid" v-if="model.userRole != 'Interviewer'" style="white-space: nowrap">
                <strong>{{ $t('MapReport.ViewInterviewContent') }}:</strong>&nbsp;
                <a v-bind:href="api.GetInterviewDetailsUrl(
                    selectedTooltip.interviewId
                )
                    " target="_blank">{{ $t('MapReport.details') }}</a>
            </div>
            <div class="row-fluid tooltip-buttons" style="white-space: nowrap" v-if="!model.isObserving">
                <button class="btn btn-sm btn-primary" v-if="model.userRole == 'Interviewer' &&
                    (selectedTooltip.status == 'InterviewerAssigned' ||
                        selectedTooltip.status ==
                        'RejectedBySupervisor')
                " click-method="openInterview">
                    {{ $t('Common.Open') }}
                </button>
                <button class="btn btn-sm btn-primary" v-if="canAssign" click-method="assignInterview">
                    {{ $t('Common.Assign') }}
                </button>
                <button class="btn btn-sm btn-primary" v-if="model.userRole == 'Supervisor' &&
                    (selectedTooltip.status == 'Completed' ||
                        selectedTooltip.status ==
                        'RejectedByHeadquarters')
                " click-method="approveSvInterview">
                    {{ $t('Common.Approve') }}
                </button>
                <button class="btn btn-sm reject" v-if="model.userRole == 'Supervisor' &&
                    (selectedTooltip.status == 'Completed' ||
                        selectedTooltip.status ==
                        'RejectedByHeadquarters')
                " click-method="rejectSvInterview">
                    {{ $t('Common.Reject') }}
                </button>
                <button class="btn btn-sm btn-primary" v-if="model.userRole == 'Headquarter' &&
                    (selectedTooltip.status == 'Completed' ||
                        selectedTooltip.status ==
                        'ApprovedBySupervisor')
                " click-method="approveHqInterview">
                    {{ $t('Common.Approve') }}
                </button>
                <button class="btn btn-sm reject" v-if="model.userRole == 'Headquarter' &&
                    (selectedTooltip.status == 'Completed' ||
                        selectedTooltip.status ==
                        'ApprovedBySupervisor')
                " click-method="rejectHqInterview">
                    {{ $t('Common.Reject') }}
                </button>
                <button class="btn btn-sm btn-primary" v-if="model.userRole == 'Headquarter' &&
                    selectedTooltip.status == 'ApprovedByHeadquarters'
                " click-method="unapproveInterview">
                    {{ $t('Common.Unapprove') }}
                </button>
            </div>
        </div>

        <div ref="assignmentTooltip">
            <div class="row-fluid">
                <strong>{{ $t('Assignments.AssignmentId') }}:</strong>
                &nbsp;{{ selectedTooltip.assignmentId }}
            </div>
            <div class="row-fluid">
                <strong>{{ $t('Common.Responsible') }}:</strong>
                &nbsp;{{ selectedTooltip.responsibleName }}
            </div>
            <div class="row-fluid">
                <strong>{{ $t('Reports.LastUpdatedDate') }}:</strong>
                &nbsp;{{ selectedTooltip.lastUpdatedDate }}
            </div>
            <div class="row-fluid" v-for="answer in selectedTooltip.identifyingData">
                <strong>{{ answer.title }}:</strong>
                &nbsp;{{ answer.answer || $t('Details.NoAnswer') }}
            </div>

            <div class="row-fluid" v-if="model.userRole != 'Interviewer'" style="white-space: nowrap">
                <strong>{{ $t('Common.ViewAssignmentDetails') }}:</strong>&nbsp;
                <a v-bind:href="api.GetAssignmentDetailsUrl(
                    selectedTooltip.assignmentId
                )
                    " target="_blank">{{ $t('MapReport.details') }}</a>
            </div>
            <div class="row-fluid tooltip-buttons" style="white-space: nowrap" v-if="!model.isObserving">
                <button class="btn btn-sm btn-primary" v-if="model.userRole == 'Supervisor' ||
                    model.userRole == 'Headquarter'
                " click-method="assignAssignment">
                    {{ $t('Common.Assign') }}
                </button>

                <button class="btn btn-sm btn-assignment" v-if="model.userRole == 'Interviewer'"
                    click-method="createInterview">
                    {{ $t('Common.Create') }}
                </button>
            </div>
        </div>

        <div ref="clusterTooltip">
            <div class="row-fluid">
                <strong>{{ $t('Reports.ClusterInfo') }}</strong>
            </div>
            <div class="row-fluid" v-if="selectedTooltip.interviewsCount > 0">
                <strong>{{ $t('Common.Interviews') }}:</strong>
                &nbsp;{{ selectedTooltip.interviewsCount }}
            </div>
            <div class="row-fluid" v-if="selectedTooltip.assignmentsCount > 0">
                <strong>{{ $t('Common.Assignments') }}:</strong>
                &nbsp;{{ selectedTooltip.assignmentsCount }}
            </div>
        </div>
    </div>
    <div id="map-canvas"></div>

    <ModalFrame ref="assignModal" :title="$t('Common.Assign')">
        <form onsubmit="return false;">
            <div class="form-group">
                <label class="control-label" for="newResponsibleId">{{
                    $t('Assignments.SelectResponsible')
                }}</label>
                <Typeahead control-id="newResponsibleId" :placeholder="$t('Common.Responsible')"
                    :value="newResponsibleId" :ajax-params="{}" @selected="newResponsibleSelected"
                    :fetch-url="model.responsible"></Typeahead>
            </div>
            <div v-if="selectedTooltip.isReceivedByTablet">
                <br />
                <input type="checkbox" id="reassignReceivedByTablet" v-model="isReassignReceivedByTablet"
                    class="checkbox-filter" />
                <label for="reassignReceivedByTablet" style="font-weight: normal">
                    <span class="tick"></span>
                    {{ $t('Reports.AssignReceivedConfirm', 1) }}
                </label>
                <br />
                <span v-if="isReassignReceivedByTablet" class="text-danger">
                    {{ $t('Reports.AssignReceivedWarning') }}
                </span>
            </div>
        </form>
        <template v-slot:actions>
            <div>
                <button type="button" class="btn btn-primary" role="confirm" @click="assign"
                    :disabled="!canClickAssign">
                    {{ $t('Common.Assign') }}
                </button>
                <button type="button" class="btn btn-link" data-bs-dismiss="modal" role="cancel">
                    {{ $t('Common.Cancel') }}
                </button>
            </div>
        </template>
    </ModalFrame>
</template>

<style scoped>
.reject {
    padding-top: 3px;
    padding-bottom: 3px;
}

.tooltip-buttons {
    margin-top: 5px;
}

.tooltip-buttons button {
    margin-right: 10px;
}
</style>

<script>
import { nextTick } from 'vue'
import { debounce, delay, forEach, find } from 'lodash'
import routeSync from '~/shared/routeSync'
import { Form, Field } from 'vee-validate'
import moment from 'moment'
import { DateFormats } from '~/shared/helpers'

export default {
    name: "MapWithMarkers",
    mixins: [routeSync],

    components: { Form, Field },

    props: {
        shapefile: { type: String, default: null },
        getMarkersParams: { type: Object, default: {} },
    },
    expose: ['init', 'isLoading', 'map', 'reloadMarkersInBounds'],
    emits: ['initialized'],

    data() {
        return {
            infoWindow: null,
            selectedTooltip: {},
            readyToUpdate: false,
            isMapReloaded: true,
            map: null,
            isLoading: false,
            totalMarkers: 0,
            responsibleId: null,
            responsibleParams: { showArchived: true, showLocked: true },
            newResponsibleId: null,
            isReassignReceivedByTablet: false,
            geoJsonFeatures: null,
        }
    },

    async mounted() {
        await this.init()
    },
    watch: {
        shapefileName(to) {
            nextTick(() => {
                this.displayShapefileName()
            })
        },
    },

    computed: {
        model() {
            return this.$config.model
        },

        shapefileName() {
            return this.shapefile;
        },

        canAssign() {
            if (this.selectedTooltip.assignmentId) {
                if (
                    this.model.userRole == 'Supervisor' ||
                    this.model.userRole == 'Headquarter'
                )
                    return true
            }

            if (
                this.model.userRole == 'Supervisor' &&
                (this.selectedTooltip.status == 'InterviewerAssigned' ||
                    this.selectedTooltip.status == 'SupervisorAssigned' ||
                    this.selectedTooltip.status == 'RejectedBySupervisor')
            )
                return true

            if (
                this.model.userRole == 'Headquarter' &&
                (this.selectedTooltip.status == 'InterviewerAssigned' ||
                    this.selectedTooltip.status == 'RejectedBySupervisor' ||
                    this.selectedTooltip.status == 'RejectedByHeadquarters')
            )
                return true

            return false
        },

        canClickAssign() {
            if (this.newResponsibleId) {
                if (this.selectedTooltip.isReceivedByTablet)
                    return this.isReassignReceivedByTablet
                return true
            }
            return false
        },

        api() {
            return this.$hq.MapDashboard
        },
    },

    methods: {

        async init() {
            this.setMapCanvasStyle()
            await this.initializeMap()
            await this.displayShapefileName();

            this.showPointsOnMap(180, 180, -180, -180, false)

            this.$emit('initialized');
        },

        openInterview() {
            window.open(
                this.$hq.basePath +
                'InterviewerHq/OpenInterview/' +
                this.selectedTooltip.interviewId,
                '_blank'
            )
        },

        createInterview() {
            const self = this
            const assignmentId = this.selectedTooltip.assignmentId
            $.post(
                {
                    url: 'InterviewerHq/StartNewInterview/' + assignmentId,
                    headers: {
                        'X-CSRF-TOKEN': self.$hq.Util.getCsrfCookie(),
                    },
                },
                (response) => {
                    const interviewId = response.interviewId
                    const workspace = self.$hq.basePath
                    const url = `${workspace}WebInterview/${interviewId}/Cover`
                    window.open(url, '_blank')
                    self.reloadMarkersInBounds()
                }
            )
        },

        assignInterview() {
            this.newResponsibleId = null
            this.isReassignReceivedByTablet = false
            this.$refs.assignModal.modal({ keyboard: false })
        },

        async assign() {
            if (this.selectedTooltip.interviewId)
                await this.sendAssignInterview()
            else if (this.selectedTooltip.assignmentId)
                await this.sendAssignAssignment()
        },

        async sendAssignInterview() {
            if (this.newResponsibleId.iconClass === 'supervisor')
                await this.$hq.InterviewsPublicApi.SvAssign(
                    this.selectedTooltip.interviewId,
                    this.newResponsibleId.key
                )
            else
                await this.$hq.InterviewsPublicApi.Assign(
                    this.selectedTooltip.interviewId,
                    this.newResponsibleId.key
                )

            this.$refs.assignModal.hide()
            this.newResponsibleId = null
            await this.refreshInterviewData()
        },

        assignAssignment() {
            this.newResponsibleId = null
            this.isReassignReceivedByTablet = false
            this.$refs.assignModal.modal({ keyboard: false })
        },

        async sendAssignAssignment() {
            await this.$hq.Assignments.assignResponsible(
                this.selectedTooltip.assignmentId,
                this.newResponsibleId.key
            )
            this.$refs.assignModal.hide()
            this.newResponsibleId = null
            await this.refreshAssignmentData()
        },

        async approveSvInterview() {
            await this.$hq.InterviewsPublicApi.SvApprove(
                this.selectedTooltip.interviewId
            )
            await this.refreshInterviewData()
        },

        async approveHqInterview() {
            await this.$hq.InterviewsPublicApi.HqApprove(
                this.selectedTooltip.interviewId
            )
            await this.refreshInterviewData()
        },

        async rejectSvInterview() {
            await this.$hq.InterviewsPublicApi.SvReject(
                this.selectedTooltip.interviewId
            )
            await this.refreshInterviewData()
        },

        async rejectHqInterview() {
            await this.$hq.InterviewsPublicApi.HqReject(
                this.selectedTooltip.interviewId
            )
            await this.refreshInterviewData()
        },

        async unapproveInterview() {
            await this.$hq.InterviewsPublicApi.HqUnapprove(
                this.selectedTooltip.interviewId
            )
            await this.refreshInterviewData()
        },

        async refreshInterviewData() {
            const self = this
            const interviewId = this.selectedTooltip.interviewId
            const marker = this.selectedTooltip.marker
            const response = await this.api.InteriewSummaryUrl(interviewId)
            const data = response.data

            if (data != null) {
                data['interviewId'] = interviewId
                data['marker'] = marker
                self.selectedTooltip = data

                nextTick(function () {
                    self.infoWindow.setContent(
                        $(self.$refs.interviewTooltip).html()
                    )
                })
            }

            marker.setProperty('status', this.selectedTooltip.status)
        },

        async refreshAssignmentData() {
            const self = this
            const assignmentId = this.selectedTooltip.assignmentId
            const marker = this.selectedTooltip.marker
            const response = await this.api.AssignmentUrl(assignmentId)
            const data = response.data

            if (data != null) {
                data['assignmentId'] = assignmentId
                data['marker'] = marker
                self.selectedTooltip = data

                nextTick(function () {
                    self.infoWindow.setContent(
                        $(self.$refs.assignmentTooltip).html()
                    )
                })
            }
        },

        setMapCanvasStyle() {
            $('body').addClass('map-report')
            var windowHeight = $(window).height()
            var navigationHeight = $('.navbar.navbar-fixed-top').height()
            $('#map-canvas').css(
                'min-height',
                windowHeight - navigationHeight + 'px'
            )
        },

        async displayShapefileName() {
            if (this.geoJsonFeatures) {
                for (let i = 0; i < this.geoJsonFeatures.length; i++)
                    this.map.data.remove(this.geoJsonFeatures[i])
                this.geoJsonFeatures = null
            }

            if (this.shapefileName) {
                const geoJsonUrl =
                    this.model.shapefileJson +
                    '?mapName=' +
                    encodeURIComponent(this.shapefileName)

                this.isLoading = true

                const data = await $.getJSON(geoJsonUrl)
                const json = JSON.parse(data.geoJson)
                this.geoJsonFeatures = this.map.data.addGeoJson(json)

                const sw = new google.maps.LatLng(data.yMax, data.xMin)
                const ne = new google.maps.LatLng(data.yMin, data.xMax)
                const latlngBounds = new google.maps.LatLngBounds(sw, ne)
                this.map.fitBounds(latlngBounds)

                this.isLoading = false

                this.reloadMarkersInBounds()
            }
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

        async initializeMap() {
            const self = this

            const { Map } = await google.maps.importLibrary("maps");
            //const { Marker } = await google.maps.importLibrary("marker");
            //const { AdvancedMarkerElement } = await google.maps.importLibrary("marker")

            const mapDiv = document.getElementById('map-canvas')
            this.map = new Map(mapDiv, this.getMapOptions())

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

            this.map.data.setStyle(function (feature) {
                const type = feature.getProperty('type')

                if (type == 'Interview') {
                    const userRole = self.model.userRole
                    const status = feature.getProperty('status')

                    let interviewStyle = {
                        icon: {
                            url: '/img/google-maps-markers/m2.png',
                            dark: false,
                        },
                    }

                    let action = ''
                    let markerForm = ''

                    if (userRole == 'Interviewer') {
                        markerForm = 'circle'

                        switch (status) {
                            case 'Created':
                            case 'InterviewerAssigned':
                            case 'Restarted':
                                action = 'action'
                                break
                            case 'RejectedBySupervisor':
                            case 'RejectedByHeadquarters':
                                action = 'reject'
                                break
                            default:
                                action = 'noaction'
                                break
                        }
                    } else if (userRole == 'Supervisor') {
                        switch (status) {
                            case 'Completed':
                            case 'SupervisorAssigned':
                                markerForm = 'thomb'
                                action = 'action'
                                break
                            case 'RejectedByHeadquarters':
                                markerForm = 'thomb'
                                action = 'reject'
                                break
                            default:
                                markerForm = 'circle'
                                action = 'noaction'
                                break
                        }
                    } else if (userRole == 'Headquarter') {
                        switch (status) {
                            case 'ApprovedBySupervisor':
                                markerForm = 'triangle'
                                action = 'action'
                                break
                            case 'UnapprovedByHeadquarters':
                                markerForm = 'triangle'
                                action = 'reject'
                                break
                            case 'ApprovedByHeadquarters':
                                markerForm = 'triangle'
                                action = 'noaction'
                                break

                            case 'Created':
                            case 'InterviewerAssigned':
                            case 'Restarted':
                            case 'RejectedBySupervisor':
                                markerForm = 'circle'
                                action = 'noaction'
                                break

                            case 'Completed':
                            case 'SupervisorAssigned':
                            case 'RejectedByHeadquarters':
                                markerForm = 'thomb'
                                action = 'noaction'
                                break

                            default:
                                markerForm = 'circle'
                                action = 'noaction'
                                break
                        }
                    }

                    interviewStyle.icon.url = `/img/google-maps-markers/${markerForm}-${action}.png`
                    return interviewStyle
                }
                if (type == 'Assignment') {
                    const rRole = feature.getProperty('responsibleRole')
                    let markerForm = ''
                    switch (rRole) {
                        case 'Interviewer':
                            markerForm = 'circle'
                            break
                        case 'Supervisor':
                            markerForm = 'thomb'
                            break
                        default:
                            markerForm = 'triangle'
                            break
                    }
                    return {
                        icon: {
                            url: `/img/google-maps-markers/${markerForm}-assignment-x.png`,
                        },
                    }
                }
                if (type == 'Cluster') {
                    const iconStyle = {
                        url: '/img/google-maps-markers/m1.png',
                        dark: true,
                    }
                    const count = feature.getProperty('count')
                    if (count < 10) {
                        iconStyle.url = '/img/google-maps-markers/m1.png'
                    } else if (count < 100) {
                        iconStyle.url = '/img/google-maps-markers/m2.png'
                        iconStyle.dark = false
                    } else if (count < 1000) {
                        iconStyle.url = '/img/google-maps-markers/m3.png'
                    } else if (count < 10000) {
                        iconStyle.url = '/img/google-maps-markers/m4.png'
                    } else {
                        iconStyle.url = '/img/google-maps-markers/m5.png'
                    }

                    const max = self.totalMarkers
                    const percent = (count / max) * 5

                    const index = Math.min(4, Math.round(percent))

                    const extend = 20
                    const radius = 60 + index * extend
                    iconStyle.scaledSize = new google.maps.Size(radius, radius)
                    iconStyle.anchor = new google.maps.Point(
                        radius / 2,
                        radius / 2
                    )

                    return {
                        label: {
                            fontSize: '12px',
                            text: count,
                            color: iconStyle.dark ? '#fff' : '#000',
                        },
                        icon: iconStyle,
                    }
                }

                if (type == null) {
                    //"Point", "MultiPoint", "LineString", "MultiLineString", "LinearRing", "Polygon", "MultiPolygon", or "GeometryCollection"
                    return {
                        strokeColor: 'red',
                        strokeWeight: 2,
                    }
                }

                return {}
            })

            this.map.data.addListener('click', async (event) => {
                const type = event.feature.getProperty('type')

                if (type == 'Cluster') {
                    //const expand = event.feature.getProperty('expand')

                    var mapZoomService = new google.maps.MaxZoomService()
                    const currentZoom = this.map.getZoom()
                    await mapZoomService.getMaxZoomAtLatLng(
                        event.latLng,
                        (result) => {
                            if (result.zoom <= currentZoom) {
                                const interviewsCount =
                                    event.feature.getProperty('interviewsCount')
                                const assignmentsCount =
                                    event.feature.getProperty(
                                        'assignmentsCount'
                                    )

                                var data = {
                                    assignmentsCount: assignmentsCount,
                                    interviewsCount: interviewsCount,
                                }

                                self.selectedTooltip = data

                                nextTick(function () {
                                    self.infoWindow.setContent(
                                        $(self.$refs.clusterTooltip).html()
                                    )
                                    self.infoWindow.setPosition(event.latLng)
                                    self.infoWindow.setOptions({
                                        pixelOffset: new google.maps.Size(
                                            0,
                                            -30
                                        ),
                                    })
                                    self.infoWindow.open(self.map)
                                })
                            } else {
                                self.map.setZoom(currentZoom + 1)
                                self.map.panTo(event.latLng)
                            }
                        }
                    )
                } else if (type == 'Interview') {
                    const interviewId = event.feature.getProperty('interviewId')

                    const response = await this.api.InteriewSummaryUrl(
                        interviewId
                    )
                    const data = response.data

                    if (data != null) {
                        data['interviewId'] = interviewId
                        data['marker'] = event.feature

                        self.selectedTooltip = data

                        nextTick(function () {
                            self.infoWindow.setContent(
                                $(self.$refs.interviewTooltip).html()
                            )
                            self.infoWindow.setPosition(event.latLng)
                            self.infoWindow.setOptions({
                                pixelOffset: new google.maps.Size(0, -30),
                            })
                            self.infoWindow.open(self.map)
                        })
                    }
                } else if (type == 'Assignment') {
                    const assignmentId =
                        event.feature.getProperty('assignmentId')

                    const response = await this.api.AssignmentUrl(assignmentId)
                    const data = response.data
                    data['assignmentId'] = assignmentId
                    data['marker'] = event.feature

                    self.selectedTooltip = data

                    nextTick(function () {
                        self.infoWindow.setContent(
                            $(self.$refs.assignmentTooltip).html()
                        )
                        self.infoWindow.setPosition(event.latLng)
                        self.infoWindow.setOptions({
                            pixelOffset: new google.maps.Size(0, -30),
                        })
                        self.infoWindow.open(self.map)
                    })
                }

                const label = event.feature.getProperty('label')
                if (label) {
                    nextTick(function () {
                        self.infoWindow.setContent(label)
                        self.infoWindow.setPosition(event.latLng)
                        self.infoWindow.open(self.map)
                    })
                }
            })

            google.maps.event.addDomListener(mapDiv, 'click', (event) => {
                if (event.srcElement.nodeName == 'BUTTON') {
                    var methodName = event.srcElement.getAttribute('click-method')
                    if (methodName) self[methodName].call(self)
                }
            })

            let washingtonCoordinates = new google.maps.LatLng(
                38.895111,
                -77.036667
            )

            if (!('geolocation' in navigator)) {
                this.map.setCenter(washingtonCoordinates)
            } else {
                navigator.geolocation.getCurrentPosition(
                    (position) => {
                        self.map.setCenter(
                            new google.maps.LatLng(
                                position.coords.latitude,
                                position.coords.longitude
                            )
                        )
                    },
                    () => {
                        self.map.setCenter(washingtonCoordinates)
                    }
                )
            }
        },

        reloadMarkersInBounds() {
            if (this.map == null) return

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
            /*const center = this.map.getCenter()
            var mapZoomService = new google.maps.MaxZoomService()
            const coor = {lng: center.lng(), lat: center.lat()}
            const maxZoom = await mapZoomService.getMaxZoomAtLatLng(coor)*/
            const maxZoom = 19

            let request = {
                //QuestionnaireId: (this.selectedQuestionnaireId || {}).key || null,
                //QuestionnaireVersion: this.selectedVersionValue,
                //ResponsibleId: (this.responsibleId || {}).key || null,
                //AssignmentId: this.assignmentId,
                Zoom: zoom,
                MaxZoom: maxZoom,
                east,
                north,
                west,
                south,
                clientMapWidth: this.map.getDiv().clientWidth,
            }

            request = Object.assign(request, this.getMarkersParams)

            let stillLoading = true

            delay(() => {
                if (stillLoading == true) this.isLoading = true
            }, 5000)

            try {
                const response = await this.api.GetMarkers(request)
                this.setMapData(response.data, extendBounds)
            } finally {
                stillLoading = false
                this.isLoading = false
            }
        },

        setMapData(data, extendBounds) {
            const toRemove = {}

            this.infoWindow.close(self.map)

            this.totalMarkers = data.totalPoint
            const features = data.featureCollection.features

            this.map.data.forEach((feature) => {
                toRemove[feature.getId()] = feature
            })

            const markers = {
                features: [],
                type: 'FeatureCollection',
            }

            forEach(features, (feature) => {
                if (!extendBounds && toRemove[feature.id]) {
                    delete toRemove[feature.id]
                } else {
                    markers.features.push(feature)
                }
            })

            if (this.geoJsonFeatures) {
                forEach(this.geoJsonFeatures, (feature) => {
                    if (toRemove[feature.id]) {
                        delete toRemove[feature.id]
                    }
                })
            }

            this.map.data.addGeoJson(markers)

            forEach(Object.keys(toRemove), (key) => {
                this.map.data.remove(toRemove[key])
            })

            if (extendBounds) {
                if (this.totalMarkers === 0) {
                    this.map.setZoom(3)
                } else {
                    const sw = new google.maps.LatLng(
                        data.bounds.south,
                        data.bounds.west
                    )
                    const ne = new google.maps.LatLng(
                        data.bounds.north,
                        data.bounds.east
                    )
                    const latlngBounds = new google.maps.LatLngBounds(sw, ne)
                    this.isMapReloaded = true
                    this.map.fitBounds(latlngBounds)
                }
            }
        },
    },
}
</script>
