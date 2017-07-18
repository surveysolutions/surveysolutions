<template>
  <div class="block-filter">
        <h5>{{title}}</h5>
        <slot></slot>
    </div>
</template>

<script>
export default {
    props: [
        "title"
    ]
}
</script>