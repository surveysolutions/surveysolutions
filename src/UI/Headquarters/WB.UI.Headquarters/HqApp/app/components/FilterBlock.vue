<template>
  <div class="block-filter" :title="tooltip">
        <h5>{{title}}</h5>
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