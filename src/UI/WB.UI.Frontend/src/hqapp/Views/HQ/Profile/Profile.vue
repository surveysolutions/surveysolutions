/* eslint-disable vue/require-v-for-key */
<template>
    <HqLayout :hasFilter="false"
        :hasRow="false"
        :mainClass="'enumerators'">
        <template slot="headers">
            <ol class="breadcrumb"
                v-if="!this.$config.model.authorizedUser.isInterviewer">
                <li>
                    <a
                        :href="this.$config.model.api.listUrl">{{$t('Pages.InterviewerProfile_Interviewers')}}</a>
                </li>
            </ol>
            <h1>
                <span v-if="this.$config.model.fullModel.isArchived"
                    class="badge">{{$t('Common.Archived')}}</span>
                {{$t('Pages.InterviewerProfile_AssignedToFormat', { interviewer: this.$config.model.fullModel.interviewerName, supervisor: this.$config.model.fullModel.supervisorName}) }}
            </h1>

            <ul class="list-unstyled">
                <li
                    v-if="this.$config.model.fullModel.email">{{$t('Pages.InterviewerProfile_EmailFormat', {email: this.$config.model.fullModel.email})}}</li>
                <li
                    v-if="this.$config.model.fullModel.fullName">{{$t('Pages.InterviewerProfile_FullNameFormat', {fullName: this.$config.model.fullModel.fullName})}}</li>
                <li
                    v-if="this.$config.model.fullModel.phone">{{$t('Pages.InterviewerProfile_PhoneFormat', {phone: this.$config.model.fullModel.phone})}}</li>

                <li v-if="this.$config.model.fullModel.isModifiable">
                    <a
                        v-if="!this.$config.model.fullModel.isArchived"
                        :href="this.$config.model.api.manageUrl">{{$t('Pages.InterviewerProfile_Info')}}</a>
                    <form
                        v-if="this.$config.model.fullModel.isArchived && $config.model.authorizedUser.isHeadquarters"
                        method="post">
                        <input class="btn btn-success"
                            type="button"
                            :value="$t('Pages.Unarchive')"
                            @click="unArchive"/>
                    </form>
                </li>
                <li v-if="!this.$config.model.authorizedUser.isInterviewer">
                    <a
                        :href="this.$config.model.api.audioAuditLogUrl">{{$t('Pages.InterviewerProfile_ShowAuditLog')}}</a>
                </li>
            </ul>
            <figure
                class="qrcode-wrapper"
                v-if="this.$config.model.fullModel.supportQRCodeGeneration">
                <a
                    target="_blank"
                    :title="$t('Pages.InterviewerProfile_QrCodeAlt')"
                    href="https://support.mysurvey.solutions/interviewer/config/set-up-an-interviewer-tablet-by-scanning-a-barcode/">
                    <img
                        id="download-qr"
                        :alt="$t('Pages.InterviewerProfile_QrCodeAlt')"
                        width="250"
                        height="250"
                        :src="this.$config.model.fullModel.qrCodeAsBase64String"/>
                </a>
            </figure>
        </template>
        <div class="row">
            <div class="col-sm-12">
                <div class="interviews-information clearfix">
                    <div class="number-information">
                        <div
                            class="amount-of-questionnaires">{{this.$config.model.fullModel.newInterviewsOnDevice}}</div>
                        <div class="description">
                            {{$t('Pages.InterviewerProfile_NewOnDevice')}}
                        </div>
                    </div>
                    <div class="number-information">
                        <div
                            class="amount-of-questionnaires">{{this.$config.model.fullModel.rejectedInterviewsOnDevice}}</div>
                        <div class="description">
                            {{$t('Pages.InterviewerProfile_Rejected')}}
                        </div>
                    </div>
                    <div class="number-information">
                        <div
                            class="amount-of-questionnaires">{{this.$config.model.fullModel.waitingInterviewsForApprovalCount}}</div>
                        <div
                            class="description">{{$t('Pages.InterviewerProfile_WaitingForApproval')}}</div>
                    </div>
                    <div class="number-information">
                        <div
                            class="amount-of-questionnaires">{{this.$config.model.fullModel.approvedInterviewsByHqCount}}</div>
                        <div class="description">
                            {{$t('Pages.InterviewerProfile_Approved')}}
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <div class="row"
            v-if="showMap">
            <div class="col-sm-9 map"
                v-if="markerExist">
                <div
                    ref="map"
                    id="map-canvas"
                    class="extra-margin-bottom"
                    style="width:100%; height: 400px"></div>
                <div style="display: none;">
                    <div ref="tooltip">
                        <div
                            class="map-tooltip-info"
                            v-for="selectedTooltip in selectedTooltips"
                            :key="selectedTooltip.interviewKey">
                            <p>
                                <span>#{{selectedTooltip.interviewKey}}</span>
                                <span>({{$t("MapReport.Assignment")}} {{selectedTooltip.assignmentId}})</span>
                            </p>
                            <p>
                                <strong>{{$t("Users.Interviewer")}} :</strong>
                                &nbsp;{{selectedTooltip.interviewerName}}
                            </p>
                            <p>
                                <strong>{{$t("Users.Supervisor")}} :</strong>
                                &nbsp;{{selectedTooltip.supervisorName}}
                            </p>
                            <p>
                                <strong>{{$t("Common.Status")}} :</strong>
                                &nbsp;{{selectedTooltip.lastStatus}}
                            </p>
                            <p>
                                <strong>{{$t("Reports.LastUpdatedDate")}} :</strong>
                                &nbsp;{{selectedTooltip.lastUpdatedDate}}
                            </p>
                            <p>
                                <a
                                    v-bind:href="model.api.interviewDetailsUrl + '/' + selectedTooltip.interviewId"
                                    target="_blank">{{$t("MapReport.ViewInterviewContent")}}</a>
                            </p>
                        </div>
                    </div>
                </div>
            </div>
            <div
                class="col-sm-9 map"
                v-if="!markerExist">{{$t("Pages.InterviewerProfile_NoMarkers")}}</div>
        </div>
        <div class="row"
            v-if="totalTrafficUsed > 0">
            <div class="col-sm-12 clearfix">
                <h3>{{$t("Pages.InterviewerProfile_TrafficUsageHeader")}}</h3>
                <div class="graphic-wrapper traffic-usage">
                    <div
                        class="t-monthly-usage"
                        v-for="monthlyUsage in trafficUsage"
                        :key="monthlyUsage.month">
                        <div class="t-month">
                            {{monthlyUsage.month}}
                        </div>
                        <div
                            class="t-daily-usage"
                            v-for="dailyUsage in monthlyUsage.dailyUsage"
                            :key="dailyUsage.timestamp">
                            <div class="t-unit-wrapper">
                                <div
                                    class="t-up"
                                    data-toggle="tooltip"
                                    data-placement="right"
                                    :title="formatKb(dailyUsage.up)"
                                    :style="{ height: dailyUsage.upInPer + '%' }"></div>
                                <div
                                    class="t-down"
                                    data-toggle="tooltip"
                                    data-placement="right"
                                    :title="formatKb(dailyUsage.down)"
                                    :style="{ height: dailyUsage.downInPer + '%' }"></div>
                            </div>
                            <div
                                class="t-day"
                                :class="{'t-no-sync': dailyUsage.up + dailyUsage.down === 0 }">{{dailyUsage.day}}</div>
                        </div>
                    </div>
                    <div class="graphic-explanation">
                        <div class="legend-block">
                            <div class="legend">
                                <div class="legend-unit down-unit">
                                    <span></span>
                                    {{$t("Pages.InterviewerProfile_IncomingTraffic")}}
                                </div>
                                <div class="legend-unit up-unit">
                                    <span></span>
                                    {{$t("Pages.InterviewerProfile_OutgoingTraffic")}}
                                </div>
                            </div>
                        </div>
                        <div class="total">
                            <p class="primary-text">
                                {{$t("Pages.InterviewerProfile_TotalTrafficUsed")}}:
                                <span
                                    v-html="formatKb(totalTrafficUsed)"></span>
                            </p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row"
            v-if="this.$config.model.fullModel.hasDeviceInfo">
            <div class="col-sm-12 clearfix">
                <h3>{{$t('Pages.InterviewerProfile_Sync_Activity_Title')}}</h3>
                <div class="graphic-wrapper clearfix">
                    <div class="graphic">
                        <div
                            class="day-unit"
                            v-for="syncDay in this.$config.model.fullModel.synchronizationActivity.days"
                            :key="syncDay.day">
                            <div class="day">
                                {{syncDay.day}}
                            </div>
                            <div
                                class="quarter-of-day"
                                v-for="(syncDayQuarter, idx) in syncDay.quarters"
                                :key="idx">
                                <div class="recent-activity"
                                    v-if="!syncDayQuarter.HasAnyActivity">
                                    <div
                                        v-if="syncDayQuarter.failedSynchronizationsCount > 0"
                                        class="failed-connection"></div>
                                    <div
                                        v-else-if="syncDayQuarter.synchronizationsWithoutChangesCount > 0"
                                        class="successful-connection"></div>
                                </div>
                                <div class="recent-activity"
                                    v-else>
                                    <div
                                        v-repeat="syncDayQuarter.downloadedAssignmentsInProportionCount"
                                        class="downloaded"></div>
                                    <div
                                        v-for="item in syncDayQuarter.uploadedInterviewsInProportionCount"
                                        :key="item"
                                        class="uploaded"></div>
                                    <div
                                        v-if="syncDayQuarter.hasMoreThanMaxActionsCount"
                                        class="over-limit">
                                        <span></span>
                                    </div>
                                </div>

                                <div class="unfinished-assignments">
                                    <div class="half-of-quarter">
                                        <div
                                            v-for="item in syncDayQuarter.allAssignmentsOnDeviceCount"
                                            :key="item"
                                            class="unfinished-unit"></div>
                                    </div>
                                    <div class="half-of-quarter">
                                        <div
                                            v-for="item in syncDayQuarter.allAssignmentsOnDeviceCount"
                                            :key="item"
                                            class="unfinished-unit"></div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div class="day-unit">
                            <div class="quarter-of-day">
                                <div class="recent-activity"></div>
                                <div class="unfinished-assignments">
                                    <div class="half-of-quarter"></div>
                                    <div class="half-of-quarter"></div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="graphic-explanation">
                        <div class="recent-activity">
                            <ul class="list-unstyled">
                                <li>
                                    <span class="downloaded"></span>
                                    {{$t('Pages.InterviewerProfile_Sync_Activity_Downloaded_Assignments_Desc')}}
                                </li>
                                <li>
                                    <span class="uploaded"></span>
                                    {{$t('Pages.InterviewerProfile_Sync_Activity_Uploaded_Interview_Desc')}}
                                </li>
                                <li>
                                    <span class="successful-connection"></span>
                                    {{$t('Pages.InterviewerProfile_Sync_Activity_Nothing_To_Sync')}}
                                </li>
                                <li>
                                    <span class="failed-connection"></span>
                                    {{$t('Pages.InterviewerProfile_Sync_Activity_Failed_Sync')}}
                                </li>
                            </ul>
                        </div>
                        <div class="unfinished-assignments">
                            <p
                                class="primary-text">{{$t('Pages.InterviewerProfile_Sync_Activity_All_Assignments_Desc')}}</p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="row"
            v-if="this.$config.model.fullModel.hasDeviceInfo">
            <div class="col-sm-6 connection-statistics">
                <h3>{{$t('Pages.InterviewerProfile_DeviceAndConnectionStatistics')}}</h3>
                <ul class="list-unstyled">
                    <li>
                        <b>{{$t('Pages.InterviewerProfile_DeviceModel')}}: {{this.fullModel.deviceType}} {{this.fullModel.deviceManufacturer}} {{this.fullModel.deviceModel}} ({{this.fullModel.deviceBuildNumber}})</b>
                    </li>
                    <li>
                        <b>
                            {{$t('Pages.InterviewerProfile_InterviewerAppVersion')}}: {{this.fullModel.interviewerAppVersion}} —
                            <span
                                v-if="!this.fullModel.hasUpdateForInterviewerApp"
                                class="success-text">{{$t('Pages.InterviewerProfile_InterviewerUpToDate')}}</span>
                            <span
                                v-else
                                class="error-text">{{$t('Pages.InterviewerProfile_InterviewerCanBeUpdated')}}</span>
                        </b>
                    </li>
                    <li v-if="this.fullModel.deviceAssignmentDate">
                        <b>
                            {{$t('Pages.InterviewerProfile_DeviceAssignmentDate')}}:
                            {{formatDate(this.fullModel.deviceAssignmentDate, true)}} (UTC)
                            <span
                                v-if="this.fullModel.registredDevicesCount > 1"
                                style="color: red;">({{$t('Pages.InterviewerProfile_Relinked')}})</span>
                        </b>
                    </li>
                </ul>
                <ul class="list-unstyled">
                    <li>{{$t('Pages.InterviewerProfile_NumberOfSuccessSynchronizations')}}: {{this.fullModel.totalNumberOfSuccessSynchronizations}}</li>
                    <li>{{$t('Pages.InterviewerProfile_NumberOfFailedSynchronizations')}}: {{this.fullModel.totalNumberOfFailedSynchronizations}}</li>
                    <li v-if="this.fullModel.averageSyncSpeedBytesPerSecond">
                        {{$t('Pages.InterviewerProfile_AverageSyncSpeed')}}:
                        <span
                            v-html="formatKb(this.fullModel.averageSyncSpeedBytesPerSecond)"></span>/s
                    </li>
                    <li>
                        {{$t('Pages.InterviewerProfile_TotalTrafficUsed')}}:
                        <span
                            v-html="formatKb(this.fullModel.trafficUsed)"></span>
                    </li>
                </ul>
                <ConnectionStats
                    :prefix="$t('Pages.InterviewerProfile_LastSuccessSync')"
                    :syncInfo="this.fullModel.lastSuccessfulSync"></ConnectionStats>
                <ConnectionStats
                    :prefix="$t('Pages.InterviewerProfile_LastFailedSync')"
                    :syncInfo="this.fullModel.lastFailedSync"></ConnectionStats>

                <ul v-if="this.fullModel.lastCommunicationDate"
                    class="list-unstyled">
                    <li>
                        {{$t('Pages.InterviewerProfile_LastSyncronizationDate')}}:
                        {{formatDate(this.fullModel.lastCommunicationDate)}} ({{formatLastCommunication()}})
                        <b>
                            <span
                                v-if="this.fullModel.lastSuccessfulSync != undefined && this.fullModel.lastCommunicationDate == this.fullModel.lastSuccessfulSync.syncDate"
                                class="success-text">Successful</span>
                            <span
                                v-if="this.fullModel.lastFailedSync != undefined && this.fullModel.lastCommunicationDate == this.fullModel.lastFailedSync.syncDate"
                                class="error-text">Failed</span>
                        </b>
                    </li>
                </ul>
            </div>
        </div>
        <div class="row"
            v-if="this.$config.model.fullModel.hasDeviceInfo">
            <div class="col-sm-8 detailed-statistics-block">
                <h3>{{$t('Pages.InterviewerProfile_DeviceInfo')}}</h3>
                <table class="table table-striped table-bordered">
                    <tbody>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_DeviceId')}}</td>
                            <td>{{this.fullModel.deviceId}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_DeviceSerial')}}</td>
                            <td>{{this.fullModel.deviceSerialNumber}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_DeviceModel')}}</td>
                            <td>
                                {{this.fullModel.deviceType}} {{this.fullModel.deviceManufacturer}}
                                {{this.fullModel.deviceModel}} ({{this.fullModel.deviceBuildNumber}})
                            </td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_DeviceLanguage')}}</td>
                            <td>{{this.fullModel.deviceLanguage}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_AndroidVersion')}}</td>
                            <td>{{this.fullModel.androidVersion}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_InterviewerVersion')}}</td>
                            <td>{{this.fullModel.surveySolutionsVersion}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_InterviewerUpdatedDate')}}</td>
                            <td>{{formatDate(this.fullModel.lastSurveySolutionsUpdatedDate)}} (UTC)</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_DeviceLocation')}}</td>
                            <td id="device-address">
                                <a v-bind:href="goolgeMapUrl"
                                    v-if="hasLastKnownLocation"
                                    :title="$t('WebInterviewUI.ShowOnMap')"
                                    target="_blank">
                                    {{fullModel.deviceLocationOrLastKnownLocationLat}}, {{fullModel.deviceLocationOrLastKnownLocationLon}}</a>
                            </td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_DeviceOrientation')}}</td>
                            <td>{{this.fullModel.deviceOrientation}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_BatteryStatus')}}</td>
                            <td>
                                {{
                                    $t('Pages.InterviewerProfile_BatteryStatusFormat',
                                       {
                                           percent: this.fullModel.batteryStatus,
                                           power: this.fullModel.batteryPowerSource,
                                           powerSaver: this.fullModel.isPowerSaveMode ? $t('Pages.InterviewerProfile_BatteryStatus_SaverIsOn') : $t('Pages.InterviewerProfile_BatteryStatus_SaverIsOff')
                                       })
                                }}
                            </td>
                        </tr>
                        <tr
                            :class="{'text-danger': this.fullModel.storageFreeInBytes < 1024 * 1024 * 100}">
                            <td>{{$t('Pages.InterviewerProfile_StorageInfo')}}</td>
                            <td
                                v-html="`${ouputBytes(this.fullModel.storageFreeInBytes)} / ${ouputBytes(this.fullModel.storageTotalInBytes)}`"></td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_MemoryInfo')}}</td>
                            <td
                                v-html="`${ouputBytes(this.fullModel.ramFreeInBytes)} / ${ouputBytes(this.fullModel.ramTotalInBytes)}`"></td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_StorageSizeInfo')}}</td>
                            <td>{{ouputBytes(this.fullModel.databaseSizeInBytes)}}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <div class="row"
            v-if="this.$config.model.fullModel.hasDeviceInfo">
            <div class="col-sm-8 detailed-statistics-block">
                <h3>{{$t('Pages.InterviewerProfile_LastConnectionStatistics')}}</h3>
                <table class="table table-striped table-bordered">
                    <tbody>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_ServerDate')}}</td>
                            <td>{{formatDate(this.fullModel.serverTimeAtTheBeginningOfSync)}} (UTC)</td>
                        </tr>
                        <tr :class="{'text-danger': this.fullModel.deviceHasWrongTime}">
                            <td>{{$t('Pages.InterviewerProfile_DeviceDate')}}</td>
                            <td>{{formatDate(this.fullModel.tabletTimeAtTeBeginningOfSync)}} (UTC)</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_ConnectionType')}}</td>
                            <td>{{this.fullModel.connectionType}} {{this.fullModel.connectionSubType}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_DownloadedQuestionnairesCount')}}</td>
                            <td>{{this.fullModel.questionnairesReceived}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_DownloadedAssignmentsCount')}}</td>
                            <td>{{this.fullModel.assignmentsReceived}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_UploadedInterviewsCount')}}</td>
                            <td>{{this.fullModel.completedInterviewsReceivedFromInterviewer}}</td>
                        </tr>
                        <tr>
                            <td>{{$t('Pages.InterviewerProfile_StartedAssignments')}}</td>
                            <td>{{this.fullModel.assignmentsThatHaveBeenStarted}}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import MarkerClusterer from '@google/markerclustererplus'
import Vue from 'vue'
import * as toastr from 'toastr'
import {DateFormats, humanFileSize} from '~/shared/helpers'
import moment from 'moment'
import ConnectionStats from './ConnectionStats'

export default {
    data: function() {
        return {
            map: null,
            markerCluster: null,
            points: new Map(),
            infoWindow: null,
            lines: [],
            interviewerId: this.$route.params.interviewerId || this.$config.model.fullModel.interviewerId,
            selectedTooltips: {},
            colorMap: {
                red: '#e74924',
                green: '#2c7613',
                blue: '#0042c8',
            },
            minimumClusterSize: 5,
            totalTrafficUsed: 0,
            trafficUsage: [],
            maxDailyUsage: 0,
            markerExist: false,
        }
    },
    mounted() {
        this.initializeMap()
        this.initializeTrafficUsage()
    },
    methods: {
        async unArchive() {
            var self = this
            await this.$http.patch(this.$config.model.api.unarhiveUrl).then(
                response => {
                    window.location.reload()
                },
                e => {
                    if (e.response.data) toastr.error(e.response.data)
                    else toastr.error(self.$t('Pages.GlobalSettings_UnhandledExceptionMessage'))
                }
            )
        },
        formatDate(date, noTime = false) {
            return moment.utc(date).format(noTime ? DateFormats.date : DateFormats.dateTime)
        },
        formatKb(value) {
            if (value == null || value == undefined) return value
            var language =
                (navigator.languages && navigator.languages[0]) || navigator.language || navigator.userLanguage
            return value.toLocaleString(language, {style: 'decimal', maximumFractionDigits : 0}) + '&nbsp;' + this.$t('Pages.Kb')
        },
        ouputBytes(val) {
            return humanFileSize(val, false)
        },
        formatLastCommunication() {
            return moment.utc(this.fullModel.lastCommunicationDate).fromNow()
        },
        initializeTrafficUsage() {
            const self = this
            this.$http.get(this.model.api.interviewerTrafficUsage + '/' + this.interviewerId).then(response => {
                var trafficUsage = response.data || {}
                this.totalTrafficUsed = trafficUsage.totalTrafficUsed || 0
                this.trafficUsage = trafficUsage.trafficUsages || []
                this.maxDailyUsage = trafficUsage.maxDailyUsage || 0
                Vue.nextTick(function() {
                    $('[data-toggle="tooltip"]').tooltip({html: true})
                })
            })
        },
        isIE() {
            var ua = navigator.userAgent
            /* MSIE used to detect old browsers and Trident used to newer ones*/
            var is_ie = ua.indexOf('MSIE ') > -1 || ua.indexOf('Trident/') > -1

            return is_ie
        },
        showPointsOnMap(points, locations) {
            const self = this

            const arrowMarker = {
                icon: {
                    path: google.maps.SymbolPath.FORWARD_CLOSED_ARROW,
                },
                offset: '100%',
            }

            this.infoWindow = new google.maps.InfoWindow()
            var bounds = new google.maps.LatLngBounds()
            var markers = []
            points.forEach(point => {
                var icon = {
                    path: 'M 0 -50C 15 -50 50 -15 50 0C 50 15 15 50 0 50C -15 50 -50 15 -50 0C -50 -15 -15 -50 0 -50 Z',
                    fillColor: point.colors[0],
                    fillOpacity: 0.9,
                    strokeColor: 'white',
                    strokeWeight: 2,
                    scale: 0.4,
                }

                var marker = new google.maps.Marker({
                    position: new google.maps.LatLng(point.latitude, point.longitude),
                    label: {text: point.index === -1 ? '' : point.index + '', color: 'white'},
                    map: self.map,
                    opacity: 1,
                    zIndex: point.Index === -1 ? 1000 : 1000 + point.index,
                    icon: icon,
                })
                marker.set('id', point.index)

                self.addDetailsOnClick(marker, point)

                markers.push(marker)
                bounds.extend(marker.getPosition())

                self.points.set(point.index, {
                    index: point.index,
                    cluster: 0,
                    point: {
                        lat: point.latitude,
                        lng: point.longitude,
                    },
                    marker: marker,
                })
            })
            locations.forEach(point => {
                var marker = new google.maps.Marker({
                    position: new google.maps.LatLng(point.latitude, point.longitude),
                    icon: {
                        path: google.maps.SymbolPath.CIRCLE,
                        scale: 5,
                        fillColor: '#2a81cb',
                        strokeColor: '#2a81cb',
                    },
                    map: self.map,
                    opacity: 1,
                    zIndex: 999,
                })
                marker.set('id', point.interviewIds[0])

                self.addDetailsOnClick(marker, point)

                markers.push(marker)
                bounds.extend(marker.getPosition())
            })

            this.markerCluster = new MarkerClusterer(this.map, markers, {
                imagePath: '/img/google-maps-markers/m',
                enableRetinaIcons: true,
                minimumClusterSize: this.minimumClusterSize,
                averageCenter: true,
            })

            google.maps.event.addListener(this.markerCluster, 'clusteringend', () => {
                this.drawLines()
            })

            this.map.fitBounds(bounds)
        },
        addDetailsOnClick: function(marker, point) {
            const self = this
            google.maps.event.addListener(
                marker,
                'click',
                (function(marker, point) {
                    return function() {
                        self.loadPointDetails(point.interviewIds, marker)
                    }
                })(marker, point)
            )
        },
        drawLines() {
            if (!this.markerCluster) return

            this.points.forEach(point => {
                point.cluster = 0
            })

            this.lines.forEach(line => {
                line.setMap(null)
            })
            this.lines = []

            var clusters = this.markerCluster.getClusters()
            var clustersMap = new Map()

            for (let i = 1; i < clusters.length + 1; i++) {
                let cluster = clusters[i - 1]
                let center = cluster.getCenter()
                let markers = cluster.getMarkers()
                clustersMap.set(i, {
                    center: center,
                    size: cluster.getSize(),
                })
                markers.forEach(marker => {
                    let markerId = marker.get('id')

                    let point = this.points.get(markerId)
                    if(point != null)
                        point.cluster = i
                })
            }

            for (let i = 1; i < this.points.size; i++) {
                let left = this.points.get(i)
                let right = this.points.get(i + 1)

                let leftCluster = clustersMap.get(left.cluster) || {size: -1}
                let rightCluster = clustersMap.get(right.cluster) || {size: -1}

                let leftPointsAreGroupedInCluster = leftCluster.size >= this.minimumClusterSize
                let rightPointsAreGroupedInCluster = rightCluster.size >= this.minimumClusterSize

                let pointsAreGroupedInTheSameCluster = left.cluster == right.cluster && leftPointsAreGroupedInCluster

                if (pointsAreGroupedInTheSameCluster) continue

                let leftPoint = left.point
                let rightPoint = right.point

                if (left.cluster > 0 && leftPointsAreGroupedInCluster) {
                    leftPoint = leftCluster.center
                }

                if (right.cluster > 0 && rightPointsAreGroupedInCluster) {
                    rightPoint = rightCluster.center
                }

                var path = new google.maps.Polyline({
                    path: [leftPoint, rightPoint],
                    geodesic: true,
                    strokeColor: '#2a81cb',
                    strokeOpacity: 0.75,
                    strokeWeight: 1,
                })

                path.setMap(this.map)

                this.lines.push(path)
            }
        },
        loadPointDetails(interviewIds, marker) {
            const self = this
            this.$http
                .post(this.model.api.interiewSummaryUrl, {
                    InterviewIds: interviewIds,
                })
                .then(response => {
                    const data = response.data

                    if (data == undefined || data == null) return

                    self.selectedTooltips = data

                    Vue.nextTick(function() {
                        self.infoWindow.setContent($(self.$refs.tooltip).html())
                        self.infoWindow.open(self.map, marker)
                    })
                })
        },
        async initializeMap() {
            if (!this.model.showMap) return

            const response = await this.$http.get(this.model.api.interviewerPoints + '/' + this.interviewerId)
            var data = response.data || {checkInPoints: [], targetLocations: []}
            var points = data.checkInPoints || []
            var locations = data.targetLocations || []

            if (points.length > 0 || locations.length > 0) {
                this.markerExist = true

                var mapOptions = {
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
                    // mapTypeControlOptions: {
                    //     position: google.maps.ControlPosition.LEFT_TOP,
                    // },
                    minZoom: 3,
                    scaleControl: true,
                    streetViewControl: false,
                    center: this.model.initialLocation,
                }

                const self = this
                Vue.nextTick(function(){
                    self.map = new google.maps.Map(self.$refs.map, mapOptions)
                    self.showPointsOnMap(points, locations)
                })
            }
        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        fullModel() {
            return this.model.fullModel
        },
        showMap() {
            return this.$config.model.showMap
        },
        hasLastKnownLocation(){
            return this.fullModel.deviceLocationOrLastKnownLocationLat != null && this.fullModel.deviceLocationOrLastKnownLocationLon != null
        },
        goolgeMapUrl(){
            return `${this.$config.googleMapsBaseUrl}/maps?q=${this.fullModel.deviceLocationOrLastKnownLocationLat},${this.fullModel.deviceLocationOrLastKnownLocationLon}`
        },
    },
    components: {
        ConnectionStats,
    },
}
</script>
