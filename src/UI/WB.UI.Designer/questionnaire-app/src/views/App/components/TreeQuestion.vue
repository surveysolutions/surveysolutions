<template> TreeQuestion {{ item.title }} </template>

<script>
export default {
    name: 'TreeQuestion',
    props: {
        id: { type: String, required: true },
        item: { type: Object, required: true }
    },
    data() {
        return {};
    }
};
</script>
