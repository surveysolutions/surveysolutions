<template> TreeVariable {{ item.variable }} </template>

<script>
export default {
    name: 'TreeVariable',
    props: {
        id: { type: String, required: true },
        item: { type: Object, required: true }
    },
    data() {
        return {};
    }
};
</script>
