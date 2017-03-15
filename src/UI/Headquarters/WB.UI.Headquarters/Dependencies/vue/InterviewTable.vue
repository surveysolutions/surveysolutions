<template>
    <table class="table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews">
        <thead>
            <tr>
                <th class="interview-id title-row">
                    Interview id
                </th>
                <th class="enumerator">enumerator</th>
                <th class="date last-update">last update</th>
            </tr>
        </thead>
        <tbody></tbody>
    </table>
</template>

<script>
export default {
    props: {
        filter: {
            type: Object,
            default: () => { return {} }
        },
       fetchUrl: {
            type: String,
       }
    },
    data () {
        return {
            table: null
        }
    },
    computed: {
        interviewFilters () {
            return JSON.stringify(this.filter)
        }
    },
    watch: {
        interviewFilters (newFilters) {
            this.table.ajax.data = this.filter
            this.table.ajax.reload()
        }
    },
    mounted () {
        const self = this
        this.table = $(this.$el).DataTable({
            processing: true,
            serverSide: true,
            language:
            {
                "url": window.input.settings.config.dataTableTranslationsUrl,
                searchPlaceholder: "@Pages.Search"
            },
            ajax: {
                url: this.fetchUrl,
                type: "POST",
                data: function(d){
                    d.QuestionnaireId = self.filter.questionnaireId
                    d.ChangedFrom = self.filter.changedFrom
                    d.ChangedTo = self.filter.changedTo
                    d.InterviewerId = self.filter.interviewerId
                }
            },
            columns: [
                {
                    data: "interviewId",
                    name: "InterviewId", // case-sensitive!
                    "class": "interview-id title-row",
                    render: function(data, type, row) {
                        return (row.wasCreatedOnClient === true ? "<span class='census-icon'>c</span>" : "") + data;
                    }
                },
                {
                    data: "responsibleName",
                    name: "ResponsibleName", // case-sensitive! should be DB name here from Designer DB questionnairelistviewitems? to sort column
                    "class": "enumerator",
                    render: function(data, type, row) {
                        return '<span class="interviewer">'+data+'</span>';
                    }
                    
                },
                {
                    data: "updateDate",
                    name: "UpdateDate", // case-sensitive! should be DB name here from Designer DB questionnairelistviewitems? to sort column
                    "class": "date"
                }
            ],
            searchHighlight: true,
            rowId: 'id',
            pagingType: "full_numbers",
            lengthChange: false, // do not show page size selector
            pageLength: 10, // page size
            "order": [[2, 'desc']],
            dom: "frtp",
            conditionalPaging: true,
            deferLoading: 0
        });
        this.$emit('DataTableRef', this.table)
    },
    destroyed () {
    }
}
</script>