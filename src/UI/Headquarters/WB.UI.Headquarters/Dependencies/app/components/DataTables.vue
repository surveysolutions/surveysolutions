<template>
    <table class="table table-striped table-ordered table-bordered table-hover table-with-checkboxes table-with-prefilled-column table-interviews responsive">
        <thead></thead>
        <tbody></tbody>
    </table>
</template>

<script>

export default {

    props: {
        addParamsToRequest: {
            type: Function,
            default(d) { return d; }
        },
        responseProcessor: {
            type: Function,
            default(r) { return r; }
        },
        tableOptions: {
            type: Object,
            default() { return {}; }
        },
        contextMenuItems: {
            type: Function,
            default() { return []; }
        },
        authorizedUser: { type: Object, default() { return {} } }
    },
    
    data() {
        return {
            table: null
        };
    },

    methods: {
        reload(data) {
            this.table.ajax.data = this.addParamsToRequest(data || {});
            this.table.rows().deselect();
            this.table.ajax.reload();
        },

        disableRow(rowIndex) {
            $(this.table.row(rowIndex).node()).addClass("disabled")
        },

        selectRowAndGetData(selectedItem) {
            this.table.rows().deselect();
            var rowIndex = selectedItem.parent().children().index(selectedItem);
            this.table.row(rowIndex).select();
            return {
                rowData: this.table.rows({ selected: true }).data()[0],
                rowIndex
            }
        },

        onTableInitComplete() {
            var self = this;

            $(this.$el).parents('.dataTables_wrapper').find('.dataTables_filter label').on('click', function (e) {
                if (e.target !== this)
                    return;
                if ($(this).hasClass("active")) {
                    $(this).removeClass("active");
                }
                else {
                    $(this).addClass("active");
                    $(this).children("input[type='search']").delay(200).queue(function () { $(this).focus(); $(this).dequeue(); });
                }
            });
        },

        initContextMenu() {
            
            var contextMenuOptions = {
                selector: "#" + this.$el.attributes.id.value + " tbody tr",
                autoHide: false,
                build: ($trigger, e) => {
                    var selectedRow = this.selectRowAndGetData($trigger);
                    var items = this.contextMenuItems(selectedRow)
                    return { items: items };
                },
                trigger: 'left'
            };

            $.contextMenu(contextMenuOptions);
        }
    },

    mounted() {
        var self = this;
        var options = $.extend({
            processing: true,
            serverSide: true,
            language:
            {
                "url": window.input.settings.config.dataTableTranslationsUrl,
            },
            searchHighlight: true,
            pagingType: "full_numbers",
            lengthChange: false, // do not show page size selector
            pageLength: 20, // page size
            dom: "frtp",
            conditionalPaging: true
        }, this.tableOptions);

        options.ajax.data = (d) => {
            this.addParamsToRequest(d);
        };

        options.ajax.complete = (response) => {
            this.responseProcessor(response.responseJSON);
        };

        this.table = $(this.$el).DataTable(options);
        this.table.on('init.dt', this.onTableInitComplete);
        this.table.on('select', function (e, dt, type, indexes) {
            self.$emit('select', e, dt, type, indexes);
        });
        this.table.on('deselect', function (e, dt, type, indexes) {
            self.$emit('deselect', e, dt, type, indexes);
        });
        this.table.on('click', 'tbody td', function () {
            var cell = self.table.cell(this);

            if (cell.index().column > 0) {
                var rowId = self.table.row(this).id();
                var columns = self.table.settings().init().columns;

                self.$emit('cell-clicked', columns[this.cellIndex].name, rowId, cell.data());
            }
        });
        this.$emit('DataTableRef', this.table);

        this.initContextMenu();
    }

}
</script>
