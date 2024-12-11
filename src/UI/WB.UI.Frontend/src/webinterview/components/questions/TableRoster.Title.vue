<template>
    <div class="ag-cell-label-container">
        <div class="ag-header-cell-label">
            <span class="ag-header-cell-text" v-dompurify-html="title"></span>
        </div>
    </div>
</template>

<script lang="js">
export default {
    name: 'TableRoster_Title',

    data() {
        return {
            title: null,
        }
    },
    created() {
        this.title = this.params.title
    },
    watch: {
        ['params.context.componentParent.$me.title']() {
            this.title = this.params.context.componentParent.$me.title
        },
    },
}
</script>
