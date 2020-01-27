<template>
    <div class="block-filter"
        :class="{ 'separate-filters-group': separate }">
        <slot />
    </div>
</template>

<script>
export default {
    props: {
        separate: {
            type: Boolean,
        },
    },
}
</script>
