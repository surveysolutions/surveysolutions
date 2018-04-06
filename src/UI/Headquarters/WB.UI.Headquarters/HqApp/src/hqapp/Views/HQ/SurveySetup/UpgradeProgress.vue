<template>
  <div>
      {{processId}}
  </div>
</template>

<script>
export default {
    computed: {
        processId(){
            return this.$route.params.processId
        }
    }
}
</script>

