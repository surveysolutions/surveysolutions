<template>
    <div class="form-group">
        <label for="edit-mask">{{ $t('QuestionnaireEditor.QuestionPattern') }}
            <help link="mask" />
        </label>
        <input id="edit-mask" type="text" v-model="activeQuestion.mask" class="form-control">
    </div>
</template>

<script>

import Help from './../Help.vue'

export default {
    name: 'TextQuestion',
    components: {
        Help,
    },
    props: {
        activeQuestion: { type: Object, required: true }
    },
    data() {
        return {

        }
    },
}
</script>
