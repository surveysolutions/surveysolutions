<template>
    <div v-if="question != null">
        <p>{{ title }}</p>
        <ol class="breadcrumb">
            <li v-for="breadcrumb in question.Breadcrumbs" :key="breadcrumb" class="btn-link">{{ breadcrumb }} </li>
        </ol>
        <dl v-if="question">
            <dt>{{ $t("Reports.QuestionLabel") }}:</dt>
            <dd>{{question.Label}}</dd>
            <dt>{{ $t("Reports.QuestionText") }}:</dt>
            <dd>{{question.QuestionText}}</dd>
        </dl>
    </div>
</template>

<script>
export default {
    props: {
        title: String,
        question: {
            type: Object
        }
    }
};
</script>
