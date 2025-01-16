<template>
    <aside class="filters">
        <div class="foldback-button" id="hide-filters" @click="toggleFilters()">
            <span class="arrow"></span>
            <span class="arrow"></span>
            <span class="glyphicon glyphicon-tasks" aria-hidden="true"></span>
        </div>
        <div class="filters-container">
            <h4>{{ titleText }}</h4>
            <slot></slot>
        </div>
        <slot name="additional">
        </slot>
    </aside>
</template>

<script>
export default {
    props: {
        title: { type: String, default: null },
    },
    computed: {
        titleText() {
            return this.title || this.$t('Pages.FilterTitle')
        }
    },
    methods: {
        toggleFilters() {
            $('.filters').toggleClass('hidden-filters')
            $('#hide-filters').parents('.row').toggleClass('fullscreen-hidden-filters')
        }
    }
}
</script>
