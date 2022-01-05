<template>
    <ul v-if="syncInfo.syncDate"
        class="list-unstyled">
        <li>{{prefix}}: {{formatDate(syncInfo.syncDate)}} ({{formatLastCommunication(syncInfo.syncDate)}})</li>

        <template v-if="syncInfo.hasStatistics">
            <li>{{$t('Pages.InterviewerProfile_TotalSyncTime')}}: {{formatDuration(syncInfo.totalSyncDuration)}}</li>
            <li>
                {{$t('Pages.InterviewerProfile_ConnectionSpeed')}}:
                {{ouputBytes(syncInfo.totalConnectionSpeed)}}/s
                <text v-if="syncInfo.NetworkType == 'WIFI'">({{$t('Pages.InterviewerProfile_ConnectionWifiFormat', {networkType: syncInfo.NetworkType})}})</text>
                <text v-else>({{$t('Pages.InterviewerProfile_ConnectionMobileFormat',
                                   {
                                       networkType: syncInfo.NetworkType,
                                       networkSubType: syncInfo.NetworkSubType,
                                       operator: syncInfo.MobileOperator
                                   })}})</text>
            </li>
            <li>
                {{$t('Pages.InterviewerProfile_DataStatsFormat', {
                    uploaded: ouputBytes(syncInfo.totalUploadedBytes),
                    downloaded: ouputBytes(syncInfo.totalDownloadedBytes)
                })}}
            </li>
        </template>
    </ul>
</template>
<script>
import moment from 'moment'
import {DateFormats, humanFileSize} from '~/shared/helpers'
export default {
    props: {
        syncInfo: {
            type: Object,
        },
        prefix: {
            type: String,
        },
    },
    methods: {
        ouputBytes(val){
            return humanFileSize(val, false)
        },
        formatLastCommunication(date){
            return moment.utc(date).fromNow()
        },
        formatDate(date){
            return moment.utc(date).format(DateFormats.dateTime)
        },
        formatDuration(d){
            return moment.utc(moment.duration(d).as('milliseconds')).format('HH:mm:ss')
        },
    },
}
</script>
