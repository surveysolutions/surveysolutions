<template>
    <Layout :title="title"
            :hasFilter="true">

        <Filters slot="filters">
            <FilterBlock :title="$t('Common.Questionnaire')">
                <Typeahead data-vv-name="questionnaireId"
                           data-vv-as="questionnaire"
                           :placeholder="$t('Common.AllQuestionnaires')"
                           control-id="questionnaireId"
                           :ajax-params="questionnaireParams"
                           :value="questionnaireId"
                           v-on:selected="questionnaireSelected"
                           :fetch-url="$config.Api.Questionnaire">
                </typeahead>
            </FilterBlock>

            <FilterBlock :title="$t('Common.Responsible')">
                <Typeahead :placeholder="$t('Common.AllResponsible')"
                           control-id="responsibleId"
                           :value="responsibleId"
                           :ajax-params="{ }"
                           v-on:selected="userSelected"
                           :fetch-url="$config.Api.Responsible"></Typeahead>
            </FilterBlock>

            <FilterBlock :title="$t('Assignments.ShowArchived')">
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
                    @ajaxComlete="isLoading = false"
                    :selectable="!$config.IsObserver && !$config.IsObserving">
            <div class="panel panel-table"
                 v-if="selectedRows.length">
                <div class="panel-body">
                    <input class="double-checkbox-white"
                           type="checkbox"
                           checked=""
                           disabled>
                    <label>
                        <span class="tick"></span>
                        {{ $t("Assignments.AssignmentsSelected", [ selectedRows.length ]) }}
                    </label>

                    <button class="btn btn-lg btn-primary"
                            v-if="showArchive && $config.IsHeadquarter"
                            @click="unarchiveSelected">{{ $t("Assignments.Unarchive") }}</button>

                    <button class="btn btn-lg btn-primary"
                            v-if="!showArchive"
                            @click="assignSelected">{{ $t("Common.Assign") }}</button>

                    <button class="btn btn-lg btn-danger"
                            v-if="!showArchive && $config.IsHeadquarter"
                            @click="archiveSelected">{{ $t("Assignments.Archive") }}</button>
                </div>
            </div>
        </DataTables>

        <ModalFrame ref="assignModal"
                    :title="$t('Pages.ConfirmationNeededTitle')">
            <p>{{ $t("Assignments.NumberOfAssignmentsAffected", [selectedRows.length] ) }}</p>
            <form onsubmit="return false;">
                <div class="form-group">
                    <label class="control-label"
                           for="newResponsibleId">{{ $t("Assignments.SelectResponsible") }}</label>
                    <Typeahead :placeholder="$t('Common.Responsible')"
                               control-id="newResponsibleId"
                               :value="newResponsibleId"
                               :ajax-params="{ }"
                               @selected="newResponsibleSelected"
                               :fetch-url="$config.Api.Responsible">
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
                    :title="$t('Assignments.ChangeSizeModalTitle', [ editedRowId ])">
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
                           maxlength="9"
                           autocomplete="off"
                           @keyup.enter="updateQuantity"
                           id="newQuantity"
                           placeholder="1">
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

    </Layout>
</template>

<script>

