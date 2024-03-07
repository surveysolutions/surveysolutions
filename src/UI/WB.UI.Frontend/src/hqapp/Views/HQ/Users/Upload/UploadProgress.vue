<template>
    <div>
        <slot name="title">
            <h3>{{ $t('UploadUsers.ImportingUserInfo') }} <br>{{ fileName }}</h3>
        </slot>
        <div class="row">
            <div class="col-sm-7 col-xs-12 action-block uploading-verifying active-preloading">
                <div class="import-progress">
                    <p>{{ $t('UploadUsers.Uploading', {
                importedUsersCount: importedUsersCount, totalUsersToImportCount:
                    totalUsersToImportCount
            }) }}</p>
                </div>
                <div class="cancelable-progress">
                    <div class="progress">
                        <div class="progress-bar" role="progressbar" aria-valuenow="60" aria-valuemin="0"
                            aria-valuemax="100" v-bind:style="{ width: importedUsersInPercents + '%' }">
                            <span class="sr-only">{{ importedUsersInPercents }}%</span>
                        </div>
                    </div>
                    <button class="btn  btn-link" type="button" @click="cancelUpload">
                        {{ $t('UploadUsers.Cancel')}}
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>


<script>
export default {
    data: function () {
        return {
            timerId: 0,
        }
    },
    computed: {
        config() {
            return this.$config.model
        },
        fileName() {
            return this.$store.getters.upload.fileName
        },
        progress() {
            return this.$store.getters.upload.progress
        },
        importedUsersCount() {
            return this.totalUsersToImportCount - this.progress.usersInQueue
        },
        totalUsersToImportCount() {
            return this.progress.totalUsersToImport
        },
        importedUsersInPercents() {
            return this.importedUsersCount / this.progress.totalUsersToImport * 100
        },
    },
    mounted() {
        this.updateStatus()
        this.timerId = window.setInterval(() => {
            this.updateStatus()
        }, 1500)
    },
    methods: {
        cancelUpload() {
            window.clearInterval(this.timerId)

            var self = this
            this.$http.post(this.config.api.importUsersCancelUrl).then(response => {
                self.$router.push({ name: 'upload' })
            })
        },
        updateStatus() {
            var self = this
            this.$http.get(this.config.api.importUsersStatusUrl).then(response => {
                this.$store.dispatch('setUploadStatus', response.data)

                if (response.data.usersInQueue == 0) {
                    window.clearInterval(self.timerId)
                    self.$http
                        .get(this.config.api.importUsersCompleteStatusUrl)
                        .then(response => {
                            self.$store.dispatch('setUploadCompleteStatus', response.data)
                            self.$router.push({ name: 'uploadcomplete' })
                        })
                }
            })
        },
    },
}
</script>
