<template>
    <v-app>
        <v-main>
            <div class="questionnaire container">
                <router-view name="header"></router-view>
                <router-view name="tree"></router-view>
                <section>
                    <router-view />
                </section>
            </div>
        </v-main>
    </v-app>
</template>

<script>
export default {
    name: 'QuestionnaireApp'
};
</script>
