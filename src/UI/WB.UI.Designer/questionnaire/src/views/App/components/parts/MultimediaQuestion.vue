<template>
    <div class="row">
        <div class="col-md-5">
            <div class="checkbox checkbox-in-column">
                <input id="cb-is-signature" type="checkbox" class="wb-checkbox" v-model="activeQuestion.isSignature" />
                <label for="cb-is-signature"><span></span>{{ $t('QuestionnaireEditor.IsSignature') }}</label>
            </div>
        </div>
    </div>
</template>

<script>

export default {
    name: 'MultimediaQuestion',
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {

        }
    },
}
</script>
