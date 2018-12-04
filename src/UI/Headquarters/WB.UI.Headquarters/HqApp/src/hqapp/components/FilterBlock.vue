<template>
  <div class="block-filter" >
        <h5 :title="tooltip" v-if="title != null">{{title}}</h5>
        <slot></slot>
    </div>
</template>

<script>
export default {
    props: [
        "title",
        "tooltip"
    ]
}
</script>
