<template>
    <HqLayout :hasFilter="false" :hasRow="false" :mainClass="'settings'">
        <template v-slot:headers>
            <div>
                <ol class="breadcrumb">
                    <li>
                        <a :href="$config.model.interviewersUrl">{{ $t('Pages.InterviewerProfile_Interviewers') }}</a>
                    </li>
                    <li>
                        <a :href="$config.model.profileUrl">{{ $t('Pages.Profile_InterviewerProfile') }}</a>
                    </li>
                </ol>
                <h1>
                    {{ $t('InterviewerAuditRecord.DetailedActionLog') }} ({{ $config.model.interviewerName }})
                </h1>

                <ul v-if="$config.model.hasDeviceInfo" class="list-unstyled">
                    <li><b>{{ $config.model.deviceModel }} (id {{ $config.model.deviceId }})</b></li>
                    <li>
                        <b>
                            {{ $t('Pages.InterviewerProfile_InterviewerAppVersion') }}:
                            {{ $config.model.interviewerAppVersion }} &mdash;

                            <span v-if="!$config.model.hasUpdateForInterviewerApp" class="success-text">{{
                                $t('Pages.InterviewerProfile_InterviewerUpToDate') }}</span>
                            <span v-else class="error-text">{{ $t('Pages.InterviewerProfile_InterviewerCanBeUpdated')
                                }}</span>
                        </b>
                    </li>
                </ul>
            </div>
        </template>
        <div class="row  extra-margin-bottom">
            <div class="col-sm-7 ">
                <a v-if="$config.model.recordsByDate.length > 0" :href="$config.model.downloadLogUrl">
                    {{ $t('InterviewerAuditRecord.DownloadTabLog') }}
                </a>
                <template v-else>
                    {{ $t('InterviewerAuditRecord.RecordsMissing') }}
                </template>
            </div>
        </div>
        <div class="row" v-for="dateRecords in $config.model.recordsByDate" :key="dateRecords.date">
            <div class="col-sm-7">
                <h2>{{ formatDateWithoutTime(dateRecords.date) }}</h2>
                <table class="table timestamps-table">
                    <tbody>
                        <tr v-for="record in dateRecords.recordsByDate" :key="record.time">
                            <td class="gray-text date">
                                {{ formatTime(record.time) }}
                            </td>
                            <td>
                                <span :class="actionCssClass(record)">
                                    {{ record.message }}
                                    <template v-if="record.description">
                                        <br />
                                        <pre v-dompurify-html="record.description"></pre>
                                    </template>
                                </span>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <div v-if="$config.model.startDateTime" class="row">
            <div class="col-sm-7 ">
                <a :href="`${$config.model.selfUrl}?startDateTime=${$config.model.startDateTime}`">
                    {{ $t('InterviewerAuditRecord.ShowPrevious7Days') }}
                </a>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import { DateFormats } from '~/shared/helpers'
import moment from 'moment'

export default {
    methods: {
        formatTime(d) {
            return moment.utc(d).format('HH:mm')
        },
        formatDateWithoutTime(d) {
            return moment.utc(d).format(DateFormats.date)
        },
        actionCssClass(action) {
            switch (action.type) {
                case 7:
                    return 'success-text'
                case 4:
                case 12:
                case 6:
                case 8:
                    return 'error-text'
                default:
                    return ''
            }
        },
    },
}
</script>
