<template>
<div>
    <Confirm ref="move" :title="$t('Pages.Interviewers_MoveInterviewerPopupTitle', {names: this.formatNames(this.interviewers)})" slot="modals"
        :disableOk="!whatToDoWithAssignments || !supervisor">

        <Typeahead
          ref="supervisorControl"
          control-id="supervisor"
          fuzzy
          data-vv-name="supervisor"
          data-vv-as="supervisor"
          :placeholder="$t('Common.AllSupervisors')"
          :value="supervisor"
          :fetch-url="$config.model.supervisorsUrl"
          v-on:selected="supervisorSelected"
        />

        <br />
        <br />
        <div v-if="supervisor && interviewersToStay.length > 0">
            <p v-html="$t('Pages.Interviewers_InterviewersToStay', { interviewers: `<b>${interviewersToStayNamesOnly}</b>`, supervisor: `<b>${selectedSupervisor}</b>` })"></p>
        </div>
        <div v-if="supervisor && interviewersToMove.length > 0">
            <p v-html="$t('Pages.Interviewers_InterviewersToMove', { interviewers: `<b>${interviewersToMoveNamesOnly}</b>`, supervisor: `<b>${selectedSupervisor}</b>`})"></p>
        </div>

        <div class="radio" v-if="supervisor && interviewersToMove.length > 0">
            <input id="reassignToOriginalSupervisor" v-model="whatToDoWithAssignments" name="whatToDoWithAssignments" value="ReassignToOriginalSupervisor" type="radio" class="wb-radio">
            <label for="reassignToOriginalSupervisor">
                <span class="tick"></span>
                {{ $t('Pages.Interviewers_ReassignToOriginalSupervisor') }}
            </label>
        </div>
        <div class="radio" v-if="supervisor && interviewersToMove.length > 0">
            <input id="moveAllToNewTeam" type="radio" v-model="whatToDoWithAssignments" name="whatToDoWithAssignments" value="MoveAllToNewTeam" class="wb-radio">
            <label for="moveAllToNewTeam">
                <span class="tick"></span>
                <span v-html="$t('Pages.Interviewers_MoveAllToNewTeam', { supervisor: `<b>${selectedSupervisor}</b>`})"></span>
            </label>
        </div>
    </Confirm>


    <ModalFrame ref="progress" id="move-interviewer-progress-template">
        <div class="max-height-in-popup">
            <table class="table table-striped table-bordered table-hover">
                <thead>
                    <tr>
                        <th>{{ $t('MainMenu.Interviewers') }}</th>
                        <th>{{ $t('MainMenu.Interviews') }}</th>
                        <th>{{ $t('MainMenu.Assignments') }}</th>
                    </tr>
                </thead>
                <tbody  v-for="interviewer in interviewers" v-bind:key="interviewer.userId">
                    <tr>
                        <td><span class="interviewer">
                            <a target="_blank" :href="model.interviewerProfileUrl + '/' + interviewer.userId" :text="interviewer.userName" :class="{'text-danger' : interviewer.inProgress }"></a>
                        </span></td>
                        <td data-bind="text: interviewsProcessed"></td>
                        <td data-bind="text: assignmentsProcessed"></td>
                    </tr>
                    <tr  v-if="interviewer.errors">
                        <td colspan="5">
                            <p>{{ $t('Pages.Interviewers_FinishedWithErrors') }}</p>
                            <ul v-for="error in errors"  v-bind:key="error.id">
                                <li :v-text="error"></li>
                            </ul>
                        </td>
                    </tr>
                </tbody>
            </table>
        </div>
    </ModalFrame>
</div>    
</template>

<script>

export default {
    data() {
        return {
            moveUserToAnotherTeamUrl: null,
            countInterviewersToAssign: 0,
            whatToDoWithAssignments : null,
            supervisor: null
        }
    },
    props: {
        interviewers: {
            type: Array, 
            default: []
        }
    },
    mounted() {

    },
    methods: {
        supervisorSelected(option) {
            this.supervisor = option
        },
        formatNames(interviewers, limit) {
            limit = limit || 3;

            var names = _.map(interviewers, function (interviewer) { 
                return interviewer.userName
            })

            if (names === null || names === undefined || names.length === 0)
                return '';

            if (names.length === 1)
                return names[0];

            if (names.length <= limit) {
                var sliceLength = Math.min(limit, names.length);
                return this.$t('Pages.Interviewers_NamesFormatMoreThanLimit', { names: names.slice(0, sliceLength - 1).join(', '), more: names[sliceLength - 1]})
            }

            return this.$t('Pages.Interviewers_NamesFormatMoreThanLimit', { names: names.slice(0, limit).join(', '), more: names.length - limit})
        },       
        moveToAnotherTeam() {
            var self = this
            this.$refs.move.promt(async ok => {
                if (ok) {
                    if (_.isUndefined(self.supervisor))
                        return;

                    if (_.isEmpty(self.whatToDoWithAssignments))
                        return;

                    if (self.interviewersToMove.length > 0) {
                        self.runMoveInterviewersProgress()
                    }
                }
            })
        },
        runMoveInterviewersProgress() {
            var self = this

            _.forEach(this.interviewers,
                function (interviewer) {
                    interviewer.inProgress = false;
                    interviewer.processed = false;
                    interviewer.interviewsProcessed = "-";
                    interviewer.interviewsProcessedWithErrors = "-";
                    interviewer.assignmentsProcessed = "-";
                    interviewer.assignmentsProcessedWithErrors = "-";
                    interviewer.errors = [];
                });

            this.$refs.progress.modal()

            _.forEach(this.interviewers,
                async function (interviewer) {
                    
                    interviewer.inProgress = true 

                    var request = {
                        InterviewerId: interviewer.userId,
                        OldSupervisorId: interviewer.supervisorId,
                        NewSupervisorId: supervisor.UserId,
                        Mode: whatToDoWithAssignments
                    }

                    var moveResult = await this.$http.post(this.moveUserToAnotherTeamUrl, {
                        interviewerId: interviewer.userId,
                        oldSupervisorId: interviewer.supervisorId,
                        newSupervisorId: supervisor.UserId,
                        mode: self.whatToDoWithAssignments
                    })

                    interviewer.inProgress = false
                    interviewer.processed = true
                    interviewer.interviewsProcessed = moveResult.interviewsProcessed
                    interviewer.interviewsProcessedWithErrors = moveResult.interviewsProcessedWithErrors
                    interviewer.assignmentsProcessed = moveResult.assignmentsProcessed
                    interviewer.assignmentsProcessedWithErrors = moveResult.assignmentsProcessedWithErrors
                    interviewer.errors = moveResult.errors
                }
            );
        }
    },
    computed: {
        model() {
            return this.$config.model;
        },
        selectedSupervisor() {
            return (this.supervisor || {}).value || ''
        },
        interviewersToMove() {
            var self = this
            var inters = _.filter(this.interviewers, function (interviewer) { 
                return interviewer.supervisorName != self.selectedSupervisor
            })
            return inters
        },
        interviewersToStay() {
            var inters = _.filter(this.interviewers, {supervisorName: this.selectedSupervisor })
            return inters
        },
        interviewersToMoveNamesOnly() {
            return this.formatNames(this.interviewersToMove)
        },
        interviewersToStayNamesOnly() {
            return this.formatNames(this.interviewersToStay)
        },
        countInterviewersToMove() {
            return this.interviewersToMove().length
        }
    }
}
</script>
