<template>
    <HqLayout
        :title="$t('Pages.Admin_InterviewPackages_Title', {count: totalCount})"
        :subtitle="$t('Pages.Admin_InterviewPackages_Subtitle')"
        :hasFilter="true">
        <Filters slot="filters">
            <FilterBlock
                :title="$t('Pages.Admin_InterviewPackages_Type')">
                <input type="radio"
                    name="type"
                    id="typeBroken"
                    :value="false"
                    v-model="returnOnlyUnknownExceptionType">
                <label for="typeBroken">
                    {{$t('Pages.Admin_InterviewPackages_Broken')}}
                </label>
                <input type="radio"
                    id="typeRejected"
                    name="type"
                    :value="true"
                    v-model="returnOnlyUnknownExceptionType">
                <label for="typeRejected">
                    {{$t('Pages.Admin_InterviewPackages_Rejected')}}
                </label>
            </FilterBlock>
            <FilterBlock
                :title="$t('Pages.Admin_InterviewPackages_Interviewer')">
                <Typeahead
                    control-id="responsibleSelector"
                    :placeholder="$t('Pages.Admin_InterviewPackages_SelectInterviewer')"
                    :value="responsible"
                    v-on:selected="selectResponsible"
                    :fetch-url="`${this.$hq.basePath}api/Teams/InterviewersCombobox`"/>
            </FilterBlock>
            <FilterBlock
                :title="$t('Pages.Admin_InterviewPackages_Questionnaire')">
                <Typeahead
                    control-id="questionnaireSelector"
                    :placeholder="$t('Pages.Admin_InterviewPackages_SelectQuestionnaire')"
                    :value="questionnaireIdentity"
                    v-on:selected="selectQuestionnaire"
                    :fetch-url="`${this.$hq.basePath}api/QuestionnairesApi/QuestionnairesCombobox`"/>
            </FilterBlock>
            <FilterBlock v-if="returnOnlyUnknownExceptionType"
                :title="$t('Pages.Admin_InterviewPackages_ExceptionType')">
                <Typeahead
                    control-id="exceptionTypeSelector"
                    :placeholder="$t('Pages.Admin_InterviewPackages_SelectExceptionType')"
                    :value="exceptionType"
                    v-on:selected="selectExceptionType"
                    :fetch-url="`${this.$hq.basePath}api/ControlPanelApi/ExceptionTypes`"/>
            </FilterBlock>
            <FilterBlock
                :title="$t('Pages.Admin_InterviewPackages_Period')">
                <DatePicker :config="datePickerConfig"
                    :value="selectedDateRange"
                    :withClear="true"></DatePicker>
            </FilterBlock>
        </Filters>

        <DataTables
            ref="table"
            :tableOptions="tableOptions"
            :selectable="true"
            selectableId="id"
            @selectedRowsChanged="rows => (selectedPackages = rows)"
            @totalRows="rows => (totalCount = rows)"
            @page="resetSelection"
            :addParamsToRequest="addParamsToRequest"
            mutliRowSelect
            :noPaging="false"></DataTables>

        <Confirm
            ref="confirmReprocessSelected"
            id="confirmReprocessSelected"
            slot="modals">{{$t('Pages.Admin_InterviewPackages_ReprocessSelectedConfirmation')}}</Confirm>
        <ModalFrame ref="putReasonModal"
            :title="$t('Pages.ConfirmationNeededTitle')">
            <p>{{ $t("Pages.Admin_InterviewPackages_NumberOfPackagesAffected", {count: selectedPackages.length} )}}</p>
            <form onsubmit="return false;">
                <div class="form-group">
                    <label
                        class="control-label"
                        for="reasonId">{{ $t("Pages.Admin_InterviewPackages_Reason") }}</label>
                    <Typeahead
                        control-id="reasonId"
                        :placeholder="$t('Pages.Admin_InterviewPackages_SelectReason')"
                        :value="reason"
                        :ajax-params="{ }"
                        @selected="selectReason"
                        :fetch-url="`${this.$hq.basePath}api/ControlPanelApi/ExpectedExceptionTypes`"></Typeahead>
                </div>
            </form>
            <div slot="actions">
                <button
                    type="button"
                    class="btn btn-primary"
                    @click="putReasonAsync"
                    :disabled="!reason">{{ $t("Common.Ok") }}</button>
                <button
                    type="button"
                    class="btn btn-link"
                    data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>
        <div class="panel panel-table"
            v-if="hasSelectedPackages">
            <div class="panel-body">
                <input
                    class="double-checkbox-white"
                    id="q1az"
                    type="checkbox"
                    checked
                    disabled="disabled"/>
                <label for="q1az">
                    <span class="tick"></span>
                    <span>{{$t('Pages.Admin_InterviewPackages_SelectedPackagesCount', {count:selectedPackages.length})}}</span>
                </label>
                <button
                    type="button"
                    class="btn btn-primary"
                    @click="reprocessSelected">{{$t('Pages.Admin_InterviewPackages_Reprocess')}}</button>
                <button
                    type="button"
                    class="btn btn-primary"
                    @click="showReasonModal">{{$t('Pages.Admin_InterviewPackages_PutReason')}}</button>
            </div>
        </div>
    </HqLayout>
