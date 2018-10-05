<template>
    <HqLayout :title="title"
            :hasFilter="true">

        <Filters slot="filters">
            <FilterBlock :title="$t('Common.Questionnaire')" :tooltip="$t('Assignments.Tooltip_Filter_Questionnaire')">
                <Typeahead data-vv-name="questionnaireId"
                           data-vv-as="questionnaire"
                           :placeholder="$t('Common.AllQuestionnaires')"
                           control-id="questionnaireId"
                           :ajax-params="questionnaireParams"
                           :value="questionnaireId"
                           v-on:selected="questionnaireSelected"
                           :fetch-url="config.api.questionnaire">
                </typeahead>
            </FilterBlock>

            <FilterBlock :title="$t('Common.Responsible')" :tooltip="$t('Assignments.Tooltip_Filter_Responsible')">
                <Typeahead :placeholder="$t('Common.AllResponsible')"
                           control-id="responsibleId"
                           :value="responsibleId"
                           :ajax-params="responsibleParams"
                           v-on:selected="userSelected"
                           :fetch-url="config.api.responsible"></Typeahead>
            </FilterBlock>

            <FilterBlock :title="$t('Assignments.ReceivedByTablet')" :tooltip="$t('Assignments.Tooltip_Filter_Received')">
                  <select class="ddl-receivedByTablet"
                        v-model="receivedByTablet">
                    <option v-bind:value="'All'" selected="selected">{{ $t("Assignments.ReceivedByTablet_All") }}</option>
                    <option v-bind:value="'Received'">{{ $t("Assignments.ReceivedByTablet_Received") }}</option>
                    <option v-bind:value="'NotReceived'">{{ $t("Assignments.ReceivedByTablet_NotReceived") }}</option>
                </select>
            </FilterBlock>

            <FilterBlock :title="$t('Assignments.ShowArchived')" :tooltip="$t('Assignments.Tooltip_Filter_ArchivedStatus')">
                <select class="ddl"
                        v-model="showArchive">
                    <option v-bind:value="false">{{ $t("Assignments.Active") }}</option>
                    <option v-bind:value="true">{{ $t("Assignments.Archived") }}</option>
                </select>
            </FilterBlock>
        </Filters>

        <DataTables ref="table"
                    :tableOptions="tableOptions"
                    :addParamsToRequest="addParamsToRequest"
                    :wrapperClass=" { 'table-wrapper': true }"
                    @cell-clicked="cellClicked"
                    @selectedRowsChanged="rows => selectedRows = rows"
                    @totalRows="(rows) => totalRows = rows"
                    @ajaxComlpete="isLoading = false"
                    @page="resetSelection"
                    :selectable="showSelectors">
            <div class="panel panel-table"
                 v-if="selectedRows.length">
                <div class="panel-body">
                    <input class="double-checkbox-white"
                           type="checkbox"
                           checked=""
                           disabled>
                    <label>
                        <span class="tick"></span>
                        {{ $t("Assignments.AssignmentsSelected", {count: selectedRows.length}) }}
                    </label>

                    <button class="btn btn-lg btn-primary"
                            v-if="showArchive && config.isHeadquarter"
                            @click="unarchiveSelected">{{ $t("Assignments.Unarchive") }}</button>

                    <button class="btn btn-lg btn-primary"
                            v-if="!showArchive"
                            @click="assignSelected">{{ $t("Common.Assign") }}</button>

                    <button class="btn btn-lg btn-danger"
                            v-if="!showArchive && config.isHeadquarter"
                            @click="archiveSelected">{{ $t("Assignments.Archive") }}</button>
                </div>
            </div>
        </DataTables>

        <ModalFrame ref="assignModal"
                    :title="$t('Pages.ConfirmationNeededTitle')">
            <p>{{ $t("Assignments.NumberOfAssignmentsAffected", {count: selectedRows.length} )}}</p>
            <form onsubmit="return false;">
                <div class="form-group">
                    <label class="control-label"
                           for="newResponsibleId">{{ $t("Assignments.SelectResponsible") }}</label>
                    <Typeahead :placeholder="$t('Common.Responsible')"
                               control-id="newResponsibleId"
                               :value="newResponsibleId"
                               :ajax-params="{ }"
                               @selected="newResponsibleSelected"
                               :fetch-url="config.api.responsible">
                    </Typeahead>
                </div>
            </form>
            <div slot="actions">
                <button type="button"
                        class="btn btn-primary"
                        @click="assign"
                        :disabled="!newResponsibleId">{{ $t("Common.Assign") }}</button>
                <button type="button"
                        class="btn btn-link"
                        data-dismiss="modal">{{ $t("Common.Cancel") }}</button>
            </div>
        </ModalFrame>

        <ModalFrame ref="editQuantityModal"
                    :title="$t('Assignments.ChangeSizeModalTitle', {assignmentId: editedRowId} )">
            <p>{{ $t("Assignments.SizeExplanation")}}</p>
            <form onsubmit="return false;">
                <div class="form-group"
                     v-bind:class="{'has-error': errors.has('editedQuantity')}">

                    <label class="control-label"
                           for="newQuantity">{{$t("Assignments.Size")}}</label>
                           
                    <input type="text"
                           class="form-control"
                           v-model.trim="editedQuantity"
                           name="editedQuantity"
                           number
                           v-validate="'regex:^-?([0-9]+)$|min_value:-1'"
                           :data-vv-as="$t('Assignments.Size')"
                           maxlength="9"
                           autocomplete="off"
                           @keyup.enter="updateQuantity"
                           id="newQuantity"
                           placeholder="1">

                    <p v-for="error in errors.collect('editedQuantity')" :key="error" class="text-danger">{{error}}</p>
                </div>
            </form>
            <div class="modal-footer">
                <button type="button"
                        class="btn btn-primary"
                        @click="updateQuantity">{{$t("Common.Save")}}</button>
                <button type="button"
                        class="btn btn-link"
                        data-dismiss="modal">{{$t("Common.Cancel")}}</button>
            </div>
        </ModalFrame>

    </HqLayout>