export default {
    data() {
        return {
            responsibleId: null,
            questionnaireId: null,
            wasInitialized: false,
            questionnaireParams: { censusOnly: false },
            isLoading: false,
            selectedRows: [],
            totalRows: 0,
            showArchive: false,
            newResponsibleId: null,
            editedRowId: null,
            editedQuantity: null
        }
    },

    computed: {
        title() {
            return this.$t("Assignments.AssignmentsHeader") + " (" + this.totalRows + ")";
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
                    responsivePriority: 3,
                    render(data, type, row) {
                        var resultString = '<span class="' + row.responsibleRole.toLowerCase() + '">';
                        if (row.responsibleRole === 'Interviewer') {
                            resultString += '<a href="' + self.$config.Api.Profile + "/" + row.responsibleId + '">' + data + "</a>";
                        } else {
                            resultString += data;
                        }
                        resultString += '</span>';
                        return resultString;
                    }
                }, {
                    data: "interviewsCount",
                    name: "InterviewsCount",
                    class: "type-numeric",
                    searchHighlight: false,
                    searchable: false,
                    title: this.$t("Assignments.Size"),
                    render(data, type, row) {
                        var result = "<a href='" + self.$config.Api.Interviews + "?assignmentId=" + row.id + "'>" + data + "</a>";
                        return result;
                    },
                    if() {
                        return self.$config.IsHeadquarter;
                    }
                }, {
                    data: "quantity",
                    name: "Quantity",
                    class: "type-numeric",
                    title: this.$t("Assignments.Count"),
                    orderable: true,
                    searchable: false,
                    defaultContent: "<span>" + this.$t("Assignments.Unlimited") + "</span>",
                    if() {
                        return self.$config.IsHeadquarter;
                    }
                }, {
                    data: "quantity",
                    name: "Quantity",
                    class: "type-numeric",
                    title: this.$t("Assignments.InterviewsNeeded"),
                    orderable: false,
                    searchable: false,
                    render(data, type, row) {
                        if (row.quantity < 0) return row.quantity;
                        return row.quantity - row.interviewsCount;
                    },
                    defaultContent: "<span>" + this.$t("Assignments.Unlimited") + "</span>",
                    if() {
                        return !self.$config.IsHeadquarter;
                    }
                }, {
                    data: "identifyingQuestions",
                    title: this.$t("Assignments.IdentifyingQuestions"),
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
                    searchable: false,
                    render(data) {
                        var date = moment.utc(data);
                        return date.local().format(global.input.settings.clientDateTimeFormat);
                    }
                }, {
                    data: "createdAtUtc",
                    name: "CreatedAtUtc",
                    title: this.$t("Assignments.CreatedAt"),
                    searchable: false,
                    render(data) {
                        var date = moment.utc(data);
                        return date.local().format(global.input.settings.clientDateTimeFormat);
                    }
                }
            ]
        },

        tableOptions() {
            const columns = this.tableOptionsraw
                .filter(x => x.if == null || x.if())

            var tableOptions = {
                rowId: "id",
                deferLoading: 0,
                order: [[6, 'desc']],
                columns,
                ajax: { url: this.$config.Api.Assignments, type: "GET" },
                select: {
                    style: 'multi',
                    selector: 'td>.checkbox-filter'
                },
                sDom: 'fr<"table-with-scroll"t>ip'
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

            this.$router.push({ query: queryString });
        },

        async archiveSelected() {
            await this.$http({
                method: 'delete',
                url: this.$config.Api.Assignments,
                data: this.selectedRows
            })

            this.reloadTable();
        },

        async unarchiveSelected() {
            await this.$http.post(this.$config.Api.Assignments + "/Unarchive",
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
            await this.$http.post(this.$config.Api.Assignments + "/Assign", {
                responsibleId: this.newResponsibleId.key,
                ids: this.selectedRows
            })

            this.$refs.assignModal.hide();
            this.newResponsibleId = null;
            this.reloadTable();
        },

        cellClicked(columnName, rowId, cellData) {
            if (columnName === 'Quantity' && this.$config.IsHeadquarter && !this.showArchive) {
                this.editedRowId = rowId;
                this.editedQuantity = cellData;
                this.$refs.editQuantityModal.modal('show')
            }
        },

        async updateQuantity() {
            await this.$validator.validateAll()
            
            const patchQuantityUrl = this.$config.Api.Assignments + "/" + this.editedRowId + "/SetQuantity";

            let targetQuantity = null;

            if (this.editedQuantity == null || this.editedQuantity === "") {
                targetQuantity = 1;
            } else if (this.editedQuantity == -1) {
                targetQuantity = null;
            } else {
                targetQuantity = this.editedQuantity;
            }

            await this.$http.patch(patchQuantityUrl, {
                quantity: targetQuantity
            })

            this.$refs.editQuantityModal.hide();
            this.editedQuantity = this.editedRowId = null;
            this.reloadTable();

            return false;
        },

        async loadResponsibleIdByName(onDone) {
            if (this.$route.query.responsible != undefined) {

                const requestParams = Object.assign({
                    query: this.$route.query.responsible, pageSize: 1, cache: false
                }, this.ajaxParams);

                const response = await this.$http.get(this.$config.Api.Responsible, { params: requestParams })

                onDone(response.data.options.length > 0 ? response.data.options[0].key : undefined);
            } else onDone();
        },

        async loadQuestionnaireId(onDone) {
            let requestParams = null;

            if (this.$route.query.questionnaire != undefined) {
                requestParams = Object.assign({ query: this.$route.query.questionnaire, pageSize: 1, cache: false }, this.ajaxParams);
                const response = await this.$http.get(this.$config.Api.Questionnaire, { params: requestParams })

                if (response.data.options.length > 0) {
                    onDone(response.data.options[0].key, response.data.options[0].value);
                }

            } else if (this.$route.query.questionnaireId != undefined) {
                requestParams = Object.assign({ questionnaireIdentity: this.$route.query.questionnaireId, cache: false }, this.ajaxParams);
                const response = await this.$http.get(this.$config.Api.QuestionnaireById, { params: requestParams })

                if (response.data.options.length > 0) {
                    onDone(response.data.options[0].key, response.data.options[0].value);
                }
            } else onDone();
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
                self.startWatchers(['responsibleId', 'questionnaireId', 'showArchive'], self.reloadTable.bind(self));

                $('.ddl').selectpicker('val', self.showArchive.toString());
            });
        });
    }
}

</script>