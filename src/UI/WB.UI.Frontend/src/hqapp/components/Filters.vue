<template>
    <aside class="filters">
        <div class="foldback-button" id="hide-filters">
            <span class="arrow"></span>
            <span class="arrow"></span>
            <span class="glyphicon glyphicon-tasks" aria-hidden="true"></span>
        </div>
        <div class="filters-container">
            <h4>{{ title }}</h4>
            <slot></slot>
        </div>
        <slot name="additional">
        </slot>
    </aside>
</template>

<script>
export default {
    props: {
        //title: { type: String, default: () => this.$t('Pages.FilterTitle') },
        //TODO:MIGRATION
        title: { type: String, default: () => 'Pages.FilterTitle' },
    },
}
</script>