</template>

<script>

export default {
    data() {
        return {
            responsibleId: null,
            questionnaireId: null,
            wasInitialized: false,
            responsibleParams: { showArchived: true, showLocked: true },
            questionnaireParams: { censusOnly: false },
            isLoading: false,
            selectedRows: [],
            totalRows: 0,
            showArchive: false,
            receivedByTablet: 'All',
            newResponsibleId: null,
            editedRowId: null,
            editedQuantity: null
        }
    },

    computed: {
        title() {
            return this.$t("Assignments.AssignmentsHeader") + " (" + this.formatNumber(this.totalRows) + ")";
        },
        config(){
            return this.$config.model;
        },

        tableOptionsraw() {
            const self = this;

            return [
                {
                    data: "id",
                    name: "Id",
                    title: "Id",
                    responsivePriority: 2
                }, {
                    data: "responsible",
                    name: "Responsible.Name",
                    title: this.$t("Common.Responsible"),
                    tooltip: this.$t("Assignments.Tooltip_Table_Responsible"),
                    responsivePriority: 3,
                    render(data, type, row) {
                        var resultString = '<span class="' + row.responsibleRole.toLowerCase() + '">';
                        if (row.responsibleRole === 'Interviewer') {
                            resultString += '<a href="' + self.config.api.profile + "/" + row.responsibleId + '">' + data + "</a>";
                        } else {
                            resultString += data;
                        }
                        resultString += '</span>';
                        return resultString;
                    }
                }, {
                    data: "quantity",
                    name: "Quantity",
                    class: "type-numeric pointer",
                    searchHighlight: false,
                    searchable: false,
                    title: this.$t("Assignments.Size"),
                    tooltip: this.$t("Assignments.Tooltip_Table_Size"),
                    if() {
                        return self.config.isHeadquarter;
                    }
                }, {
                    data: "interviewsCount",
                    name: "InterviewsCount",
                    class: "type-numeric",
                    title: this.$t("Assignments.Count"),
                    tooltip: this.$t("Assignments.Tooltip_Table_Count"),
                    orderable: true,
                    searchable: false,
                    render(data, type, row) {
                        var result = "<a href='" + self.config.api.interviews + "?assignmentId=" + row.id + "'>" + data + "</a>";
                        return result;
                    },
                    defaultContent: "<span>" + this.$t("Assignments.Unlimited") + "</span>",
                    if() {
                        return self.config.isHeadquarter;
                    }
                }, {
                    data: "interviewsCount",
                    name: "InterviewsCount",
                    class: "type-numeric",
                    title: this.$t("Assignments.InterviewsNeeded"),
                    tooltip: this.$t("Assignments.Tooltip_Table_InterviewsNeeded"),
                    orderable: false,
                    searchable: false,
                    render(data, type, row) {
                        if (row.quantity < 0) return row.quantity;
                        return row.interviewsCount > row.quantity ? 0 : row.quantity - row.interviewsCount;
                    },
                    defaultContent: "<span>" + this.$t("Assignments.Unlimited") + "</span>",
                    if() {
                        return !self.config.isHeadquarter;
                    }
                }, {
                    data: "identifyingQuestions",
                    title: this.$t("Assignments.IdentifyingQuestions"),
                    tooltip: this.$t("Assignments.Tooltip_Table_IdentifyingQuestions"),
                    class: "prefield-column first-identifying last-identifying sorting_disabled visible",
                    orderable: false,
                    searchable: false,
                    render(data) {
                        const questionsWithTitles = _.map(data, (question) => { return question.title + ": " + question.answer });
                        return _.join(questionsWithTitles, ", ");
                    },
                    responsivePriority: 4
                }, {
                    data: "updatedAtUtc",
                    name: "UpdatedAtUtc",
                    title: this.$t("Assignments.UpdatedAt"),
                    tooltip: this.$t("Assignments.Tooltip_Table_UpdatedAt"),
                    searchable: false,
                    render(data) {
                        var date = moment.utc(data);
                        return date.local().format(global.input.settings.clientDateTimeFormat);
                    }
                }, {
                    data: "createdAtUtc",
                    name: "CreatedAtUtc",
                    title: this.$t("Assignments.CreatedAt"),
                    tooltip: this.$t("Assignments.Tooltip_Table_CreatedAt"),
                    searchable: false,
                    render(data) {
                        var date = moment.utc(data);
                        return date.local().format(global.input.settings.clientDateTimeFormat);
                    }
                }
            ]
        },

        showSelectors() {
            return !this.config.isObserver && !this.config.isObserving;
        },

        tableOptions() {
            const columns = this.tableOptionsraw
                .filter(x => x.if == null || x.if())

            var defaultSortIndex = _.findIndex(columns, {name: "UpdatedAtUtc"});
            if(this.showSelectors) defaultSortIndex += 1;

            var tableOptions = {
                rowId: "id",
                deferLoading: 0,
                order: [[defaultSortIndex, 'desc']],
                columns,
                ajax: { url: this.config.api.assignments, type: "GET" },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter',
                    info: false
                },
                sDom: 'fr<"table-with-scroll"t>ip',
                headerCallback: (thead) => {
                    for(let i=0;i<columns.length;i++){
                        $(thead).find('th').eq(i).attr("title", columns[i].tooltip);
                    }
                },
                searchHighlight: true
            };

            return tableOptions;
        }
    },

    methods: {
        addParamsToRequest(requestData) {
            requestData.responsibleId = (this.responsibleId || {}).key;
            requestData.questionnaireId = (this.questionnaireId || {}).key;
            requestData.showArchive = this.showArchive;
            requestData.dateStart = this.dateStart;
            requestData.dateEnd = this.dateEnd;
            requestData.userRole = this.userRole;
            requestData.receivedByTablet = this.receivedByTablet;
            requestData.teamId = this.teamId;
        },

        userSelected(newValue) {
            this.responsibleId = newValue;
        },

        questionnaireSelected(newValue) {
            this.questionnaireId = newValue;
        },

        newResponsibleSelected(newValue) {
            this.newResponsibleId = newValue;
        },

        startWatchers(props, watcher) {
            var iterator = (prop) => this.$watch(prop, watcher);

            props.forEach(iterator, this);
        },

        reloadTable() {
            this.isLoading = true;
            this.selectedRows.splice(0, this.selectedRows.length);
            this.$refs.table.reload(self.reloadTable);

            this.addParamsToQueryString();
        },

        addParamsToQueryString() {
            var queryString = { showArchive: this.showArchive };

            if (this.questionnaireId != null) {
                queryString.questionnaire =
                    this.questionnaireId.value.substring(this.questionnaireId.value.indexOf(")") + 2,
                        this.questionnaireId.value.length);
                queryString.version = this.questionnaireId.key.split("$")[1];
            }

            if (this.responsibleId)
                queryString.responsible = this.responsibleId.value;
            if (this.dateStart)
                queryString.dateStart = this.dateStart;
            if (this.dateEnd)
                queryString.dateEnd = this.dateEnd;
            if (this.userRole)
                queryString.userRole = this.userRole;
            if (this.receivedByTablet)
                queryString.receivedByTablet = this.receivedByTablet;
            if (this.teamId)
                queryString.teamId = this.teamId;

            this.$router.push({ query: queryString });
        },

        async archiveSelected() {
            await this.$http({
                method: 'delete',
                url: this.config.api.assignments,
                data: this.selectedRows
            })

            this.reloadTable();
        },

        async unarchiveSelected() {
            await this.$http.post(this.config.api.assignments + "/Unarchive",
                this.selectedRows
            )

            this.reloadTable();
        },

        assignSelected() {
            this.$refs.assignModal.modal({
                keyboard: false
            });
        },

        async assign() {
            await this.$http.post(this.config.api.assignments + "/Assign", {
                responsibleId: this.newResponsibleId.key,
                ids: this.selectedRows
            })

            this.$refs.assignModal.hide();
            this.newResponsibleId = null;
            this.reloadTable();
        },

        cellClicked(columnName, rowId, cellData) {
            if (columnName === 'Quantity' && this.config.isHeadquarter && !this.showArchive) {
                this.editedRowId = rowId;
                this.editedQuantity = cellData;
                this.$refs.editQuantityModal.modal('show')
            }
        },

        async updateQuantity() {
            const validationResult = await this.$validator.validateAll()

            if(validationResult == false) {
                return false;
            }
            
            const patchQuantityUrl = this.config.api.assignments + "/" + this.editedRowId + "/SetQuantity";

            let targetQuantity = null;

            if (this.editedQuantity == null || this.editedQuantity === "") {
                targetQuantity = 1;
            } else if (this.editedQuantity == -1) {
                targetQuantity = null;
            } else {
                targetQuantity = this.editedQuantity;
            }

            const self = this;
            this.$http.patch(patchQuantityUrl, {
                quantity: targetQuantity
            })
            .then(() => {
                this.$refs.editQuantityModal.hide();
                this.editedQuantity = this.editedRowId = null;
                this.reloadTable();
            })
            .catch(error => {
                self.errors.clear();
                self.errors.add({
                    field: 'editedQuantity',
                    msg: error.response.data.message,
                    id: error.toString()
                })
            })

            return false;
        },

        async loadResponsibleIdByName(onDone) {
            if (this.$route.query.responsible != undefined) {

                const requestParams = Object.assign({
                    query: this.$route.query.responsible, pageSize: 1, cache: false
                }, this.ajaxParams);

                const response = await this.$http.get(this.config.api.responsible, { params: requestParams })

                onDone(response.data.options.length > 0 ? response.data.options[0].key : undefined);
            } else onDone();
        },

        async loadQuestionnaireId(onDone) {
            let requestParams = null;

            if (this.$route.query.questionnaire != undefined) {
                requestParams = Object.assign({ query: this.$route.query.questionnaire, pageSize: 1, cache: false }, this.ajaxParams);
                const response = await this.$http.get(this.config.api.questionnaire, { params: requestParams })

                if (response.data.options.length > 0) {
                    onDone(response.data.options[0].key, response.data.options[0].value);
                }

            } else if (this.$route.query.questionnaireId != undefined) {
                requestParams = Object.assign({ questionnaireIdentity: this.$route.query.questionnaireId, cache: false }, this.ajaxParams);
                const response = await this.$http.get(this.config.api.questionnaireById, { params: requestParams })

                if (response.data.options.length > 0) {
                    onDone(response.data.options[0].key, response.data.options[0].value);
                }
            } else onDone();
        },
        
        resetSelection() {
            this.selectedRows.splice(0,this.selectedRows.length);
        },
        formatNumber(value) {
            if (value == null || value == undefined)
                return value;
            var language = navigator.languages && navigator.languages[0] || 
               navigator.language || 
               navigator.userLanguage; 
            return value.toLocaleString(language);
        }
    },

    mounted() {
        var self = this;

        $("main").removeClass("hold-transition");
        $("footer").addClass("footer-adaptive");

        this.showArchive = this.$route.query.showArchive != undefined &&
            this.$route.query.showArchive === "true";

        this.dateStart = this.$route.query.dateStart;
        this.dateEnd = this.$route.query.dateEnd;
        this.userRole = this.$route.query.userRole;
        this.receivedByTablet = this.$route.query.receivedByTablet;
        this.teamId = this.$route.query.teamId;

        self.loadQuestionnaireId((questionnaireId, questionnaireTitle) => {

            if (questionnaireId != undefined) {
                self.questionnaireId = {
                    key: questionnaireId,
                    value: questionnaireTitle
                };
            }

            self.loadResponsibleIdByName((responsibleId) => {
                if (responsibleId != undefined)
                    self.responsibleId = { key: responsibleId, value: self.$route.query.responsible };

                self.reloadTable();
                self.startWatchers(['responsibleId', 'questionnaireId', 'showArchive', 'receivedByTablet'], self.reloadTable.bind(self));

                $('.ddl').selectpicker('val', self.showArchive.toString());
                $('.ddl-receivedByTablet').selectpicker('val', self.receivedByTablet);
            });
        });
    }
}

</script>
