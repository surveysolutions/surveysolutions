<template>
    <div>
        <Confirm
            id="move-interviewer-confirmation"
            ref="move"
            :title="$t('Pages.Interviewers_MoveInterviewerPopupTitle', {names: this.formatNames(this.interviewers)})"
            slot="modals"
            :disableOk="!whatToDoWithAssignments || !supervisor">
            <div class="alert">
                <Typeahead
                    ref="supervisorControl"
                    control-id="supervisorToAssign"
                    data-vv-name="supervisor"
                    data-vv-as="supervisor"
                    :placeholder="$t('Common.AllSupervisors')"
                    :value="supervisor"
                    :ajax-params="{ workspace: this.workspace }"
                    :fetch-url="$config.model.supervisorWorkspaceUrl"
                    v-on:selected="supervisorSelected"/>

                <br />
                <br />
                <div v-if="supervisor && interviewersToStay.length > 0">
                    <p
                        v-html="$t('Pages.Interviewers_InterviewersToStay', { interviewers: `<b>${interviewersToStayNamesOnly}</b>`, supervisor: `<b>${selectedSupervisor}</b>` })"></p>
                </div>
                <div v-if="supervisor && interviewersToMove.length > 0">
                    <p
                        v-html="$t('Pages.Interviewers_InterviewersToMove', { interviewers: `<b>${interviewersToMoveNamesOnly}</b>`, supervisor: `<b>${selectedSupervisor}</b>`})"></p>
                </div>

                <div class="radio"
                    v-if="supervisor && interviewersToMove.length > 0">
                    <input
                        id="reassignToOriginalSupervisor"
                        v-model="whatToDoWithAssignments"
                        name="whatToDoWithAssignments"
                        value="ReassignToOriginalSupervisor"
                        type="radio"
                        class="wb-radio"/>
                    <label for="reassignToOriginalSupervisor">
                        <span class="tick"></span>
                        {{ $t('Pages.Interviewers_ReassignToOriginalSupervisor') }}
                    </label>
                </div>
                <div class="radio"
                    v-if="supervisor && interviewersToMove.length > 0">
                    <input
                        id="moveAllToNewTeam"
                        type="radio"
                        v-model="whatToDoWithAssignments"
                        name="whatToDoWithAssignments"
                        value="MoveAllToNewTeam"
                        class="wb-radio"/>
                    <label for="moveAllToNewTeam">
                        <span class="tick"></span>
                        <span
                            v-html="$t('Pages.Interviewers_MoveAllToNewTeam', { supervisor: `<b>${selectedSupervisor}</b>`})"></span>
                    </label>
                </div>

                <span class="text-warning"
                    v-if="showWebModeReassignWarning">
                    {{$t('Pages.Interviewers_MoveWebAssigment')}}
                </span>
            </div>
        </Confirm>

        <ModalFrame ref="progress"
            id="move-interviewer-progress-template"
            :title="movingDialogTitle">
            <div class="max-height-in-popup">
                <table class="table table-striped table-bordered table-hover">
                    <thead>
                        <tr>
                            <th>{{ $t('MainMenu.Interviewers') }}</th>
                            <th>{{ $t('MainMenu.Interviews') }}</th>
                            <th>{{ $t('MainMenu.Assignments') }}</th>
                        </tr>
                    </thead>
                    <tbody
                        v-for="interviewer in progressInterviewers"
                        v-bind:key="interviewer.userId">
                        <tr>
                            <td>
                                <span class="interviewer">
                                    <a
                                        target="_blank"
                                        :href="$config.model.interviewerProfile + '/' + interviewer.userId"
                                        :v-text="interviewer.userName"
                                        :class="{'text-danger' : interviewer.inProgress }">{{interviewer.userName}}</a>
                                </span>
                            </td>
                            <td v-text="interviewer.interviewsProcessed"></td>
                            <td v-text="interviewer.assignmentsProcessed"></td>
                        </tr>
                        <tr v-if="interviewer.errors.length > 0">
                            <td colspan="5">
                                <p>{{ $t('Pages.Interviewers_FinishedWithErrors') }}</p>
                                <ul v-for="error in interviewer.errors"
                                    v-bind:key="error">
                                    <li :v-text="error">{{error}}</li>
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
import {map, isUndefined, isEmpty, filter} from 'lodash'
import gql from 'graphql-tag'

const query = gql`query assignments($workspace: String!, $where: AssignmentsFilter) {
  assignments(
      workspace: $workspace 
      where: $where 
      take: 1) 
  {
    filteredCount
  }
}`

