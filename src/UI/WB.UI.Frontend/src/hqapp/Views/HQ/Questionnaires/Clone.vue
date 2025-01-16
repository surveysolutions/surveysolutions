<template>
    <HqLayout :hasFilter="false" :fixedWidth="true" :has-row="false">
        <template v-slot:headers>
            <div>
                <ol class="breadcrumb">
                    <li>
                        <a href="../../SurveySetup">{{ $t('MainMenu.SurveySetup') }}</a>
                    </li>
                </ol>
                <h1>{{ $t('QuestionnaireClonning.CloneQuestionnaireTitle') }}</h1>
            </div>
        </template>

        <div class="row">
            <div class="col-sm-8">
                <h2>
                    {{ $t('QuestionnaireClonning.ToCloneQuestionnaire') }}
                    <b>{{ $t('Pages.QuestionnaireNameFormat', {
                        name: this.$config.model.originalTitle, version:
                            this.$config.model.version
                    }) }}</b>
                </h2>
            </div>
        </div>
        <div class="row">
            <Form method="post" class="col-sm-8" ref="frmClone" v-slot="{ meta }">
                <div class="form-group" v-bind:class="{ 'has-error': meta.valid == false && meta.touched }">
                    <label for="NewTitle" class="control-label">
                        {{ $t('FieldsAndValidations.CloneQuestionnaireModel_NewTitle_Label') }}
                    </label>
                    <Field type="text" id="NewTitle" name="NewTitle" :rules="{ required: true }" class="form-control"
                        autocomplete="off" />
                    <span v-if="meta.valid == false && meta.touched" class="help-block field-validation-error">
                        <span>{{ $t('FieldsAndValidations.CloneQuestionnaireModel_NewTitle_Error_Required') }}</span>
                    </span>
                    <span v-if="$config.model.error" class="help-block field-validation-error">
                        <span>{{ $config.model.error }}</span>
                    </span>
                </div>
                <div class="form-group">
                    <label for="Comment" class="control-label">
                        {{ $t('Assignments.DetailsComments') }}
                    </label>
                    <textarea name="Comment" id="Comment" class="form-control"
                        :placeholder="$t('Assignments.EnterComments')" rows="6" maxlength="500"></textarea>
                </div>
                <input name="__RequestVerificationToken" type="hidden" :value="this.$hq.Util.getCsrfCookie()" />
                <input name="Id" type="hidden" :value="$config.model.id" />
                <input name="Version" type="hidden" :value="$config.model.version" />
                <input name="OriginalTitle" type="hidden" :value="$config.model.originalTitle" />
                <input name="IsCensus" type="hidden" :value="$config.model.isCensus" />

                <div class="action-buttons">
                    <button type="submit" class="btn btn-success">
                        {{ $t('QuestionnaireClonning.Clone') }}
                    </button>
                    <a href="../../SurveySetup" class="back-link">
                        {{ $t('Common.Cancel') }}
                    </a>
                </div>
            </Form>
        </div>
    </HqLayout>
</template>
<script>

import { Form, Field, ErrorMessage } from 'vee-validate'

export default {
    components: {
        Form,
        Field,
        ErrorMessage,
    }
}
</script>