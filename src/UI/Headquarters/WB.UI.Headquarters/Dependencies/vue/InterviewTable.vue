<template>
    <table class="table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews">
        <thead>
            
        </thead>
        <tbody></tbody>
    </table>
</template>

<script>
export default {
    props: {
        addParamsToRequest: {
            type: Function,
            default: (d) => { return d }
        },
        responseProcessor: {
            type: Function,
            default: (r) => { return r }
        },
        tableOptions: {
            type: Object,
            default: () => { return {} }
        }
    },
    data () {
        return {
            table: null
        }
    },
    computed: {
    },
    watch: {
    },
    methods: {
        reload: function(data){
            this.table.ajax.data = data;
            this.table.ajax.reload()
        }
    },
    mounted () {
        const self = this
        var options = Object.assign({
            processing: true,
            serverSide: true,
            language:
            {
                "url": window.input.settings.config.dataTableTranslationsUrl,
            },
            searchHighlight: true,
            pagingType: "full_numbers",
            lengthChange: false, // do not show page size selector
            pageLength: 10, // page size
            dom: "frtp",
            conditionalPaging: true
        }, this.tableOptions)

        options.ajax.data = function(d){
            self.addParamsToRequest(d);
        };

        options.ajax.complete = function(response){
            self.responseProcessor(response.responseJSON);
        };

        this.table = $(this.$el).DataTable(options);
        this.$emit('DataTableRef', this.table)
    },
    destroyed () {
    }
}
</script>