<template>
    <div class="wrapper-info">
        <div class="container-info" :class="[cssClass]">
            <h2>
                <button class="btn btn-link btn-plus" v-if="hasChild" :class="{ collapsed: !isExpanded }" type="button"
                    @click="() => (isExpanded = !isExpanded)">
                    <span></span>
                </button>
                <span @click="() => (isExpanded = !isExpanded)">{{ title }}</span>
            </h2>
            <div class="list-item-body p-8 wrapper-info-details" v-show="isExpanded">
                <slot />
            </div>
        </div>
    </div>
</template>

<style>
.complete-section .unanswered .btn-plus:after,
.complete-section .unanswered .btn-plus:before {
    background-color: #2a81cb;
}

.complete-section .errors .btn-plus:after,
.complete-section .errors .btn-plus:before,
.complete-section .critical-rule-errors .btn-plus:after,
.complete-section .critical-rule-errors .btn-plus:before {
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
