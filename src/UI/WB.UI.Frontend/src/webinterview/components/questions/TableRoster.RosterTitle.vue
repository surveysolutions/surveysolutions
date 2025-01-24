<template>
    <div class="cell-unit">
        <span class="cell-content" v-dompurify-html="roster?.rosterTitle"></span>
    </div>
</template>

<script lang="js">
export default {
    name: 'TableRoster_RosterTitle',

    data() {
        return {
            rowIndex: null,
            tableRoster: null,
        }
    },
    computed: {
        roster() {
            if (this.tableRoster.$me.isLoading)
                return null

            return this.tableRoster.$me.instances[this.rowIndex]
        },
    },
    created() {
        this.rowIndex = this.params.value.rowIndex
        this.tableRoster = this.params.value.tableRoster
    },
}
</script>
