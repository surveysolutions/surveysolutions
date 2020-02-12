<template>
    <div v-if="question != null">
        <p>{{ title }}</p>
        <ol class="breadcrumb">
            <li v-for="breadcrumb in question.breadcrumbs"
                :key="breadcrumb"
                class="btn-link">{{ breadcrumb }} </li>
        </ol>
        <dl v-if="question">
            <dt v-if="question.label">
                {{ $t("Reports.QuestionLabel") }}:
            </dt>
            <dd v-if="question.label">
                {{question.label}}
            </dd>
            <dt>{{ $t("Reports.QuestionText") }}:</dt>
            <dd>{{question.questionText}}</dd>
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
