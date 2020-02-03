<template>
    <HqLayout>
        <div class="row">
            <table class="table table-striped table-condensed">
                <thead>
                    <tr>
                        <th>file name</th>
                        <th>build</th>
                        <th>hash</th>
                        <th>size</th>
                        <th>updated at (utc)</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="file in files"
                        v-bind:key="file.fileName">
                        <td>{{ file.fileName }}</td>
                        <td>{{ file.build }}</td>
                        <td>{{ file.hash }}</td>
                        <td>{{ humanFileSize(file.fileSizeInBytes) }}</td>
                        <td>{{ formatDate(file.lastWriteTimeUtc) }}</td>
                    </tr>
                </tbody>
            </table>
        </div>
    </HqLayout>
</template>

<script>
import { DateFormats, humanFileSize } from '~/shared/helpers'
import moment from 'moment'

export default {
    data() {
        return {
            files: [],
        }
    },
    mounted() {
        this.$hq.ControlPanel.getApkInfos().then(response => {
            this.files = response.data
        })
    },
    methods: {
        formatDate(date) {
            return new moment(date).format(DateFormats.dateTime)
        },
        humanFileSize(size) {
            if (size == null) return ''
            return humanFileSize(size)
        },
    },
}
</script>
