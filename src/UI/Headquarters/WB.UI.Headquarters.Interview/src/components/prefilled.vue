<template>
    <div>
        <h1></h1>
        <ul>
            <li v-for="question in prefilledQuestions">
                {{question.title}}
            </li>
        </ul>
    </div>
</template>

<script lang="ts">
  import {mapGetters} from "vuex"

  export default {
    name: 'prefilled-view',
    watch: {
        "$route": (to, from) => {

        }
    },
    computed: mapGetters(["prefilledQuestions"]),
    beforeMount () {
    //   this.$store.dispatch('InterviewMount', {
    //     id: this.$route.params.id
    //   })
    }
  }
</script>
