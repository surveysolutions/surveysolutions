<template>
    <div class="wrapper-info">
        <div class="container-info">
            <button class="btn btn-link btn-plus" v-if="hasChild" :class="{ collapsed: !isExpanded }" type="button"
                @click="() => (isExpanded = !isExpanded)">
                <span></span>
            </button>
            <h4 class="gray-uppercase" @click="() => (isExpanded = !isExpanded)">{{ title }}</h4>
            <div class="list-item-body p-8" v-show="isExpanded">
                <slot />
            </div>
        </div>
    </div>
</template>

<style>
.complete .btn-plus {
    display: inline;
    position: absolute;
    left: 35px;
    padding-top: 0px;
}
</style>

<script type="text/javascript">

export default {
    data() {
        return {
            isExpanded: false,
            toggletText: ["show", "hide"],
        }
    },
    props: {
        title: { type: String, default: '' },
        hasChild: { type: Boolean, default: true },
    },
    methods: {

    },
}
</script>