</template>

<script>
import Vue from 'vue'
import moment from 'moment'
import {DateFormats, humanFileSize} from '~/shared/helpers'
export default {
    data() {
        return {
            totalCount: 0,
            selectedPackages: [],
            responsible: null,
            questionnaireIdentity: null,
            exceptionType: null,
            returnOnlyUnknownExceptionType: false,
            dateRange: null,
            reason: null,
        }
    },
    mounted() {
        this.loadData()
        window.ajustDetailsPanelHeight()
    },
    watch: {
        responsible: function (value) {
            this.loadData()
        },
        questionnaireIdentity: function (value) {
            this.loadData()
        },
        returnOnlyUnknownExceptionType: function (value) {
            this.loadData()
        },
        exceptionType: function (value) {
            this.loadData()
        },
        dateRange:function(value){
            this.loadData()
        },
    },
    methods: {
        addParamsToRequest(requestData) {
            requestData.responsibleId = (this.responsible || {}).key
            requestData.questionnaireIdentity = (
                this.questionnaireIdentity || {}
            ).key
            if(this.dateRange != null){
                requestData.fromProcessingDateTime = moment(this.dateRange.startDate).format(DateFormats.date)
                requestData.toProcessingDateTime = moment(this.dateRange.endDate).format(DateFormats.date)
            }

            requestData.exceptionType = (this.exceptionType || {}).key
            requestData.returnOnlyUnknownExceptionType = this.returnOnlyUnknownExceptionType
        },
        loadData() {
            if (this.$refs.table) {
                this.$refs.table.reload()
            }
        },
        resetSelection() {
            this.selectedPackages.splice(0, this.selectedPackages.length)
        },
        selectResponsible(value){
            this.responsible = value
        },
        selectQuestionnaire(value){
            this.questionnaireIdentity = value
        },
        selectExceptionType(value){
            this.exceptionType = value
        },
        selectReason(value){
            this.reason = value
        },
        reprocessSelected() {
            var self = this
            this.$refs.confirmReprocessSelected.promt(async ok => {
                if (ok) await this.$http.post(`${self.$hq.basePath}api/ControlPanelApi/ReprocessSelectedBrokenPackages`, {
                    packageIds: self.selectedPackages,
                }).then(function(response) {self.loadData()})
            })
        },
        showReasonModal() {
            this.$refs.putReasonModal.modal()
        },
        async putReasonAsync() {
            var self = this
            await this.$http.post(`${self.$hq.basePath}api/ControlPanelApi/MarkReasonAsKnown`, {
                errorType: self.reason.key,
                packageIds: self.selectedPackages,
            })

            this.$refs.putReasonModal.hide()
            this.reason = null
            this.loadData()
        },
    },
    computed: {
        model() {
            return this.$config.model
        },
        tableOptions() {
            var self = this
            return {
                deferLoading: 0,
                columns: [
                    {
                        data: 'interviewKey',
                        name: 'InterviewKey',
                        title: this.$t('Pages.Admin_InterviewPackages_InterviewKey'),
                        orderable: true,
                        className: 'dt-head-nowrap',
                    },
                    {
                        data: 'interviewId',
                        name: 'InterviewId',
                        title: this.$t('Pages.Admin_InterviewPackages_InterviewId'),
                        orderable: true,
                        className: 'nowrap',
                        render: function(data, _, row) {
                            return  `<a href='${self.$hq.basePath}Interview/Review/${data}'>${data}</a>`
                        },
                    },
                    {
                        data: 'incomingDate',
                        name: 'IncomingDate',
                        className: 'date',
                        title: this.$t('Pages.Admin_InterviewPackages_IncomingDate'),
                        orderable: true,
                        render: function(data, type, row) {
                            var localDate = moment.utc(data).local()
                            return localDate.format(DateFormats.dateTimeInList)
                        },
                    },
                    {
                        data: 'processingDate',
                        name: 'ProcessingDate',
                        className: 'date',
                        title: this.$t('Pages.Admin_InterviewPackages_ProcessingDate'),
                        orderable: true,
                        render: function(data, type, row) {
                            var localDate = moment.utc(data).local()
                            return localDate.format(DateFormats.dateTimeInList)
                        },
                    },
                    {
                        data: 'packageSize',
                        name: 'PackageSize',
                        title: this.$t('Pages.Admin_InterviewPackages_PackageSize'),
                        orderable: true,
                        render: function(data, type, row) {
                            return  `<a href='${self.$hq.basePath}api/ControlPanelApi/DownloadSyncPackage/${row.id}'>${humanFileSize(data, false)}</a> / <a href='${self.$hq.basePath}api/ControlPanelApi/DownloadSyncPackage/${row.id}?format=json'>json</a>`
                        },
                    },
                    {
                        data: 'exceptionType',
                        name: 'ExceptionType',
                        title: this.$t('Pages.Admin_InterviewPackages_ExceptionType'),
                        orderable: true,
                    },
                    {
                        data: 'exceptionMessage',
                        name: 'ExceptionMessage',
                        title: this.$t('Pages.Admin_InterviewPackages_Exception'),
                        orderable: true,
                        render: function(data, type, row) {
                            return '<div class="accordion-group accordion-caret">' +
                            '<div class="accordion-heading">' +
                                `<a class="accordion-toggle collapsed" data-toggle="collapse" href= "#show${row.id}">` +
                                    `<strong>${data}</strong>` +
                                '</a>' +
                            '</div>' +
                            `<div class="accordion-body collapse" id="show${row.id}">` +
                                `<pre class="accordion-inner margin-left10">${row.exceptionStackTrace}</pre>` +
                            '</div>' +
                        '</div>'
                        },
                    },
                ],
                ajax: {
                    url: `${this.$hq.basePath}api/ControlPanelApi/InterviewPackages`,
                    type: 'GET',
                    contentType: 'application/json',
                },
                sDom: 'rf<"table-with-scroll"t>ip',
            }
        },
        datePickerConfig() {
            var self = this
            return {
                mode: 'range',
                maxDate: 'today',
                wrap: true,
                onChange: (selectedDates, dateStr, instance) => {
                    const start = selectedDates.length > 0 ? selectedDates[0] : null
                    const end = selectedDates.length > 1 ? selectedDates[1] : null
                    if(start != null && end != null){
                        self.dateRange = {
                            startDate : start,
                            endDate : end,
                        }
                    }
                },
            }
        },
        selectedDateRange() {
            if (this.dateRange == null) return null
            return `${moment(this.dateRange.startDate).format(DateFormats.date)} to ${moment(this.dateRange.endDate).format(DateFormats.date)}`
        },
        hasSelectedPackages() {
            return this.selectedPackages.length > 0
        },
    },
}
</script>
