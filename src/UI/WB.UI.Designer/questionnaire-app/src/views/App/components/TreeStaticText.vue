<template> TreeStaticText {{ item.text }} </template>

<script>
export default {
    name: 'TreeStaticText',
    props: {
        id: { type: String, required: true },
        item: { type: Object, required: true }
    },
    data() {
        return {};
    }
};
</script>
