<template>
    <div class="container-fluid">
        <div class="row" v-if="hasFilter">
            <slot name="filters" />
        </div>
        <div :class="{'main-information' : hasFilter, 'information': !hasFilter}">
            <div class="page-header clearfix">
                <h1>{{title}}</h1>
            </div>
            <slot />
        </div>
        <slot name="modals" />
    </div>
</template>

<script>
export default {
    props: ["title", "hasFilter"],
    mounted() {
        window.title = this.title
    }
}
</script>
