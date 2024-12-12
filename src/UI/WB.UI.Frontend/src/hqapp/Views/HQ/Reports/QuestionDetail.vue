<template>
    <div v-if="question != null">
        <p>{{ title }}</p>
        <ol class="breadcrumb">
            <li v-for="breadcrumb in question.Breadcrumbs" :key="breadcrumb" class="btn-link">{{ breadcrumb }} </li>
        </ol>
        <dl v-if="question">
            <dt v-if="question.Label">
                {{ $t("Reports.QuestionLabel") }}:
            </dt>
            <dd v-if="question.Label" v-dompurify-html="question.Label" />
            <dt>{{ $t("Reports.QuestionText") }}:</dt>
            <dd>{{ question.QuestionText }}</dd>
        </dl>
    </div>
</template>

<script>
export default {
    props: {
        title: String,
        question: {
            type: Object,
        },
    },
}
</script>