export default {
    name: 'move-interviewers-to-other-team',

    data() {
        return {
            whatToDoWithAssignments: null,
            supervisor: null,
            progressInterviewers: [],
            movingDialogTitle: '',
            showWebModeReassignWarning: false,
            workspace: null,
            interviewers: [],
        }
    },
    mounted() {},
    methods: {
        supervisorSelected(option) {
            this.supervisor = option
        },
        formatNames(interviewers, limit) {
            limit = limit || 3

            var names = map(interviewers, function(interviewer) {
                return interviewer.userName
            })

            if (names === null || names === undefined || names.length === 0) return ''

            if (names.length === 1) return names[0]

            if (names.length <= limit) {
                var sliceLength = Math.min(limit, names.length)
                return this.$t('Pages.Interviewers_NamesFormatLessThanLimit', {
                    names: names.slice(0, sliceLength - 1).join(', '),
                    lastname: names[sliceLength - 1],
                })
            }

            return this.$t('Pages.Interviewers_NamesFormatMoreThanLimit', {
                names: names.slice(0, limit).join(', '),
                more: names.length - limit,
            })
        },
        moveToAnotherTeam(workspace, interviewers) {
            this.workspace = workspace
            this.interviewers = interviewers
            this.whatToDoWithAssignments = null
            this.supervisor = null
            this.movingDialogTitle = ''

            var self = this
            this.$refs.move.promt(async ok => {
                if (ok) {
                    if (isUndefined(self.supervisor)) return

                    if (isEmpty(self.whatToDoWithAssignments)) return

                    if (self.interviewersToMove.length > 0) {
                        self.runMoveInterviewersProgress()
                    }
                }
            })
        },
        async runMoveInterviewersProgress() {
            var self = this

            this.movingDialogTitle = this.$t('Pages.Interviewers_MovingIsInProgress')

            this.progressInterviewers = map(this.interviewers, function(interviewer) {
                var progressItem = {}
                progressItem.userId = interviewer.userId
                progressItem.userName = interviewer.userName
                progressItem.supervisorId = interviewer.supervisorId
                progressItem.inProgress = false
                progressItem.processed = false
                progressItem.interviewsProcessed = '-'
                progressItem.interviewsProcessedWithErrors = '-'
                progressItem.assignmentsProcessed = '-'
                progressItem.assignmentsProcessedWithErrors = '-'
                progressItem.errors = []
                return progressItem
            })

            await this.$refs.progress.modal()

            for (var index in this.progressInterviewers) {
                var interviewer = this.progressInterviewers[index]
                await self.migarateInterviewer(interviewer)
                await self.timeout(500)
            }

            this.movingDialogTitle = this.$t('Pages.Interviewers_MovingCompleted')

            this.$emit('moveInterviewersCompleted')
        },
        async migarateInterviewer(interviewer) {
            var self = this

            interviewer.inProgress = true

            var moveResult = await self.$http.post(self.$config.model.moveUserToAnotherTeamUrl, {
                interviewerId: interviewer.userId,
                oldSupervisorId: interviewer.supervisorId,
                newSupervisorId: self.supervisor.key,
                mode: self.whatToDoWithAssignments,
            })

            interviewer.inProgress = false
            interviewer.processed = true
            interviewer.interviewsProcessed = moveResult.data.interviewsProcessed
            interviewer.interviewsProcessedWithErrors = moveResult.data.interviewsProcessedWithErrors
            interviewer.assignmentsProcessed = moveResult.data.assignmentsProcessed
            interviewer.assignmentsProcessedWithErrors = moveResult.data.assignmentsProcessedWithErrors
            interviewer.errors = moveResult.data.errors
        },
        timeout(ms) {
            return new Promise(resolve => setTimeout(resolve, ms))
        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        movingTitle() {
            return this.$config.model
        },
        selectedSupervisor() {
            return (this.supervisor || {}).value || ''
        },
        interviewersToMove() {
            var self = this
            return filter(this.interviewers, function(interviewer) {
                return interviewer.supervisorName != self.selectedSupervisor
            })
        },
        interviewersToStay() {
            return filter(this.interviewers, {supervisorName: this.selectedSupervisor})
        },
        interviewersToMoveNamesOnly() {
            return this.formatNames(this.interviewersToMove)
        },
        interviewersToStayNamesOnly() {
            return this.formatNames(this.interviewersToStay)
        },
        countInterviewersToMove() {
            return this.interviewersToMove().length
        },
    },
    watch: {
        async whatToDoWithAssignments(newValue) {
            const self = this
            if(newValue === 'ReassignToOriginalSupervisor'){
                const interviewersArray = map(self.interviewersToMove, (i) => i.userId.replaceAll('-',''))

                const where = {and :[
                    {webMode: {eq: true}},
                    {archived: {eq: false}},
                    {responsibleId: {in: interviewersArray}}]}

                const response = await self.$apollo.query({
                    query,
                    variables: {
                        where: where,
                        workspace: this.$store.getters.workspace,
                    },
                    fetchPolicy: 'network-only',
                })

                self.showWebModeReassignWarning = response.data.assignments.filteredCount > 0
            } else {
                self.showWebModeReassignWarning = false
            }
        },
    },
}
</script>
