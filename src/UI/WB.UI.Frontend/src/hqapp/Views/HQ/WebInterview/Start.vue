<template>
    <div>
        <div class="row-element mb-20">
            <h2>{{ $config.model.welcomeText }}</h2>
        </div>

        <StartOrResumeForm :buttonTitle="$config.model.startInterviewButton" />
    </div>
</template>

<script>

import StartOrResumeForm from './_StartOrResumeForm'

export default {
    components: {
        StartOrResumeForm,
    },
}

</script>
