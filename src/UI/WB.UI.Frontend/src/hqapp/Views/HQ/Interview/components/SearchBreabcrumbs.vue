<template>
    <ol class="breadcrumb">
        <li v-for="link in sections"
            v-bind:key="link.target">
            <router-link :to="{
                name: 'section', params: { sectionId: link.target }
            }">{{link.title}}</router-link>
        </li>
    </ol>
</template>

<script>
export default {
    props: {
        sections: {
            type: Array,
            required: true,
        },
    },
}
</script>
