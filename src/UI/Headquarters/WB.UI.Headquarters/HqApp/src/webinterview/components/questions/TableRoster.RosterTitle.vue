<template>
    <span v-html="roster.rosterTitle"></span>
</template>

<script lang="js">
    export default {
        name: 'TableRoster_RosterTitle',

        data() {
            return {
                rowIndex : null,
                tableRoster : null,
            }
        }, 
        computed: {
            roster(){
                return this.tableRoster.$me.instances[this.rowIndex];
            },
        },
        created() {
            this.rowIndex = this.params.value.rowIndex
            this.tableRoster = this.params.value.tableRoster
        }
    }
</script>











