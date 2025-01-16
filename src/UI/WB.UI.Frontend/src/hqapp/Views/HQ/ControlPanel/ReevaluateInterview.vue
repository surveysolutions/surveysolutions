<template>
    <HqLayout :hasFilter="false">
        <template v-slot:headers>
            <div>
                <ol class="breadcrumb">
                    <li>
                        <a href="/ControlPanel">{{ $t('Pages.Admin_ControlPanelLinkTitle') }}</a>
                    </li>
                </ol>
                <h1>{{ $t('Pages.ReevaluateInterview') }}</h1>
            </div>
        </template>
        <div class="extra-margin-bottom">
            <div class="profile">
                <div class="col-sm-12">
                    <form-group :label="$t('Pages.Admin_ReevaluateInterview_InterviewId')" :error="errorByInterviewId"
                        :mandatory="true">
                        <TextInput v-model.trim="interviewId" :haserror="errorByInterviewId !== undefined"
                            id="InterviewId" />
                    </form-group>
                </div>
                <div class="col-sm-12">
                    <div class="block-filter">
                        <button type="submit" class="btn btn-success" style="margin-right:5px" id="btnReevaluate"
                            @click="reevaluateInterview"
                            :disabled="interviewId == null || errorByInterviewId != undefined">{{
                                $t('Pages.Admin_Reevaluate') }}</button>
                        <a class="btn btn-default" href="/ControlPanel" id="lnkCancel">
                            {{ $t('Common.Cancel') }}
                        </a>
                    </div>
                </div>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import * as toastr from 'toastr'

export default {
    data() {
        return {
            interviewId: null,
            errorByInterviewId: undefined,
        }
    },
    watch: {
        interviewId: function (val) {

            if (val == null || val == '')
                this.errorByInterviewId = this.$t('Pages.Admin_Reevaluate_InterviewId_Required')
            else if (!/^[0-9A-F]{8}-?[0-9A-F]{4}-?[0-9A-F]{4}-?[0-9A-F]{4}-?[0-9A-F]{12}$/i.test(val))
                this.errorByInterviewId = this.$t('Pages.Admin_Reevaluate_InterviewId_Invalid')
            else
                this.errorByInterviewId = undefined
        },
    },
    methods: {
        reevaluateInterview: function () {
            if (this.errorByInterviewId != undefined) return

            var self = this
            this.$http({
                method: 'post',
                url: `/ControlPanel/ReevaluateInterview/${self.interviewId}`,
            }).then(
                response => {
                    toastr.success(self.$t('Pages.Admin_Reevaluate_Success'))
                },
                e => {
                    if (e.response.data.message) toastr.error(e.response.data.message)
                    else if (e.response.data.ExceptionMessage)
                        toastr.error(e.response.data.ExceptionMessage)
                    else
                        toastr.error(
                            self.$t('Pages.GlobalSettings_UnhandledExceptionMessage')
                        )
                }
            )
        },
    },
}
</script>
