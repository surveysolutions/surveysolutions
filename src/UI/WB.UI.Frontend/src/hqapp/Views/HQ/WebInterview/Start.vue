<template>
    <div>
        <div class="row-element mb-20">
            <h2>{{ $config.model.welcomeText }}</h2>
            <p v-if="$config.model.description">
                {{$config.model.description}}
            </p>
        </div>

        <StartOrResumeForm :buttonTitle="$t('WebInterview.Start')" />
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
