<template>
    <table class="table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews">
        <thead>
            <tr>
                <th class=" interview-id title-row">
                    Interview id
                </th>
                <th class="has-comments"><span class="comment-icon responded"></span></th>
                <th class="uploaded-to-hq">Uploaded to HQ</th>
                <th class="interview-conducted">interview conducted</th>
                <th class="flags">Flags</th>
                <th class="status">current Status</th>
                <th class="answered-questions">answered questions</th>
                <th class="left-empty">left empty</th>
                <th class="errors">errors</th>
                <th class="date last-update">last update</th>
                <th class="download-on-device">downloaded on device</th>
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
            this.table.ajax.reload();
        }
    },
    mounted () {
        const self = this
        this.table = $(this.$el).DataTable({
            processing: true,
            serverSide: true,
            ajax: {
                url: this.fetchUrl,
                type: "POST",
                data: self.filter
            },
            searchHighlight: true,
            rowId: 'id',
            pagingType: "full_numbers",
            lengthChange: false, // do not show page size selector
            pageLength: 50, // page size
            "order": [[2, 'desc']],
            dom: "frtp",
            conditionalPaging: true
        });
        this.$emit('DataTableRef', this.table)
    },
    destroyed () {
    }
}
</script>