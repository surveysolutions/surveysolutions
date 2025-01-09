<template>
    <div class="information-block text-danger" v-if="!$me.validity.isValid || $me.validity.errorMessage"
        :id="`validation_${$me.id}_block`">
        <h6 v-if="$me.validity.errorMessage">
            {{ $t("WebInterviewUI.AnswerWasNotSaved") + ($me.validity.notSavedAnswerValue ? ': "' +
                $me.validity.notSavedAnswerValue + '"' : '') }}
        </h6>
        <template v-for="message in $me.validity.messages" :key="message">
            <p v-dateTimeFormatting v-dompurify-html="message"></p>
        </template>
    </div>
</template>
<script lang="js">
import { entityPartial } from '~/webinterview/components/mixins'

export default {
    mixins: [entityPartial],
    name: 'wb-validation',
}
</script>
