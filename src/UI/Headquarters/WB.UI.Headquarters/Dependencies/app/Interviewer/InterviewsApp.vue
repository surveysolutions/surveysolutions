<template>

    <Layout :title="title" hasFilter="true">
        <Filters slot="filters">
            <FilterBlock :title="title">
                <Typeahead data-vv-name="questionnaireId"
                                       data-vv-as="questionnaire"
                                       :placeholder="$t('Common.Any')"
                                       control-id="questionnaireId"
                                       
                                       :value="questionnaireId"
                                       v-on:selected="questionnaireSelected"
                                       :fetch-url="$config.interviewerHqEndpoint + '/QuestionnairesCombobox'"></Typeahead>
            </FilterBlock>
        </Filters>
        <router-view
            :questionnaireId="questionnaireId"
        ></router-view>
    </Layout>

</template>

<script>

export default {
    data() {
        return {
            questionnaireId: null,
            title: "Change me"// this.$t('MainMenu.' + this.$router.props.status)
        }
    },

    methods: {
        questionnaireSelected() {
            console.log(arguments)
        }
    }
}
</script>