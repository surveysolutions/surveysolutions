<template>
    <div>
        <div class="row-element mb-20">
            <h2>{{ $config.model.welcomeText }}</h2>
            <p v-if="$config.model.description"
                v-html="description">
            </p>
        </div>
        <StartOrResumeForm :buttonTitle="$config.model.startNewButton"
            :resumeButtonTitle="$config.model.resumeButton" />
    </div>
</template>

<script>

import StartOrResumeForm from './_StartOrResumeForm'
import { marked } from 'marked'

export default {
    components: {
        StartOrResumeForm,
    },
    computed:{
        description() {
            return marked(this.$config.model.description)
        },
    },
}

</script>
