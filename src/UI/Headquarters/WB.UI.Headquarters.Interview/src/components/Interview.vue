<template>
  <h1>{{ interviewId }}</h1>
</template>
<script>
  export default {
    name: 'interview-view',
    computed: {
      interviewId () {
        return this.$store.state.interview.id
      }
    },
    beforeMount () {
      this.$store.dispatch('InterviewMount', {
        id: this.$route.params.id
      })
    }
  }
</script>