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
                    :selectedKey="this.query.questionnaireId"
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
            <FilterBlock :title="$t('Common.Responsible')"
                v-if="model.userRole != 'Interviewer'">
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
                        v-model="this.assignmentId"/>
                    <div class="input-group-btn"
                        @click="clearAssignmentFilter">
                        <div class="btn btn-default">
                            <span class="glyphicon glyphicon-remove"
                                aria-hidden="true"></span>
                        </div>
                    </div>
                </div>
            </FilterBlock>
            <FilterBlock :title="$t('Pages.Filters_Shapefiles')">
                <Typeahead
                    control-id="shapefileName"
                    :placeholder="$t('Pages.Filters_None')"
                    :ajax-params="{ }"
                    :fetch-url="model.shapefiles"
                    :value="shapefileName"
                    v-on:selected="selectedShapefileName"/>
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
                    v-for="answer in selectedTooltip.identifyingData">
                    <strong>{{answer.title}}:</strong>
                    &nbsp;{{answer.answer || $t("Details.NoAnswer") }}
                </div>

                <div class="row-fluid"
                    v-if="model.userRole != 'Interviewer'"
                    style="white-space:nowrap;">
                    <strong>{{$t("MapReport.ViewInterviewContent")}}:</strong>&nbsp;
                    <a
                        v-bind:href="api.GetInterviewDetailsUrl(selectedTooltip.interviewId)"
                        target="_blank">{{$t("MapReport.details")}}</a>
                </div>
                <div class="row-fluid tooltip-buttons"
                    style="white-space:nowrap;">
                    <button
                        class="btn btn-sm btn-success"
                        v-if="model.userRole == 'Interviewer' && (selectedTooltip.status == 'InterviewerAssigned' || selectedTooltip.status == 'RejectedBySupervisor')"
                        click="openInterview">{{ $t("Common.Open")}}</button>
                    <button
                        class="btn btn-sm btn-success"
                        v-if="canAssign"
                        click="assignInterview">{{ $t("Common.Assign") }}</button>
                    <button
                        class="btn btn-sm btn-success"
                        v-if="model.userRole == 'Supervisor' && (selectedTooltip.status == 'Completed' || selectedTooltip.status == 'RejectedByHeadquarters')"
                        click="approveSvInterview">{{ $t("Common.Approve")}}</button>
                    <button
                        class="btn btn-sm reject"
                        v-if="model.userRole == 'Supervisor' && (selectedTooltip.status == 'Completed' || selectedTooltip.status == 'RejectedByHeadquarters')"
                        click="rejectSvInterview">{{ $t("Common.Reject")}}</button>
                    <button
                        class="btn btn-sm btn-success"
                        v-if="model.userRole == 'Headquarter' && (selectedTooltip.status == 'Completed' || selectedTooltip.status == 'ApprovedBySupervisor')"
                        click="approveHqInterview">{{ $t("Common.Approve")}}</button>
                    <button
                        class="btn btn-sm reject"
                        v-if="model.userRole == 'Headquarter' && (selectedTooltip.status == 'Completed' || selectedTooltip.status == 'ApprovedBySupervisor')"
                        click="rejectHqInterview">{{ $t("Common.Reject")}}</button>
                    <button
                        class="btn btn-sm btn-primary"
                        v-if="model.userRole == 'Headquarter' && selectedTooltip.status == 'ApprovedByHeadquarters'"
                        click="unapproveInterview">{{ $t("Common.Unapprove")}}</button>
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
                    v-for="answer in selectedTooltip.identifyingData">
                    <strong>{{answer.title}}:</strong>
                    &nbsp;{{answer.answer || $t("Details.NoAnswer")}}
                </div>

                <div class="row-fluid"
                    v-if="model.userRole != 'Interviewer'"
                    style="white-space:nowrap;">
                    <strong>{{$t("Common.ViewAssignmentDetails")}}:</strong>&nbsp;
                    <a
                        v-bind:href="api.GetAssignmentDetailsUrl(selectedTooltip.assignmentId)"
                        target="_blank">{{$t("MapReport.details")}}</a>
                </div>
                <div class="row-fluid tooltip-buttons"
                    style="white-space:nowrap;">
                    <button
                        class="btn btn-sm btn-success"
                        v-if="model.userRole == 'Interviewer'"
                        click="createInterview">{{ $t("Common.Create") }}</button>
                </div>
            </div>

            <div ref="clusterTooltip">
                <div class="row-fluid">
                    <strong>{{$t("Reports.ClusterInfo")}}</strong>
                </div>
                <div class="row-fluid"
                    v-if="selectedTooltip.interviewsCount > 0">
                    <strong>{{$t("Common.Interviews")}}:</strong>
                    &nbsp;{{selectedTooltip.interviewsCount}}
                </div>
                <div class="row-fluid"
                    v-if="selectedTooltip.assignmentsCount > 0">
                    <strong>{{$t("Common.Assignments")}}:</strong>
                    &nbsp;{{selectedTooltip.assignmentsCount}}
                </div>
            </div>
        </div>
        <div id="map-canvas"></div>

        <ModalFrame ref="assignModal"
            :title="$t('Common.Assign')">
            <form onsubmit="return false;">
                <div class="form-group">
                    <label
                        class="control-label"
                        for="newResponsibleId">{{$t("Assignments.SelectResponsible")}}</label>
                    <Typeahead
                        control-id="newResponsibleId"
                        :placeholder="$t('Common.Responsible')"
                        :value="newResponsibleId"
                        :ajax-params="{ }"
                        @selected="newResponsibleSelected"
                        :fetch-url="model.responsible"></Typeahead>
                </div>
                <div v-if="selectedTooltip.isReceivedByTablet">
                    <br />
                    <input
                        type="checkbox"
                        id="reassignReceivedByTablet"
                        v-model="isReassignReceivedByTablet"
                        class="checkbox-filter"/>
                    <label for="reassignReceivedByTablet"
                        style="font-weight: normal">
                        <span class="tick"></span>
                        {{$t("Reports.AssignReceivedConfirm", 1)}}
                    </label>
                    <br />
                    <span v-if="isReassignReceivedByTablet"
                        class="text-danger">
                        {{$t("Reports.AssignReceivedWarning")}}
                    </span>
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    role="confirm"
                    @click="assign"
                    :disabled="!canClickAssign">{{ $t("Common.Assign") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal"
                    role="cancel">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>

    </HqLayout>
</template>
<style scoped>
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
import * as toastr from 'toastr'
import Vue from 'vue'
import {isNull, chain, debounce, delay, forEach, find } from 'lodash'
import routeSync from '~/shared/routeSync'

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
            newResponsibleId: null,
            isReassignReceivedByTablet: false,
            shapefileName: null,
            geoJsonFeatures: null,
            geoJsonLabels: [],
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

        canAssign() {
            if (this.model.userRole == 'Supervisor' &&
                (this.selectedTooltip.status == 'InterviewerAssigned'
                || this.selectedTooltip.status == 'SupervisorAssigned'
                || this.selectedTooltip.status == 'RejectedBySupervisor'
                )
            )
                return true

            if (this.model.userRole == 'Headquarter' &&
                (this.selectedTooltip.status == 'InterviewerAssigned'
                || this.selectedTooltip.status == 'RejectedBySupervisor'
                || this.selectedTooltip.status == 'RejectedByHeadquarters'
                )
            )
                return true

            return false
        },

        canClickAssign() {
            if (this.newResponsibleId)
            {
                if (this.selectedTooltip.isReceivedByTablet)
                    return this.isReassignReceivedByTablet
                return true
            }
            return false
        },

        selectedVersionValue() {
            return this.selectedVersion == null ? null : this.selectedVersion.key
        },

        queryString() {
            return {
                questionnaire: this.query.questionnaire,
                questionnaireId: this.query.questionnaireId,
                version: this.query.version,
                responsible: this.query.responsible,
                assignmentId: this.query.assignmentId,
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

        this.assignmentId = this.query.assignmentId

        this.showPointsOnMap(180, 180, -180, -180, true)
    },

    methods: {

        openInterview() {
            window.open(this.$hq.basePath + 'InterviewerHq/OpenInterview/' + this.selectedTooltip.interviewId, '_blank')
        },

        createInterview() {
            const self = this
            const assignmentId = this.selectedTooltip.assignmentId
            $.post('InterviewerHq/StartNewInterview/' + assignmentId, response => {
                const interviewId = response.interviewId
                const workspace = self.$hq.basePath
                const url = `${workspace}WebInterview/${interviewId}/Cover`
                window.open(url, '_blank')
                self.reloadMarkersInBounds()
            })
        },

        assignInterview() {
            this.newResponsibleId = null
            this.isReassignReceivedByTablet = false
            this.$refs.assignModal.modal({keyboard: false})
        },

        async assign() {
            if (this.newResponsibleId.iconClass === 'supervisor')
                await this.$hq.InterviewsPublicApi.SvAssign(this.selectedTooltip.interviewId, this.newResponsibleId.key)
            else
                await this.$hq.InterviewsPublicApi.Assign(this.selectedTooltip.interviewId, this.newResponsibleId.key)

            this.$refs.assignModal.hide()
            this.newResponsibleId = null
            await this.refreshInterviewData()
        },

        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue
        },


        async approveSvInterview() {
            await this.$hq.InterviewsPublicApi.SvApprove(this.selectedTooltip.interviewId)
            await this.refreshInterviewData()
        },

        async approveHqInterview() {
            await this.$hq.InterviewsPublicApi.HqApprove(this.selectedTooltip.interviewId)
            await this.refreshInterviewData()
        },

        async rejectSvInterview() {
            await this.$hq.InterviewsPublicApi.SvReject(this.selectedTooltip.interviewId)
            await this.refreshInterviewData()
        },

        async rejectHqInterview() {
            await this.$hq.InterviewsPublicApi.HqReject(this.selectedTooltip.interviewId)
            await this.refreshInterviewData()
        },

        async unapproveInterview() {
            await this.$hq.InterviewsPublicApi.HqUnapprove(this.selectedTooltip.interviewId)
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

                Vue.nextTick(function() {
                    self.infoWindow.setContent($(self.$refs.interviewTooltip).html())
                })
            }

            marker.setProperty('status', this.selectedTooltip.status)
        },

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

            if (value == null || this.$route.query.questionnaireId !== value.key)
                this.selectQuestionnaireVersion(null)
            else
                this.selectQuestionnaireVersion(this.$route.query.version ? {key: this.$route.query.version} : null)

            this.onChange(q => {
                q.questionnaireId = value == null ? null : value.key
            })
        },

        selectResponsible(newValue) {
            this.responsibleId = newValue
            this.onChange(q => {
                q.responsible = newValue == null ? null : newValue.value
            })
            this.reloadMarkersInBounds()
        },

        selectedShapefileName(newValue) {
            this.shapefileName = newValue

            if (this.geoJsonFeatures) {
                for (var i = 0; i < this.geoJsonFeatures.length; i++)
                    this.map.data.remove(this.geoJsonFeatures[i]);
                this.geoJsonFeatures = null
                
                for (var i = 0; i < this.geoJsonLabels.length; i++)
                    this.map.data.remove(this.geoJsonLabels[i]);
                this.geoJsonLabels = []
            }
            
            if (this.shapefileName) {
                const geoJsonUrl = this.model.shapefileJson + '?mapName=' + this.shapefileName.key
                
                this.isLoading = true

                const self = this
                $.getJSON(geoJsonUrl, function (data) {
                    self.geoJsonFeatures = self.map.data.addGeoJson(data);
                    /*for (var i = 0; i < self.geoJsonFeatures.length; i++) {
                        const feature = self.geoJsonFeatures[i]
                        const label = feature.getProperty('label')
                        if (label) {
                            const geometry = feature.getGeometry()
                            var bounds = new google.maps.LatLngBounds();
                            geometry.forEachLatLng(function(latLng) {
                                bounds.extend(latLng);
                            })
                            var labelLatlng = bounds.getCenter();
                            var labelMarker = new google.maps.Marker({
                                position: labelLatlng,
                                label: {
                                    text: label,
                                    color: 'black',
                                    fontSize: '24px',
                                },
                                map: self.map,
                                icon: '/img/google-maps-markers/invisible.png'
                            });
                            self.geoJsonLabels.push(labelMarker)

                        }
                    }*/
                });

                this.isLoading = false
            }

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

            const mapDiv = document.getElementById('map-canvas')
            this.map = new google.maps.Map(mapDiv, this.getMapOptions())

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
                const type = feature.getProperty('type')

                if (type == 'Interview')
                {
                    const userRole = self.model.userRole
                    const status = feature.getProperty('status')

                    let interviewStyle ={
                        icon: {
                            url: '/img/google-maps-markers/m2.png',
                            dark: false,
                        },
                    }

                    let action = ''
                    let markerForm = ''

                    if (userRole == 'Interviewer') {
                        markerForm = 'circle'

                        switch(status) {
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
                    }

                    else if (userRole == 'Supervisor') {
                        switch(status) {
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
                    }

                    else if (userRole == 'Headquarter') {
                        switch(status) {
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
                if (type == 'Assignment')
                {
                    const rRole = feature.getProperty('responsibleRole')
                    let markerForm = ''
                    switch(rRole) {
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
                            url: `/img/google-maps-markers/${markerForm}-assignment.png`,
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
                    iconStyle.anchor = new google.maps.Point(radius / 2, radius / 2)

                    return {
                        label: {
                            fontSize: '12px',
                            text: count,
                            color: iconStyle.dark ? '#fff' : '#000',
                        },
                        icon: iconStyle,
                    }
                }

                /*const label = feature.getProperty('label')
                if (label) {
                    return {
                        fillColor: 'green',
                        strokeWeight: 1,
                        label: {
                            fontSize: '12px',
                            text: label,
                        },
                    }
                }*/

                const geometry = feature.getGeometry()
                const geometryType = geometry.getType()
                if (geometryType == "Polygon" || geometryType == "MultiPolygon") {
                    return {
                        fillColor: '#DE9131',
                        fillOpacity: 0.5,
                        strokeColor: '#FCF7F1',
                        //strokeOpacity: ,
                        strokeWeight: 1,
                    }
                }

                return {}
            })

            this.map.data.addListener('click', async event => {
                const type = event.feature.getProperty('type')

                if (type == 'Cluster') {
                    //const expand = event.feature.getProperty('expand')

                    var mapZoomService = new google.maps.MaxZoomService()
                    const currentZoom = this.map.getZoom()
                    await mapZoomService.getMaxZoomAtLatLng(event.latLng, result => {
                        if (result.zoom <= currentZoom) {
                            const interviewsCount = event.feature.getProperty('interviewsCount')
                            const assignmentsCount = event.feature.getProperty('assignmentsCount')

                            var data = {
                                assignmentsCount: assignmentsCount,
                                interviewsCount: interviewsCount,
                            }

                            self.selectedTooltip = data

                            Vue.nextTick(function() {
                                self.infoWindow.setContent($(self.$refs.clusterTooltip).html())
                                self.infoWindow.setPosition(event.latLng)
                                self.infoWindow.setOptions({
                                    pixelOffset: new google.maps.Size(0, -30),
                                })
                                self.infoWindow.open(self.map)
                            })

                        } else {
                            self.map.setZoom(currentZoom + 1)
                            self.map.panTo(event.latLng)
                        }
                    })
                }
                else if (type == 'Interview')
                {
                    const interviewId = event.feature.getProperty('interviewId')

                    const response = await this.api.InteriewSummaryUrl(interviewId)
                    const data = response.data

                    if (data != null) {

                        data['interviewId'] = interviewId
                        data['marker'] = event.feature

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
                    data['marker'] = event.feature

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

                const label = event.feature.getProperty('label')
                if (label) {
                    Vue.nextTick(function() {
                        self.infoWindow.setContent(label)
                        self.infoWindow.setPosition(event.latLng)
                        self.infoWindow.open(self.map)
                    })
                }
            })

            google.maps.event.addDomListener(mapDiv, 'click', event => {
                if (event.srcElement.nodeName == 'BUTTON') {
                    var methodName = event.srcElement.getAttribute('click')
                    if (methodName)
                        self[methodName].call(self)
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
            /*const center = this.map.getCenter()
            var mapZoomService = new google.maps.MaxZoomService()
            const coor = {lng: center.lng(), lat: center.lat()}
            const maxZoom = await mapZoomService.getMaxZoomAtLatLng(coor)*/
            const maxZoom = 19

            var request = {
                QuestionnaireId: (this.selectedQuestionnaireId || {}).key || null,
                QuestionnaireVersion: this.selectedVersionValue,
                ResponsibleId: (this.responsibleId || {}).key || null,
                AssignmentId: this.assignmentId,
                Zoom: zoom,
                MaxZoom: maxZoom,
                east,
                north,
                west,
                south,
                clientMapWidth: this.map.getDiv().clientWidth,
            }

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

            if (this.geoJsonFeatures) {
                forEach(this.geoJsonFeatures, feature => {
                    if (toRemove[feature.id]) {
                        delete toRemove[feature.id]
                    }}
                )
            }

            this.map.data.addGeoJson(markers)

            forEach(Object.keys(toRemove), key => {
                this.map.data.remove(toRemove[key])
            })

            /*if (this.shapefileName) {
                const geoJsonUrl = this.model.shapefileJson + '?mapName=' + this.shapefileName.key
                this.map.data.loadGeoJson(geoJsonUrl);
            }*/

            if (extendBounds) {
                if (this.totalMarkers === 0) {
                    this.map.setZoom(3)
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
