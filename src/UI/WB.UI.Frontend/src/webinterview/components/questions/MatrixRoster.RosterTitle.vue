<template>
    <div class="cell-unit">
        <span class="cell-content" v-dompurify-html="roster.rosterTitle"></span>
    </div>
</template>

<script lang="js">
export default {
    name: 'MatrixRoster_RosterTitle',

    data() {
        return {
            rowIndex: null,
            matrixRoster: null,
        }
    },
    computed: {
        roster() {
            return this.matrixRoster.$me.instances[this.rowIndex]
        },
    },
    created() {
        this.rowIndex = this.params.value.rowIndex
        this.matrixRoster = this.params.value.matrixRoster
    },
}
</script>
