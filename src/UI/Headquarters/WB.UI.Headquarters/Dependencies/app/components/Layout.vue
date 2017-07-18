<template>
    <div class="container-fluid">
        <div class="row">
            <slot name="filters" />
        </div>
        <div :class="information">
            <div class="page-header clearfix">
                        <h1>
                            {{title}}
                        </h1>
                    </div>
            <slot />
        </div>
        <slot name="modals" />
    </div>
</template>

<script>
export default {
    props: {
        title: String,
        hasFilter: {
            type: Boolean,
            default() { return false; }
        }},
    computed: {
        information() {
            return {
                "main-information" : this.hasFilter,
                "information" : !this.hasFilter
            }
        }
    }
}
</script>
