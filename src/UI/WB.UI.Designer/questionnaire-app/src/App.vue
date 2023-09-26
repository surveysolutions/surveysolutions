<template>
    <v-app>
        <v-main>
            <div  id="designer-editor" class="questionnaire container">
                <router-view name="header"></router-view>
                <section id="spacer" class="row">
                    <div class="left">
                        <router-view name="tree"></router-view>
                    </div>
                    <div class="right">
                        <router-view />
                    </div>
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
