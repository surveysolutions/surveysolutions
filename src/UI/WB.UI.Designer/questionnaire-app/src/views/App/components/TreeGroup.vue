<template> TreeGroup {{ item.title }} </template>

<script>
export default {
    name: 'TreeGroup',
    props: {
        id: { type: String, required: true },
        item: { type: Object, required: true }
    },
    data() {
        return {};
    }
};
</script>
