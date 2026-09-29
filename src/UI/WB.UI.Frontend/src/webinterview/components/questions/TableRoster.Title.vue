<template>
    <div class="ag-cell-label-container">
        <div class="ag-header-cell-label">
            <span class="ag-header-cell-text"
                v-dompurify-html="title"></span>
        </div>
    </div>
</template>

<script lang="js">
export default {
    name: 'TableRoster_Title',
    computed: {
        title() {
            return this.params.context.componentParent.$me.title ?? this.params.title ?? ''
        },
    },
}
</script>
