<template>
    <div class="wrapper-info">
        <div class="container-info" :class="[cssClass]">
            <h4>
                <button class="btn btn-link btn-plus" v-if="hasChild" :class="{ collapsed: !isExpanded }" type="button"
                    @click="() => (isExpanded = !isExpanded)">
                    <span></span>
                </button>
                <span @click="() => (isExpanded = !isExpanded)">{{ title }}</span>
            </h4>
            <div class="list-item-body p-8 wrapper-info-details" v-show="isExpanded">
                <slot />
            </div>
        </div>
    </div>
</template>

<style>
.complete .unanswered .btn-plus:after,
.complete .unanswered .btn-plus:before {
    background-color: #2a81cb;
}

.complete .errors .btn-plus:after,
.complete .errors .btn-plus:before,
.complete .critical-rule-errors .btn-plus:after,
.complete .critical-rule-errors .btn-plus:before {
    background-color: #e74924;
}
</style>

<script type="text/javascript">

export default {
    data() {
        return {
            isExpanded: false,
        }
    },
    props: {
        title: { type: String, default: '' },
        cssClass: { type: String, default: null },
        hasChild: { type: Boolean, default: true },
    },
    methods: {

    },
}
</script>
