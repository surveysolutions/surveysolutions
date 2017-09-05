<template>
    <aside class="filters">
        <div class="foldback-button" id="hide-filters">
            <span class="arrow"></span>
            <span class="arrow"></span>
            <span class="glyphicon glyphicon-tasks" aria-hidden="true"></span>
        </div>
        <div class="filters-container">
            <h4>{{$t('Pages.FilterTitle')}}</h4>
            <slot></slot>
        </div>
    </aside>
</template>

<script>
export default {

}
</script>